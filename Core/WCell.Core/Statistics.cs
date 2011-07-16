/*************************************************************************
 *
 *   file		: StatisticsTimer.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-06-08 00:55:09 +0800 (Sun, 08 Jun 2008) $
 
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
using System.Diagnostics;
using System.Threading;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.Util.Logging;
using WCell.Util;
using WCell.Util.Variables;

namespace WCell.Core
{
	public abstract class Statistics<T> : Statistics where T : Statistics, new()
	{
		protected static T instance;

		public static Statistics<T> Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new T();
				}
				return instance as Statistics<T>;
			}
		}

		/// <summary>
		/// The Statistic-timer update interval in seconds
		/// </summary>
		public int StatsPostInterval
		{
			get
			{
				return s_interval;
			}
			set
			{
				if (value > 0)
				{
					instance.Change(value * 1000);
				}
				else
				{
					instance.Change(Timeout.Infinite);
				}
				s_interval = value;
			}
		}

	}

	public abstract class Statistics
	{
		protected static int s_interval = 30 * 60 * 1000;

		protected static bool inited;

		protected static Logger log = LogManager.GetCurrentClassLogger();
		protected Timer m_statTimer;
		protected long m_lastBytesSent, m_lastBytesReceived;
		protected DateTime m_lastUpdate = DateTime.Now;

		public readonly PerformanceCounter CPUPerfCounter, MemPerfCounter;

		protected Statistics()
		{
			try
			{
				m_lastUpdate = DateTime.Now;

				m_lastBytesSent = 0;
				m_lastBytesReceived = 0;

				var thisProcess = Process.GetCurrentProcess();

				CPUPerfCounter = new PerformanceCounter("Process", "% Processor Time", thisProcess.ProcessName);
				MemPerfCounter = new PerformanceCounter("Process", "Private Bytes", thisProcess.ProcessName);

				m_statTimer = new Timer(OnTick);
			}
			catch (Exception e)
			{
				log.Warn("Could not initialize Performance Counters.", e);
			}
		}

		public void Change(int seconds)
		{
			s_interval = seconds;
			if (seconds > 0)
			{
				seconds *= 1000;
			}
			m_statTimer.Change(seconds, seconds);
		}

		private void OnTick(object state)
		{
			var list = GetFullStats();
			foreach (var line in list)
			{
				log.Info(line);
			}
		}

		public abstract long TotalBytesSent
		{
			get;
		}

		public abstract long TotalBytesReceived
		{
			get;
		}

		public List<string> GetFullStats()
		{
			var list = new List<string>();
			list.Add("----------------- Statistics ------------------");
			GetStats(list);
			list.Add("-----------------------------------------------");
			return list;
		}

		public virtual void GetStats(ICollection<string> statLines)
		{
			//GC.Collect(2, GCCollectionMode.Optimized);
			var thisProcess = Process.GetCurrentProcess();

			var processUptime = DateTime.Now - thisProcess.StartTime;

			var totalBytesSent = TotalBytesSent;
			var totalBytesRcvd = TotalBytesReceived;

			var averageThroughputUp = totalBytesSent / processUptime.TotalSeconds;
			var averageThroughputDown = totalBytesRcvd / processUptime.TotalSeconds;

			double currentUploadSpeed, currentDownloadSpeed;

			var delta = (DateTime.Now - m_lastUpdate).TotalSeconds;
			m_lastUpdate = DateTime.Now;

			currentUploadSpeed = (totalBytesSent - m_lastBytesSent) / delta;
			currentDownloadSpeed = (totalBytesRcvd - m_lastBytesReceived) / delta;
			m_lastBytesSent = totalBytesSent;
			m_lastBytesReceived = totalBytesRcvd;


			var cpuUsage = CPUPerfCounter.NextValue();
			var memUsage = MemPerfCounter.NextValue();

			statLines.Add(string.Format("+ CPU Usage: {0:0.00}% <-> Memory Usage: {1}", cpuUsage, WCellUtil.FormatBytes(memUsage)));
			statLines.Add(string.Format("+ Upload: Total {0} - Avg {1}/s - Current {2}/s",
										WCellUtil.FormatBytes(totalBytesSent), WCellUtil.FormatBytes(averageThroughputUp),
										WCellUtil.FormatBytes(currentUploadSpeed)));
			statLines.Add(string.Format("+ Download: Total: {0} - Avg: {1}/s - Current {2}/s",
										WCellUtil.FormatBytes(totalBytesRcvd), WCellUtil.FormatBytes(averageThroughputDown),
										WCellUtil.FormatBytes(currentDownloadSpeed)));

			var gcCounts = new int[GC.MaxGeneration + 1];
			for (var i = 0; i <= GC.MaxGeneration; i++)
			{
				gcCounts[i] = GC.CollectionCount(i);
			}
			statLines.Add(string.Format("+ Thread Count: {0} - GC Counts: {1}", thisProcess.Threads.Count, gcCounts.ToString(", ")));
		}
	}
}