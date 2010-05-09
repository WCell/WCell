/*************************************************************************
 *
 *   file		: Program.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-05-31 23:20:00 +0200 (lø, 31 maj 2008) $
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using NLog;

using WCell.Core;
using WCell.RealmServer;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Talents;

using WCell.Tools.Domi;
using WCell.Tools.Domi.Output;

using WCell.Tools.Ralek;
using WCell.RealmServer.World;
using WCell.RealmServer.Privileges;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.NPCs;
using WCell.Core.Timers;
using System.Threading;
using WCell.RealmServer.Items;
using WCell.RealmServer.Commands;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Quests;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.RacesClassesClasses;
using System.Runtime.CompilerServices;
using WCell.RealmServer.Misc;
using WCell.Core.Threading;
using WCell.Tools.Ralek.UpdateFields;
using WCell.RealmServer.Database;
using WCell.Core.Initialization;
using WCell.Tools.Domi.DB;
using Microsoft.Xna.Framework;
using System.Reflection;
using WCell.PacketAnalysis;
using WCell.PacketAnalysis.Xml;
using WCell.PacketAnalysis.Updates;


namespace WCell.Tools
{
	using PacketAnalyzer = WCell.PacketAnalysis.PacketAnalyzer;
	using WCell.Core.Network;
	using WCell.RealmServer.Debug;

	public class Program
	{
		public static string OutputDir = "../../output/";

		const string ContentDir = "../../../../Run/RealmServer/Content";
		const string AddonDir = "../../../../Run/RealmServer/Lib";
		static Program()
		{
			LogUtil.SetupConsoleLogging();

			Console.WriteLine("Output Directory: " + new DirectoryInfo(Program.OutputDir).FullName);
			if (!Directory.Exists(Program.OutputDir))
			{
				Directory.CreateDirectory(Program.OutputDir);
			}

			RealmServer.RealmServer.Instance.Configuration.ContentDir = ContentDir;
			RealmServer.RealmServer.Instance.Configuration.AddonDir = AddonDir;
			Console.WriteLine("Content Directory: " + new DirectoryInfo(RealmServer.RealmServer.Instance.Configuration.ContentDir).FullName);

			InitMgr.Initialize(typeof(Program).Assembly);

			ItemMgr.LoadOnStartup = false;
		}

		private static Logger log = LogManager.GetCurrentClassLogger();


		static void Main(string[] args)
		{
			DebugHelper.Init();
			LogConverter.ConvertLog(@"F:\coding\C#\WCell\Dumps\GroupDump.txt", "f:/dump.txt");
			//DoPacketAnalyzr();
			//SpellHandler.Initialize();
			//FactionMgr.Initialize();
			//SkillHandler.Initialize();
			//TalentHandler.Initialize();
			//ItemMgr.LoadAll();

			//SpellOutput.WriteAll();

			//SpellOutput.WriteAddModifierFlatSpells();
			//SpellOutput.WriteAddModifierPercentSpells();
			//SpellOutput.DisplayAll((spell) => spell.IsDeprecated);

			// DumpUpdatePackets(@"F:\coding\C#\WCell\ida\samples\uncompressed_lines.txt", "UpdatePackets.txt", false);

			// DumpUpdatePackets(@"F:\coding\C#\WCell\ida\samples\uncompressed_lines.txt", "UpdatePackets.txt", false);
			//var parser = new UpdateFieldParser(@"F:\coding\C#\WCell\ida\samples\character_blocks.txt", false);
			//var parser = new UpdateFieldParser(@"F:\coding\C#\WCell\ida\samples\creature_blocks.txt", false);
			//var parser = new UpdateFieldParser(@"F:\coding\C#\WCell\ida\samples\gameobject_blocks.txt", false);
			//SpellHandler.Initialize();

			//LockEntry.Initialize();
			//SkillHandler.Initialize();
			//FactionMgr.Initialize();
			//TalentHandler.Initialize();

			//// RaceClassMgr.Initialize();

			////GOMgr.LoadAll();
			////NPCMgr.LoadAll();

			//ItemMgr.LoadAll();
			//QuestMgr.LoadAll();
			////SpellOutput.WriteAll();
			//// SpellOutput.WriteSkillSpells();
			//ItemOutput.WriteAll();

			//WorldMgr.Start();
			//var kalimdor = WorldMgr.GetRegion(MapId.Kalimdor);
			//kalimdor.AddMessage(new Message1<Region>((region) => region.AddDefaultObjects(), kalimdor));

			//SpellOutput.WriteSpellsAndEffects();
			//ClassesRaces.WriteClassesRaces(Program.OutputDir, "Races.xml", "Classes.xml", "Archetypes.xml");
			//Ralek.Program.RalekMain();

			//CommandHandler.Initialize();
			//CommandHandler.Mgr.ReactTo(new DefaultCmdTrigger("?"));

			//SkillHandler.Init();

			//ItemMgr.Templates.Where((templ) => {
			//    if (templ.ItemClass != ItemClass.Weapon && templ.Damages.TotalMin() > 0f)
			//    {
			//        Console.WriteLine(templ);
			//    }
			//    return false;
			//});

			//var templs = ItemMgr.Templates.Where((temp) => temp != null && temp.SetId == 700);
			//Console.WriteLine(templs.ToString(", "));

			//var bag = ItemMgr.GetTemplate(4245);
			//Console.WriteLine(bag);

			//Utility.Measure("spawning", 1, () => {
			//    var region = WorldMgr.GetRegion(MapId.EasternKingdoms);
			//    region.Start();
			//    region.AddDefaultObjects(false);
			//});

			//var success = PrivilegeMgr.Instance.CallMethod(null, null, "World.WorldMgr.Objects.Clear", new string[0], out result);

			// DBCEnumBuilder.WriteRideEnum();
			//DBCEnumBuilder.WriteSpellFocusEnum();
			//DBCEnumBuilder.WriteSpellEnums();
			//DBCEnumBuilder.WriteSpellMechanicEnum();
			//DBCEnumBuilder.WriteZoneEnum();
			//SkillOutput.Go();


			//SpellOutput.WriteSpellFocusSpells();
			//SpellOutput.WritePeriodicAreaAuras();
			//SpellOutput.WriteSpellsAndEffects();
			//SpellOutput.WriteAddModifierFlatSpells();
			//SpellOutput.WriteAddModifierPercentSpells();
			//SpellOutput.WriteModRatingSpells();
			//SpellOutput.WriteInvisSpells();
			//SpellOutput.WriteDynamicObjects();
			//SpellOutput.WriteFinishingMoves();
			//SpellOutput.WriteDoubleTargetSpells();
			//SpellOutput.WriteEffectBySpells();
			//SpellOutput.WriteSpellsByEffect();
			//SpellOutput.WritePassiveEffects();
			//SpellOutput.WriteChanneledSpells();
			//SpellOutput.WriteRideSpells();
			//TalentOutput.Go();

			Console.WriteLine("Press ANY key to continue...");
			Console.ReadKey();
		}

		public static void Startup()
		{
			Utility.Measure("Basic startup sequence", 1, () => {
				RealmServer.RealmServer.Instance.Start();
				NPCMgr.ForceInitialize();

				Region.AutoSpawn = true;
				
				var easternKD = WorldMgr.GetRegion(MapId.EasternKingdoms);
				var kalimdor = WorldMgr.GetRegion(MapId.Kalimdor);
				var outlands = WorldMgr.GetRegion(MapId.Outland);
				Utility.Measure("Spawning Main Maps", 1, () => {
					//easternKD.Start();
					//kalimdor.Start();
				});
				

				GC.Collect();
				Console.WriteLine("Total memory usage with fully spawned world: {0}", GC.GetTotalMemory(true));
			});
		}

		static void DumpUpdatePackets(string inputFile, string outputFile, bool isSingleBlocks)
		{
			FieldRenderUtil.IsOldEntity = false;
			FieldRenderUtil.Init();

			ParsedUpdatePacket.DumpToFile(inputFile, false, isSingleBlocks,
				Path.Combine(OutputDir, outputFile));
		}

		static void DoPacketAnalyzr()
		{
			DebugHelper.Init();
			var packet = new RealmPacketOut(RealmServerOpCode.CMSG_MESSAGECHAT, 40);
			packet.Write((uint)ChatMsgType.Guild);
			packet.Write((uint)ChatLanguage.DemonTongue);
			packet.WriteCString("huhu Guild!");

			DebugHelper.DumpPacket(packet.GetFinalizedPacket());
		}

	}
}