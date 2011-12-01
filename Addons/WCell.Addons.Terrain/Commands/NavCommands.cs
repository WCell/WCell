using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Commands;
using WCell.RealmServer.Editor.Figurines;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.NPCs;
using WCell.Terrain;
using WCell.Util.Collections;
using WCell.Util.Commands;

namespace WCell.Addons.Terrain.Commands
{
	/// <summary>
	/// 
	/// </summary>
	public class NavCommands : RealmServerCommand
	{

		protected override void Initialize()
		{
			Init("Nav");
			EnglishDescription = "Commands to work with and debug the navigation meshes.";
		}
		public class NavClearCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Clear", "C");
				EnglishDescription = "Clears any path visualization.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var chr = trigger.Args.Character;
				if (chr == null)
				{
					trigger.Reply("Character required in context.");
					return;
				}

				TerrainVisualizations.ClearVis(chr);
			}
		}

		public class NavShowCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Show", "S");
				EnglishDescription = "Visualizes the navmesh in your vaccinity";
				EnglishParamInfo = "[<radius>]";
			}


			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var chr = trigger.Args.Character;
				if (chr == null)
				{
					trigger.Reply("Character required in context.");
					return;
				}

				if (!NPCMgr.Loaded)
				{
					trigger.Reply("Loading NPCs...");
					NPCMgr.LoadAllLater();
					return;
				}

				var figurines = TerrainVisualizations.ClearVis(chr);

				var radius = trigger.Text.NextFloat(30.0f);
				var p = chr.Position;
				var map = chr.Map;
				var terrain = TerrainMgr.GetTerrain(map.MapId) as SimpleTerrain;
				if (terrain == null)
				{
					trigger.Reply("No Terrain created for Map: " + map);
					return;
				}

				int tileX, tileY;
				PositionUtil.GetTileXYForPos(p, out tileX, out tileY);
				var tile = terrain.GetTile(tileX, tileY);
				if (tile == null || tile.NavMesh == null)
				{
					trigger.Reply("No NavMesh present on Tile.");
					return;
				}

				var mesh = tile.NavMesh;
				foreach (var poly in mesh.Polygons)
				{
					Debug.Assert(poly.Indices.Length == 3);
					var v1 = mesh.Vertices[poly.Indices[0]];
					var v2 = mesh.Vertices[poly.Indices[1]];
					var v3 = mesh.Vertices[poly.Indices[2]];

					// only get polygons that are narby
					if (p.GetDistance(ref v1) > radius &&
						p.GetDistance(ref v2) > radius &&
						p.GetDistance(ref v3) > radius) continue;

					var dumm1 = new NavFigurine();
					var dumm2 = new NavFigurine();
					var dumm3 = new NavFigurine();

					dumm1.SetOrientationTowards(ref v2);
					dumm1.ChannelObject = dumm2;
					map.AddObject(dumm1, ref v1);

					dumm2.SetOrientationTowards(ref v3);
					dumm2.ChannelObject = dumm3;
					map.AddObject(dumm2, ref v2);

					dumm3.SetOrientationTowards(ref v1);
					dumm3.ChannelObject = dumm1;
					map.AddObject(dumm3, ref v3);

					figurines.Add(dumm1);
					figurines.Add(dumm2);
					figurines.Add(dumm3);
				}

				trigger.Reply("Created {0} figurines.", figurines.Count);
			}
		}

		public class PathCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Path", "P");
				EnglishDescription = "Gives you the path highlighter spell";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var chr = trigger.Args.Character;
				if (chr == null)
				{
					trigger.Reply("Need character in context.");
					return;
				}

				chr.Spells.AddSpell(DebugSpells.PathDisplaySpell);
			}
		}
	}
}
