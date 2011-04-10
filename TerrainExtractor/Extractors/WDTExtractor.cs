using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using TerrainDisplay;
using TerrainDisplay.MPQ.WDT;
using TerrainDisplay.World.DBC;
using TerrainExtractor.Extractors;
using WCell.Constants.World;
using WCell.MPQTool;

namespace TerrainExtractor.Parsers
{
    public class WDTExtractor
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static event Action<WDT> Parsed;
        public static MpqManager MpqManager;

        private const string baseDir = "WORLD\\MAPS\\";
        private const string FileTypeId = "map";
        public const string Extension = ".map";

        internal static void EnsureEventEmpty()
        {
            if (Parsed != null)
            {
                throw new Exception("Parsed event is already in use.");
            }
        }

        public static void ExportAll()
        {
            log.Info("Extracting Terrain and World Objects - This will take a long time.....");
            var startTime = DateTime.Now;

            TileExtractor.Prepare();
            WorldObjectExtractor.Prepare();

            ProcessAll();

            log.Info("Done (Elapsed time: {0})", DateTime.Now - startTime);
        }

        /// <summary>
        /// Starts processing all WDT and ADT files
        /// </summary>
        public static void ProcessAll()
        {
            if (Parsed == null)
            {
                throw new Exception("WDTExtractor.Parsed must be set before calling WDTExtractor.Process");
            }

            var wowRootDir = DBCTool.FindWowDir();
            MpqManager = new MpqManager(wowRootDir);
            var entryList = GetMapEntries();

            WDTParser.MpqManager = MpqManager;
            foreach (var mapEntry in entryList)
            {
                //if (mapEntry.Id != MapId.EasternKingdoms) continue;
                var wdt = WDTParser.Process(mapEntry);
                if (wdt == null) continue;
                
                var path = Path.Combine(TerrainDisplayConfig.MapDir, wdt.Entry.Id.ToString());
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var filePath = Path.Combine(path, string.Format("{0}{1}", wdt.Entry.Id, Extension));
                using (var file = File.Create(filePath))
                {
                    WriteWdt(file, wdt);
                    file.Close();
                }
                Parsed(wdt);
            }
        }

        private static IEnumerable<MapInfo> GetMapEntries()
        {
            var dbcPath = Path.Combine(TerrainDisplayConfig.DBCDir, TerrainDisplayConfig.MapDBCName);
            var dbcMapReader = new MappedDBCReader<MapInfo, DBCMapEntryConverter>(dbcPath);
            return dbcMapReader.Entries.Values.ToList();
        }

        private static void WriteWdt(Stream file, WDT wdt)
        {
            var writer = new BinaryWriter(file);
            writer.Write(FileTypeId);

            writer.Write(wdt.IsWMOOnly);
            if (wdt.IsWMOOnly) return;

            for (var x = 0; x < TerrainConstants.MapTileCount; x++)
            {
                // Columns are along the y-axis
                for (var y = 0; y < TerrainConstants.MapTileCount; y++)
                {
                    // Stored as [col, row], that's weird.
                    writer.Write(wdt.TileProfile[y, x]);
                }
            }
        }
    }
}
