using System;
using NLog;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Terrain.MPQ;
using WCell.Terrain.Serialization;
using WCell.Util.Graphics;

namespace WCell.Terrain.Tools
{
	public static class ZoneTileSetReader
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();
		private static int count;
		public static readonly ZoneTileSet[] Tiles = new ZoneTileSet[(int)MapId.End];

		public static ZoneTileSet[] ExportTileSets()
		{
			foreach (var wdt in WDTReader.ReadAllWDTs())
			{
				GetTileSets(wdt);
			}

			log.Info("Exported {0} ZoneTileSets.", count);
			return Tiles;
		}

		public static void GetTileSets(WDT wdt)
		{
			if (!wdt.IsWMOOnly)
			{
				var tileSet = new ZoneTileSet();
				Tiles[(int) wdt.MapId] = tileSet;

				ZoneGrid grid;

                // Rows are along the x-axis
				for (var x = 0; x < 64; x++)
				{
                    // Columns are along the y-axis
					for (var y = 0; y < 64; y++)
					{
						if (!wdt.TileProfile[y, x]) continue;
						++count;
						var adt = ADTReader.ReadADT(wdt.Finder, wdt, new Point2D(x, y));
						if (adt == null) continue;

						tileSet.ZoneGrids[y, x] = grid = new ZoneGrid(new uint[TerrainConstants.ChunksPerTileSide, TerrainConstants.ChunksPerTileSide]);
						
                        // Rows along the x-axis 
						for (var chunkX = 0; chunkX < 16; chunkX++)
						{
                            // Columns along the y-axis
							for (var chunkY = 0; chunkY < 16; chunkY++)
							{
								var areaId = adt.GetADTChunk(chunkY, chunkX).Header.AreaId;
								if (Enum.IsDefined(typeof(ZoneId), areaId))
								{
									grid.ZoneIds[chunkY, chunkX] = (uint)areaId;
								}
								else
								{
									grid.ZoneIds[chunkY, chunkX] = 0;
								}
							}
						}
						//return tiles;
					}
				}
			}
			else
			{
				log.Info("Could not read Zones from WMO: " + wdt.MapId);
			}
		}
	}
}