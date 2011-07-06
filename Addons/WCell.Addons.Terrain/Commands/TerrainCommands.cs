using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.World;
using WCell.RealmServer.Commands;
using WCell.RealmServer.Global;
using WCell.Terrain.MPQ;
using WCell.Terrain.Serialization;
using WCell.Util.Commands;

namespace WCell.Addons.Terrain.Commands
{
	public class TileCommand : RealmServerCommand
	{
		protected override void Initialize()
		{
			Init("Tile");
			EnglishDescription = "Provides commands to work with Terrain tiles.";
		}

		public class LoadTileCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Load", "L");
				EnglishDescription = "Forces loading of the given tile. Careful, if the tile does not exist, this will use a lot of resources and consume quite a bit of time. " +
									"If no parameters are given, loads the tile where the user stands. " +
									"Also note that this command does NOT re-load a tile!";
				EnglishParamInfo = "[<mapid> <tilex> <tiley>]";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				MapId mapId;
				int tileX, tileY;
				if (trigger.Text.HasNext)
				{
					mapId = trigger.Text.NextEnum(MapId.End);
					tileX = trigger.Text.NextInt(-1);
					tileY = trigger.Text.NextInt(-1);
				}
				else if (trigger.Args.Character == null)
				{
					trigger.Reply("You did not specify map id and tile location.");
					return;
				}
				else
				{
					mapId = trigger.Args.Character.MapId;
					PositionUtil.GetTileXYForPos(trigger.Args.Character.Position, out tileX, out tileY);
				}

				var terrain = TerrainMgr.GetTerrain(mapId);
				if (terrain == null)
				{
					trigger.Reply("Invalid MapId.");
					return;
				}

				if (!PositionUtil.VerifyTileCoords(tileX, tileY))
				{
					trigger.Reply("Invalid tile coordinates.");
					return;
				}

				trigger.Reply("Loading tile...");
				RealmServer.RealmServer.IOQueue.AddMessage(() =>
				{
					terrain.ForceLoadTile(tileX, tileY);

					if (terrain.IsAvailable(tileX, tileY))
					{
						trigger.Reply("Done. Tile ({0}, {1}) in Map {2} has been loaded.", tileX, tileY, mapId);
					}
					else
					{
						// try to extract from MPQ
						//var adt = ADTReader.ReadADT(terrain, tileX, tileY);
						trigger.Reply("WARNING: Tile ({0}, {1}) in Map {2} has not been exported yet...", tileX, tileY, mapId);
						trigger.Reply("Extraction will take a while and block the IO queue, please have patience...");

						var adt = WDT.LoadTile(mapId, tileX, tileY);
						if (adt != null)
						{
							trigger.Reply("Tile ({0}, {1}) in Map {2} has been imported...", tileX, tileY, mapId);
							trigger.Reply("Writing to file...");

							// export to file
							SimpleTileWriter.WriteADT(adt);

							// try loading again
							trigger.Reply("Loading extracted tile and generating Navigation mesh...");
							terrain.ForceLoadTile(tileX, tileY);

							if (terrain.IsAvailable(tileX, tileY))
							{
								trigger.Reply("Done. Tile ({0}, {1}) in Map {2} has been loaded successfully.", tileX, tileY, mapId);
								return;
							}
						}

						trigger.Reply("Loading FAILED: Tile ({0}, {1}) in Map {2} could not be loaded", tileX, tileY, mapId);
					}
				});
			}
		}
	}
}
