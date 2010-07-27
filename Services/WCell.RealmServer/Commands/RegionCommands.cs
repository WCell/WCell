using WCell.Constants;
using WCell.Constants.World;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Global;
using WCell.RealmServer.Lang;
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
			Description = new TranslatableItem(LangKey.CmdRegionDescription);
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
							trigger.Reply(LangKey.CmdRegionSpawnResponse + region.ToString());
						}
					});
				}
				return;
			}

			protected override void Initialize()
			{
				Init("Spawn", "S");
				ParamInfo = new TranslatableItem(LangKey.CmdRegionSpawnParamInfo);
				Description = new TranslatableItem(LangKey.CmdRegionSpawnDescription);
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
						trigger.Reply(LangKey.CmdRegionSpawnResponse1);
						return;
					}
					else
					{
						var regionId = trigger.Text.NextEnum(MapId.End);
						region = World.GetRegion(regionId);
						if (region == null)
						{
							trigger.Reply(LangKey.CmdRegionSpawnError1);
							return;
						}
					}
				}
				else
				{
					if (trigger.Args.Target == null)
					{
						trigger.Reply(LangKey.CmdRegionSpawnError2);
						return;
					}
					region = trigger.Args.Target.Region;
				}

				if (region.IsSpawned)
				{
					trigger.Reply(LangKey.CmdRegionSpawnError3);
				}
				else
				{
					trigger.Reply(LangKey.CmdRegionSpawnResponse2, region.Name);
					if (!GOMgr.Loaded)
					{
						trigger.Reply(LangKey.CmdRegionSpawnError4);
					}

					if (!NPCMgr.Loaded)
					{
						trigger.Reply(LangKey.CmdRegionSpawnError5);
					}

					region.AddMessage(() =>
					{
						region.SpawnRegion();
						trigger.Reply(LangKey.CmdRegionSpawnResponse3 + region.ToString());
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
				ParamInfo = new TranslatableItem(LangKey.CmdRegionClearParamInfo);
				Description = new TranslatableItem(LangKey.CmdRegionClearDescription);
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
						trigger.Reply(LangKey.CmdRegionClearError1);
						return;
					}
				}
				else
				{
					if (trigger.Args.Character == null)
					{
						trigger.Reply(LangKey.CmdRegionClearError2);
						return;
					}
					region = trigger.Args.Character.Region;
				}

				region.AddMessage(() =>
				{
					region.RemoveObjects();
					trigger.Reply(LangKey.CmdRegionClearResponse + region.ToString());
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
				Description = new TranslatableItem(LangKey.CmdRegionUpdateDescription);
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
				trigger.Reply(LangKey.Done);
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
				Description = new TranslatableItem(LangKey.CmdRegionListDescription);
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var regions = World.GetAllRegions();
				if (regions != null)
				{
					trigger.Reply(LangKey.CmdRegionListResponse);
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