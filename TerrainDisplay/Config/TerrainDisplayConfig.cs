using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using NLog;
using WCell.Util;
//using WCell.MPQTool;

namespace TerrainDisplay
{
    public class Config : XmlFile<Config>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public static readonly string ConfigFile = "TerrainDisplay.Config.xml";

        public static string TerrainDisplayRoot = "../../";
        public static string RunDir = TerrainDisplayRoot + "bin/";
        public static string ContentDir = TerrainDisplayRoot + "../../";
        public static string MapDir = ContentDir + "Maps/";
        public static string WMODir = ContentDir + "Maps/";
        public static string M2Dir = ContentDir + "Maps/";
        public static string DBCDir = TerrainDisplayRoot + @"../dbc/";
        public static string LogFolder = ContentDir + "/Logs/";

        public static bool UseExtractedData = false;
        public static string ExtractedDataPath = @"D:\Games\WCell\Run\Content\Maps";
        public static string MpqPath = @"D:\Games\MPQFiles";
        public static TileIdentifier DefaultTileIdentifier = TileIdentifier.Redridge;
        

        //public string wowDir;
        //public string GetWoWDir()
        //{
        //    return wowDir ?? (wowDir = DBCTool.FindWowDir());
        //}

        //public string WoWFileLocation
        //{
        //    get { return Path.Combine(GetWoWDir(), "wow.exe"); }
        //}

        
        [XmlElement]
        public static string OutputDir = Path.GetFullPath(TerrainDisplayRoot + "output/");

        private static Config s_instance;

        public static Config Instance
        {
            get
            {
                if (s_instance == null)
                {
                    if (File.Exists(ConfigFile))
                    {
                        log.Info("Loading ToolConfig from {0}...", Path.GetFullPath(ConfigFile));
                        s_instance = Load(ConfigFile);
                    }
                    else
                    {
                        s_instance = new Config {
                                                    m_filename = ConfigFile
                                                };
                    }
                    
                    s_instance.Save();
                }
                return s_instance;
            }
        }
    }
}
