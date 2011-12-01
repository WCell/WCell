/*************************************************************************
 *
 *   file		: NPCCommands.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-14 20:38:52 +0100 (to, 14 jan 2010) $

 *   revision		: $Rev: 1195 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Linq;
using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.RealmServer.AI;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Instances;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Spawns;
using WCell.RealmServer.NPCs.Vehicles;
using WCell.Util;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
	public class NPCCommand : RealmServerCommand
	{
		protected NPCCommand() { }

		protected override void Initialize()
		{
			Init("NPC");
		}

		public override ObjectTypeCustom TargetTypes
		{
			get { return ObjectTypeCustom.None; }
		}

		#region GetNPCSpawnEntry
		public static NPCSpawnEntry GetNPCSpawnEntry(CmdTrigger<RealmServerCmdArgs> trigger, bool closest, out Map map)
		{
			var target = trigger.Args.Target;
			NPCSpawnEntry entry;
			map = null;
			if (closest)
			{
				if (target == null)
				{
					trigger.Reply("Cannot use the -c switch without active Target.");
					return null;
				}

				entry = NPCMgr.GetClosestSpawnEntry(target);
				if (entry == null)
				{
					trigger.Reply("No Spawnpoint found.");
					return null;
				}
			}
			else
			{
				var word = trigger.Text.NextWord();
				NPCId npcId;
				if (!EnumUtil.TryParse(word, out npcId))
				{
					uint spawnId;
					uint.TryParse(word, out spawnId);
					entry = NPCMgr.GetSpawnEntry(spawnId);
					if (entry == null)
					{
						trigger.Reply("Invalid SpawnId: " + spawnId);
						return null;
					}
				}
				else
				{
					var npcEntry = NPCMgr.GetEntry(npcId);
					if (npcEntry == null)
					{
						trigger.Reply("Entry not found: " + npcId);
						return null;
					}
					if (npcEntry.SpawnEntries.Count == 0)
					{
						trigger.Reply("Entry has no SpawnEntries: " + npcEntry);
						return null;
					}

					entry = target != null ? npcEntry.SpawnEntries.GetClosestSpawnEntry(target) : npcEntry.SpawnEntries.First();
				}
			}

			// found entry - now determine Map
			map = entry.Map;
			if (map == null)
			{
				if (target != null && entry.MapId == target.MapId)
				{
					// is in same map
					map = target.Map;
				}
				else
				{
					// must create Map
					if (World.IsInstance(entry.MapId))
					{
						// create instance
						map = InstanceMgr.CreateInstance(target as Character, entry.MapId);
						if (map == null)
						{
							trigger.Reply("Failed to create instance: " + entry.MapId);
							return null;
						}
					}
					else
					{
						trigger.Reply("Cannot spawn NPC for map: " + entry.MapId);
						return null;
					}
				}
			}
			return entry;
		}
		#endregion

		#region Add
		public class AddNPCCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Add", "A", "Create");
				EnglishParamInfo = "[-[i][d <dest>]] <entry> [<amount>]";
				EnglishDescription = "Creates one or more NPCs with the given entry id. NPCs are set to active by default." +
					" Use -i to spawn an idle NPC." +
					" Use -d <dest> to spawn an NPC at a given location.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var mod = trigger.Text.NextModifiers();
				string destName = null;
				if (mod.Contains("d"))
				{
					destName = trigger.Text.NextWord();
				}

				var entryId = trigger.Text.NextEnum(NPCId.End);
				if (entryId == NPCId.End)
				{
					trigger.Reply("Invalid NPC.");
					return;
				}

				var amount = trigger.Text.NextUInt(1);

				if (amount < 1)
				{
					trigger.Reply("Invalid amount: " + amount);
				}
				else
				{
					string name = null;
					var entry = NPCMgr.GetEntry(entryId);
					if (entry == null)
					{
						trigger.Reply("Invalid NPCId: " + entryId);
					}
					else
					{
						NPC newNpc;
						IWorldLocation dest;
						if (!string.IsNullOrEmpty(destName))
						{
							dest = WorldLocationMgr.Get(destName);
							if (dest == null)
							{
								trigger.Reply("Invalid destination: " + destName);
								return;
							}
						}
						else if (trigger.Args.Target == null)
						{
							trigger.Reply("No destination given.");
							return;
						}
						else
						{
							dest = trigger.Args.Target;
						}

						for (var i = 0; i < amount; i++)
						{
							newNpc = entry.SpawnAt(dest);
							name = newNpc.Name;
							if (mod.Contains("i"))
							{
								newNpc.Brain.DefaultState = BrainState.Idle;
							}
							newNpc.Brain.EnterDefaultState();
						}
						// tricky way to make ourselves not welcome:
						//trigger.Args.Owner.Reputations.SetValue(FactionRepListId.BootyBay, -30000);

						trigger.Reply("Created {0}.", name);
					}
				}
			}
		}
		#endregion

		#region Spawn
		public class NPCSpawnCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("AddSpawn", "Spawn");
				EnglishParamInfo = "[-c]|[<NPCId or spawnid> [<amount>]]";
				EnglishDescription = "Creates the NPC-spawnpoint with the given id. -c switch simply creates the spawnpoint that is closest to you";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var mod = trigger.Text.NextModifiers();
				Map map;
				var spawnEntry = GetNPCSpawnEntry(trigger, mod == "c", out map);
				if (spawnEntry == null) return;

				var amount = trigger.Text.NextUInt(1);

				if (amount < 1)
				{
					trigger.Reply("Invalid amount: " + amount);
					return;
				}

				// create & teleport
				map.AddNPCSpawnPool(spawnEntry.PoolTemplate);
				if (trigger.Args.Target != null)
				{
					trigger.Args.Target.TeleportTo(map, spawnEntry.Position);
				}

				trigger.Reply("Created spawn: {0}", spawnEntry);
			}
		}
		#endregion

		#region Goto
		public class NPCGotoCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Goto");
				EnglishParamInfo = "[-c]|[<NPCId or spawnid>";
				EnglishDescription = "Teleports the target to the first (or given spawn-index of) NPC of the given type.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var mod = trigger.Text.NextModifiers();
				Map map;
				var spawnEntry = GetNPCSpawnEntry(trigger, mod == "c", out map);
				if (spawnEntry == null) return;

				// teleport
				if (trigger.Args.Target != null)
				{
					trigger.Args.Target.TeleportTo(map, spawnEntry.Position);
				}

				trigger.Reply("Created spawn: {0}", spawnEntry);
			}
		}
		#endregion

		#region Select
		public class SelectNPCCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Select", "Sel");
				EnglishParamInfo = "[-[n][d] [<name>][<destination>]]";
				EnglishDescription = "Selects the NPC whose name matches the given name and is closest to the given location. " +
					"All arguments are optional. If no arguments are supplied, the first available NPC will be selected. " +
					"If the destination is not given, it will search, starting at the current Target or oneself.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var mod = trigger.Text.NextModifiers();
				var name = "";
				uint phase;

				if (mod.Contains("n"))
				{
					name = trigger.Text.NextWord();
				}

				Map rgn;
				if (mod.Contains("d"))
				{
					var destName = trigger.Text.NextWord();
					var dest = WorldLocationMgr.Get(destName);
					if (dest == null)
					{
						MapId mapId;
						if (EnumUtil.TryParse(destName, out mapId))
						{
							rgn = World.GetNonInstancedMap(mapId);
						}
						else
						{
							rgn = null;
						}

						if (rgn == null)
						{
							trigger.Reply("Invalid Destination: " + destName);
							return;
						}
					}
					else
					{
						rgn = dest.Map;
					}
					phase = uint.MaxValue;
				}
				else
				{
					var target = trigger.Args.Target;
					if (target == null)
					{
						trigger.Reply("Must have target or specify destination (using the -d switch).");
						return;
					}
					rgn = target.Map;
					phase = target.Phase;
				}

				if (rgn == null)
				{
					trigger.Reply("Instance-destinations are currently not supported.");
					return;
				}

				NPC npc = null;

				// add message to iterate and then reply
				rgn.ExecuteInContext(() =>
				{
					foreach (var obj in rgn)
					{
						if (obj is NPC && (name == "" || obj.Name.ContainsIgnoreCase(name)))
						{
							npc = (NPC)obj;
							break;
						}
					}

					if (npc == null)
					{
						trigger.Reply("Could not find a matching NPC.");
					}
					else
					{
						var chr = trigger.Args.Character;
                        if (trigger.Args.HasCharacter)
                        {
                            if (name == "" && chr.Target != null)
                            {
                                if (chr.Target is NPC)
                                    npc = chr.Target as NPC;
                                else
                                    chr.Target = npc;
                            }
                            else
                            {
                                chr.Target = npc;
                            }
                        }
						else
						{
							trigger.Args.Target = npc;
							trigger.Args.Context = npc;
						}
						trigger.Reply("Selected: {0}", npc);
					}
				}
				);
			}
		}
		#endregion

        #region Flags
        public class FlagsNPCCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Flags", "F");
				EnglishParamInfo = "[-[n] [<name>]]";
			    EnglishDescription = "Selects the NPC that is the characters current selection or whose name matches"
			                         + " the given name and is closest to the given location. All arguments are optional. " +
			                         "If no arguments are supplied and there is no current selection, the first available NPC will be selected. ";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var mod = trigger.Text.NextModifiers();
				var name = "";

				if (mod.Contains("n"))
				{
					name = trigger.Text.NextWord();
				}

				Map rgn;
				{
					var target = trigger.Args.Target;
					if (target == null)
					{
						trigger.Reply("No target found.");
						return;
					}
					rgn = target.Map;
				}

				if (rgn == null)
				{
					trigger.Reply("Instances are currently not supported.");
					return;
				}

				NPC npc = null;

				// add message to iterate and then reply
			    rgn.ExecuteInContext(() =>
			                             {
			                                 foreach (var obj in rgn)
			                                 {
			                                     if (obj is NPC && (name == "" || obj.Name.ContainsIgnoreCase(name)))
			                                     {
			                                         npc = (NPC) obj;
			                                         break;
			                                     }
			                                 }
			                             });

                if (npc == null)
                {
                    trigger.Reply("Could not find a matching NPC.");
                }
                else
                {
                    var chr = trigger.Args.Character;
                    if (trigger.Args.HasCharacter)
                    {
                        if (name == "" && chr.Target != null)
                        {
                            if (chr.Target is NPC)
                                npc = chr.Target as NPC;
                            else
                                chr.Target = npc;
                        }
                        else
                        {
                            chr.Target = npc;
                        }
                    }
                    else
                    {
                        trigger.Args.Target = npc;
                        trigger.Args.Context = npc;
                    }
                    trigger.Reply("Selected: {0}", npc);
                    var npcflags = npc.NPCFlags;
                    trigger.Reply("NPCFlags {0}:{1}", npcflags, (int)npcflags);
                    var dynamicFlags = npc.DynamicFlags;
                    trigger.Reply("DynamicFlags {0}:{1}", dynamicFlags, (int)dynamicFlags);
                    var extraFlags = npc.ExtraFlags;
                    trigger.Reply("ExtraFlags {0}:{1}", extraFlags, (int)extraFlags);
                    var stateFlags = npc.StateFlags;
                    trigger.Reply("StateFlags {0}:{1}", stateFlags, (int)stateFlags);
                    var unitFlags = npc.UnitFlags;
                    trigger.Reply("UnitFlags {0}:{1}", unitFlags, (int)unitFlags);
                    var unitFlags2 = npc.UnitFlags2;
                    trigger.Reply("UnitFlags2 {0}:{1}", unitFlags2, (int)unitFlags2);
                    trigger.Reply("Minimum required expansion {0}:{1}", (ClientId)npc.Entry.Expansion, npc.Entry.Expansion);
                }
			}
		}
        #endregion

		#region Selectable
		public class SelectableNPCCommand : RealmServerCommand
		{
			protected override void Initialize()
			{
				Init("Selectable", "NPCSel");
				EnglishDescription = "Makes all NPCs on the current Map selectable";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var rgn = trigger.Args.Character.Map;

				if (rgn == null)
				{
					trigger.Reply("Instances are currently not supported.");
					return;
				}


				// add message to iterate and then reply
				rgn.ExecuteInContext(() =>
				{
					foreach (var obj in rgn)
					{
						if (obj is NPC)
						{
							((NPC)obj).UnitFlags &= ~UnitFlags.NotSelectable;
						}
					}
				});
				trigger.Reply("Done.");
			}

			public override bool RequiresCharacter
			{
				get
				{
					return true;
				}
			}
		}
		#endregion
    }

	#region Respawn
	public class RespawnCommand : RealmServerCommand
	{
		protected override void Initialize()
		{
			Init("Respawn");
			EnglishParamInfo = "[<radius>]";
			EnglishDescription = "Respawns all NPCs in the area. Default Radius = 50";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var radius = trigger.Text.NextFloat(50);
			var target = trigger.Args.Target;
			var map = target.Map;
			var objCount = map.ObjectCount;
			map.RespawnInRadius(target.Position, radius);
			trigger.Reply("Done. Spawned {0} objects.", map.ObjectCount - objCount);
		}

		public override ObjectTypeCustom TargetTypes
		{
			get { return ObjectTypeCustom.All; }
		}
	}
	#endregion

	#region Vehicle
	public class VehicleCommand : RealmServerCommand
	{
		protected override void Initialize()
		{
			Init("Vehicle", "Veh");
			EnglishDescription = "Provides commands to manage Vehicles.";
		}

		public class VehicleClearCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Clear", "C");
				EnglishDescription = "Removes all passengers from a Vehicle.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var target = trigger.Args.GetTarget<Vehicle>();

				if (target == null)
				{
					trigger.Reply("No Vehicle selected.");
				}
				else
				{
					var count = target.PassengerCount;
					target.ClearAllSeats();
					trigger.Reply("Done. - Removed {0} passengers from {1}", count, target);
				}
			}
		}
	}
	#endregion

	#region Spawn
	//public class SpawnCommand : RealmServerCommand
	//{
	//    protected override void Initialize()
	//    {
	//        Init("Spawn");
	//    }

	//    public class SpawnShowCommand : SubCommand
	//    {
	//        protected override void Initialize()
	//        {
	//            Init("Show", "Display");
	//        }

	//        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
	//        {
	//            var targetObj = trigger.Args.Target;
	//            if (targetObj == null)
	//            {
	//                trigger.Reply("Target required.");
	//                return;
	//            }
	//            if (!(targetObj is NPC))
	//            {
	//                targetObj = targetObj.Target;
	//            }

	//            var target = targetObj as NPC;
	//            if (target == null)
	//            {
	//                trigger.Reply("Invalid Target.");
	//                return;
	//            }

	//            var spawnPoint = target.SpawnPoint;
	//            if (spawnPoint == null)
	//            {
	//                trigger.Reply("Target has no SpawnPoint.");
	//                return;
	//            }

	//            spawnPoint.ToggleVisiblity();
	//        }
	//    }

	//    public class SpawnMenuCommand : SubCommand
	//    {
	//        protected override void Initialize()
	//        {
	//            Init("Menu");
	//        }

	//        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
	//        {
	//            var chr = trigger.Args.Character;
	//            if (chr == null)
	//            {
	//                trigger.Reply("Character required.");
	//                return;
	//            }

	//            var targetObj = trigger.Args.Target;
	//            if (!(targetObj is NPC))
	//            {
	//                targetObj = targetObj.Target;
	//            }

	//            var target = targetObj as NPC;
	//            if (target == null)
	//            {
	//                trigger.Reply("Invalid Target.");
	//                return;
	//            }

	//            var spawnPoint = target.SpawnPoint;
	//            if (spawnPoint == null)
	//            {
	//                trigger.Reply("Target has no SpawnPoint.");
	//                return;
	//            }

	//            chr.StartGossip(spawnPoint.GossipMenu);
	//        }
	//    }
	//}
	#endregion

	#region Control
	public class ControlCommand : RealmServerCommand
	{

		protected override void Initialize()
		{
			Init("Control", "Enslave");
			EnglishParamInfo = "[-p]";
			EnglishDescription = "Makes the current Target your minion. -p makes it your current Pet or companion if possible.";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var target = trigger.Args.Target;
			var chr = trigger.Args.Character;
			if (target == chr)
			{
				target = chr.Target;
			}

			if (!(target is NPC))
			{
				trigger.Reply("Invalid target - Need to target an NPC.");
			}
			else
			{
				var npc = (NPC)target;
				if (trigger.Text.NextModifiers() == "p")
				{
					chr.MakePet(npc, 0);
				}
				else
				{
					chr.Enslave(npc, 0);
				}
			}
		}

		public override bool RequiresCharacter
		{
			get { return true; }
		}
	}
	#endregion

	#region MakeWild
	public class MakeWildCommand : RealmServerCommand
	{

		protected override void Initialize()
		{
			Init("MakeWild");
			EnglishParamInfo = "";
			EnglishDescription = "Makes the current Target wild (i.e. removes it from it's owner).";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var target = trigger.Args.Target;
			var chr = trigger.Args.Character;
			if (target == chr)
			{
				target = chr.Target;
			}

			if (!(target is NPC))
			{
				trigger.Reply("Invalid target - Need to target an NPC.");
			}
			else
			{
				var npc = (NPC)target;
				if (trigger.Text.NextModifiers() == "p")
				{
					chr.MakePet(npc, 0);
				}
				else
				{
					chr.Enslave(npc, 0);
				}
			}
		}

		public override bool RequiresCharacter
		{
			get { return true; }
		}
	}
	#endregion

	#region Abandon
	public class AbandonCommand : RealmServerCommand
	{

		protected override void Initialize()
		{
			Init("Abandon");
			EnglishDescription = "Abandons the current Target";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var target = trigger.Args.Target;
			var chr = trigger.Args.Character;
			if (target == chr)
			{
				target = chr.Target;
			}

			if (!(target is NPC))
			{
				trigger.Reply("Invalid target - Need to target an NPC.");
			}
			else
			{
				// Units are their own Masters
				target.Master = target;
			}
		}

		public override bool RequiresCharacter
		{
			get
			{
				return true;
			}
		}
	}
	#endregion
}