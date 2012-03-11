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
using System.Diagnostics;
using WCell.Core.Initialization;

namespace WCell.RealmServer.Stats
{
    /// <summary>
    /// Creates and manages custom WCell performance counters.
    /// </summary>
    public static class PerformanceCounters
    {
        private const string CategoryName = "WCell";

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
        [Initialization(InitializationPass.Fifth, "Initialize performance counters")]
        public static void Initialize()
        {
            if (!PerformanceCounterCategory.Exists(CategoryName))
            {
                Console.WriteLine("Installing Performance Counters...");
                //Execute our perf counter installer
                //which will request admin privs
                var startInfo = new ProcessStartInfo("WCell.PerformanceCounterInstaller.exe") { UseShellExecute = true };
                var p = Process.Start(startInfo);
                //Wait for the process to end.
                p.WaitForExit();
                if (p.ExitCode == 0)
                    Console.WriteLine("Done...");
                else
                {
                    throw new Exception(
                    "WCell.PerformanceCounterInstaller.exe has not been run. Please run it and restart the application");
                }
            }

            InitCounters();
        }

        public static void InitCounters()
        {
            if (!PerformanceCounterCategory.Exists(CategoryName))
            {
                throw new Exception(
                    "WCell.PerformanceCounterInstaller.exe has not been run. Please run it and restart the application");
            }

            PacketsSentPerSecond = new PerformanceCounter(CategoryName, "Packets Sent/sec", false);
            PacketsReceivedPerSecond = new PerformanceCounter(CategoryName, "Packets Received/sec", false);
            TotalBytesSent = new PerformanceCounter(CategoryName, "Bytes Sent", false);
            TotalBytesReceived = new PerformanceCounter(CategoryName, "Bytes Received", false);
            NumbersOfClientsInAuthQueue = new PerformanceCounter(CategoryName, "Auth Queue Size", false);
        }
    }
}