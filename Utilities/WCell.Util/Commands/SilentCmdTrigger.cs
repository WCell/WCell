using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.Commands
{
	public class SilentCmdTrigger<C> : CmdTrigger<C>
		where C : ICmdArgs
	{
		public SilentCmdTrigger(StringStream text, C args)
		{
			m_text = text;
			Args = args;
		}

		public override void Reply(string text)
		{
		}

		public override void ReplyFormat(string text)
		{
		}
	}
}
