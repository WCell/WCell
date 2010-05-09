using System;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Misc;
using WCell.Util;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{

	#region Ingame
	/// <summary>
	/// Represents a trigger for commands through ingame chat
	/// </summary>
	public class IngameCmdTrigger : RealmServerCmdTrigger
	{
		public IngameCmdTrigger(StringStream text, IUser user, IGenericChatTarget target, bool dbl)
			: base(text, new RealmServerCmdArgs(user, dbl, target))
		{
		}

		public IngameCmdTrigger(RealmServerCmdArgs args)
			: base(args)
		{
		}

		public override void Reply(string txt)
		{
			Args.Character.SendSystemMessage(txt);
		}

		public override void ReplyFormat(string txt)
		{
			Args.Character.SendSystemMessage(txt);
		}
	}
	#endregion

	#region Console
	/// <summary>
	/// Default trigger for console
	/// </summary>
	public class DefaultCmdTrigger : RealmServerCmdTrigger
	{
		public DefaultCmdTrigger()
		{
		}

		public DefaultCmdTrigger(string text,BaseCommand<RealmServerCmdArgs> selectedCommand, RealmServerCmdArgs args)
			: base(new StringStream(text), selectedCommand, args)
		{
		}

		public DefaultCmdTrigger(string text)
			: base(new StringStream(text), null, new RealmServerCmdArgs(null, false, null))
		{
		}

		public DefaultCmdTrigger(string text, BaseCommand<RealmServerCmdArgs> selectedCommand)
			: base(new StringStream(text), selectedCommand, new RealmServerCmdArgs(null, false, null))
		{
		}

		public DefaultCmdTrigger(StringStream args)
			: base(args, null, new RealmServerCmdArgs(null, false, null))
		{
		}

		public DefaultCmdTrigger(StringStream args, BaseCommand<RealmServerCmdArgs> selectedCommand)
			: base(args, selectedCommand, new RealmServerCmdArgs(null, false, null))
		{
		}

		public override void Reply(string txt)
		{
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine(txt);
			Console.ResetColor();
		}

		public override void ReplyFormat(string txt)
		{
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine(ChatUtility.Strip(txt));
			Console.ResetColor();
		}
	}
	#endregion

	#region Buffered
	public class BufferedCommandTrigger : DefaultCmdTrigger
	{
		public readonly BufferedCommandResponse Response = new BufferedCommandResponse();

		public BufferedCommandTrigger()
		{
		}

		public BufferedCommandTrigger(string text) : base(text)
		{
		}

		public BufferedCommandTrigger(string text, BaseCommand<RealmServerCmdArgs> selectedCommand, RealmServerCmdArgs args) : base(text, selectedCommand, args)
		{
		}

		public BufferedCommandTrigger(StringStream args, BaseCommand<RealmServerCmdArgs> selectedCommand) : base(args, selectedCommand)
		{
		}

		public BufferedCommandTrigger(StringStream args) : base(args)
		{
		}

		public BufferedCommandTrigger(string text, BaseCommand<RealmServerCmdArgs> selectedCommand) : base(text, selectedCommand)
		{
		}

		public override void Reply(string text)
		{
			Response.Replies.Add(text);
		}

		public override void ReplyFormat(string text)
		{
			Response.Replies.Add(text);
		}
	}
	#endregion

	public abstract class RealmServerCmdTrigger : CmdTrigger<RealmServerCmdArgs>
	{
		protected RealmServerCmdTrigger()
		{
		}

		protected RealmServerCmdTrigger(StringStream text, RealmServerCmdArgs args) :
			base(text, args)
		{
		}

		protected RealmServerCmdTrigger(StringStream text, BaseCommand<RealmServerCmdArgs> selectedCmd, RealmServerCmdArgs args) :
			base(text, selectedCmd, args)
		{
		}

		protected RealmServerCmdTrigger(RealmServerCmdArgs args) :
			base(args)
		{
		}
	}
}