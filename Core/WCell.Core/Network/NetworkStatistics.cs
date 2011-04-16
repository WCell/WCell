using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core.Timers;
using WCell.Util.Collections;

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
			m_consumerTimer = new TimerEntry(ConsumerCallback);
			m_queuedStats = new SynchronizedQueue<PacketInfo>();
		}

		public virtual void UpdatePacketInfo(PacketInfo pktInfo)
		{
			m_queuedStats.Enqueue(pktInfo);
		}

		protected abstract void ConsumeStatistics();

		protected virtual void ConsumerCallback(int dt)
		{
			ConsumeStatistics();
		}

		#region IUpdatable Members

		public void Update(int dt)
		{
			m_consumerTimer.Update(dt);
		}

		#endregion
	}
}