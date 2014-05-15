/*************************************************************************
 *
 *   file		: PerformanceCounters.cs
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
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using WCell.Core.Initialization;

namespace WCell.RealmServer.Stats
{
    /// <summary>
    /// Creates and manages custom WCell performance counters.
    /// </summary>
    public static class PerformanceCounters
    {
        private const string CATEGORY_NAME = "WCellCounterCategory";
        private const string CATEGORY_HELP = "Performance counters for the WCell server platform.";

        /// <summary>
        /// Performance counter for the number of packets sent per second.
        /// </summary>
        public static PerformanceCounter PacketsSentPerSecond
        {
            get;
            internal set;
        }

        /// <summary>
        /// Performance counter for the number of packets received per second.
        /// </summary>
        public static PerformanceCounter PacketsReceivedPerSecond
        {
            get;
			internal set;
        }

        /// <summary>
        /// Performance counter for the total number of bytes sent.
        /// </summary>
        public static PerformanceCounter TotalBytesSent
        {
            get;
			internal set;
        }

        /// <summary>
        /// Performance counter for the total numbers of bytes received.
        /// </summary>
        public static PerformanceCounter TotalBytesReceived
        {
            get;
			internal set;
        }

        /// <summary>
        /// Performance counter for the number of clients in the auth queue.
        /// </summary>
        public static PerformanceCounter NumbersOfClientsInAuthQueue
        {
            get;
			internal set;
        }

        /// <summary>
        /// Initializes the performance counters if they haven't already been created.
        /// </summary>
        [Initialization(InitializationPass.Fifth, "Initialize performance counters",IsRequired = false)]
        public static void Initialize()
        {
            CounterCreationDataCollection counterList = new CounterCreationDataCollection();

            CounterCreationData sentPacketCounter = new CounterCreationData
                                                    	{
                CounterName = "Packets Sent/sec",
                CounterType = PerformanceCounterType.RateOfCountsPerSecond64,
                CounterHelp = "Number of packets sent per second."
            };

            CounterCreationData recvPacketCounter = new CounterCreationData
                                                    	{
                CounterName = "Packets Received/sec",
                CounterType = PerformanceCounterType.RateOfCountsPerSecond64,
                CounterHelp = "Number of packets received per second."
            };

            CounterCreationData bytesSentCounter = new CounterCreationData
                                                   	{
                CounterName = "Bytes Sent",
                CounterType = PerformanceCounterType.NumberOfItems64,
                CounterHelp = "Total number of bytes sent."
            };

            CounterCreationData bytesRecvCounter = new CounterCreationData
                                                   	{
                CounterName = "Bytes Received",
                CounterType = PerformanceCounterType.NumberOfItems64,
                CounterHelp = "Total number of bytes received."
            };

            CounterCreationData authQueueCounter = new CounterCreationData
                                                   	{
                CounterName = "Auth Queue Size",
                CounterType = PerformanceCounterType.CountPerTimeInterval32,
                CounterHelp = "Number of clients waiting in the auth queue."
            };

            counterList.Add(sentPacketCounter);
            counterList.Add(recvPacketCounter);
            counterList.Add(bytesSentCounter);
            counterList.Add(bytesRecvCounter);
            counterList.Add(authQueueCounter);

            if (!PerformanceCounterCategory.Exists(CATEGORY_NAME))
            {
				var identity = WindowsIdentity.GetCurrent();
				if (identity == null) throw new InvalidOperationException("Couldn't get the current user identity");
				var principal = new WindowsPrincipal(identity);
	            if (principal.IsInRole(WindowsBuiltInRole.Administrator))
	            {
		            PerformanceCounterCategory.Create(CATEGORY_NAME, CATEGORY_HELP, PerformanceCounterCategoryType.SingleInstance, counterList);
	            }
	            else
	            {
					throw new WarningException("The user is not an administrator, unable to install performance counters.");
	            }
            }

            PacketsSentPerSecond = new PerformanceCounter(CATEGORY_NAME, "Packets Sent/sec", false);
            PacketsReceivedPerSecond = new PerformanceCounter(CATEGORY_NAME, "Packets Received/sec", false);
            TotalBytesSent = new PerformanceCounter(CATEGORY_NAME, "Bytes Sent", false);
            TotalBytesReceived = new PerformanceCounter(CATEGORY_NAME, "Bytes Received", false);
            NumbersOfClientsInAuthQueue = new PerformanceCounter(CATEGORY_NAME, "Auth Queue Size", false);
        }
    }
}