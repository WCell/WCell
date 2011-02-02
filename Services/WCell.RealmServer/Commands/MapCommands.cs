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
	#region Map
	public class MapCommand : RealmServerCommand
	{

		public override RoleStatus RequiredStatusDefault
		{
			get { return RoleStatus.Admin; }
		}

		protected override void Initialize()
		{
			Init("Map");
			EnglishParamInfo = "";
			Description = new TranslatableItem(RealmLangKey.CmdMapDescription);
		}

		#region SpawnMap
		public class SpawnMapCommand : SubCommand
		{
			protected SpawnMapCommand() { }

			/// <summary>
			/// Spawns all active Maps
			/// </summary>
			/// <param name="trigger"></param>
			public static void SpawnAllMaps(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				foreach (var rgn in World.GetAllMaps())
				{
					if (!rgn.IsRunning)
					{
						continue;
					}

					var map = rgn;		// create a copy so we don't spawn the wrong Map
					map.AddMessage(() =>
					{
						if (!map.IsSpawned)
						{
							map.SpawnMap();
							trigger.Reply(RealmLangKey.CmdMapSpawnResponse, map.ToString());
						}
					});
				}
				return;
			}

			protected override void Initialize()
			{
				Init("Spawn", "S");
				ParamInfo = new TranslatableItem(RealmLangKey.CmdMapSpawnParamInfo);
				Description = new TranslatableItem(RealmLangKey.CmdMapSpawnDescription);
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				Map map;
				if (trigger.Text.HasNext)
				{
					var mod = trigger.Text.NextModifiers();
					if (mod == "a")
					{
						// spawn all
						SpawnAllMaps(trigger);
						trigger.Reply(RealmLangKey.CmdMapSpawnResponse1);
						return;
					}
					else
					{
						var mapId = trigger.Text.NextEnum(MapId.End);
						map = World.GetNonInstancedMap(mapId);
						if (map == null)
						{
							trigger.Reply(RealmLangKey.CmdMapSpawnError1);
							return;
						}
					}
				}
				else
				{
					if (trigger.Args.Target == null)
					{
						trigger.Reply(RealmLangKey.CmdMapSpawnError2);
						return;
					}
					map = trigger.Args.Target.Map;
				}

				if (map.IsSpawned)
				{
					trigger.Reply(RealmLangKey.CmdMapSpawnError3);
				}
				else
				{
					trigger.Reply(RealmLangKey.CmdMapSpawnResponse2, map.Name);
					if (!GOMgr.Loaded)
					{
						trigger.Reply(RealmLangKey.CmdMapSpawnError4);
					}

					if (!NPCMgr.Loaded)
					{
						trigger.Reply(RealmLangKey.CmdMapSpawnError5);
					}

					map.AddMessage(() =>
					{
						map.SpawnMap();
						trigger.Reply(RealmLangKey.CmdMapSpawnResponse3, map);
					});
				}
			}
		}
		#endregion

		#region Clear
		public class ClearMapCommand : SubCommand
		{
			protected ClearMapCommand() { }

			protected override void Initialize()
			{
				Init("Clear");
				ParamInfo = new TranslatableItem(RealmLangKey.CmdMapClearParamInfo);
				Description = new TranslatableItem(RealmLangKey.CmdMapClearDescription);
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				Map map;
				if (trigger.Text.HasNext)
				{
					var mapId = trigger.Text.NextEnum(MapId.End);
					map = World.GetNonInstancedMap(mapId);
					if (map == null)
					{
						trigger.Reply(RealmLangKey.CmdMapClearError1);
						return;
					}
				}
				else
				{
					if (trigger.Args.Character == null)
					{
						trigger.Reply(RealmLangKey.CmdMapClearError2);
						return;
					}
					map = trigger.Args.Character.Map;
				}

				map.AddMessage(() =>
				{
					map.RemoveObjects();
					trigger.Reply(RealmLangKey.CmdMapClearResponse, map.ToString());
				});
			}
		}
		#endregion

		#region Toggle Updates
		public class ToggleMapUpdatesCommand : SubCommand
		{
			protected ToggleMapUpdatesCommand() { }

			protected override void Initialize()
			{
				Init("Updates", "Upd");
				EnglishParamInfo = "0|1";
				Description = new TranslatableItem(RealmLangKey.CmdMapUpdateDescription);
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var on = trigger.Text.NextBool();

				foreach (var rgn in World.Maps)
				{
					if (rgn != null && on != rgn.IsRunning)
					{
						if (on)
						{
							rgn.Start();
						}
						else
						{
							// not very helpful since, map will be re-activated in no time
							rgn.Stop();
						}
					}
				}
				trigger.Reply(RealmLangKey.Done);
			}
		}
		#endregion

		#region List
		public class MapListCommand : SubCommand
		{
			protected MapListCommand() { }

			protected override void Initialize()
			{
				Init("List", "L");
				Description = new TranslatableItem(RealmLangKey.CmdMapListDescription);
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var maps = World.GetAllMaps();
				if (maps != null)
				{
					trigger.Reply(RealmLangKey.CmdMapListResponse);
					foreach (var rgn in maps)
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
				" Only used for development purposes where the maps arent spawned automatically.";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			trigger.Reply("Not yet implemented - Use \"Map Spawn\" instead");
		}
	}
	#endregion
}