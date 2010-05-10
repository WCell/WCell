using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util;
using WCell.Util.Commands;
using WCell.Intercommunication.DataTypes;
using WCell.Util.Strings;

namespace WCell.AuthServer.Commands
{
	public class BufferedCommandTrigger : CmdTrigger<AuthServerCmdArgs>
	{
		public readonly BufferedCommandResponse Response = new BufferedCommandResponse();

		public BufferedCommandTrigger(StringStream text, BaseCommand<AuthServerCmdArgs> selectedCmd, AuthServerCmdArgs args) : base(text, selectedCmd, args)
		{
		}

		public BufferedCommandTrigger(AuthServerCmdArgs args) : base(args)
		{
		}

		public BufferedCommandTrigger(StringStream text, AuthServerCmdArgs args) : base(text, args)
		{
		}

		public BufferedCommandTrigger()
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
}
