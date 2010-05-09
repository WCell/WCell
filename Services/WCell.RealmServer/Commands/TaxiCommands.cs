using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Taxi;
using WCell.Util.Commands;
using WCell.Util;
using WCell.Constants.Updates;

namespace WCell.RealmServer.Commands
{
	public class TaxiCommand : RealmServerCommand
	{
		protected override void Initialize()
		{
			Init("Taxi");
			EnglishDescription = "Provides commands to manage Taxi nodes.";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			if (trigger.Text.HasNext)
			{
				base.Process(trigger);
			}
			else
			{
				// opens a taxi screen

			}
		}

		#region Stop
		public class StopTaxiCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Stop", "Cancel", "C");
				ParamInfo = "";
				EnglishDescription = "Stops the current taxi flight (if flying).";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var target = trigger.Args.Target;
				if (target == null)
				{
					trigger.Reply("No one selected.");
				}
				else
				{
					if (target.IsOnTaxi)
					{
						target.CancelTaxiFlight();
						trigger.Reply("Cancelled.");
					}
					else
					{
						trigger.Reply("Not on Taxi.");
					}
				}
			}
		}
		#endregion

		#region Show
		public class ShowTaxiMapCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Show");
				//ParamInfo = "[-r[a] <Region>]";
				ParamInfo = "[-a]";
				EnglishDescription = "Shows the Taxi-Map. The -a switch automatically activates all Nodes beforehand."
					//+ " The -r switch shows the Map of the given Region rather than the current one."
					;
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var target = trigger.Args.Target as Character;
				if (target == null)
				{
					trigger.Reply("No one selected.");
				}
				else
				{
					var mod = trigger.Text.NextModifiers();
					if (mod.Contains("a"))
					{
						target.ActivateAllTaxiNodes();
					}

					Region region;
					PathNode node;
					if (mod.Contains("r"))
					{
						region = World.GetRegion(trigger.Text.NextEnum(MapId.End));
						if (region == null)
						{
							trigger.Reply("Invalid Region.");
							return;
						}
						node = region.FirstTaxiNode;
					}
					else
					{
						region = target.Region;
						node = TaxiMgr.GetNearestTaxiNode(target.Position);
					}

					if (node != null)
					{
						TaxiHandler.ShowTaxiList(target, node);
					}
					else
					{
						trigger.Reply("There are no Taxis available on this Map ({0})", region.Name);
					}
				}
			}
		}
		#endregion

		#region Goto
		public class GotoNodeCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Go", "Goto");
				ParamInfo = "[<name>|<id>]";
				EnglishDescription = "Goes to the first node matching the given Name or Id.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var list = GetNodes(trigger);

				var target = trigger.Args.Target;
				if (target == null)
				{
					trigger.Reply("No one selected.");
				}
				else
				{
					var node = list.FirstOrDefault();
					if (node != null)
					{
						target.TeleportTo(node.Region, node.Position);
					}
				}
			}
		}
		#endregion

		#region Next
		public class NextNodeCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("GotoNext", "TeleNext", "Next");
				EnglishDescription = "Teleports to the closest Taxi Node in the current Region.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var target = trigger.Args.Target;
				if (target == null)
				{
					trigger.Reply("No one selected.");
				}
				else
				{
					PathNode closest = null;
					var closestDist = float.MaxValue;
					foreach (var node in GetNodes(trigger))
					{
						if (node.MapId == target.Region.Id)
						{
							var dist = target.GetDistanceSq(node.Position);
							if (dist < closestDist)
							{
								closestDist = dist;
								closest = node;
							}
						}
					}
					if (closest == null)
					{
						trigger.Reply("No Node found in Region.");
					}
					else
					{
						target.TeleportTo(closest.Region, closest.Position);
					}
				}
			}
		}
		#endregion

		#region List
		public class ListNodesCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("List");
				ParamInfo = "[-rc <Map>][<name>|<id>]";
				EnglishDescription = "Lists all Taxi nodes or only those matching the given Name or Id. " +
					"-r swtich filters Nodes of the given Region. -rc switch filters Nodes of the current Region.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var map = MapId.End;
				var mod = trigger.Text.NextModifiers();
				if (mod.Contains("r"))
				{
					if (mod.Contains("c") && trigger.Args.Target != null)
					{
						map = trigger.Args.Target.Region.Id;
					}
					else
					{
						map = trigger.Text.NextEnum(MapId.End);
						if (map == MapId.End)
						{
							trigger.Reply("Invalid Map: " + map);
							return;
						}
					}
				}
				foreach (var node in GetNodes(trigger))
				{
					if (map == MapId.End || node.MapId == map)
					{
						trigger.Reply(node);
					}
				}
			}
		}
		#endregion

		#region Info
		public class TaxiInfoCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Info");
				ParamInfo = "";
				EnglishDescription = "Shows information about the current Taxi-path.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				if (trigger.Args.Target == null)
				{
					trigger.Reply("Nothing selected.");
				}
				else
				{
					if (!trigger.Args.Target.IsOnTaxi)
					{
						trigger.Reply("{0} is not on a Taxi.", trigger.Args.Target.Name);
					}
					else
					{
						var path = trigger.Args.Target.TaxiPaths.Peek();
						trigger.Reply("Flying on: " + path);
						trigger.Reply("Flying for {0}m {1}s / {2}m {3}s ({4}%)",
							trigger.Args.Target.TaxiTime / (60 * 1000),
							(trigger.Args.Target.TaxiTime / 1000) % 60,
							path.PathTime / (60 * 1000),
							(path.PathTime / 1000) % 60,
							(100 * trigger.Args.Target.TaxiTime) / path.PathTime);
					}
				}
			}
		}
		#endregion

		#region Activate
		public class ActivateNodesCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Activate");
				ParamInfo = "[<name>|<id>]";
				EnglishDescription = "Activates all Taxi nodes or only those matching the given Name or Id.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var chr = trigger.Args.Target as Character;
				if (chr == null)
				{
					trigger.Reply("No Character selected.");
				}
				else
				{
					chr.ActivateAllTaxiNodes();
				}
			}
		}
		#endregion

		/// <summary>
		/// Returns all Nodes matching the arguments in the given trigger.
		/// </summary>
		/// <param name="trigger">[part of name|id]</param>
		/// <returns></returns>
		public static List<PathNode> GetNodes(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var name = trigger.Text.NextWord();
			var list = new List<PathNode>();
			uint id;
			if (uint.TryParse(name, out id))
			{
				var node = TaxiMgr.GetNode(id);
				if (node == null)
				{
					trigger.Reply("Invalid Id: " + id);
					return list;
				}
				list.Add(node);
			}
			else if (name.Length > 0)
			{
				foreach (var node in TaxiMgr.PathNodesById)
				{
					if (node != null && node.Name.IndexOf(name, StringComparison.InvariantCultureIgnoreCase) >= 0)
					{
						list.Add(node);
					}
				}
			}
			else
			{
				foreach (var node in TaxiMgr.PathNodesById)
				{
					if (node != null)
					{
						list.Add(node);
					}
				}
			}
			if (list.Count == 0)
			{
				if (name.Length > 0)
				{
					trigger.Reply("Invalid Node-name: " + name);
				}
				else
				{
					trigger.Reply("No Node.");
				}
			}
			return list;
		}

		public override ObjectTypeCustom TargetTypes
		{
			get { return ObjectTypeCustom.None; }
		}
	}
}
