using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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