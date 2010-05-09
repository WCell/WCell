using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NLog;

namespace WCell.RealmServerConsole.Commands
{
	public static class DumpThreadPoolInfo
	{
		private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

		[ConsoleCommand("dumptpinfo")]
		public static void Execute(string[] arguments)
		{
			int minThreads, maxThreads, availThreads;
			int minIOCPThreads, maxIOCPThreads, availIOCPThreads;

			ThreadPool.GetMinThreads(out minThreads, out minIOCPThreads);
			ThreadPool.GetMaxThreads(out maxThreads, out maxIOCPThreads);
			ThreadPool.GetAvailableThreads(out availThreads, out availIOCPThreads);

			s_log.Info("[Thread Pool] {0} available worker threads out of {1} maximum ({2} minimum)",
						availThreads.ToString(), maxThreads.ToString(), minThreads.ToString());
			s_log.Info("[Thread Pool] {0} available IOCP threads out of {1} maximum ({2} minimum)",
						availIOCPThreads.ToString(), maxIOCPThreads.ToString(), minIOCPThreads.ToString());
		}
	}
}
