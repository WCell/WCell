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
        public const string Extension = ".wdt";

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
            WorldObjectExtractor.PrepareExtractor();

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

            foreach (var mapEntry in entryList)
            {
                if (mapEntry.Id != MapId.EasternKingdoms) continue;
                WDTParser.Process(mapEntry);
            }
        }

        
        private static IEnumerable<MapInfo> GetMapEntries()
        {
            var dbcPath = Path.Combine(TerrainDisplayConfig.DBCDir, TerrainDisplayConfig.MapDBCName);
            var dbcMapReader = new MappedDBCReader<MapInfo, DBCMapEntryConverter>(dbcPath);
            return dbcMapReader.Entries.Values.ToList();
        }
    }
}
