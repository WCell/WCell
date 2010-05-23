/*************************************************************************
 *
 *   file		: CommandConsole.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-09-01 21:50:20 +0800 (Tue, 01 Sep 2009) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1065 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Threading;
using WCell.AuthServer;
using WCell.AuthServer.Commands;
using WCell.Core.Database;

namespace WCell.AuthServerConsole
{
	internal static class AuthServerConsole
	{

		internal static void Run()
		{
			Console.WriteLine("Console ready - Type ? for help");

			DatabaseUtil.ReleaseConsole();

			while (true)
			{
				string line;
				try
				{
					while (!Console.KeyAvailable && AuthenticationServer.Instance.IsRunning)
					{
						Thread.Sleep(100);
					}
					if (!AuthenticationServer.Instance.IsRunning)
					{
						break;
					}
					line = Console.ReadLine();
				}
				catch
				{
					// console shutdown
					break;
				}
				if (line == null || !AuthenticationServer.Instance.IsRunning)
				{
					break;
				}
				if (DatabaseUtil.IsWaiting)
				{
					DatabaseUtil.Input.Write(line);
				}
				else
				{
					lock (Console.Out)
					{
						if (line.StartsWith("."))
						{
							line = line.Substring(1);
							AuthCommandHandler.ReadAccountAndReactTo(line);
						}
						else
						{
							AuthCommandHandler.Execute(null, line);
						}
					}
				}
			}
		}
	}
}