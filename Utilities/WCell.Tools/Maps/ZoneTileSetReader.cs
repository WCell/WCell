using System;
using NLog;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Tools.Maps.Parsing.ADT;

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

				// Read in the Tiles
				for (var y = 0; y < 64; y++)
				{
					for (var x = 0; x < 64; x++)
					{
						if (!wdt.TileProfile[x, y]) continue;
						++count;
						var adtName = TerrainConstants.GetADTFile(wdt.Name, x, y);
						var adt = ADTParser.Process(WDTParser.MpqManager, wdt.Path, adtName);
						if (adt == null) continue;

						tileSet.ZoneGrids[x, y] = grid = new ZoneGrid(new uint[TerrainConstants.ChunksPerTileSide, TerrainConstants.ChunksPerTileSide]);
						// Read in the TileChunks 
						for (var j = 0; j < 16; j++)
						{
							for (var i = 0; i < 16; i++)
							{
								var areaId = adt.MapChunks[i, j].AreaId;
								if (Enum.IsDefined(typeof(ZoneId), areaId))
								{
									grid.ZoneIds[i, j] = areaId;
								}
								else
								{
									grid.ZoneIds[i, j] = 0;
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
