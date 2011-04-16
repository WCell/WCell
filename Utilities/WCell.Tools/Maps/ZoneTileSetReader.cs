using System;
using NLog;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Tools.Maps.Parsing.ADT;
using WCell.Tools.Maps.Parsing.WDT;

namespace WCell.Tools.Maps
{
	public static class ZoneTileSetReader
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();
		private static int count;
		public static readonly ZoneTileSet[] Tiles = new ZoneTileSet[(int)MapId.End];

		public static ZoneTileSet[] ExportTileSets()
		{
			count = 0;

			WDTParser.EnsureEventEmpty();
			WDTParser.Parsed += GetTileSets;
			WDTParser.ProcessAll();

			log.Info("Exported {0} ZoneTileSets.", count);
			return Tiles;
		}

		public static void GetTileSets(WDTFile wdt)
		{
			if ((wdt.Header.Header1 & WDTFlags.GlobalWMO) == 0)
			{
				var tileSet = new ZoneTileSet();
				Tiles[wdt.Entry.Id] = tileSet;

				ZoneGrid grid;

                // Rows are along the x-axis
				for (var x = 0; x < 64; x++)
				{
                    // Columns are along the y-axis
					for (var y = 0; y < 64; y++)
					{
						if (!wdt.TileProfile[y, x]) continue;
						++count;
						var adt = ADTParser.Process(WDTParser.MpqManager, wdt.Entry, y, x);
						if (adt == null) continue;

						tileSet.ZoneGrids[y, x] = grid = new ZoneGrid(new uint[TerrainConstants.ChunksPerTileSide, TerrainConstants.ChunksPerTileSide]);
						
                        // Rows along the x-axis 
						for (var chunkX = 0; chunkX < 16; chunkX++)
						{
                            // Columns along the y-axis
							for (var chunkY = 0; chunkY < 16; chunkY++)
							{
								var areaId = adt.MapChunks[chunkY, chunkX].Header.AreaId;
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
				log.Info("Could not read Zones from WMO: " + (MapId)wdt.Entry.Id);
			}
		}
	}
}