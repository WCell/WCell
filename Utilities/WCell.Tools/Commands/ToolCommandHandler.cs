using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util;
using WCell.Util.Commands;
using WCell.Core.Initialization;
using WCell.Constants;
using WCell.RealmServer.Chat;
using WCell.Util.Strings;

namespace WCell.Tools.Commands
{
	public class ToolCommandHandler : CommandMgr<ToolCmdArgs>
	{
		public static readonly ToolCommandHandler Instance = new ToolCommandHandler();

		/// <summary>
		/// Clears the CommandsByAlias, invokes an instance of every Class that is inherited from Command and adds it
		/// to the CommandsByAlias and the List.
		/// Is automatically called when an instance of IrcClient is created in order to find all Commands.
		/// </summary>
		[Initialization(InitializationPass.Fourth, "Initialize ToolCommands")]
		public static void Initialize()
		{
			Instance.TriggerValidator = (trigger, cmd, silent) => true;

			Instance.AddCmdsOfAsm(typeof (Tools).Assembly);

			Instance.UnknownCommand +=
				(trigger) => { trigger.Reply("Unknown Command \"" + trigger.Alias + "\" - Type ? for help."); };
		}

		public override string ExecFileDir
		{
			get { return ToolConfig.ToolsRoot; }
		}

		/// <summary>
		/// Reacts to the given arguments and replies to the console
		/// </summary>
		/// <param name="text"></param>
		/// <returns>whether reacton succeeded</returns>
		public static bool ReactTo(string text)
		{
			if (text.Length > 0)
			{
				var args = new ToolCmdArgs();
				return Instance.Execute(new ConsoleCmdTrigger(new StringStream(text), args));
			}
			return false;
		}

		//public override bool ReactTo(CmdTrigger<ToolCmdArgs> trigger)
		//{
		//    return;
		//}

		/// <summary>
		/// Reacts to console and reads accounts as first parameter
		/// </summary>
		//public static bool ReadAccountAndReactTo(string text)
		//{
		//    var trigger = new ConsoleCmdTrigger(new StringStream(text), null);
		//    return ReadAccountAndReactTo(trigger);
		//}

		///// <summary>
		///// 
		///// </summary>
		///// <returns></returns>
		//public static bool ReadAccountAndReactTo(CmdTrigger<ToolCmdArgs> emptyTrigger)
		//{
		//    emptyTrigger.Args = new ToolCmdArgs();
		//    var cmd = Mgr.GetCommand(emptyTrigger);
		//    if (cmd != null)
		//    {
		//        var acc = AccountMgr.GetAccount(emptyTrigger.Text.NextWord());
		//        emptyTrigger.Args.Account = acc;
		//        return Mgr.Trigger(emptyTrigger, cmd);
		//    }
		//    return false;
		//}
	}

	public class ToolCmdTrigger : CmdTrigger<ToolCmdArgs>
	{
		public override void Reply(string text)
		{
			Console.WriteLine(text);
		}

		public override void ReplyFormat(string text)
		{
			Console.WriteLine(ChatUtility.Strip(text), true);
		}
	}

	public class ToolCmdArgs : ICmdArgs
	{
	}

	public abstract class ToolCommand : Command<ToolCmdArgs>
	{
		public virtual bool RequiresAccount
		{
			get { return false; }
		}
	}
}