/*************************************************************************
 *
 *   file		: CommandConsole.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-04-05 08:29:47 +0800 (Sun, 05 Apr 2009) $
 
 *   revision		: $Rev: 864 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using WCell.RealmServer.Commands;
using WCell.Util.Logging;
using System.Threading;
using WCell.Util.Commands;
using WCell.Util.Strings;

using RealmServ = WCell.RealmServer.RealmServer;

namespace WCell.RealmServerConsole
{
	/// <summary>
	/// Delegate for consome command methods
	/// </summary>
	/// <param name="arguments">the arguments of the command</param>
	public delegate void CommandDelegate(string[] arguments);

	///<summary>
	/// 
	///</summary>
	public static class RealmServerConsole
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Default command trigger.
		/// </summary>
		public static DefaultCmdTrigger DefaultTrigger;

		internal static void Run()
		{
			//DatabaseUtil.ReleaseConsole(); TODO: Work out why the database util would ever have such a ridiculous console binding..
			var server = RealmServ.Instance;

			if (!server.IsRunning)
			{
				Thread.Sleep(300);
				Console.WriteLine("Press any key to exit...");
				Console.ReadKey();
			}
			else
			{
				Console.WriteLine("Console ready. Type ? for help");

				DefaultTrigger = new DefaultCmdTrigger
				{
					Args = new RealmServerCmdArgs(null, false, null)
				};

				while (RealmServ.Instance.IsRunning)
				{
					string line;
					try
					{
						while (!Console.KeyAvailable && RealmServ.Instance.IsRunning)
						{
							Thread.Sleep(100);
						}
						if (!RealmServ.Instance.IsRunning)
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
					if (line == null || !RealmServ.Instance.IsRunning)
					{
						break;
					}
					try
					{
						/*if (DatabaseUtil.IsWaiting) TODO: More odd database blocking...
						{
							DatabaseUtil.Input.Write(line);
						}
						else
						{*/
						var text = new StringStream(line);
						DefaultTrigger.Text = text;
						if (!DefaultTrigger.InitTrigger())
						{
							continue;
						}

						var isSelect = text.ConsumeNext(RealmCommandHandler.SelectCommandPrefix);
						if (isSelect)
						{
							var cmd = RealmCommandHandler.Instance.SelectCommand(text);
							if (cmd != null)
							{
								Console.WriteLine(@"Selected: {0}", cmd);
								DefaultTrigger.SelectedCommand = cmd;
							}
							else if (DefaultTrigger.SelectedCommand != null)
							{
								Console.WriteLine(@"Cleared Command selection.");
								DefaultTrigger.SelectedCommand = null;
							}
						}
						else
						{
							bool dbl;
							RealmCommandHandler.ConsumeCommandPrefix(text, out dbl);
							DefaultTrigger.Args.Double = dbl;

							RealmCommandHandler.Instance.ExecuteInContext(DefaultTrigger,
																		  OnExecuted,
																		  OnFail);
						}
						//}
					}
					catch (Exception e)
					{
						LogUtil.ErrorException(e, false, "Failed to execute Command.");
					}
				}
			}
		}

		private static void OnExecuted(CmdTrigger<RealmServerCmdArgs> obj)
		{
		}

		private static void OnFail(CmdTrigger<RealmServerCmdArgs> obj)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Type ? for help");
			Console.ResetColor();
		}
	}
}