using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Terrain.MPQ;
using WCell.Terrain.Serialization;
using WCell.Constants.World;
using WCell.MPQTool;
using System.IO;
using WCell.Util.NLog;
using WCell.MPQTool.StormLibWrapper;
using NLog;
using WCell.Util;

namespace WCell.Terrain.Extractor
{
    public class HeightfieldExtractor
    {
        protected static Logger s_log = LogManager.GetCurrentClassLogger();

        public const int Version = 3;

        public const string FileTypeId = "adtheights";
        public const string FileExtension = "." + FileTypeId;
        
        /// <summary>
        ///  The directory to write the files to
        /// </summary>
        public static string GetMapDirectory(MapId map)
        {
            return Path.Combine(WCellTerrainSettings.SimpleMapDir, ((int)map).ToString());
        }

        /// <summary>
        ///  The filename of extracted heightfields
        /// </summary>
        public static string GetFileName(MapId map, int tileX, int tileY)
        {
            var path = GetMapDirectory(map);
            return Path.Combine(path, TerrainConstants.GetTileName(tileX, tileY) + FileExtension);
        }

        /// <summary>
        ///  Extracts all heightfields of the given map
        /// </summary>
        public static bool ExtractHeightfield(MapId mapId)
        {
            var name = TileIdentifier.GetName(mapId);
            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine(@"No ADT for map {0}.", mapId);
                return false;
            }

			// get started
			var mpqFinder = WCellTerrainSettings.GetDefaultMPQFinder();
            

            // Create WDT
            var wdt = new WDT(mapId);

            var startTime = DateTime.Now;

            // compute total size
            Console.Write(@"Estimating workload... ");
            var totalAmount = 0;
			var archives = new HashSet<string>();
            for (var tileX = 0; tileX < TerrainConstants.TilesPerMapSide; tileX++)
            {
                for (var tileY = 0; tileY < TerrainConstants.TilesPerMapSide; tileY++)
                {
					var fname = ADTReader.GetFilename(mapId, tileX, tileY);
					var archive = mpqFinder.GetArchive(fname);
					if (archive != null && archive.GetFileSize(fname) > 0)
					{
						//totalSize += archive.GetFileSize(fname);
						++totalAmount;
						archives.Add(archive.Path);
					}
                }
            }
            Console.WriteLine(@"Done - Found {0} tiles in {1} files.", totalAmount, archives.Count);

			// Get cooking:
			Console.WriteLine();
			Console.WriteLine("Extracting...");
            
            // Load all ADTs and write them to file
        	var processedTiles = 0;
            for (var tileX = 0; tileX < TerrainConstants.TilesPerMapSide; tileX++)
            {
                for (var tileY = 0; tileY < TerrainConstants.TilesPerMapSide; tileY++)
                {
                    try
                    {
						var fname = ADTReader.GetFilename(mapId, tileX, tileY);
                    	long fsize;
						var archive = mpqFinder.GetArchive(fname);
                        if (archive != null && (fsize = archive.GetFileSize(fname)) > 0)
						{
                            //processedSize += fsize;

                            var adt = ADTReader.ReadADT(wdt, tileX, tileY, false);
                            Console.Write(@"Tile ({0}, {1}) in Map {2} has been read from {3}. Writing... ",
								tileX, tileY, mapId, Path.GetFileName(archive.Path));

                            // write to file
                            WriteHeightfield(adt);

							// stats
							++processedTiles;
                            var timePassed = DateTime.Now - startTime;
							var timePerTile = timePassed.Ticks / processedTiles;
							var progress = processedTiles / (float)totalAmount;
                            var timeRemaining = new TimeSpan((totalAmount - processedTiles) * timePerTile);
                            Console.WriteLine(@"Done. [{0}/{1} {2:F2}% - {3} (Remaining: {4})]", 
								processedTiles,
								totalAmount,
								100 * progress,
                                timePassed.Format(),
								timeRemaining.Format());
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine();
                        LogUtil.ErrorException(e, @"Extraction FAILED: Tile ({0}, {1}) in Map {2} could not be loaded", tileX, tileY, mapId);
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Write the given ADT's heightfield to disk
        /// </summary>
        public static void WriteHeightfield(ADT adt)
        {
            var path = Path.Combine(WCellTerrainSettings.SimpleMapDir, ((int)adt.Terrain.MapId).ToString());
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            using (var file = File.Create(GetFileName(adt.Terrain.MapId, adt.TileX, adt.TileY)))
            {
                using (var writer = new BinaryWriter(file))
                {
                    for (var y = 0; y < TerrainConstants.ChunksPerTileSide; ++y)
                    {
                        for (var unitY = 0; unitY <= TerrainConstants.UnitsPerChunkSide; ++unitY)
                        {
                            for (var x = 0; x < TerrainConstants.ChunksPerTileSide; ++x)
                            {
                                var chunk = adt.Chunks[x, y];
                                var heights = chunk.Heights.GetLowResMapMatrix();
                                //var holes = (chunk.HolesMask > 0) ? chunk.HolesMap : ADT.EmptyHolesArray;

                                // Add the height map values, inserting them into their correct positions
                                for (var unitX = 0; unitX <= TerrainConstants.UnitsPerChunkSide; ++unitX)
                                {
                                    //var tileX = (x * TerrainConstants.UnitsPerChunkSide) + unitX;
                                    //var tileY = (y * TerrainConstants.UnitsPerChunkSide) + unitY;
                                    //var xPos = TerrainConstants.CenterPoint
                                    //           - (TileX * TerrainConstants.TileSize)
                                    //           - (tileX * TerrainConstants.UnitSize);
                                    //var yPos = TerrainConstants.CenterPoint
                                    //           - (TileY * TerrainConstants.TileSize)
                                    //           - (tileY * TerrainConstants.UnitSize);
									
                                    var h = (heights[unitX, unitY] + chunk.MedianHeight);

                                    // Write height
                                    writer.Write(h);

                                    //tileHolesMap[tileX, tileY] = holes[unitX / 2, unitY / 2];
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
