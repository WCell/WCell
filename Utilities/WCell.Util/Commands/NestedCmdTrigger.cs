using WCell.Util.Strings;

namespace WCell.Util.Commands
{
	public class NestedCmdTrigger<C> : CmdTrigger<C>
		where C : ICmdArgs
	{
		private readonly CmdTrigger<C> m_Trigger;

		public NestedCmdTrigger(CmdTrigger<C> trigger, C args)
			: this(trigger, args, trigger.Text)
		{
		}

		public NestedCmdTrigger(CmdTrigger<C> trigger, C args, StringStream text)
		{
			Args = args;
			m_text = text;
			selectedCmd = trigger.selectedCmd;
			m_Trigger = trigger;
		}

		public CmdTrigger<C> Trigger
		{
			get { return m_Trigger; }
		}

		public override void Reply(string text)
		{
			m_Trigger.Reply(text);
		}

		public override void ReplyFormat(string text)
		{
			m_Trigger.ReplyFormat(text);
		}
	}
}