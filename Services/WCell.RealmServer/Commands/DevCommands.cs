using System;
using System.Collections.Generic;
using Cell.Core;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Looting;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Quests;
using WCell.RealmServer.Spells;
using WCell.Util.Commands;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Debugging;
using WCell.RealmServer.Global;
using WCell.Util.Threading;
using WCell.Constants.World;
using WCell.RealmServer.Misc;
using WCell.Constants.Updates;

namespace WCell.RealmServer.Commands
{
	#region SendRaw
	public class SendRawCommand : RealmServerCommand
	{
		protected SendRawCommand() { }

		protected override void Initialize()
		{
			Init("SendRaw");
			EnglishParamInfo = "<opcode> [<int value1> [<int value2> ...]]";
			EnglishDescription = "Sends a raw packet to the client";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var opcode = trigger.Text.NextEnum(RealmServerOpCode.Unknown);
			if (opcode == RealmServerOpCode.Unknown)
			{
				trigger.Reply("Invalid OpCode.");
				return;
			}

			using (var packet = new RealmPacketOut(opcode))
			{
				while (trigger.Text.HasNext)
				{
					var val = trigger.Text.NextInt();
					packet.Write(val);
				}
				((Character)trigger.Args.Target).Client.Send(packet);
			}
		}

		public override ObjectTypeCustom TargetTypes
		{
			get { return ObjectTypeCustom.Player; }
		}
	}
	#endregion

	#region SendPacket
	public class SendPacketCommand : RealmServerCommand
	{
		protected SendPacketCommand() { }

		protected override void Initialize()
		{
			Init("SendPacket", "SendP");
			EnglishParamInfo = "<packet> <args>";
			EnglishDescription = "Sends the given packet with corresponding args to the client";
		}

		#region SendSpellDamageLog
		public class SendSpellDamageLogCommand : SubCommand
		{
			protected SendSpellDamageLogCommand() { }

			protected override void Initialize()
			{
				Init("SpellLog", "SLog");
				EnglishParamInfo = "[<unkBool> [<flags> [<spell> [<damage> [<overkill> [<schools> [<absorbed> [<resisted> [<blocked>]]]]]]]]]";
				EnglishDescription = "Sends a SpellMissLog packet to everyone in the area where you are the caster and everyone within 10y radius is the targets.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var unkBool = trigger.Text.NextBool();
				var flags = trigger.Text.NextEnum(SpellLogFlags.SpellLogFlag_0x1);
				var spell = trigger.Text.NextEnum(SpellId.ClassSkillFireballRank1);
				var damage = trigger.Text.NextUInt(10);
				var overkill = trigger.Text.NextUInt(0);
				var schools = trigger.Text.NextEnum(DamageSchoolMask.Fire);
				var absorbed = trigger.Text.NextUInt(0);
				var resisted = trigger.Text.NextUInt(0);
				var blocked = trigger.Text.NextUInt(0);

				CombatLogHandler.SendMagicDamage(trigger.Args.Target, trigger.Args.User, spell, damage, overkill, schools, absorbed, resisted, blocked, unkBool, flags);
			}
		}
		#endregion

		#region SendSpellMiss
		public class SendSpellMissCommand : SubCommand
		{
			protected SendSpellMissCommand() { }

			protected override void Initialize()
			{
				Init("SpellMiss", "SM");
				EnglishParamInfo = "<spellId> <0/1> <defaultReason>";
				EnglishDescription = "Sends a SpellMissLog packet to everyone in the area where you are the caster and everyone within 10y radius is the targets.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var spell = trigger.Text.NextEnum(SpellId.None);
				var doIt = trigger.Text.NextBool();
				var reason = trigger.Text.NextEnum(CastMissReason.None);

				var objs = trigger.Args.Target.GetObjectsInRadius(10f, ObjectTypes.All, false, 0);

				var misses = new List<CastMiss>();
				foreach (var obj in objs)
				{
					if (obj != trigger.Args.Character)
					{
						misses.Add(new CastMiss(obj, reason));
					}
				}

				CombatLogHandler.SendSpellMiss(spell, trigger.Args.Character, doIt, misses);
			}
		}
		#endregion

		#region SendBGError
		public class SendBGErrorCommand : SubCommand
		{
			protected SendBGErrorCommand() { }

			protected override void Initialize()
			{
				Init("BGError", "BGErr");
				EnglishParamInfo = "<err> [<bg>]";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var err = trigger.Text.NextEnum(BattlegroundJoinError.None);

				BattlegroundHandler.SendBattlegroundError(trigger.Args.Character, err);
			}
		}
		#endregion

		public override ObjectTypeCustom TargetTypes
		{
			get { return ObjectTypeCustom.Player; }
		}
	}
	#endregion

	#region SendGossipPOI
	public class POICommand : RealmServerCommand
	{
		protected POICommand() { }

		protected override void Initialize()
		{
			Init("POI");
			EnglishParamInfo = "[-[e][f] <extra> <flags>] <x> <y> [<name>]";
			EnglishDescription = "Sends a Point of interest entry to the target (shows up on the minimap while not too close).";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var mod = trigger.Text.NextModifiers();
			int extra;
			GossipPOIFlags flags;
			if (mod.Contains("e"))
			{
				extra = trigger.Text.NextInt(0);
			}
			else
			{
				extra = 0;
			}
			if (mod.Contains("f"))
			{
				flags = trigger.Text.NextEnum(GossipPOIFlags.None);
			}
			else
			{
				flags = GossipPOIFlags.None;
			}
			var x = trigger.Text.NextFloat();
			var y = trigger.Text.NextFloat();
			var name = trigger.Text.Remainder;

			if (name.Length == 0)
			{
				name = trigger.Args.User.Name;
			}

			GossipHandler.SendGossipPOI(trigger.Args.Target as Character, flags, x, y, extra, name);
		}

		public override ObjectTypeCustom TargetTypes
		{
			get { return ObjectTypeCustom.Player; }
		}
	}
	#endregion

	#region ModAuras
	public class ModAurasCommand : RealmServerCommand
	{
		protected ModAurasCommand() { }

		protected override void Initialize()
		{
			base.Init("ModAuras", "MAura");
			EnglishParamInfo = "<n> <subcommand> ...";
			EnglishDescription = "Modifies the nth Aura of the target";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			ProcessNth(trigger);
		}

		public class ModLevelCommand : SubCommand
		{
			protected ModLevelCommand() { }

			protected override void Initialize()
			{
				base.Init("Level", "Lvl", "L");
				EnglishParamInfo = "<AuraLevel>";
				EnglishDescription = "Modifies the Level of the nth Aura.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var n = ((RealmServerNCmdArgs)trigger.Args).N - 1;
				var aura = trigger.Args.Target.Auras.GetAt(n);

				if (aura != null)
				{
					ModPropCommand.ModProp(aura, aura.GetType().GetProperty("Level"), trigger);
				}
				else
				{
					trigger.Reply("There aren't " + n + " Auras.");
				}
			}
		}

		public class ModFlagsCommand : SubCommand
		{
			protected ModFlagsCommand() { }

			protected override void Initialize()
			{
				base.Init("Flags", "Fl", "F");
				EnglishParamInfo = "<AuraFlags>";
				EnglishDescription = "Modifies the Flags of the nth Aura.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var n = ((RealmServerNCmdArgs)trigger.Args).N - 1;
				var aura = trigger.Args.Target.Auras.GetAt(n);

				if (aura != null)
				{
					ModPropCommand.ModProp(aura, aura.GetType().GetProperty("Flags"), trigger);
				}
				else
				{
					trigger.Reply("There aren't " + n + " Auras.");
				}
			}
		}

		public override bool NeedsCharacter
		{
			get
			{
				return false;
			}
		}

		public override ObjectTypeCustom TargetTypes
		{
			get
			{
				return ObjectTypeCustom.Unit;
			}
		}
	}
	#endregion

	#region Load Command
	public class LoadCommand : RealmServerCommand
	{
		protected LoadCommand() { }

		protected override void Initialize()
		{
			Init("Load");
			EnglishDescription = "Loads static data from DB.";
		}

		public override RoleStatus RequiredStatusDefault
		{
			get { return RoleStatus.Admin; }
		}

		public class ItemsCommand : SubCommand
		{
			protected ItemsCommand() { }

			protected override void Initialize()
			{
				Init("Items");
				EnglishDescription = "Loads all ItemTemplates and -Spawns.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				if (ItemMgr.Loaded)
				{
					trigger.Reply("Item definitions have already been loaded.");
				}
				else
				{
					RealmServer.Instance.AddMessage(() =>
					{
						trigger.Reply("Loading Items...");
						ItemMgr.LoadAll();
						trigger.Reply("Done.");
					});
				}
			}
		}


		public class GOsCommand : SubCommand
		{
			protected GOsCommand() { }

			protected override void Initialize()
			{
				base.Init("GOs");
				EnglishDescription = "Loads all GOTemplates and spawns.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				if (GOMgr.Loaded)
				{
					trigger.Reply("GO definitions have already been loaded.");
				}
				else
				{
					RealmServer.Instance.AddMessage(() =>
					{
						trigger.Reply("Loading GOs...");
						GOMgr.LoadAll();

						if (Region.AutoSpawn)
						{
							RegionCommand.SpawnRegionCommand.SpawnAllRegions(trigger);
						}
						trigger.Reply("Done.");
					});
				}
			}
		}


		public class NPCsCommand : SubCommand
		{
			protected NPCsCommand() { }

			protected override void Initialize()
			{
				Init("NPCs");
				EnglishParamInfo = "[esw]";
				EnglishDescription = "Loads all NPC definitions from files and/or DB. e: Load entries; s: Load Spawns; w: Load Waypoints (together with s)";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				if (NPCMgr.Loaded)
				{
					trigger.Reply("NPC definitions have already been loaded.");
				}
				else
				{
					RealmServer.Instance.AddMessage(() =>
					{
						trigger.Reply("Loading NPCs...");
						if (!trigger.Text.HasNext)
						{
							NPCMgr.LoadNPCDefs();
						}
						else
						{
							var word = trigger.Text.NextWord();
							if (word.Contains("e"))
							{
								NPCMgr.LoadEntries();
							}
							if (word.Contains("s"))
							{
								NPCMgr.OnlyLoadSpawns();
							}
							if (word.Contains("w"))
							{
								NPCMgr.LoadWaypoints();
							}
						}

						if (Region.AutoSpawn)
						{
							RegionCommand.SpawnRegionCommand.SpawnAllRegions(trigger);
						}

						trigger.Reply("Done.");
					});
				}
			}
		}


		public class LoadQuestCommand : SubCommand
		{
			protected LoadQuestCommand() { }

			protected override void Initialize()
			{
				base.Init("Quests");
				EnglishDescription = "Loads all Quest definitions from files and/or DB.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				if (QuestMgr.Loaded)
				{
					trigger.Reply("Quest definitions have already been loaded.");
				}
				else
				{
					RealmServer.Instance.AddMessage(() =>
					{
						trigger.Reply("Loading Quests...");
						QuestMgr.LoadAll();
						trigger.Reply("Done.");
					});
				}
			}
		}

		public class LootCommand : SubCommand
		{
			protected LootCommand() { }

			protected override void Initialize()
			{
				base.Init("Loot");
				EnglishDescription = "Loads all Loot from files and/or DB.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				if (LootMgr.Loaded)
				{
					trigger.Reply("Loot has already been loaded.");
				}
				else
				{
					RealmServer.Instance.AddMessage(() =>
					{
						trigger.Reply("Loading Loot...");
						LootMgr.LoadAll();
						trigger.Reply("Done.");
					});
				}
			}
		}

		public class AllCommand : SubCommand
		{
			protected AllCommand() { }

			protected override void Initialize()
			{
				Init("All");
				EnglishParamInfo = "[-w]";
				EnglishDescription = "Loads all static content definitions from DB. "
						+ "The -w switch will ensure that execution (of the current Region) won't continue until Loading finished.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				if (trigger.Text.NextModifiers() == "w")
				{
					LoadAll(trigger);
				}
				else
				{
					RealmServer.Instance.AddMessage(() => LoadAll(trigger));
				}
			}

			void LoadAll(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var start = DateTime.Now;

				try
				{
					if (NPCMgr.Loaded)
					{
						trigger.Reply("NPC definitions have already been loaded.");
					}
					else
					{
						trigger.Reply("Loading NPCs...");
						NPCMgr.LoadNPCDefs();
						trigger.Reply("Done.");
					}
				}
				catch (Exception ex)
				{
					FailNotify(trigger, ex);
				}

				try
				{
					if (GOMgr.Loaded)
					{
						trigger.Reply("GO definitions have already been loaded.");
					}
					else
					{
						trigger.Reply("Loading GOs...");
						GOMgr.LoadAll();
						trigger.Reply("Done.");
					}
				}
				catch (Exception ex)
				{
					FailNotify(trigger, ex);
				}

				try
				{
					if (ItemMgr.Loaded)
					{
						trigger.Reply("Item definitions have already been loaded.");
					}
					else
					{
						trigger.Reply("Loading Items...");
						ItemMgr.LoadAll();
						trigger.Reply("Done.");
					}
				}
				catch (Exception ex)
				{
					FailNotify(trigger, ex);
				}


				try
				{
					if (QuestMgr.Loaded)
					{
						trigger.Reply("Quest definitions have already been loaded.");
					}
					else
					{
						trigger.Reply("Loading Quests...");
						QuestMgr.LoadAll();
						trigger.Reply("Done.");
					}
				}
				catch (Exception ex)
				{
					FailNotify(trigger, ex);
				}

				try
				{
					if (LootMgr.Loaded)
					{
						trigger.Reply("Loot has already been loaded.");
					}
					else
					{
						trigger.Reply("Loading Loot...");
						LootMgr.LoadAll();
						trigger.Reply("Done.");
					}
				}
				catch (Exception ex)
				{
					FailNotify(trigger, ex);
				}

                try
                {
                    if (NPCAiTextMgr.Loaded)
                    {
                        trigger.Reply("NPC AI texts has already been loaded.");
                    }
                    else
                    {
                        trigger.Reply("Loading NPC AI texts...");
                        NPCAiTextMgr.LoadEntries();
                        trigger.Reply("Done.");
                    }
                }
                catch (Exception ex)
                {
                    FailNotify(trigger, ex);
                }

				trigger.Reply("All done - Loading took: " + (DateTime.Now - start));
				GC.Collect(2, GCCollectionMode.Optimized);

				if (Region.AutoSpawn)
				{
					RegionCommand.SpawnRegionCommand.SpawnAllRegions(trigger);
				}
			}
		}
	}
	#endregion

	#region ForgetSelf
	public class ForgetSelfCommand : RealmServerCommand
	{
		protected override void Initialize()
		{
			Init("ForgetSelf");
			EnglishParamInfo = "";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var chr = (Character)trigger.Args.Target;
			var pos = chr.Position;

			MovementHandler.SendNewWorld(chr.Client, chr.RegionId, ref pos, chr.Orientation);
			chr.ClearSelfKnowledge();
		}

		public override ObjectTypeCustom TargetTypes
		{
			get { return ObjectTypeCustom.Player; }
		}
	}
	#endregion

	#region World States
	public class WSCommand : RealmServerCommand
	{
		protected override void Initialize()
		{
			Init("WorldState", "WS");
		}

		public class InitWSCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Init", "I");
				EnglishParamInfo = "<area> [<state> <value>[ <state2> <value2> ...]]";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var chr = (Character)trigger.Args.Target;
				var area = trigger.Text.NextInt(-1);
				if (area < 0)
				{
					trigger.Reply("No area given");
					return;
				}
				var states = new List<WorldState>();
				while (trigger.Text.HasNext)
				{
					var key = trigger.Text.NextEnum(WorldStateId.End);
					if (key == WorldStateId.End)
					{
						trigger.Reply("Found invalid state.");
						return;
					}
					var value = trigger.Text.NextInt();
					//states.Add(new WorldState(key, value));
				}
				MiscHandler.SendInitWorldStates(chr, chr.Region.Id, chr.ZoneId, (uint)area, states.ToArray());
			}
		}

		public override ObjectTypeCustom TargetTypes
		{
			get { return ObjectTypeCustom.Player; }
		}
	}
	#endregion

	#region Debug
	public class DebugCommand : RealmServerCommand
	{
		public override RoleStatus RequiredStatusDefault
		{
			get { return RoleStatus.Admin; }
		}

		protected override void Initialize()
		{
			Init("Debug");
			EnglishDescription = "Provides Debug-capabilities and management of Debug-tools for Devs.";
		}

		#region Reload PA Defs
		public class ReloadPADefsCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("ReloadDefs", "Reload");
				EnglishDescription = "Reloads the Packet-definitions for the Packet Analyzer.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				DebugUtil.LoadDefinitions();
				trigger.Reply("Packet definitions have been reloaded.");
			}
		}
		#endregion

		#region GC
		public class GCCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("GC");
				EnglishDescription = "Don't use this unless you are well aware of the stages and heuristics involved in the GC process!";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				GC.Collect();
				trigger.Reply("Done.");
			}
		}
		#endregion

		#region Info
		public class InfoCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Info");
				EnglishDescription = "Shows all available Debug information.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				BufferPoolCommand.ShowInfo(trigger);
				ObjectPoolCommand.ShowInfo(trigger);
			}
		}
		#endregion

		#region ObjectPool
		public class ObjectPoolCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("ObjectPool", "Obj");
				EnglishDescription = "Overviews the Object Pools";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				ShowInfo(trigger);
			}

			public static void ShowInfo(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				trigger.Reply("There are {0} ObjectPools in use.", ObjectPoolMgr.Pools.Count);
				IObjectPool biggest = null, sparsest = null;
				foreach (var pool in ObjectPoolMgr.Pools)
				{
					if (biggest == null || pool.AvailableCount > biggest.AvailableCount)
					{
						biggest = pool;
					}
					if (sparsest == null || pool.ObtainedCount > sparsest.ObtainedCount)
					{
						sparsest = pool;
					}
				}
				trigger.Reply("Biggest Pool ({0}): {1} - Pool with most Objects checked out ({2}): {3}", biggest.AvailableCount, biggest, sparsest.ObtainedCount, sparsest);
			}
		}
		#endregion

		#region Buffers
		public class BufferPoolCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Buffers", "Buf");
				EnglishDescription = "Overviews the Buffer Managers.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				ShowInfo(trigger);
			}

			public static void ShowInfo(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				trigger.Reply("Total Buffer Memory: {0} - Pools in use:", BufferManager.GlobalAllocatedMemory);

				foreach (var mgr in BufferManager.Managers)
				{
					if (mgr.InUse)
					{
						trigger.Reply("{2}k Buffer: {0}/{1}", mgr.UsedSegmentCount, mgr.TotalSegmentCount, mgr.SegmentSize / 1024f);
					}
				}
			}
		}
		#endregion
	}
	#endregion

	#region IOWait
	public class IOWaitCommand : RealmServerCommand
	{
		public override RoleStatus RequiredStatusDefault
		{
			get { return RoleStatus.Admin; }
		}

		protected override void Initialize()
		{
			Init("IOWait");
			EnglishDescription = "Lets the current Thread wait for the next tick of the IO Queue. " +
				"Don't use on public servers if you don't know what you are doing!";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			RealmServer.Instance.WaitOne();
		}
	}
	#endregion
}