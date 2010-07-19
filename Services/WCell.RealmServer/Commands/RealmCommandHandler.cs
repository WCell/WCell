using System;
using System.IO;
using System.Linq;
using WCell.Constants;
using WCell.Core.Initialization;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Misc;
using WCell.Util;
using WCell.Util.Commands;
using WCell.Util.Strings;
using WCell.Util.Variables;

namespace WCell.RealmServer.Commands
{
	public class RealmCommandHandler : CommandMgr<RealmServerCmdArgs>
	{
		public static readonly RealmCommandHandler Instance = new RealmCommandHandler();

		/// <summary>
		/// A directory containing a list of autoexec files, containing autoexec files similar to this:
		/// charname.txt
		/// Every file will be executed when the Character with the given name logs in.
		/// </summary>
		[Variable("AutoExecDir")]
		public static string AutoExecDir = "../RealmServerAutoExec/";

		/// <summary>
		/// The file (within the Content dir) containing a set of commands to be executed upon startup
		/// </summary>
		[Variable("AutoExecStartupFile")]
		public static string AutoExecStartupFile = "_Startup.txt";

		/// <summary>
		/// The file (within the Content dir) containing a set of commands to be executed on everyone on login
		/// </summary>
		[Variable("AutoExecAllCharsFile")]
		public static string AutoExecAllCharsFile = "AllChars.txt";

		/// <summary>
		/// The file (within the Content dir) containing a set of commands to be executed on everyone's first login
		/// </summary>
		[Variable("AutoExecAllCharsFirstLoginFile")]
		public static string AutoExecAllCharsFirstLoginFile = "AllCharsFirstLogin.txt";

		public override string ExecFileDir
		{
			get { return Path.GetDirectoryName(RealmServer.EntryLocation); }
		}


		/// <summary>
		/// Sets the default command-prefixes that trigger commands when using chat
		/// </summary>
		public static string CommandPrefixes = ".#![";

		/// <summary>
		/// Used for dynamic method calls
		/// </summary>
		public static char ExecCommandPrefix = '@';

		/// <summary>
		/// Used for selecting commands
		/// </summary>
		public static char SelectCommandPrefix = ':';

		/// <summary>
		/// Clears the CommandsByAlias, invokes an instance of every Class that is inherited from Command and adds it
		/// to the CommandsByAlias and the List.
		/// Is automatically called when an instance of IrcClient is created in order to find all Commands.
		/// </summary>
		[Initialization(InitializationPass.Fourth, "Initialize Commands")]
		public static void Initialize()
		{
			if (Instance.TriggerValidator != null)
			{
				return;
			}

			char prefix;
			if (CommandPrefixes.Contains(prefix = ExecCommandPrefix) || CommandPrefixes.Contains(prefix = SelectCommandPrefix))
			{
				throw new ArgumentException("Invalid Command-prefix may not be used as Command-prefix: " + prefix);
			}

			Instance.TriggerValidator =
				delegate(CmdTrigger<RealmServerCmdArgs> trigger, BaseCommand<RealmServerCmdArgs> cmd, bool silent)
				{
					var rootCmd = cmd.RootCmd;
					if (rootCmd is HelpCommand)
					{
						return true;
					}
					if (!trigger.Args.Role.MayUse(rootCmd))
					{
						if (!silent)
						{
							trigger.Reply(LangKey.MustNotUseCommand, cmd.Name);
						}
						return false;
					}
					else if (rootCmd is RealmServerCommand && !trigger.Args.CheckArgs(rootCmd))
					{
						if (!silent)
						{
							OnInvalidArguments(trigger, (RealmServerCommand)rootCmd);
						}
						return false;
					}
					// check whether we may do anything to the given target
					else if (trigger.Args.Target is Character)
					{
						if (((Character)trigger.Args.Target).Account.Role > trigger.Args.Role)
						{
							if (!silent)
							{
								trigger.Reply("Insufficient privileges.");
							}
							return false;
						}
					}
					return true;
				};

			Instance.AddCmdsOfAsm(typeof(RealmCommandHandler).Assembly);

			Instance.UnknownCommand += (trigger) =>
			{
				trigger.Reply("Unknown Command \"" + trigger.Alias + "\" - Type ? for help.");
			};
		}

		private static void OnInvalidArguments(CmdTrigger<RealmServerCmdArgs> trigger, RealmServerCommand cmd)
		{
			trigger.Reply("Invalid command arguments - " +
								"Required target-type: " + cmd.TargetTypes +
								" - Context required: " + cmd.GetRequiresContext());
		}


		/// <summary>
		/// Whether the given character is a command prefix
		/// </summary>
		public static bool IsCommandPrefix(char c)
		{
			return CommandPrefixes.Contains(c);
		}

		public override bool Execute(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			return Execute(trigger, true);
		}

		public bool Execute(CmdTrigger<RealmServerCmdArgs> trigger, bool checkForCall)
		{
			if (checkForCall && trigger.Text.ConsumeNext(ExecCommandPrefix))
			{
				return Call(trigger);
			}
			else
			{
				return base.Execute(trigger);
			}
		}

		public override BaseCommand<RealmServerCmdArgs> GetCommand(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			if (trigger.Text.ConsumeNext(ExecCommandPrefix))
			{
				return WCell.RealmServer.Commands.CallCommand.Instance;
			}
			return base.GetCommand(trigger);
		}

		/// <summary>
		/// Executes the trigger in Context
		/// </summary>
		public void ExecuteInContext(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			ExecuteInContext(trigger, true, null, null);
		}

		/// <summary>
		/// Executes the trigger in Context
		/// </summary>
		public void ExecuteInContext(CmdTrigger<RealmServerCmdArgs> trigger,
			bool checkForCall,
			Action<CmdTrigger<RealmServerCmdArgs>> doneCallback,
			Action<CmdTrigger<RealmServerCmdArgs>> failCalback)
		{
			BaseCommand<RealmServerCmdArgs> cmd;
			if (checkForCall && trigger.Text.ConsumeNext(ExecCommandPrefix))
			{
				cmd = WCell.RealmServer.Commands.CallCommand.Instance;
			}
			else
			{
				cmd = GetCommand(trigger);
				if (cmd == null)
				{
					return;
				}
			}

			if (cmd.GetRequiresContext())
			{
				if (trigger.Args.Context == null)
				{
					OnInvalidArguments(trigger, (RealmServerCommand)cmd.RootCmd);
					return;
				}
				else
				{
					trigger.Args.Context.ExecuteInContext(() =>
						Execute(trigger, cmd, doneCallback, failCalback));
					return;
				}
			}
			else
			{
				Execute(trigger, cmd, doneCallback, failCalback);
				return;
			}
		}

		void Execute(CmdTrigger<RealmServerCmdArgs> trigger,
			BaseCommand<RealmServerCmdArgs> cmd,
			Action<CmdTrigger<RealmServerCmdArgs>> doneCallback,
			Action<CmdTrigger<RealmServerCmdArgs>> failCalback
			)
		{
			if (Execute(trigger, cmd, false))
			{
				doneCallback(trigger);
			}
			else
			{
				failCalback(trigger);
			}
		}

		/// <summary>
		/// Calls <code>return Execute(new CmdTrigger(text));</code>.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static bool Execute(StringStream text)
		{
			var trigger = new DefaultCmdTrigger(text);
			if (!trigger.InitTrigger())
			{
				return true;
			}
			return Instance.Execute(trigger);
		}

		/// <summary>
		/// Calls <code>return Execute(new CmdTrigger(text));</code>.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static bool Execute(string text)
		{
			var trigger = new DefaultCmdTrigger(text);
			if (!trigger.InitTrigger())
			{
				return true;
			}
			return Instance.Execute(trigger);
		}

		public override bool Execute(CmdTrigger<RealmServerCmdArgs> trigger, BaseCommand<RealmServerCmdArgs> cmd, bool silentFail)
		{
			// verify context
			if (trigger.Args.Context != null &&
				!trigger.Args.Context.IsInContext &&
				cmd.RootCmd.GetRequiresContext())
			{
				// will throw Exception
				trigger.Args.Context.EnsureContext();
			}

			return base.Execute(trigger, cmd, silentFail);
		}

		public override object Eval(CmdTrigger<RealmServerCmdArgs> trigger, BaseCommand<RealmServerCmdArgs> cmd, bool silentFail)
		{
			// verify context
			if (trigger.Args.Context != null &&
				!trigger.Args.Context.IsInContext &&
				cmd.RootCmd.GetRequiresContext())
			{
				// will throw Exception
				trigger.Args.Context.EnsureContext();
			}

			return base.Eval(trigger, cmd, silentFail);
		}

		/// <summary>
		/// Default Command-Handling method
		/// </summary>
		/// <returns>Whether the given msg triggered a command</returns>
		public static bool HandleCommand(IUser user, string msg, IGenericChatTarget target)
		{
			if (msg.Length > 0 && user.Role.Commands.Count > 0)
			{
				bool isCall;
				char prefix;
				if (!(isCall = !IsCommandPrefix(prefix = msg[0])) || (prefix == ExecCommandPrefix))
				{
					if (msg.Length != 2 || msg[1] != '?')	// help command is special!
					{
						var found = false;
						foreach (var c in msg)
						{
							if (c >= 'A')
							{
								// only try to parse command if it contains any actual characters
								// (which can form a Command-alias)
								found = true;
								break;
							}
						}
						if (!found)
						{
							return false;
						}
					}

					var dbl = false;
					var offset = 1;

					if (msg[1] == prefix)
					{
						// double prefix
						if (!user.Role.CanUseCommandsOnOthers)
						{
							user.SendMessage("You are not allowed to use Commands on others.");
							return true;
						}
						else
						{
							if (user.Target == null)
							{
								user.SendMessage("Invalid target.");
								return true;
							}
							else
							{
								dbl = true;
								offset++;
							}
						}
					}

					var trigger = new IngameCmdTrigger(new StringStream(msg.Substring(offset)), user, target, dbl);

					if (trigger.InitTrigger())
					{
						trigger.Args.Context.ExecuteInContext(() =>
						{
							if (!isCall)
							{
								Instance.Execute(trigger, false);
							}
							else
							{
								Call(trigger);
							}
						});
					}
					return true;
				}

				if (prefix == SelectCommandPrefix && user.Role.IsStaff)
				{
					return SelectCommand(user, msg.Substring(1));
				}
			}
			return false;
		}

		#region Command Selection
		/// <summary>
		/// Tries to select the corresponding command for the given User
		/// </summary>
		/// <param name="user"></param>
		/// <param name="cmdString"></param>
		/// <returns>Whether Command was selected</returns>
		public static bool SelectCommand(IUser user, string cmdString)
		{
			// select the given command
			if (cmdString.Length > 1)
			{
				BaseCommand<RealmServerCmdArgs> cmd;
				if (user.SelectedCommand != null)
				{
					cmd = user.SelectedCommand.SelectSubCommand(cmdString);
				}
				else
				{
					cmd = Instance.SelectCommand(cmdString);
				}

				if (cmd != null && user.Role.Commands.Contains(cmd.RootCmd))
				{
					if (cmd.SubCommands.Count == 0)
					{
						user.SendMessage("Invalid Command selection - Command does not have SubCommands: " + cmd);
					}
					else
					{
						user.SelectedCommand = cmd;
						user.SendMessage("Selected: " + cmd.Name);
					}
					return true;
				}
			}
			return false;
		}
		#endregion

		public static bool Call(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			trigger.Alias = "Call";
			return Instance.Trigger(trigger, WCell.RealmServer.Commands.CallCommand.Instance);
		}

		#region Autoexec
		[Initialization(InitializationPass.Tenth)]
		public static void AutoexecStartup()
		{
			var args = new RealmServerCmdArgs(null, false, null);
		    var file = AutoExecDir + AutoExecStartupFile;
			if (File.Exists(file))
			{
				Instance.ExecFile(file, args);
			}
		}

		public override void ExecFile(string filename, CmdTrigger<RealmServerCmdArgs> trigger)
		{
			if (trigger.Args.Character != null)
			{
				if (File.Exists(filename))
				{
					ExecFileFor(filename, trigger.Args.Character, trigger);
				}
				else
				{
					trigger.Reply("File to execute does not exist: " + filename);
				}
			}
			else
			{
				base.ExecFile(filename, trigger);
			}
		}

		public static void ExecFileFor(Character user)
		{
			if (AutoExecDir != null)
			{
				var file = Path.Combine(AutoExecDir, "Chars/" + user.Account.Name + ".txt");
				ExecFileFor(file, user);
			}
		}

		public static void ExecFirstLoginFileFor(Character user)
		{
		    ExecFileFor(AutoExecDir + AutoExecAllCharsFirstLoginFile, user);
		}

		public static void ExecAllCharsFileFor(Character user)
		{
		    ExecFileFor(AutoExecDir + AutoExecAllCharsFile, user);
		}

		public static void ExecFileFor(string file, Character user)
		{
			ExecFileFor(file, user, new IngameCmdTrigger(new RealmServerCmdArgs(user, false, null)));
		}

		public static void ExecFileFor(string file, Character user, CmdTrigger<RealmServerCmdArgs> trigger)
		{
			if (!File.Exists(file))
			{
				return;
			}

			var mayExec = true;
			Func<CmdTrigger<RealmServerCmdArgs>, int, bool> cmdValidator = (trig, line) =>
			{
				var text = trigger.Text;
				if (text.ConsumeNext('+'))
				{
					// special line (TODO: Make it something better)
					var word = text.NextWord();
					if (word == "char" || word == "name")
					{
						var allowedName = text.NextWord();
						mayExec = allowedName.Length > 0 ?
							user.Name.IndexOf(allowedName, StringComparison.InvariantCultureIgnoreCase) > -1 : true;
						return false;

					}
					else if (word == "class")
					{
						var classStr = text.Remainder.Trim();
						var val = 0L;
						object err = null;
						if (!Utility.Eval(typeof(ClassMask), ref val, classStr, ref err, false))
						{
							log.Warn("Invalid Class restriction in file {0} (line {1}): {2}", file, line, err);
						}
						else
						{
							var clss = (ClassMask)val;
							mayExec = clss == ClassMask.None || user.ClassMask.HasAnyFlag(clss);
						}
						return false;
					}
					else
					{
						trig.Reply("Invalid statement in file {0} (line: {1}): " + text.String.Substring(1).Trim(), file, line);
					}
				}

				if (mayExec)
				{
					if (!trig.InitTrigger())
					{
						mayExec = false;
					}
				}
				return mayExec;
			};
			Instance.ExecFile(file, trigger, cmdValidator);
		}
		#endregion
	}

	public static class CommandUtility
	{
		public static bool GetRequiresContext(this BaseCommand<RealmServerCmdArgs> cmd)
		{
			var rootCmd = cmd.RootCmd as RealmServerCommand;
			return rootCmd != null &&
				   rootCmd.RequiresContext;
		}

		/// <summary>
		/// Returns one of the possible values: EvalNext if next argument is in parentheses, else target ?? user
		/// </summary>
		public static object EvalNextOrTargetOrUser(this CmdTrigger<RealmServerCmdArgs> trigger)
		{
			return trigger.EvalNext(trigger.Args.Target ?? (object)trigger.Args.User);
		}

		#region Handle -a and -c prefix
		/// <summary>
		/// Check for selected Target and selected Command
		/// </summary>
		public static bool InitTrigger(this CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var args = trigger.Args;
			if (args.Role.IsStaff)
			{
				var mod = trigger.Text.NextModifiers();
				if (mod.Length > 0)
				{
					var chr = trigger.GetCharacter(mod);
					if (chr != null)
					{
						args.Character = chr;
						args.Double = false;
					}
					else
					{
						return false;
					}
				}
				else if (args.Character != null && trigger.SelectedCommand == null)
				{
					trigger.SelectedCommand = args.Character.ExtraInfo.SelectedCommand;
				}
			}
			return true;
		}

		/// <summary>
		/// Sets the Character of this trigger, according to the -a or -c switch, followed by the account- or character-name
		/// </summary>
		/// <param name="mod"></param>
		/// <returns></returns>
		public static Character GetCharacter(this CmdTrigger<RealmServerCmdArgs> trigger, string mod)
		{
			var args = trigger.Args;
			Character chr = null;
			var role = args.Role;
			var isAcc = mod.Contains("a");
			var isChar = mod.Contains("c");
			if (isAcc || isChar)
			{
				if (isAcc && isChar)
				{
					trigger.Reply("Invalid command-switch, cannot use -a and -c switch at the same time.");
				}
				else
				{
					if (role != null && !role.CanUseCommandsOnOthers)
					{
						trigger.Reply("You may not use the -c or -a command-switch!");
					}

					var name = trigger.Text.NextWord();
					if (isAcc)
					{
						var acc = RealmServer.Instance.GetLoggedInAccount(name);
						if (acc == null || (chr = acc.ActiveCharacter) == null)
						{
							trigger.Reply("Account {0} is not online.", name);
						}
					}
					else
					{
						chr = World.GetCharacter(name, false);
						if (chr == null)
						{
							trigger.Reply("Character {0} is not online.", name);
						}
					}

					if (chr != null)
					{
						if (role != null && chr.Account.Role > role)
						{
							if (isAcc)
							{
								// disguise accounts - don't let anyone guess account-names of higher staff members
								trigger.Reply("Account {0} is not online.", name);
							}
							else
							{
								if (chr.Stealthed == 0)
								{
									trigger.Reply("Cannot use this Command on {0}.", chr.Name);
								}
								else
								{
									trigger.Reply("Character {0} is not online.", name);
								}
							}
						}
						else
						{
							return chr;
						}
					}
				}
			}
			else
			{
				trigger.Reply("Invalid Command-Switch: " + mod);
			}
			return null;
		}
		#endregion
	}
}