using System;
using System.IO;
using NLog;
using WCell.Constants.World;
using WCell.MPQTool;
using WCell.Tools.Maps.Constants;
using WCell.Tools.Maps.Parsing.ADT.Components;
using WCell.Tools.Maps.Structures;
using WCell.Tools.Maps.Utils;


namespace WCell.Tools.Maps.Parsing.WDT
{
    
    public static class WDTParser
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();


        public static event Action<WDTFile> Parsed;
        public static MpqManager MpqManager;

        private const string baseDir = "WORLD\\MAPS\\";
        public const string Extension = ".wdt";

        private static string _basePath;
        private static ContinentType _continent;

        internal static void EnsureEventEmpty()
        {
            if (Parsed != null)
            {
                throw new Exception("Parsed event is already in use.");
            }
        }

        public static void ExportAll()
        {
            log.Info("Extracting Terrain and World Objects - This will take a few minutes.....");
            var startTime = DateTime.Now;

            MapTileExtractor.Prepare();
            WorldObjectExtractor.PrepareExtractor();

            ProcessAll();

            log.Info("Done ({0})", DateTime.Now - startTime);
        }

        /// <summary>
        /// Starts processing all WDT and ADT files
        /// </summary>
        public static void ProcessAll()
        {
            var wowRootDir = ToolConfig.Instance.GetWoWDir();
            MpqManager = new MpqManager(wowRootDir);
            var entryList = DBCMapReader.GetMapEntries();

            foreach (var mapEntry in entryList)
            {
                //if (mapEntry.Id != (int)MapId.EasternKingdoms) continue;
                Process(mapEntry);
            }
        }

        static void Process(DBCMapEntry entry)
        {
            if (Parsed == null)
            {
                throw new Exception("WDTParser.Parsed must be set before calling WDTParser.Process");
            }

            var dir = entry.MapDirName;
            var wdtDir = Path.Combine(baseDir, dir);
            var wdtName = dir;

            var wdtFilePath = Path.Combine(wdtDir, wdtName + Extension);
            if (!MpqManager.FileExists(wdtFilePath)) return;

            var wdt = new WDTFile
            {
                Manager = MpqManager,
                Entry = entry,
                Name = wdtName,
                Path = wdtDir
            };

            using (var fileReader = new BinaryReader(MpqManager.OpenFile(wdtFilePath)))
            {
                ReadMVER(fileReader, wdt);
                ReadMPHD(fileReader, wdt);
                ReadMAIN(fileReader, wdt);

                if (wdt.Header.IsWMOMap)
                {
                    // No terrain, the map is a "global" wmo
                    // MWMO and MODF chunks follow
                    wdt.IsWMOOnly = true;
                    ReadMWMO(fileReader, wdt);
                    ReadMODF(fileReader, wdt);
                }
            }

            Parsed(wdt);
        }

        static void ReadMWMO(BinaryReader fileReader, WDTFile wdt)
        {
            var type = fileReader.ReadUInt32();
            var size = fileReader.ReadUInt32();

            var endPos = fileReader.BaseStream.Position + size;
            while (fileReader.BaseStream.Position < endPos)
            {
                if (fileReader.PeekByte() == 0)
                {
                    fileReader.BaseStream.Position++;
                }
                else
                {
                    wdt.WmoFiles.Add(fileReader.ReadCString());
                }
            }
        }

        static void ReadMODF(BinaryReader fileReader, WDTFile wdt)
        {
            var type = fileReader.ReadUInt32();
            var size = fileReader.ReadUInt32();

            var endPos = fileReader.BaseStream.Position + size;
            while (fileReader.BaseStream.Position < endPos)
            {
                var objectDef = new MapObjectDefinition();
                var nameIndex = fileReader.ReadInt32(); // 4 bytes
                objectDef.FilePath = wdt.WmoFiles[nameIndex];
                objectDef.UniqueId = fileReader.ReadUInt32(); // 4 bytes
                objectDef.Position = fileReader.ReadVector3(); // 12 bytes
                objectDef.OrientationA = fileReader.ReadSingle(); // 4 Bytes
                objectDef.OrientationB = fileReader.ReadSingle(); // 4 Bytes
                objectDef.OrientationC = fileReader.ReadSingle(); // 4 Bytes
                objectDef.Extents = fileReader.ReadBoundingBox(); // 12*2 bytes
                objectDef.Flags = fileReader.ReadUInt16(); // 2 bytes
                objectDef.DoodadSetId = fileReader.ReadUInt16(); // 2 bytes
                objectDef.NameSet = fileReader.ReadUInt16(); // 2 bytes
                fileReader.ReadUInt16(); // padding

                wdt.WmoDefinitions.Add(objectDef);
            }
        }

        static void ReadMVER(BinaryReader fileReader, WDTFile wdt)
        {
            var type = fileReader.ReadUInt32();
            var size = fileReader.ReadUInt32();
            wdt.Version = fileReader.ReadInt32();
        }

        static void ReadMPHD(BinaryReader fileReader, WDTFile wdt)
        {
            var type = fileReader.ReadUInt32();
            var size = fileReader.ReadUInt32();
            wdt.Header.Header1 = (WDTFlags)fileReader.ReadInt32();
            wdt.Header.Header2 = fileReader.ReadInt32();
            wdt.Header.Header3 = fileReader.ReadInt32();
            wdt.Header.Header4 = fileReader.ReadInt32();
            wdt.Header.Header5 = fileReader.ReadInt32();
            wdt.Header.Header6 = fileReader.ReadInt32(); 
            wdt.Header.Header7 = fileReader.ReadInt32();
            wdt.Header.Header8 = fileReader.ReadInt32();
        }

        static void ReadMAIN(BinaryReader fileReader, WDTFile wdt)
        {
            var type = fileReader.ReadUInt32();
            var size = fileReader.ReadUInt32();

            // Rows are along the x-axis
            for (var x = 0; x < TerrainConstants.MapTileCount; x++)
            {
                // Columns are along the y-axis
                for (var y = 0; y < TerrainConstants.MapTileCount; y++)
                {
                    //if (x == 48 && y == 30)
                    //{
                    //    wdt.TileProfile[y, x] = true;
                    //}
                    //else
                    //{
                    //    wdt.TileProfile[y, x] = false;
                    //}
                    // Stored as [col, row], that's weird.
                    wdt.TileProfile[y, x] = (fileReader.ReadInt64() != 0);
                }
            }
        }
    }
}