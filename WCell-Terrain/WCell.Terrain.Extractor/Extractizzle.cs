using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.World;
using WCell.Terrain.MPQ;
using WCell.Terrain.Serialization;

namespace WCell.Terrain.Extractor
{
	public static class Extractizzle
	{
		public static void DoShit()
		{
			foreach (MapId mapId in Enum.GetValues(typeof(MapId)))
			{

				if (mapId == MapId.EasternKingdoms || mapId == MapId.Kalimdor)
					continue;

				var terrain = new SimpleTerrain(mapId);
				if (terrain == null)
				{
					Console.WriteLine(@"Invalid MapId.");
					return;
				}

				for (var tileX = 0; tileX < 64; tileX++)
				{
					for (var tileY = 0; tileY < 64; tileY++)
					{
						// try to extract from MPQ
						//var adt = ADTReader.ReadADT(terrain, tileX, tileY);
						//Console.WriteLine(@"EXPORTING: Tile ({0}, {1}) in Map {2} ...", tileX, tileY, mapId);
						//Console.WriteLine(@"Extraction will take a while, please have patience...");

						try
						{
							var adt = WDT.LoadTile(mapId, tileX, tileY);
							if (adt != null)
							{
								Console.WriteLine(@"Tile ({0}, {1}) in Map {2} has been imported...", tileX, tileY, mapId);
								Console.WriteLine(@"Writing to file...");

								// export to file
								SimpleTileWriter.WriteADT(adt);

								// try loading again
								Console.WriteLine(@"Loading extracted tile and generating Navigation mesh...");
								terrain.ForceLoadTile(tileX, tileY);

								if (terrain.IsAvailable(tileX, tileY))
								{
									Console.WriteLine(@"Done. Tile ({0}, {1}) in Map {2} has been loaded successfully.", tileX, tileY, mapId);
									continue;
								}
							}
						}
						catch (ArgumentException)
						{
						}

						Console.WriteLine(@"Loading FAILED: Tile ({0}, {1}) in Map {2} could not be loaded", tileX, tileY, mapId);
					}
				}
			}
		}
	}
}
