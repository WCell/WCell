/*************************************************************************
 *
 *   file		: Statistics.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-06-08 00:55:09 +0800 (Sun, 08 Jun 2008) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 458 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Core.Network;

namespace WCell.RealmServer.Stats
{
	/// <summary>
	/// Handles network statistics and trend tracking.
	/// </summary>
	public class RealmStatsExtended : NetworkStatistics
	{
		private float m_consumeInterval = 300.0f; // 5 minutes

		public static RealmStatsExtended Instance
		{
			get { return StatisticsSingleton.Instance; }
		}

		protected override void ConsumeStatistics()
		{
			var packetCounts = new Dictionary<uint, List<int>>();

			// every 5 minutes we update packet count/total size and flush to the DB
			foreach (PacketInfo pktInfo in m_queuedStats)
			{
				if (!packetCounts.ContainsKey(pktInfo.PacketID.RawId))
				{
					packetCounts.Add(pktInfo.PacketID.RawId, new List<int>());
				}

				packetCounts[pktInfo.PacketID.RawId].Add(pktInfo.PacketSize);
			}

			var extInfoList = new List<ExtendedPacketInfo>();

			// calculate extended pkt info
			foreach (var packetCount in packetCounts)
			{
				var pktInfo = new ExtendedPacketInfo
				              	{
					OpCode = (RealmServerOpCode)packetCount.Key,
					PacketCount = packetCount.Value.Count,
					TotalPacketSize = packetCount.Value.Sum(),
					AveragePacketSize = (int)packetCount.Value.Average(),
					StartTime = DateTime.Now.Subtract(TimeSpan.FromMinutes(5)),
					EndTime = DateTime.Now
				};

				List<int> packetSizes = packetCount.Value;

				int stdDev = GetStandardDeviation(packetSizes);
				pktInfo.StandardDeviation = stdDev;

				// push to le db.
			}
		}

		private int GetStandardDeviation(IEnumerable<int> values)
		{
			var setAverage = values.Average();
			var diffSqTotal = values.Sum((val) => (val - setAverage) * (val - setAverage));
			var diffSqAvg = (diffSqTotal / (values.Count() - 1));

			return (int)Math.Sqrt(diffSqAvg);
		}

		public void Start()
		{
			m_consumerTimer.Start(m_consumeInterval, m_consumeInterval);
			// global timer register
		}

		public void Start(float interval)
		{
			m_consumeInterval = interval;

			m_consumerTimer.Start(m_consumeInterval, m_consumeInterval);
			// global register timer
		}

		public void Stop()
		{
			m_consumerTimer.Stop();
			// global unregister timer
		}

		#region Singleton

		class StatisticsSingleton
		{
			internal static readonly RealmStatsExtended Instance = new RealmStatsExtended();
		}

		#endregion
	}
}