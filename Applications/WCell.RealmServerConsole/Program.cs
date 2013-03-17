/*************************************************************************
 *
 *   file		: Program.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-09-26 23:15:07 +0200 (lø, 26 sep 2009) $
 
 *   revision		: $Rev: 1115 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Runtime.InteropServices;
using WCell.Util.Logging;
using System.Runtime;
using System.Threading;
using WCell.Core.DBC;
using WCell.RealmServer.Content;

namespace WCell.RealmServerConsole
{
	internal class Program
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		private static void Main(string[] args)
		{
			GCSettings.LatencyMode = GCSettings.IsServerGC ? GCLatencyMode.Batch : GCLatencyMode.Interactive; //TODO: Surely this will make perf slower in server mode which is the opposite of what we want..

			Thread.CurrentThread.IsBackground = true;

			ContentMgr.ForceDataPresence = true;
			RealmServer.Program.Start();
			RealmServerConsole.Run();
		}
	}
}