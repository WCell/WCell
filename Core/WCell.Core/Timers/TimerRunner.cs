namespace WCell.Core.Timers
{
	public class TimerRunner
	{
		private float m_totalSeconds;
		private readonly TimerBucket[] m_buckets = new TimerBucket[(int)TimerPriority.End];

		public TimerRunner()
		{
			AddBucket(TimerPriority.OneSec, 1f);
			AddBucket(TimerPriority.OnePointFiveSec, 1.5f);
			AddBucket(TimerPriority.FiveSec, 5f);
			AddBucket(TimerPriority.TenSec, 10f);
			AddBucket(TimerPriority.ThirtySec, 30f);
			AddBucket(TimerPriority.OneMin, 60f);
			AddBucket(TimerPriority.OneHour, 60 * 60f);
		}

		void AddBucket(TimerPriority prio, float delay)
		{
			m_buckets[(int)prio] = new TimerBucket(prio, delay);
		}

		public void AddOneShot(BucketTimer timer, uint millis)
		{

		}

		public void AddPeriodic(BucketTimer timer, uint millis)
		{

		}

		public void Remove(BucketTimer timer)
		{
			this[timer.priority].Timers.Remove(timer);
		}

		public TimerBucket this[TimerPriority priority]
		{
			get { return m_buckets[(int)priority]; }
		}

		public void Update(float secondsPassed)
		{
			m_totalSeconds += secondsPassed;

			this[TimerPriority.Always].Tick();
			
			for (var i = 1; i < m_buckets.Length; i++)
			{
				var bucket = m_buckets[i];
				while (bucket.m_LastUpdate + bucket.Delay >= m_totalSeconds)
				{
					// make up for lost time
					bucket.m_LastUpdate += bucket.m_LastUpdate + bucket.Delay - m_totalSeconds;
					bucket.Tick();
				}
			}
		}
	}
}