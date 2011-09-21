using System.Collections.Generic;
using System.Linq;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Instances;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
	public class InstanceCommand : RealmServerCommand
	{
		protected InstanceCommand() { }

		protected override void Initialize()
		{
			Init("Instance", "Inst");
			EnglishDescription = "Provides some Commands to manage and use Instances.";
		}

		public static InstancedMap GetInstance(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			if (!trigger.Text.HasNext)
			{
				trigger.Reply("No MapId specified.");
			}

			var mapId = trigger.Text.NextEnum(MapId.End);
			if (mapId == MapId.End)
			{
				trigger.Reply("Invalid MapId.");
				return null;
			}

			if (!trigger.Text.HasNext)
			{
				trigger.Reply("No Instance-Id specified.");
			}

			var id = trigger.Text.NextUInt();
			var instance = InstanceMgr.Instances.GetInstance(mapId, id);
			if (instance == null)
			{
				trigger.Reply("Instance id does not exist: #{1} (for {0})", mapId, id);
			}
			return instance;
		}

		#region List
		public class InstanceListAllCommand : SubCommand
		{
			protected InstanceListAllCommand() { }

			protected override void Initialize()
			{
				Init("List", "L");
				EnglishParamInfo = "[<MapId>]";
				EnglishDescription = "Lists all active Instances, or those of the given Map.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				IEnumerable<BaseInstance> instances;
				if (trigger.Text.HasNext)
				{
					var id = trigger.Text.NextEnum(MapId.End);
					if (id == MapId.End)
					{
						trigger.Reply("Invalid BattlegroundId.");
						return;
					}
					instances = InstanceMgr.Instances.GetInstances(id);
				}
				else
				{
					instances = InstanceMgr.Instances.GetAllInstances();
				}

				trigger.Reply("Found {0} instances:", instances.Count());
				foreach (var instance in instances)
				{
					trigger.Reply(instance.ToString());
				}
			}
		}
		#endregion

		#region Create
		public class InstanceCreateCommand : SubCommand
		{
			protected InstanceCreateCommand() { }

			protected override void Initialize()
			{
				Init("Create", "C");
				EnglishParamInfo = "[-e[d]] <MapId> [<difficulty>]";
				EnglishDescription = "Creates a new Instance of the given Map. -d allows to specify the difficulty (value between 0 and 3). -e enters it right away.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var chr = trigger.Args.Target as Character;

				var mod = trigger.Text.NextModifiers();
				var mapid = trigger.Text.NextEnum(MapId.End);
				if (mapid == MapId.End)
				{
					trigger.Reply("Invalid MapId.");
					return;
				}

				var mapTemplate = World.GetMapTemplate(mapid);

				if (mapTemplate != null && mapTemplate.IsInstance)
				{
					uint diffIndex;
					if (mod.Contains("d"))
					{
						diffIndex = trigger.Text.NextUInt();
						var diff = mapTemplate.GetDifficulty(diffIndex);
						if (diff == null)
						{
							trigger.Reply("Invalid Difficulty: {0}");
						}
					}
					else if (chr != null)
					{
						diffIndex = chr.GetInstanceDifficulty(mapTemplate.IsRaid);
					}
					else
					{
						diffIndex = 0;
					}
					var instance = InstanceMgr.CreateInstance(chr, mapTemplate.InstanceTemplate, diffIndex);
					if (instance != null)
					{
						trigger.Reply("Instance created: " + instance);
						if (mod.Contains("e"))
						{
							if (chr != null)
							{
								instance.TeleportInside((Character)trigger.Args.Target);
							}
						}
					}
					else
					{
						trigger.Reply("Unable to create Instance of: " + mapTemplate);
					}
				}
				else
				{
					trigger.Reply("Invalid MapId.");
				}
			}
		}
		#endregion

		#region Enter
		public class InstanceEnterCommand : SubCommand
		{
			protected InstanceEnterCommand() { }

			protected override void Initialize()
			{
				Init("Enter", "E");
				EnglishParamInfo = "<MapId> <InstanceId> [<entrance>]";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var instance = GetInstance(trigger);
				if (instance != null)
				{
					var entrance = trigger.Text.NextInt(0);
					instance.TeleportInside(trigger.Args.Character, entrance);
				}
			}
		}
		#endregion

		#region Delete
		public class InstanceDeleteCommand : SubCommand
		{
			protected InstanceDeleteCommand() { }

			protected override void Initialize()
			{
				Init("Delete", "Del");
				EnglishParamInfo = "[<MapId> <InstanceId>]";
				EnglishDescription = "Delets the Instance of the given Map with the given Id, or the current one if no arguments are supplied.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				InstancedMap instance;
				if (!trigger.Text.HasNext && trigger.Args.Character != null)
				{
					instance = trigger.Args.Character.Map as InstancedMap;
					if (instance == null)
					{
						trigger.Reply("Current Map is not an Instance.");
						return;
					}
				}
				else
				{
					instance = GetInstance(trigger);
					if (instance == null)
					{
						return;
					}
				}

				instance.Delete();
				trigger.Reply("Instance Deleted");
			}
		}
		#endregion
	}
}