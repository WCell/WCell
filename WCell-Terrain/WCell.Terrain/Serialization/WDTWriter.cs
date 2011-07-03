using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using WCell.Constants;
using WCell.Terrain.MPQ;
using WCell.MPQTool;
using WCell.Terrain.MPQ.DBC;

namespace WCell.Terrain.Serialization
{
    public class WDTWriter
    {
		/// <summary>
		/// Version of our custom map file format
		/// </summary>
		public const int Version = 2;

		private const string FileTypeId = "map";

        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public static MPQFinder MpqFinder;

        private const string baseDir = "WORLD\\MAPS\\";
        public const string Extension = ".map";

        public static void WriteAllWDTs()
        {
            log.Info("Extracting Terrain and World Objects - This will take a long time.....");
            var startTime = DateTime.Now;

			_WriteAllWDTs();

            log.Info("Done (Elapsed time: {0})", DateTime.Now - startTime);
        }

        /// <summary>
        /// Starts processing all WDT and ADT files
        /// </summary>
        private static void _WriteAllWDTs()
        {
			foreach (var wdt in WDTReader.ReadAllWDTs())
            {   
                var path = Path.Combine(WCellTerrainSettings.RawMapDir, wdt.Entry.Id.ToString());
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var filePath = Path.Combine(path, string.Format("{0}{1}", wdt.Entry.Id, Extension));
                using (var file = File.Create(filePath))
                {
                    WriteWdt(file, wdt);
                }

				ADTWriter.WriteADTs(wdt);
				WorldObjectWriter.ExtractMapObjects(wdt);
            }
        }

        private static void WriteWdt(Stream file, WDT wdt)
        {
			var writer = new BinaryWriter(file);

			writer.Write(FileTypeId);
			writer.Write(Version);

            writer.Write(wdt.IsWMOOnly);
            if (wdt.IsWMOOnly) return;

			for (var x = 0; x < TerrainConstants.TilesPerMapSide; x++)
            {
                // Columns are along the y-axis
				for (var y = 0; y < TerrainConstants.TilesPerMapSide; y++)
                {
                    // Stored as [col, row], that's weird.
                    writer.Write(wdt.TileProfile[y, x]);
                }
            }
        }
    }
}
