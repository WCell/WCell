using WCell.Constants;
using WCell.Constants.World;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Global;
using WCell.RealmServer.NPCs;
using WCell.Util.Commands;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Battlegrounds;

namespace WCell.RealmServer.Commands
{
	#region Region
	public class RegionCommand : RealmServerCommand
	{

		public override RoleStatus RequiredStatusDefault
		{
			get { return RoleStatus.Admin; }
		}

		protected override void Initialize()
		{
			Init("Region", "Rgn");
			EnglishParamInfo = "";
			EnglishDescription = "Provides Commands to manipulate Regions and their content.";
		}

		#region SpawnRegion
		public class SpawnRegionCommand : SubCommand
		{
			protected SpawnRegionCommand() { }

			/// <summary>
			/// Spawns all active Regions
			/// </summary>
			/// <param name="trigger"></param>
			public static void SpawnAllRegions(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				foreach (var rgn in World.GetAllRegions())
				{
					if (!rgn.IsRunning)
					{
						continue;
					}

					var region = rgn;		// create a copy so we don't spawn the wrong Region
					region.AddMessage(() =>
					{
						if (!region.IsSpawned)
						{
							region.SpawnRegion();
							trigger.Reply("Spawned region " + region);
						}
					});
				}
				return;
			}

			protected override void Initialize()
			{
				Init("Spawn", "S");
				EnglishParamInfo = "[-a|<name>]";
				EnglishDescription = "Spawns GOs and NPCs in the current (or specified) region. " +
					"-a switch spawns all active Regions." +
					"(Only used for development purposes where the regions arent spawned automatically.)";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				Region region;
				if (trigger.Text.HasNext)
				{
					var mod = trigger.Text.NextModifiers();
					if (mod == "a")
					{
						// spawn all
						SpawnAllRegions(trigger);
						trigger.Reply("All regions spawned.");
						return;
					}
					else
					{
						var regionId = trigger.Text.NextEnum(MapId.End);
						region = World.GetRegion(regionId);
						if (region == null)
						{
							trigger.Reply("Invalid Region.");
							return;
						}
					}
				}
				else
				{
					if (trigger.Args.Target == null)
					{
						trigger.Reply("You did not specify the Region to be spawned.");
						return;
					}
					region = trigger.Args.Target.Region;
				}

				if (region.IsSpawned)
				{
					trigger.Reply("Region " + region + " is already spawned.");
				}
				else
				{
					trigger.Reply("Spawning {0}...", region.Name);
					if (!GOMgr.Loaded)
					{
						trigger.Reply("No GOs will be spawned (Use 'Load GOs' before spawning)");
					}

					if (!NPCMgr.Loaded)
					{
						trigger.Reply("No NPCs will be spawned (Use 'Load NPCs' before spawning)");
					}

					region.AddMessage(() =>
					{
						region.SpawnRegion();
						trigger.Reply("Spawned region " + region);
					});
				}
			}
		}
		#endregion

		#region Clear
		public class ClearRegionCommand : SubCommand
		{
			protected ClearRegionCommand() { }

			protected override void Initialize()
			{
				Init("Clear");
				EnglishParamInfo = "[<name>]";
				EnglishDescription = "Removes all objects from the current or given Region.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				Region region;
				if (trigger.Text.HasNext)
				{
					var regionId = trigger.Text.NextEnum(MapId.End);
					region = World.GetRegion(regionId);
					if (region == null)
					{
						trigger.Reply("Invalid Region.");
						return;
					}
				}
				else
				{
					if (trigger.Args.Character == null)
					{
						trigger.Reply("You did not specify a Region to be cleared.");
						return;
					}
					region = trigger.Args.Character.Region;
				}

				region.AddMessage(() =>
				{
					region.RemoveObjects();
					trigger.Reply("Cleared Region " + region);
				});
			}
		}
		#endregion

		#region Toggle Updates
		public class ToggleRegionUpdatesCommand : SubCommand
		{
			protected ToggleRegionUpdatesCommand() { }

			protected override void Initialize()
			{
				Init("Updates", "Upd");
				EnglishParamInfo = "0|1";
				EnglishDescription = "Toggles region updates in all Regions on or off";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var on = trigger.Text.NextBool();

				foreach (var rgn in World.Regions)
				{
					if (rgn != null && on != rgn.IsRunning)
					{
						if (on)
						{
							rgn.Start();
						}
						else
						{
							// not very helpful since, region will be re-activated in no time
							rgn.Stop();
						}
					}
				}
				trigger.Reply("Done.");
			}
		}
		#endregion

		#region List
		public class RegionListCommand : SubCommand
		{
			protected RegionListCommand() { }

			protected override void Initialize()
			{
				Init("List", "L");
				EnglishParamInfo = "";
				EnglishDescription = "Lists active regions";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var regions = World.GetAllRegions();
				if (regions != null)
				{
					trigger.Reply("Active Regions:");
					foreach (var rgn in regions)
					{
						if (rgn.IsRunning)
						{
							trigger.Reply(rgn.ToString());
						}
					}
				}
			}
		}
		#endregion
	}
	#endregion

	#region Spawn Zone
	public class SpawnZoneCommand : RealmServerCommand
	{
		protected SpawnZoneCommand() { }

		public override RoleStatus RequiredStatusDefault
		{
			get
			{
				return RoleStatus.Admin;
			}
		}

		protected override void Initialize()
		{
			Init("SpawnZone");
			EnglishParamInfo = "[<name>]";
			EnglishDescription = "Spawns GOs and NPCs in the current (or specified) Zone. -" +
				" Only used for development purposes where the regions arent spawned automatically.";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			trigger.Reply("Not yet implemented - Use \"Region Spawn\" instead");
		}
	}
	#endregion
}