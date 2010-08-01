/*************************************************************************
 *
 *   file		: Program.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-06-01 05:20:00 +0800 (Sun, 01 Jun 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 430 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.IO;
using System.Reflection;
using NLog;
using WCell.Constants.World;
using WCell.Core.Initialization;
using WCell.PacketAnalysis;
using WCell.RealmServer;
using WCell.RealmServer.Addons;
using WCell.RealmServer.Commands;
using WCell.RealmServer.Content;
using WCell.RealmServer.Database;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Global;
using WCell.RealmServer.Items;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Quests;
using WCell.Tools.Commands;
using WCell.Tools.PATools;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.Strings;
using WCell.Util.Toolshed;
using RealmServ = WCell.RealmServer.RealmServer;


namespace WCell.Tools
{
#if TESTS
	using WCell.RealmServer.Tests.Entities;
	using WCell.RealmServer.Tests.Misc;
#endif

	// TODO: Merge all Enum-writers together and write them directly to a centralized place, 
	//			have all constants together in the new WCell.Constants project (merge SpellIds project)

	// TODO: Re-Export Enums
	// TODO: Merge the UpdateField - writers together

	public class Tools
	{
		public static readonly ToolMgr Mgr = new ToolMgr();

		private static readonly Logger log = LogManager.GetCurrentClassLogger();
		public static ToolConfig Config;

		/// <summary>
		/// Little trick to get Constants initialized right away
		/// </summary>
        public static PATool PATool
		{
			get { return ToolConfig.Instance.PATool; }
		}

		internal static bool Init(params Assembly[] assemblies)
		{
			return Init(ToolConfig.ToolsRoot, assemblies);
		}

		public static bool Init(string toolsRoot, params Assembly[] assemblies)
		{
			ToolConfig.ToolsRoot = toolsRoot;
			RealmServ.EntryLocation = Path.GetFullPath(ToolConfig.WCellRealmServerConsoleExe);
			var realmServ = RealmServ.Instance; // make sure to create the RealmServ instance first

			ToolConfig.InitCfg();

			LogUtil.SetupConsoleLogging();

			Console.WriteLine("Output Directory: " + new DirectoryInfo(ToolConfig.OutputDir).FullName);
			if (!Directory.Exists(ToolConfig.OutputDir))
			{
				Directory.CreateDirectory(ToolConfig.OutputDir);
			}

			RealmServerConfiguration.Instance.AutoSave = false;
			RealmServerConfiguration.ContentDirName = Path.GetFullPath(ToolConfig.ContentDir);
			RealmServerConfiguration.Initialize();
			RealmAddonMgr.AddonDir = ToolConfig.AddonDir;

			Console.WriteLine("Content Directory: " + new DirectoryInfo(RealmServerConfiguration.ContentDir).FullName);

			if (!InitMgr.Initialize(typeof(Tools).Assembly) ||
				!InitMgr.Initialize(typeof(PacketAnalyzer).Assembly))
			{
				log.Error("Cancelled - Press any key to exit...");
				Console.ReadKey();
				return false;
			}

			foreach (var asm in assemblies)
			{
				if (!InitMgr.Initialize(asm))
				{
					log.Error("Unable to initialize Assembly: \"{0}\" - Press any key to exit...", asm);
					return false;
				}
				ToolCommandHandler.Instance.AddCmdsOfAsm(asm);
				Mgr.AddStaticMethodsOfAsm(asm);
			}

			Mgr.AddStaticMethodsOfAsm(typeof(Tools).Assembly);
			return true;
		}

		/// <summary>
		/// You can pass args to this Program, where every line represents one command to be executed.
		/// </summary>
		public static void Main(string[] args)
		{
			if (!Init())
			{
				return;
			}

			//RealmDBUtil.Initialize();

			//LogConverter.DefaultValidator = (opCode) => opCode.ToString().Contains("UPDATE");
			//ConvertKSnifferLogSingleLine(@"3.0.2/dump-108-9-18-20-12-4.txt");
			//WCellEnumWriter.WriteItemId();

			//var x = WCellVariables.Load(Path.Combine(WCellRoot, "test.xml"));

			ToolCommandHandler.Instance.AddDefaultCallCommand(Mgr);
			if (args.Length == 0)
			{
				StartCommandLine();

				Console.WriteLine("Press ANY key to exit...");
				Console.ReadKey();
			}
			else
			{
				var argStr = args.ToString(" ");		// merge string again and take apart by different seperator
				args = argStr.Split(new [] {'\n', '|'}, StringSplitOptions.RemoveEmptyEntries);

				Console.WriteLine("Found {0} commands - Processing...", args.Length);
				var trigger = new ToolCommandHandler.ConsoleCmdTrigger(new ToolCmdArgs());
				foreach (var arg in args)
				{
					ExecuteCommandLine(trigger, arg.Trim());
				}
			}
		}

		public static void StartCommandLine()
		{
			Console.WriteLine("Tools started:");
			Console.Write(" Enter ");
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write("?");
			Console.ResetColor();
			Console.WriteLine(" for help");

			Console.Write(" Enter ");
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write("q");
			Console.ResetColor();
			Console.WriteLine(" to quit");

			var defaultTrigger = new ToolCommandHandler.ConsoleCmdTrigger(new ToolCmdArgs());
			string line;
			do
			{
				try
				{
					line = Console.ReadLine();
				}
				catch
				{
					// console shutdown
					break;
				}
				if (line != null && line != "q" && line != "quit")
				{
					ExecuteCommandLine(defaultTrigger, line);
				}
				else
				{
					break;
				}
			} while (true);
		}

		public static void ExecuteCommandLine(ToolCommandHandler.ConsoleCmdTrigger trigger, string line)
		{
			var text = new StringStream(line);
			trigger.Text = text;

			var isSelect = text.ConsumeNext(RealmCommandHandler.SelectCommandPrefix);
			if (isSelect)
			{
				var cmd = ToolCommandHandler.Instance.SelectCommand(text);
				if (cmd != null)
				{
					Console.WriteLine("Selected: " + cmd);
					trigger.SelectedCommand = cmd;
					return;
				}
				else if (trigger.SelectedCommand != null)
				{
					Console.WriteLine("Cleared Command selection.");
					trigger.SelectedCommand = null;
					return;
				}
			}
			ToolCommandHandler.Instance.Execute(trigger);
		}

#if TESTS
		static void DebugLoginLogoutTest()
		{
			Setup.RunDir = RunDir;
			Setup.EnsureMinimalSetup();
			Console.WriteLine("Config at: " + new DirectoryInfo(RealmServ.EntryLocation).FullName);
			Setup.EnsureBasicSetup();
			Setup.WriteLine("Setup done... Starting test...");
			new CharacterTest().TestManyCharsLoginLogout();
		}

		static void DebugCombatTest()
		{
			Setup.RunDir = RunDir;
			Setup.EnsureBasicSetup();
			new CombatTest().TestSimpleAttack();
		}
#endif

		#region Helper methods
		/*
		/// <summary>
		/// Converts the log-file in the given file (within the <c>LogFolder</c>)
		/// to a human-readable file within the <c>LogOutputFolder</c>.
		/// </summary>
		/// <param name="filename">The name of the file within the LogFolder to be converter</param>
		/// <param name="converter">The Converter-method, either <c>KSnifferLogConverter.ConvertLog</c> or <c>SniffzitLogConverter.ConvertLog</c></param>
		public static void ConvertLog(string filename, Action<string, string> converter)
		{
			Directory.CreateDirectory(ToolConfig.LogFolder);
			Directory.CreateDirectory(ToolConfig.LogOutputFolder);

			var inputFile = Path.Combine(ToolConfig.LogFolder, filename);
			Console.WriteLine("Converting log-file: " + new FileInfo(inputFile).FullName);

			DebugUtil.Init();

			var outFile = Path.Combine(ToolConfig.LogOutputFolder, filename);
			if (!outFile.EndsWith(".txt"))
			{
				outFile += ".txt";
			}
			converter(inputFile, outFile);
		}

		[Tool]
		public static void ConvertKSnifferLogSingleLine(string filename)
		{
			Directory.CreateDirectory(ToolConfig.LogFolder);
			Directory.CreateDirectory(ToolConfig.LogOutputFolder);

			var inputFile = Path.Combine(ToolConfig.LogFolder, filename);
			Console.Write("Converting log-file: " + new FileInfo(inputFile).FullName + " ...");

			DebugUtil.Init();

			var outFile = Path.Combine(ToolConfig.LogOutputFolder, filename);
			if (!outFile.EndsWith(".txt"))
			{
				outFile += ".txt";
			}

			KSnifferLogConverter.ConvertLog(inputFile, outFile, true);
			Console.WriteLine("Done. - Output has been written to: " + new FileInfo(outFile).FullName);
			//Console.WriteLine();
		}*/

		public static void Startup()
		{
			Utility.Measure("Load all", 1, () =>
			{
				ItemMgr.LoadAll();
				NPCMgr.LoadNPCDefs();
				GOMgr.LoadAll();
				QuestMgr.LoadAll();
			});


			Utility.Measure("Basic startup sequence", 1, () =>
			{
				RealmServ.Instance.Start();

				Utility.Measure("Load all", 1, () =>
				{
					ItemMgr.LoadAll();
					NPCMgr.LoadNPCDefs();
					GOMgr.LoadAll();
					QuestMgr.LoadAll();
				});

				Region.AutoSpawn = true;
				var easternKD = World.GetRegion(MapId.EasternKingdoms);
				var kalimdor = World.GetRegion(MapId.Kalimdor);
				var outlands = World.GetRegion(MapId.Outland);
				Utility.Measure("Spawning Main Maps", 1, () =>
				{
					//easternKD.Start();
					//kalimdor.Start();
				});


				GC.Collect();
				Console.WriteLine("Total memory usage with fully spawned world: {0}", GC.GetTotalMemory(true));
			});
		}

		[Tool]
		public static void StartRealm()
		{
			if (!RealmServ.Instance.IsRunning)
			{
				Utility.Measure("Full startup sequence", 1, RealmServ.Instance.Start);
			}
		}

		public static void FetchAll()
		{
			StartRealm();

			Utility.Measure("Loading of Items and NPCs", 1, ContentHandler.FetchAll);
		}

		[Tool]
		public static void WriteContentStubs()
		{
			Utility.Measure("DBSetup.Initialize()", 1, () => RealmDBUtil.Initialize());
			Utility.Measure("ContentHandler.SaveDefaultStubs()", 1, ContentHandler.SaveDefaultStubs);

			ContentHandler.SaveDefaultStubs();
		}

		#endregion
	}
}