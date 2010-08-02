/*************************************************************************
 *
 *   file		: NPCCommands.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-14 20:38:52 +0100 (to, 14 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1195 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;
using WCell.Util.Commands;
using WCell.Util;
using WCell.RealmServer.AI;
using WCell.RealmServer.NPCs.Vehicles;
using WCell.RealmServer.Global;

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
							newNpc = entry.Create();
							name = newNpc.Name;
							if (dest is IWorldZoneLocation)
							{
								newNpc.Zone = dest.Region.GetZone(((IWorldZoneLocation)dest).ZoneId);
							}
							if (mod.Contains("i"))
							{
								newNpc.Brain.DefaultState = BrainState.Idle;
							}
							newNpc.Brain.EnterDefaultState();

							//trigger.Args.Target.PlaceInFront(newNpc);
							newNpc.TeleportTo(dest);
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
		public class SpawnNPCCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("AddSpawn", "Spawn");
				EnglishParamInfo = "[-c]|[<spawnid> [<amount>]]";
				EnglishDescription = "Creates the NPC-spawnpoint with the given id. -c switch simply creates the spawnpoint that is closest to you";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var mod = trigger.Text.NextModifiers();

				if (mod == "c")
				{
					if (trigger.Args.Target == null)
					{
						trigger.Reply("Cannot use the -c switch without active Target.");
						return;
					}
					var spawnPoint = NPCMgr.SpawnClosestSpawnEntry(trigger.Args.Target);
					if (spawnPoint != null)
					{
						trigger.Args.Target.TeleportTo(spawnPoint);
					}
					else
					{
						trigger.Reply("No Spawnpoint found.");
					}
				}
				else
				{
					var deputyWilemId = 168224u;
					var spawnId = trigger.Text.NextUInt(deputyWilemId);
					var amount = trigger.Text.NextUInt(1);

					if (amount < 1)
					{
						trigger.Reply("Invalid amount: " + amount);
					}
					else
					{
						var spawnEntry = NPCMgr.GetSpawnEntry(spawnId);
						if (spawnEntry == null)
						{
							trigger.Reply("Invalid SpawnId: " + spawnId);
						}
						else
						{
							trigger.Args.Target.Region.AddSpawn(spawnEntry);

							if (trigger.Args.Target != null)
							{
								trigger.Args.Target.TeleportTo(spawnEntry);
							}

							trigger.Reply("Created {0}.", spawnEntry);
						}
					}
				}
			}
		}
		#endregion

		#region Goto
		public class NPCGotoCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Goto");
				EnglishParamInfo = "(-s spawnindex)|(<id>[ <spawn index>])";
				EnglishDescription = "Teleports the target to the first (or given spawn-index of) NPC of the given type.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var target = trigger.Args.Target;
				if (target == null)
				{
					trigger.Reply("No active Target.");
					return;
				}

				var mod = trigger.Text.NextModifiers();

				if (mod.Contains("s"))
				{
					var spawnId = trigger.Text.NextUInt();
					var spawn = NPCMgr.GetSpawnEntry(spawnId);
					if (spawn == null)
					{
						trigger.Reply("Invalid spawn Id: " + spawnId);
						return;
					}
					if (target.TeleportTo(spawn))
					{
						trigger.Reply("Going to: " + spawn);
					}
					else
					{
						trigger.Reply("Spawn is located in {0} ({1}) and not accessible in this Context.",
									  spawn.RegionId, spawn.Position);
					}
				}
				else
				{
					var entryId = trigger.Text.NextEnum(NPCId.End);
					if (entryId == NPCId.End)
					{
						trigger.Reply("Invalid NPC.");
						return;
					}

					var entry = NPCMgr.GetEntry(entryId);
					if (entry != null)
					{
						var spawns = entry.SpawnEntries;

						if (spawns.Count == 0)
						{
							trigger.Reply("Cannot go to NPC because it does not have any SpawnEntries: {0}", entry);
							return;
						}
						trigger.Reply("Found {0} Spawns: " + spawns.ToString(", "), spawns.Count);

						int spawnIndex;
						if (trigger.Text.HasNext)
						{
							spawnIndex = trigger.Text.NextInt(-1);
							if (spawnIndex < 0 || spawnIndex >= spawns.Count)
							{
								trigger.Reply("Invalid index for spawns (Required: 0 - {0})", spawns.Count - 1);
								return;
							}
						}
						else
						{
							spawnIndex = 0;
						}

						var spawn = spawns[spawnIndex];

						if (target.TeleportTo(spawn))
						{
							if (spawns.Count > 1)
							{
								trigger.Reply("Going to: " + spawn);
							}
						}
						else
						{
							trigger.Reply("Spawn is located in {0} ({1}) and not accessible in this Context.",
										  spawn.RegionId, spawn.Position);
						}
					}
				}
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

				Region rgn;
				if (mod.Contains("d"))
				{
					var destName = trigger.Text.NextWord();
					var dest = WorldLocationMgr.Get(destName);
					if (dest == null)
					{
						MapId mapId;
						if (EnumUtil.TryParse(destName, out mapId))
						{
							rgn = World.GetRegion(mapId);
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
						rgn = dest.Region;
					}
					phase = uint.MaxValue;
				}
				else
				{
					rgn = trigger.Args.Target.Region;
					phase = trigger.Args.Target.Phase;
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
							chr.Target = npc;
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
	}

	#region Respawn
	public class RespawnCommand : RealmServerCommand
	{
		protected override void Initialize()
		{
			Init("Respawn");
			EnglishParamInfo = "[<radius>]";
			EnglishDescription = "Respawns all NPCs in the area. Radius by default = 50";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var radius = trigger.Text.NextFloat(50);
			var target = trigger.Args.Target;
			var region = target.Region;
			var objCount = region.ObjectCount;
			region.RespawnInRadius(target.Position, radius);
			trigger.Reply("Done. Spawned {0} objects.", region.ObjectCount - objCount);
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
	public class SpawnCommand : RealmServerCommand
	{
		protected override void Initialize()
		{
			Init("Spawn");
		}

		public class SpawnShowCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Show", "Display");
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var targetObj = trigger.Args.Target;
				if (!(targetObj is NPC))
				{
					targetObj = targetObj.Target;
				}

				var target = targetObj as NPC;
				if (target == null)
				{
					trigger.Reply("Invalid Target.");
					return;
				}

				var spawnPoint = target.SpawnPoint;
				if (spawnPoint == null)
				{
					trigger.Reply("Target has no SpawnPoint.");
					return;
				}

				spawnPoint.ToggleVisiblity();
			}
		}

		public class SpawnMenuCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Menu");
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var chr = trigger.Args.Character;
				if (chr == null)
				{
					trigger.Reply("Character required.");
					return;
				}

				var targetObj = trigger.Args.Target;
				if (!(targetObj is NPC))
				{
					targetObj = targetObj.Target;
				}

				var target = targetObj as NPC;
				if (target == null)
				{
					trigger.Reply("Invalid Target.");
					return;
				}

				var spawnPoint = target.SpawnPoint;
				if (spawnPoint == null)
				{
					trigger.Reply("Target has no SpawnPoint.");
					return;
				}

				chr.StartGossip(spawnPoint.GossipMenu);
			}
		}
	}
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