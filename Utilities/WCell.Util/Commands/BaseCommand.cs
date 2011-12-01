using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NLog;
using WCell.Util.Strings;

namespace WCell.Util.Commands
{
	/// <summary>
	/// An abstract base class, for Command and SubCommand. 
	/// Can hold SubCommands which again can hold further SubCommands.
	/// </summary>
	public abstract class BaseCommand<C>
		where C : ICmdArgs
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// All Aliases which can trigger the Process method of this Command.
		/// </summary>
		public string[] Aliases;

		protected string m_englishParamInfo;
		protected string m_EnglishDescription;
		protected bool m_enabled;

		internal protected BaseCommand<C> m_parentCmd;

		protected IDictionary<string, SubCommand> m_subCommands = new Dictionary<string, SubCommand>(StringComparer.InvariantCultureIgnoreCase);
		protected HashSet<SubCommand> m_subCommandSet = new HashSet<SubCommand>();
		internal protected CommandMgr<C> mgr;

		/// <summary>
		/// The actual Command itself to which this SubCommand (and maybe its ancestors) belongs
		/// </summary>
		public Command<C> RootCmd
		{
			get
			{
				var current = this;
				while (current is SubCommand)
				{
					current = current.m_parentCmd;
				}
				return (Command<C>)current;
			}
		}

		/// <summary>
		/// The parent of this SubCommand (can be a further SubCommand or a Command)
		/// </summary>
		public BaseCommand<C> ParentCmd
		{
			get { return m_parentCmd; }
		}

		public IDictionary<string, SubCommand> SubCommandsByAlias
		{
			get
			{
				return m_subCommands;
			}
		}

		public HashSet<SubCommand> SubCommands
		{
			get
			{
				return m_subCommandSet;
			}
		}

		/// <summary>
		/// Indicates whether or not this command is enabled.
		/// If false, Commands.ReactTo will not trigger this Command'str Process method.
		/// Alternatively you can Add/Remove this Command to/from Commands.CommandsByAlias to control whether or not
		/// certain Commands should or should not be used.
		/// </summary>
		public bool Enabled
		{
			get { return m_enabled; }
			set { m_enabled = value; }
		}

		public virtual string Name
		{
			get
			{
				var name = GetType().Name;
				var index = name.IndexOf("Command");
				if (index >= 0)
				{
					name = name.Substring(0, index);
				}
				return name;
			}
		}

		/// <summary>
		/// A human-readable list of expected parameters
		/// </summary>
		//[Obsolete("Use a localized version of this")]
		public string EnglishParamInfo
		{
			get { return m_englishParamInfo; }
			set { m_englishParamInfo = value; }
		}

		/// <summary>
		/// Describes the command itself.
		/// </summary>
		//[Obsolete("Use a localized version of this")]
		public string EnglishDescription
		{
			get { return m_EnglishDescription; }
			set { m_EnglishDescription = value; }
		}

		internal void DoInit()
		{
			Initialize();
			if (Aliases.Length == 0)
			{
				throw new Exception("Command has no Aliases: " + this);
			}
			foreach (var alias in Aliases)
			{
				if (alias.Contains(" "))
				{
					throw new Exception("Command-Alias \"" + alias + "\" must not contain spaces in " + this);
				}
				if (alias.Length == 0)
				{
					throw new Exception("Command has empty Alias: " + this);
				}
			}
		}

		protected abstract void Initialize();

		protected void Init(params string[] aliases)
		{
			m_enabled = true;
			m_englishParamInfo = "";
			Aliases = aliases;

			AddSubCmds();
		}

		public virtual string GetDescription(CmdTrigger<C> trigger)
		{
			return EnglishDescription;
		}

		public virtual string GetParamInfo(CmdTrigger<C> trigger)
		{
			return m_englishParamInfo;
		}

		public string CreateInfo(CmdTrigger<C> trigger)
		{
			return CreateUsage(trigger) + " (" + GetDescription(trigger) + ")";
			//return CreateUsage();
		}

		/// <summary>
		/// Returns a simple usage string
		/// </summary>
		public string CreateUsage()
		{
			return CreateUsage(GetParamInfo(null));
		}

		public string CreateUsage(CmdTrigger<C> trigger)
		{
			return CreateUsage(GetParamInfo(trigger));
		}

		public virtual string CreateUsage(string paramInfo)
		{
			paramInfo = Aliases.ToString("|") + " " + paramInfo;
			if (m_parentCmd != null)
			{
				paramInfo = m_parentCmd.CreateUsage(paramInfo);
			}
			return paramInfo;
		}

		/// <summary>
		/// Is called when the command is triggered (case-insensitive).
		/// </summary>
		public abstract void Process(CmdTrigger<C> trigger);

		/// <summary>
		/// Processes a command that yields an object to return
		/// </summary>
		/// <param name="trigger"></param>
		/// <returns></returns>
		public virtual object Eval(CmdTrigger<C> trigger)
		{
			return null;
		}

		protected void TriggerSubCommand(CmdTrigger<C> trigger)
		{
			var subAlias = trigger.Text.NextWord();

			SubCommand subCmd;
			if (m_subCommands.TryGetValue(subAlias, out subCmd))
			{
				if (RootCmd.MayTrigger(trigger, subCmd, false))
				{
					subCmd.Process(trigger);
				}
			}
			else
			{
				trigger.Reply("SubCommand not found: " + subAlias);
				trigger.Text.Skip(trigger.Text.Length);
				mgr.DisplayCmd(trigger, this, false, false);
			}
		}


		protected internal void AddSubCmds()
		{
			var type = GetType();
			var nestedClasses = type.GetNestedTypes(BindingFlags.Public);
			if (nestedClasses.Length > 0)
			{
				foreach (var nestedType in nestedClasses)
				{
					if (nestedType.IsSubclassOf(typeof(SubCommand)) && !nestedType.IsAbstract)
					{
						var ctor = nestedType.GetConstructor(
							BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
							null, new Type[0], new ParameterModifier[0]);

						if (ctor == null)
						{
							throw new ArgumentException(nestedType.FullName + " lacks parameterless constructor.");
						}

						var subCmd = ctor.Invoke(null) as SubCommand;
						AddSubCmd(subCmd);
					}
				}
			}
		}

		public void AddSubCmd(SubCommand cmd)
		{
			cmd.m_parentCmd = this;
			cmd.Initialize();

			foreach (var alias in cmd.Aliases)
			{
				m_subCommands[alias] = cmd;
				m_subCommandSet.Add(cmd);
			}
		}

		/// <summary>
		/// Creates a default string of all aliases and all subcommands
		/// </summary>
		protected virtual string CreateGroupUsageString()
		{
			return Aliases.ToString("|") + " " + m_subCommands.Keys.ToString("|");
		}

		/// <summary>
		/// Returns the direct SubCommand with the given alias.
		/// </summary>
		/// <param name="alias"></param>
		/// <returns></returns>
		public SubCommand SelectSubCommand(string alias)
		{
			SubCommand subCmd;
			m_subCommands.TryGetValue(alias, out subCmd);
			return subCmd;
		}


		/// <summary>
		/// Returns the SubCommand, following the given set of aliases through the SubCommand structure.
		/// </summary>
		public SubCommand SelectSubCommand(StringStream cmdString)
		{
			var cmd = SelectSubCommand(cmdString.NextWord());
			if (cmd != null && cmd.SubCommands.Count > 0 && cmdString.HasNext)
			{
				return cmd.SelectSubCommand(cmdString);
			}
			return cmd;
		}

		/// <summary>
		/// Returns the SubCommand, following the given set of aliases through the SubCommand structure.
		/// </summary>
		public void GetSubCommands(StringStream cmdString, List<BaseCommand<C>> list)
		{
			var str = cmdString.NextWord();
			var cmds = m_subCommandSet.Where(comd => comd.Aliases.Where(alias => alias.IndexOf(str, StringComparison.InvariantCultureIgnoreCase) > -1).Count() > 0);

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

		public override string ToString()
		{
			return Name + "-Command";
		}

		internal protected void FailNotify(CmdTrigger<C> trigger, Exception ex)
		{
			log.Warn(ex);
			OnFail(trigger, ex);
		}

		/// <summary>
		/// Is triggered when the processing throws an Exception.
		/// </summary>
		protected virtual void OnFail(CmdTrigger<C> trigger, Exception ex)
		{
			trigger.Reply("Command failed: ");
			foreach (var msg in ex.GetAllMessages())
			{
				trigger.Reply(msg);
			}
		}

		public abstract class SubCommand : BaseCommand<C>
		{
		}
	}
}