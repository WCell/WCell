using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.World;
using WCell.MPQTool;
using WCell.Terrain.MPQ;
using WCell.Terrain.Serialization;

namespace WCell.Terrain.Extractor
{
	public static class Extractor
	{
		public static void ExtractAndWriteAll()
		{
			foreach (MapId mapId in Enum.GetValues(typeof(MapId)))
			{
				var name = TileIdentifier.GetName(mapId);
				if (string.IsNullOrEmpty(name))
				{
					Console.WriteLine(@"No ADT for map {0}.", mapId);
					continue;
				}

				var terrain = new SimpleTerrain(mapId);

				for (var tileX = 0; tileX < 64; tileX++)
				{
					for (var tileY = 0; tileY < 64; tileY++)
					{
						string filePath;
						MpqLibrarian mpqFinder;
						if (!ADTReader.TryGetADTPath(mapId, tileX, tileY, out filePath, out mpqFinder))
							continue;
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

		public static void ExtractAllADTs()
		{
			foreach (MapId mapId in Enum.GetValues(typeof(MapId)))
			{
				var name = TileIdentifier.GetName(mapId);
				if (string.IsNullOrEmpty(name))
				{
					Console.WriteLine(@"No ADT for map {0}.", mapId);
					continue;
				}

				var wdt = new WDT(mapId);

				for (var tileX = 0; tileX < 64; tileX++)
				{
					for (var tileY = 0; tileY < 64; tileY++)
					{
						string filePath;
						MpqLibrarian mpqFinder;
						if (!ADTReader.TryGetADTPath(mapId, tileX, tileY, out filePath, out mpqFinder))
							continue;

						try
						{
							wdt.TileProfile[tileX, tileY] = true;
							var adt = ADTReader.ReadADT(wdt, tileX, tileY);
							if (adt != null)
							{
								adt.GenerateMapWithNoSimplification();
								Console.WriteLine(@"Tile ({0}, {1}) in Map {2} has been imported...", tileX, tileY, mapId);
								Console.WriteLine(@"Writing to file...");

								// export to file
								SimpleTileWriter.WriteADT(adt);
								continue;
							}
						}
						catch (ArgumentException)
						{
						}

						Console.WriteLine(@"Extracting FAILED: Tile ({0}, {1}) in Map {2} could not be loaded", tileX, tileY, mapId);
					}
				}
			}
		}

		public static void CreateAndWriteAllMeshes()
		{
			foreach (MapId mapId in Enum.GetValues(typeof(MapId)))
			{
				var name = TileIdentifier.GetName(mapId);
				if (string.IsNullOrEmpty(name))
				{
					Console.WriteLine(@"No ADT for map {0}.", mapId);
					continue;
				}

				var terrain = new SimpleTerrain(mapId);

				for (var tileX = 0; tileX < 64; tileX++)
				{
					for (var tileY = 0; tileY < 64; tileY++)
					{
						string filePath;
						MpqLibrarian mpqFinder;
						if (!ADTReader.TryGetADTPath(mapId, tileX, tileY, out filePath, out mpqFinder))
							continue;

						try
						{
							var adt = WDT.LoadTile(mapId, tileX, tileY);
							if (adt != null)
							{
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

						Console.WriteLine(@"Extracting FAILED: Tile ({0}, {1}) in Map {2} could not be loaded", tileX, tileY, mapId);
					}
				}
			}
		}
	}
}
