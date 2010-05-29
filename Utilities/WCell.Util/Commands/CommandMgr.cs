/*************************************************************************
 *
 *   file		: CommandMgr.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-04-03 16:34:47 +0800 (Thu, 03 Apr 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 221 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NLog;
using WCell.Util;
using WCell.Util.DynamicAccess;
using WCell.Util.Strings;
using WCell.Util.Toolshed;
using System.IO;

namespace WCell.Util.Commands
{
	/// <summary>
	/// Command provider class
	/// </summary>
	public abstract class CommandMgr<C>
		where C : ICmdArgs
	{
		/// <summary>
		/// Validates whether a command may be triggered.
		/// </summary>
		/// <param name="trigger"></param>
		/// <param name="cmd"></param>
		/// <param name="silent">Whether there should be no output during this check</param>
		/// <returns>Whether the given command may be triggered by the given trigger</returns>
		public delegate bool TriggerValidationHandler(CmdTrigger<C> trigger, BaseCommand<C> cmd, bool silent);
		public delegate void UnknownCommandHandler(CmdTrigger<C> trigger);

		protected static Logger log = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Is triggered whenever an unknown command has been used
		/// </summary>
		public event UnknownCommandHandler UnknownCommand;

		/// <summary>
		/// Validates whether a the given command may be triggered (checks for privileges etc)
		/// </summary>
		public TriggerValidationHandler TriggerValidator;

		readonly IDictionary<string, Command<C>> commandsByAlias;
		readonly IDictionary<string, Command<C>> commandsByName;

		/// <summary>
		/// Includes Subcommands
		/// </summary>
		readonly internal IDictionary<long, BaseCommand<C>> allCommandsByType = new Dictionary<long, BaseCommand<C>>();
		//public readonly HelpCommand HelpCommandInstance;

		public CommandMgr()
		{
			commandsByAlias = new Dictionary<string, Command<C>>(StringComparer.InvariantCultureIgnoreCase);
			commandsByName = new Dictionary<string, Command<C>>(StringComparer.InvariantCultureIgnoreCase);

			//HelpCommandInstance = new HelpCommand(this);
			Add(new HelpCommand(this));
			Add(new ExecFileCommand(this));
		}

		public abstract string ExecFileDir
		{
			get;
		}

        #region Execute custom commands
		/// <summary>
		/// Executes a specific Command with parameters.
		/// 
		/// Interprets the first word as alias, takes all enabled Commands with the specific alias out of the 
		/// CommandsByAlias-map and triggers the specific Process() method on all of them.
		/// If the processing of the command raises an Exception, the fail events are triggered.
		/// </summary>
		/// <returns>True if at least one Command was triggered, otherwise false.</returns>
		public virtual bool Trigger(CmdTrigger<C> trigger)
		{
			var cmd = GetCommand(trigger);
			if (cmd != null)
			{
				return Trigger(trigger, cmd);
			}
			return false;
		}

		public bool TriggerAll(CmdTrigger<C> trigger, List<Command<C>> commands)
		{
			foreach (var cmd in commands)
			{
				if (!Trigger(trigger, cmd))
				{
					return false;
				}
			}
			return true;
		}

        /// <summary>
        /// Call Eval on specified command
        /// </summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public virtual object EvalNext(CmdTrigger<C> trigger, object deflt)
        {
            if (trigger.Text.ConsumeNext('('))
            {
                var nestedCmdStr = trigger.Text.NextWord(")");
                if (!trigger.Text.HasNext && !trigger.Text.String.EndsWith(")"))
                {
                    // no closing bracket
                    return null;
                }

                var forkTrigger = trigger.Nest(nestedCmdStr);
                var cmd = GetCommand(forkTrigger);
                if (cmd != null)
                {
                    return Eval(forkTrigger, cmd);
                }
                return false;
            }
            // not an evaluatable expression
            return deflt;
        }

		public virtual BaseCommand<C> GetCommand(CmdTrigger<C> trigger)
		{
			string alias;
			var pos = trigger.Text.Position;

			BaseCommand<C> cmd;
			if (trigger.selectedCmd != null && (cmd = trigger.selectedCmd.SelectSubCommand(trigger.Text)) != null)
			{
				alias = trigger.selectedCmd.Aliases[0];
			}
			else
			{
				trigger.Text.Position = pos;
				alias = trigger.Text.NextWord();
				cmd = Get(alias);
			}

			trigger.Alias = alias;

			if (cmd != null)
			{
				return cmd;
			}

			var evt = UnknownCommand;
			if (evt != null)
			{
				evt(trigger);
			}
			return null;
		}

		public bool Trigger(CmdTrigger<C> trigger, BaseCommand<C> cmd)
		{
			return Trigger(trigger, cmd, false);
		}

		/// <summary>
		/// Lets the given CmdTrigger trigger the given Command.
		/// </summary>
		/// <param name="trigger"></param>
		/// <param name="cmd"></param>
		/// <param name="silentFail">Will not reply if it failed due to target restrictions or privileges etc</param>
		/// <returns></returns>
		public virtual bool Trigger(CmdTrigger<C> trigger, BaseCommand<C> cmd, bool silentFail)
		{
			if (cmd.Enabled)
			{
				var rootCmd = cmd.RootCmd;
				if (!rootCmd.MayTrigger(trigger, cmd, silentFail))
				{
					//trigger.Reply("Cannot trigger Command: " + rootCmd);
				}
				else if (TriggerValidator(trigger, cmd, silentFail))
				{
					trigger.cmd = cmd;

					try
					{
						cmd.Process(trigger);

						// command callbacks
						rootCmd.ExecutedNotify(trigger);

						// TODO: Create a tree of expected responses?
						//string[] expectedReplies = cmd.ExpectedServResponses;
						//if (expectedReplies != null) {
						//    trigger.expectsServResponse = true;

						//    foreach (string reply in expectedReplies) {
						//        AddWaitingTrigger(reply, trigger);
						//    }
						//}
					}
					catch (Exception e)
					{
						rootCmd.FailNotify(trigger, e);
					}
					return true;
				}
				return false;
			}
			trigger.Reply("Command is disabled: " + cmd);
			return false;
        }

	    /// <summary>
	    /// Lets the given CmdTrigger trigger the given Command.
	    /// </summary>
	    /// <param name="trigger"></param>
	    /// <param name="cmd"></param>
	    /// <param name="silentFail">Will not reply if it failed due to target restrictions or privileges etc</param>
	    /// <returns></returns>
	    public virtual object Eval(CmdTrigger<C> trigger, BaseCommand<C> cmd)
	    {
	        return Eval(trigger, cmd, false);
	    }

	    /// <summary>
        /// Lets the given CmdTrigger trigger the given Command.
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="cmd"></param>
        /// <param name="silentFail">Will not reply if it failed due to target restrictions or privileges etc</param>
        /// <returns></returns>
        public virtual object Eval(CmdTrigger<C> trigger, BaseCommand<C> cmd, bool silentFail)
        {
            if (cmd.Enabled)
            {
                var rootCmd = cmd.RootCmd;
                if (rootCmd.MayTrigger(trigger, cmd, silentFail) && TriggerValidator(trigger, cmd, silentFail))
                {
                    trigger.cmd = cmd;

                    try
                    {
                        var obj = cmd.Eval(trigger);

                        rootCmd.ExecutedNotify(trigger);

                        return obj;
                    }
                    catch (Exception e)
                    {
                        rootCmd.FailNotify(trigger, e);
                    }
                    return true;
                }
                return false;
            }
            trigger.Reply("Command is disabled: " + cmd);
            return false;
        }

		/// <summary>
		/// Returns the Command with the given Name.
		/// </summary>
		public Command<C> this[string name]
		{
			get
			{
				Command<C> cmd;
				commandsByName.TryGetValue(name, out cmd);
				return cmd;
			}
		}

		/// <summary>
		/// Add a trigger that awaits a server-response;
		/// </summary>
		//internal void AddWaitingTrigger(string reply, CmdTrigger trigger)
		//{
		//    Queue<CmdTrigger> triggers;
		//    if (awaitedResponses.TryGetValue(reply, out triggers)) {
		//        triggers = new Queue<CmdTrigger>();
		//        awaitedResponses.Add(reply, triggers);
		//    }
		//    triggers.Enqueue(trigger);
		//}


		//internal void NotifyServResponse(string sender, string action, string remainder)
		//{
		//    // get the next trigger from the queue that awaits a certain response to the command being used.
		//    // TODO: implement protocol engine that defines requests and response-maps between
		//    //			protocol actions and client-states
		//    Queue<CmdTrigger> triggers;
		//    awaitedResponses.TryGetValue(action, out triggers);
		//    if (triggers != null) {
		//        CmdTrigger trigger = triggers.Dequeue();
		//        trigger.NotifyServResponse(sender, action, remainder);
		//    }
		//}
		#endregion

		//static IDictionary<Type, Command> commandMapByType;

		/// <summary>
		/// The Table of all Commands which exists for the use of the ReactTo() method
		/// (Filled by the Initialize() method).
		/// The keys are all possible aliases of all commands and the values are ArrayLists of Commands 
		/// which are associated with the specific alias.
		/// The aliases are stored case-insensitively. 
		/// Use the Remove(Command) and Add(Command) methods to manipulate this CommandsByAlias.
		/// </summary>
		public IDictionary<string, Command<C>> CommandsByAlias
		{
			get { return commandsByAlias; }
		}

		public ICollection<Command<C>> Commands
		{
			get { return commandsByName.Values; }
		}


		#region CRUD
		//public static IDictionary<Type, Command> CommandsByType {
		//    get { return commandMapByType; }
		//}

		/// <summary>
		/// Adds a Command to the CommandsByAlias.
		/// </summary>
		public void Add(Command<C> cmd)
		{
			//Type type = cmd.GetType();
			//if (commandMapByType.ContainsKey(type)) {
			//    throw new Exception("Trying to create a second instance of a Singleton Command-object");
			//}

			//// map by type
			//commandMapByType[type] = cmd;
			cmd.DoInit();
			commandsByName.Add(cmd.Name, cmd);

			// Add to table, mapped by aliases
			foreach (var alias in cmd.Aliases)
			{
				if (commandsByAlias.ContainsKey(alias))
				{
					log.Warn("Command alias \"{0}\" was used by more than 1 Command: {1} and {2}", alias, commandsByAlias[alias], cmd);
				}
				commandsByAlias[alias] = cmd;
			}

			var ptr = cmd.GetType().TypeHandle.Value.ToInt64();
			allCommandsByType.Add(ptr, cmd);

			cmd.mgr = this;
			foreach (var subCmd in cmd.SubCommands)
			{
				ptr = subCmd.GetType().TypeHandle.Value.ToInt64();
				if (allCommandsByType.ContainsKey(ptr))
				{
					throw new InvalidOperationException("Tried to add 2 Commands of the same Type: " + subCmd.GetType());
				}
				allCommandsByType.Add(ptr, subCmd);
				subCmd.mgr = this;
			}
		}

		/// <summary>
		/// Adds a Command-instance of the specific type, if it is a Command-type
		/// </summary>
		public void Add(Type cmdType)
		{
			if (cmdType.IsSubclassOf(typeof(Command<C>)) && !cmdType.IsAbstract)
			{
				//var cmd = (Command<C>)Activator.CreateInstance(cmdType);
				var ctor = cmdType.GetConstructor(
					BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
					null, new Type[0], new ParameterModifier[0]);

				if (ctor == null)
				{
					throw new ArgumentException(cmdType.FullName + " lacks parameterless constructor.");
				}

				var cmd = (Command<C>)ctor.Invoke(new object[0]);
				Add(cmd);
			}
		}

		/// <summary>
		/// Finds and adds all Commands of the given Assembly
		/// </summary>
		public void AddCmdsOfAsm(Assembly asm)
		{
			foreach (var type in asm.GetTypes())
			{
				Add(type);
			}
		}

		/// <summary>
		/// Removes a Command.
		/// </summary>
		public void Remove(Command<C> cmd)
		{
			//commandMapByType.Remove(cmd.GetType());
			commandsByName.Remove(cmd.Name);

			foreach (string alias in cmd.Aliases)
			{
				commandsByAlias.Remove(alias);
			}

			allCommandsByType.Remove(cmd.GetType().TypeHandle.Value.ToInt64());
			foreach (var subCmd in cmd.SubCommands)
			{
				allCommandsByType.Remove(subCmd.GetType().TypeHandle.Value.ToInt64());
			}
		}

		/// <summary>
		/// Returns all Commands with the given Alias
		/// </summary>
		public Command<C> Get(string alias)
		{
			Command<C> commands;
			commandsByAlias.TryGetValue(alias, out commands);
			return commands;
		}

		public T Get<T>() where T : BaseCommand<C>
		{
			BaseCommand<C> cmd;
			allCommandsByType.TryGetValue(typeof(T).TypeHandle.Value.ToInt64(), out cmd);
			return (T)cmd;
		}

		public List<BaseCommand<C>> GetCommands(string cmdString)
		{
			var list = new List<BaseCommand<C>>();
			GetCommands(new StringStream(cmdString), list);
			return list;
		}

		public void GetCommands(StringStream cmdString, List<BaseCommand<C>> list)
		{
			var str = cmdString.NextWord();
			var cmds = commandsByName.Values.Where(comd =>
				comd.Aliases.Where(alias => alias.IndexOf(str, StringComparison.InvariantCultureIgnoreCase) > -1).Count() > 0);

			if (cmdString.HasNext && cmds.Count() == 1)
			{
				cmds.First().GetSubCommands(cmdString, list);
			}
			else
			{
				foreach (var cmd in cmds)
				{
					list.Add(cmd);
				}
			}
		}
		#endregion

		public bool MayDisplay(CmdTrigger<C> trigger, BaseCommand<C> cmd, bool ignoreRestrictions)
		{
			return cmd.Enabled &&
				   (ignoreRestrictions || cmd.RootCmd.MayTrigger(trigger, cmd, true));
		}

		public BaseCommand<C> SelectCommand(string cmdString)
		{
			return SelectCommand(new StringStream(cmdString));
		}

		public BaseCommand<C> SelectCommand(StringStream cmdString)
		{
			var cmd = Get(cmdString.NextWord());
			if (cmd != null && cmdString.HasNext)
			{
				return cmd.SelectSubCommand(cmdString);
			}
			return cmd;
		}

		/// <summary>
		/// Removes all Commands of the specific Type from the CommandsByAlias.
		/// </summary>
		/// <returns>True if any commands have been removed, otherwise false.</returns>
		//public static bool Remove(Type cmdType) {
		//    Command cmd = Get(cmdType);
		//    if (cmd != null) {
		//        Remove(cmd);
		//        return true;
		//    }
		//    return false;
		//}

		public void AddDefaultCallCommand(ToolMgr mgr)
		{
			Add(new CallCommand(mgr));
		}

		/// <summary>
		/// Gives help
		/// TODO: Localization
		/// </summary>
		public void TriggerHelp(CmdTrigger<C> trigger)
		{
			TriggerHelp(trigger, false);
		}

		/// <summary>
		/// Removes all Commands of the specific Type from the CommandsByAlias.
		/// </summary>
		/// <returns>True if any commands have been removed, otherwise false.</returns>
		//public static bool Remove(Type cmdType) {
		//    Command cmd = Get(cmdType);
		//    if (cmd != null) {
		//        Remove(cmd);
		//        return true;
		//    }
		//    return false;
		//}

		/// <summary>
		/// Gives help
		/// TODO: Localization
		/// </summary>
		public void TriggerHelp(CmdTrigger<C> trigger, bool ignoreRestrictions)
		{
			if (trigger.Text.HasNext)
			{
				// help for a specific command
				var cmdStr = trigger.Text.Remainder;
				var cmds = GetCommands(cmdStr);
				var count = cmds.Count;
				foreach (var cmd in cmds)
				{
					if (MayDisplay(trigger, cmd, ignoreRestrictions))
					{
						DisplayCmd(trigger, cmd, ignoreRestrictions, true);
					}
					else
					{
						count--;
					}
				}

				if (count == 0)
				{
					trigger.ReplyFormat("Did not find any Command that matches '{0}'.", cmdStr);
				}
			}
			else
			{
				// help for all commands
				trigger.ReplyFormat("Use: ? <Alias> [<subalias> [<subalias> ...]] for help on a certain command.");
				trigger.Reply("All available commands:");

				foreach (var cmd in Commands)
				{
					if (MayDisplay(trigger, cmd, ignoreRestrictions))
					{
						trigger.Reply(cmd.CreateUsage(trigger));
					}
				}
			}
		}

		public void DisplayCmd(CmdTrigger<C> trigger, BaseCommand<C> cmd)
		{
			DisplayCmd(trigger, cmd, false, true);
		}

		public void DisplayCmd(CmdTrigger<C> trigger, BaseCommand<C> cmd, bool ignoreRestrictions, bool detail)
		{
			trigger.Reply(string.Format("{0}{1}", cmd.CreateUsage(trigger), detail ? " (" + cmd.GetDescription(trigger) + ")" : ""));

			if (cmd.SubCommands.Count > 0)
			{
				trigger.Reply("All SubCommands:");
				foreach (var subCmd in cmd.SubCommands)
				{
					if (MayDisplay(trigger, subCmd, ignoreRestrictions))
					{
						DisplayCmd(trigger, subCmd, ignoreRestrictions, detail);
					}
				}
			}
		}

		#region ExecFile
		public void ExecFile(string filename, C args)
		{
			var trigger = new ConsoleCmdTrigger(args);
			ExecFile(filename, trigger, null);
		}

		public void ExecFile(string filename, C args, Func<CmdTrigger<C>, int, bool> cmdValidator)
		{
			var trigger = new ConsoleCmdTrigger(args);
			ExecFile(filename, trigger, cmdValidator);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="trigger"></param>
		public virtual void ExecFile(string filename, CmdTrigger<C> trigger)
		{
			ExecFile(filename, trigger, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="trigger"></param>
		/// <param name="cmdValidator">Validates whether the given trigger may execute. Second parameter is line no.</param>
		public void ExecFile(string filename, CmdTrigger<C> trigger, Func<CmdTrigger<C>, int, bool> cmdValidator)
		{
			var line = 0;
			using (var reader = new StreamReader(filename))
			{
				string cmd;
				while ((cmd = reader.ReadLine()) != null)
				{
					line++;
					cmd = cmd.Trim();
					// '#' starts a comment
					if (!cmd.StartsWith("#") && cmd.Length > 0)
					{
						var text = new StringStream(cmd);
						trigger.Text = text;
						if (cmdValidator != null && !cmdValidator(trigger, line))
						{
							continue;
						}

						if (!Trigger(trigger))
						{
							trigger.Reply("Could not execute Command from file \"{0}\" (line {1}): \"{2}\"", filename, line,
										  cmd);
						}
					}
				}
			}
		}
		#endregion

		#region ConsoleCmdTrigger
		/// <summary>
		/// Default trigger for Console-interaction
		/// </summary>
		public class ConsoleCmdTrigger : CmdTrigger<C>
		{
			public ConsoleCmdTrigger(string text, C args)
				: this(new StringStream(text), args)
			{
			}

			public ConsoleCmdTrigger(StringStream text, C args)
				: base(text, args)
			{
			}

			public ConsoleCmdTrigger(C args)
				: base(null, args)
			{
			}

			public override void Reply(string text)
			{
				Console.WriteLine(text);
			}

			public override void ReplyFormat(string text)
			{
				Console.WriteLine(text);
			}
		}
		#endregion

		#region CallCommand
		public class CallCommand : Command<C>
		{
			public readonly ToolMgr ToolMgr;

			public CallCommand(ToolMgr toolMgr)
			{
				ToolMgr = toolMgr;
			}

			protected override void Initialize()
			{
				Init("Call", "C", "@", "$");
				EnglishParamInfo = "(-l [<wildmatch>]|-<i>)|<methodname>[ <arg0> [<arg1> [...]]]";
				EnglishDescription = "Calls any static method or custom function with the given arguments. Either use the name or the index of the function.";
			}

			public override void Process(CmdTrigger<C> trigger)
			{
				//var subAlias = trigger.Text.NextWord();

				//SubCommand subCmd;
				//if (m_subCommands.TryGetValue(subAlias, out subCmd))
				//{
				//    subCmd.Process(trigger);
				//}
				//else
				//{
				var txt = trigger.Text;
				if (!txt.HasNext)
				{
					trigger.Reply("Invalid arguments - " + CreateInfo(trigger));
				}
				else
				{
					IExecutable exec;
					if (txt.ConsumeNext('-'))
					{
						if (!txt.HasNext)
						{
							trigger.Reply("Invalid arguments - " + CreateInfo(trigger));
							return;
						}

						var c = txt.Remainder[0];
						if (Char.ToLower(c) == 'l')
						{
							txt.Position++;
							var matches = txt.Remainder.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
							var toolMgr = ((CallCommand)RootCmd).ToolMgr;
							trigger.Reply("Callable functions ({0}):", toolMgr.Executables.Count);
							for (var i = 0; i < toolMgr.ExecutableList.Count; i++)
							{
								var executable = toolMgr.ExecutableList[i];
								if (matches.Length != 0)
								{
									var found = false;
									foreach (var match in matches)
									{
										if (executable.Name.IndexOf(match, StringComparison.InvariantCultureIgnoreCase) > -1)
										{
											// match
											found = true;
											break;
										}
									}
									if (!found)
									{
										continue;
									}
								}
								trigger.Reply(" {0}: {1}", i, executable);
							}
							return;
						}

						// specified index
						var index = txt.NextUInt(uint.MaxValue);
						if (index < ToolMgr.ExecutableList.Count)
						{
							exec = ToolMgr.ExecutableList[(int)index];
						}
						else
						{
							exec = null;
						}
					}
					else
					{
						// name
						var name = txt.NextWord();
						exec = ToolMgr.Get(name);
					}

					if (exec == null)
					{
						trigger.Reply("Could not find specified Executable.");
					}
					else
					{
						var len = exec.ParameterTypes.Length;
						var args = new object[len];
						object value = null;
						for (var i = 0; i < len; i++)
						{
							var paramType = exec.ParameterTypes[i];
							var str = (i == len - 1) ? txt.Remainder : txt.NextWord(); // check for last argument
							Utility.Parse(str, paramType, ref value);
							args[i] = value;
						}
						exec.Exec(args);
					}

					//base.Process(trigger);
					//trigger.Reply("SubCommand not found: " + subAlias);
					//trigger.Text.Skip(trigger.Text.Length);
					//mgr.DisplayCmd(trigger, this, false);
				}
			}

			//public class ListCommand : SubCommand
			//{
			//    protected override void Initialize()
			//    {
			//        Init("List", "L");
			//        ParamInfo = "";
			//        Description = "Lists all callable functions.";
			//    }

			//    public override void Process(CmdTrigger<C> trigger)
			//    {
			//    }
			//}
		}
		#endregion

		#region HelpCommand
		/// <summary>
		/// TODO: Use localized strings
		/// The help command is special since it generates output.
		/// This output needs to be shown in the GUI if used from commandline and 
		/// sent to the requester if executed remotely.
		/// </summary>
		public class HelpCommand : Command<C>
		{
			private readonly CommandMgr<C> m_Mgr;

			public HelpCommand(CommandMgr<C> mgr)
			{
				m_Mgr = mgr;
			}

			public CommandMgr<C> Mgr
			{
				get { return m_Mgr; }
			}

			protected override void Initialize()
			{
				Init("Help", "?");
				EnglishParamInfo = "[<part of cmd> [[<part of subcmd>] <part of subcmd> ...]]";
				EnglishDescription = "Shows an overview over all Commands or -if specified- the help for a specific Command (and its subcommands).";
			}

			public override void Process(CmdTrigger<C> trigger)
			{
				m_Mgr.TriggerHelp(trigger, false);
			}
		}
		#endregion

		#region ExecFileCommand
		/// <summary>
		/// The help command is special since it generates output.
		/// This output needs to be shown in the GUI if used from commandline and 
		/// sent to the requester if executed remotely.
		/// </summary>
		public class ExecFileCommand : Command<C>
		{
			private readonly CommandMgr<C> m_Mgr;

			public ExecFileCommand(CommandMgr<C> mgr)
			{
				m_Mgr = mgr;
			}

			public CommandMgr<C> Mgr
			{
				get { return m_Mgr; }
			}

			protected override void Initialize()
			{
				Init("ExecFile");
				EnglishParamInfo = "<filename>";
				EnglishDescription = "Executes the given file.";
			}

			public override void Process(CmdTrigger<C> trigger)
			{
				if (!trigger.Text.HasNext)
				{
					trigger.Reply("No file was specified.");
					return;
				}
				var file = trigger.Text.NextWord();
				if (!Path.IsPathRooted(file))
				{
					file = Path.Combine(m_Mgr.ExecFileDir, file);
				}
				if (File.Exists(file))
				{
					m_Mgr.ExecFile(file, trigger);
				}
				else
				{
					trigger.Reply("File to execute does not exist: " + file);
				}
			}
		}
		#endregion

		#region Globals
		/// <summary>
		/// 
		/// </summary>
		//public class GlobalCommand : Command<C>
		//{
		//    //public readonly GlobalVariableCollection<WCellVariableDefinition>

		//    protected override void Initialize()
		//    {
		//        Init("Global", "Glob");
		//        Description = "Provides commands to interact with Global variables.";
		//    }

		//    public abstract class GlobalSubCmd : SubCommand
		//    {
		//        public GlobalCommand GlobalRootCmd
		//        {
		//            get { return (GlobalCommand) RootCmd; }
		//        }
		//    }

		//    public class SetGlobalCommand : GlobalSubCmd
		//    {
		//        protected SetGlobalCommand() { }

		//        protected override void Initialize()
		//        {
		//            Init("Set", "S");
		//            ParamInfo = "<globalVar> <value>";
		//            Description = "Sets the value of the given global variable.";
		//        }

		//        public override void Process(CmdTrigger<C> trigger)
		//        {
		//            if (trigger.Text.HasNext)
		//            {
		//                var name = trigger.Text.NextWord();
		//                var value = trigger.Text.Remainder;
		//                if (value.Length == 0)
		//                {
		//                    trigger.Reply("Invalid value:");
		//                    trigger.Reply(CreateInfo(trigger));
		//                }
		//                else
		//                {
		//                    var rootCmd = GlobalRootCmd;
		//                    var def = rootCmd.mgr.GetDefinition(name);
		//                    if (def != null)
		//                    {
		//                        if (def.MaySet(trigger.Args.Role))
		//                        {
		//                            if (WCellVariables.Instance.Set(name, value))
		//                            {
		//                                trigger.Reply("Variable \"{0}\" is now set to: " + value, def.Name);
		//                            }
		//                            else
		//                            {
		//                                trigger.Reply("Unable to set Variable \"{0}\" to value: {1}.", def.Name, value);
		//                            }
		//                        }
		//                        else
		//                        {
		//                            trigger.Reply("You do not have enough privileges to set Variable \"{0}\".", def.Name);
		//                        }
		//                    }
		//                    else
		//                    {
		//                        trigger.Reply("Variable \"{0}\" does not exist.", name);
		//                    }
		//                }
		//            }
		//            else
		//            {
		//                trigger.Reply("Invalid arguments:");
		//                trigger.Reply(CreateInfo(trigger));
		//            }
		//        }
		//    }

		//    /// <summary>
		//    /// 
		//    /// </summary>
		//    public class GetGlobalCommand : GlobalSubCmd
		//    {
		//        protected GetGlobalCommand() { }

		//        protected override void Initialize()
		//        {
		//            Init("Get", "G");
		//            ParamInfo = "<globalVar>";
		//            Description = "Gets the value of the given global variable.";
		//        }

		//        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		//        {
		//            if (trigger.Text.HasNext)
		//            {
		//                string name = trigger.Text.NextWord();
		//                var def = WCellVariables.Instance.GetDefinition(name);

		//                if (def != null)
		//                {
		//                    if (def.MayGet(trigger.Args.Role))
		//                    {
		//                        trigger.Reply("Variable \"{0}\" is: " + def.Value, def.Name);
		//                    }
		//                    else
		//                    {
		//                        trigger.Reply("You do not have enough privileges to get Variable \"{0}\".", def.Name);
		//                    }
		//                }
		//                else
		//                {
		//                    trigger.Reply("Variable \"{0}\" does not exist.", name);
		//                }
		//            }
		//            else
		//            {
		//                trigger.Reply("Invalid arguments:");
		//                trigger.Reply(CreateInfo(trigger));
		//            }
		//        }
		//    }

		//    /// <summary>
		//    /// 
		//    /// </summary>
		//    public class ListGlobalsCommand : GlobalSubCmd
		//    {
		//        protected ListGlobalsCommand() { }

		//        protected override void Initialize()
		//        {
		//            Init("List", "L");
		//            ParamInfo = "[<name Part>]";
		//            Description = "Lists all global variables. If specified only shows variables that contain the given name Part.";
		//        }

		//        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		//        {
		//            var vars = new List<WCellVariableDefinition>(50);

		//            var filter = trigger.Text.Remainder;
		//            if (filter.Length > 0)
		//            {
		//                vars.AddRange(WCellVariables.Instance.ByName.Values.Where((def) =>
		//                    def.MayGet(trigger.Args.Role) && def.Name.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0));
		//            }
		//            else
		//            {
		//                foreach (var def in WCellVariables.Instance.ByName.Values)
		//                {
		//                    if (def.MayGet(trigger.Args.Role))
		//                    {
		//                        vars.Add(def);
		//                    }
		//                }
		//            }

		//            if (vars.Count > 0)
		//            {
		//                vars.Sort();

		//                trigger.Reply("Found {0} globals:", vars.Count);
		//                foreach (var var in vars)
		//                {
		//                    trigger.Reply(var.Name + " (" + var.FullName + ")");
		//                }
		//            }
		//            else
		//            {
		//                trigger.Reply("Found no globals that contain \"{0}\". - Do you have sufficient rights?", filter);
		//            }
		//        }
		//    }
		//}
		#endregion
	}
}
