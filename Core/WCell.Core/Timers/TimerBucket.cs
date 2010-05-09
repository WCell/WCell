using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Core.Timers
{
	public class TimerBucket
	{
		public readonly TimerPriority Priority;
		public readonly float Delay;
		public readonly ICollection<BucketTimer> Timers;

		internal float m_LastUpdate;

		public TimerBucket(TimerPriority priority, float delay)
		{
			Priority = priority;
			Delay = delay;
			Timers = new HashSet<BucketTimer>();
		}

		public float LastUpdate
		{
			get { return m_LastUpdate; }
		}

		public void Tick()
		{
			foreach (var timer in Timers)
			{
				timer.Action();
			}
		}
	}
}
