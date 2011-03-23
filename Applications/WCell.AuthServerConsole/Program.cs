/*************************************************************************
 *
 *   file		: Program.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-09-26 23:15:07 +0200 (lø, 26 sep 2009) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1115 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Threading;
using Cell.Core;
using WCell.Util.Logging;
using System.Runtime;
using WCell.AuthServer;
using System.Diagnostics;
using System;

namespace WCell.AuthServerConsole
{
	internal static class Program
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();
		
#if STARTREALM
		private static Process realmProcess;
#endif

		private static void Main(string[] args)
		{
			log.Info("Starting the auth server console.");

#if STARTREALM
			if (AuthenticationServer.Instance.Configuration.AutoStartRealm)
			{
				var startInfo = new ProcessStartInfo("WCell.RealmServerConsole.exe");
				startInfo.WorkingDirectory = "../../RealmServer/Debug/";
				realmProcess = Process.Start(startInfo);
				AuthenticationServer.Shutdown += OnShutdown;
			}
#endif

			if (GCSettings.IsServerGC)
			{
				GCSettings.LatencyMode = GCLatencyMode.Batch;
			}
			else
			{
				GCSettings.LatencyMode = GCLatencyMode.Interactive;
			}

			if (!AuthServer.Program.Start())
			{
				log.Error("Startup was not successful - Press any key to exit...");
				Console.ReadKey();
				return;
			}

#if STARTREALM
			if (!AuthenticationServer.Instance.IsRunning)
			{
				OnShutdown();
			}
#endif
			//Thread.CurrentThread.IsBackground = true;
			AuthServerConsole.Run();
		}

#if STARTREALM
		static void OnShutdown()
		{
			try {
				if (!realmProcess.CloseMainWindow())
				{
					realmProcess.Kill();
				}
			}
			catch {}
		}
#endif
	}
}