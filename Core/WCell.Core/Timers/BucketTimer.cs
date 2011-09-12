using System;

namespace WCell.Core.Timers
{
	public class BucketTimer
	{
		private Action m_Action;
		internal TimerPriority priority;

		public BucketTimer(Action action, TimerPriority prio)
		{
			m_Action = action;
            priority = prio;
		}

		public Action Action
		{
			get { return m_Action; }
			set { m_Action = value; }
		}
	}
}