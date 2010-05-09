using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core.Timers;
using Cell.Core.Collections;

namespace WCell.Core.Network
{
	/// <summary>
	/// Manages network-related statistics.
	/// </summary>
	public abstract class NetworkStatistics : IUpdatable
	{
		protected TimerEntry m_consumerTimer;
		protected SynchronizedQueue<PacketInfo> m_queuedStats;

		protected NetworkStatistics()
		{
			m_consumerTimer = new TimerEntry(0f, 0f, ConsumerCallback);
			m_queuedStats = new SynchronizedQueue<PacketInfo>();
		}

		public virtual void UpdatePacketInfo(PacketInfo pktInfo)
		{
			m_queuedStats.Enqueue(pktInfo);
		}

		protected abstract void ConsumeStatistics();

		protected virtual void ConsumerCallback(float dt)
		{
			ConsumeStatistics();
		}

		#region IUpdatable Members

		public void Update(float dt)
		{
			m_consumerTimer.Update(dt);
		}

		#endregion
	}
}
