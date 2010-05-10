using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Intercommunication.DataTypes;
using WCell.Util;
using WCell.Util.Commands;
using WCell.Core.Initialization;
using WCell.Constants;
using WCell.AuthServer.Accounts;
using WCell.Util.Variables;
using System.IO;

namespace WCell.AuthServer.Commands
{
	public class AuthCommandHandler : CommandMgr<AuthServerCmdArgs>
	{
		public static AuthCommandHandler Instance;

		/// <summary>
		/// The file containing a set of commands to be executed upon startup
		/// </summary>
		[Variable("AutoExecStartupFile")]
		public static string AutoExecStartupFile = "_Startup.txt";

		/// <summary>
		/// A directory containing a list of autoexec files, containing autoexec files similar to this:
		/// accountname.txt
		/// Every file will be executed when the account with the given name logs in.
		/// </summary>
		[Variable("AutoExecDir")]
		public static string AutoExecDir = "../AuthServerAutoExec/";

		public override string ExecFileDir
		{
			get { return Path.GetDirectoryName(AuthenticationServer.EntryLocation); }
		}

		/// <summary>
		/// Clears the CommandsByAlias, invokes an instance of every Class that is inherited from Command and adds it
		/// to the CommandsByAlias and the List.
		/// Is automatically called when an instance of IrcClient is created in order to find all Commands.
		/// </summary>
		[Initialization(InitializationPass.Fourth, "Initialize Commands")]
		public static void Initialize()
		{
			Instance = new AuthCommandHandler {
				TriggerValidator =
					delegate(CmdTrigger<AuthServerCmdArgs> trigger, BaseCommand<AuthServerCmdArgs> cmd, bool silent) {
						var rootCmd = cmd.RootCmd as AuthServerCommand;

						if ((rootCmd != null && rootCmd.RequiresAccount) && trigger.Args.Account == null)
						{
							if (!silent)
							{
								trigger.Reply("Invalid Account.");
							}
							return false;
						}
						return true;
					}
			};

			Instance.AddCmdsOfAsm(typeof(AuthCommandHandler).Assembly);

			Instance.UnknownCommand += (trigger) => {
				trigger.Reply("Unknown Command \"" + trigger.Alias + "\" - Type ? for help.");
			};
		}

		/// <summary>
		/// Reacts to the given arguments and replies to the console
		/// </summary>
		/// <param name="text"></param>
		/// <returns>Whether reacton succeeded</returns>
		public static bool Execute(Account acc, string text)
		{
			var args = new AuthServerCmdArgs(acc);
			return Instance.Trigger(new ConsoleCmdTrigger(new StringStream(text), args));
		}

		public override bool Trigger(CmdTrigger<AuthServerCmdArgs> trigger)
		{
			return Execute(trigger, true);
		}

		public bool Execute(CmdTrigger<AuthServerCmdArgs> trigger, bool checkForCall)
		{
			//if (checkForCall && trigger.Text.ConsumeNext(ExecCommandPrefix))
			//{
			//    return Call(trigger);
			//}
			//else
			//{
			//    return base.Execute(trigger);
			//}
			return base.Trigger(trigger);
		}

		/// <summary>
		/// 
		/// </summary>
		public static BufferedCommandResponse ExecuteBufferedCommand(string cmd)
		{
			var trigger = new BufferedCommandTrigger(new StringStream(cmd), new AuthServerCmdArgs(null));
			Instance.Trigger(trigger);
			return trigger.Response;
		}

		/// <summary>
		/// Reacts to console and reads accounts as first parameter
		/// </summary>
		public static bool ReadAccountAndReactTo(string text)
		{
			var trigger = new ConsoleCmdTrigger(new StringStream(text), null);
			return ReadAccountAndReactTo(trigger);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static bool ReadAccountAndReactTo(CmdTrigger<AuthServerCmdArgs> emptyTrigger)
		{
			emptyTrigger.Args = new AuthServerCmdArgs(null);
			var cmd = Instance.GetCommand(emptyTrigger);
			if (cmd != null)
			{
				var acc = AccountMgr.GetAccount(emptyTrigger.Text.NextWord());
				emptyTrigger.Args.Account = acc;
				return Instance.Trigger(emptyTrigger, cmd);
			}
			return false;
		}

		#region Autoexec
		[Initialization(InitializationPass.Tenth)]
		public static void AutoexecStartup()
		{
			var args = new AuthServerCmdArgs(null);
		    var file = AutoExecDir + AutoExecStartupFile;
			if (File.Exists(file))
			{
				Instance.ExecFile(file, args);
			}
		}

		public static void AutoExecute(Account acc)
		{
			if (!string.IsNullOrEmpty(AutoExecDir) && Directory.Exists(AutoExecDir))
			{
			    var file = AutoExecDir + acc.Name + ".txt";
				if (File.Exists(file))
				{
					Instance.ExecFile(file, new AuthServerCmdArgs(acc));
				}
			}
		}
		#endregion
	}
}
