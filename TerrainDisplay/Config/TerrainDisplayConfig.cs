using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using NLog;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.Variables;

//using WCell.MPQTool;

namespace TerrainDisplay
{
    public class TerrainDisplayConfig : VariableConfiguration<TerrainDisplayConfig, TypeVariableDefinition>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public static readonly string ConfigFileName = "TerrainDisplay.Config.xml";

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
        
        public static string OutputDir = Path.GetFullPath(TerrainDisplayRoot + "output/");


        public TerrainDisplayConfig(Action<string> onError)
            : base(onError)
        {
        }

        public TerrainDisplayConfig() : base(OnError)
        {
            RootNodeName = "TerrainDisplayConfig";
            s_instance = this;
        }

        private static TerrainDisplayConfig s_instance;
        public static TerrainDisplayConfig Instance
        {
            get
            {
               return s_instance;
            }
        }

        public override string FilePath
        {
            get { return GetFullPath(ConfigFileName); }
            set { throw new InvalidOperationException("Cannot change the config's filename!");}
        }

        public override bool AutoSave { get; set; }

        [NotVariable]
        public static bool Loaded { get; private set; }

        public static bool Initialize()
        {
            if (!Loaded)
            {
                Loaded = true;
                s_instance.AddVariablesOfAsm<VariableAttribute>(typeof(TerrainDisplayConfig).Assembly);
                
                try
                {
                    if (!s_instance.Load())
                    {
                        s_instance.Save(true, false);
                        log.Warn("Config-file \"{0}\" not found - Created new file.", Instance.FilePath);
                        log.Warn("Please take a little time to configure your server and then restart the Application.");
                        log.Warn("See http://wiki.wcell.org/index.php/Configuration for more information.");
                        return false;
                    }
                    else
                    {
                        if (s_instance.AutoSave)
                        {
                            s_instance.Save(true, true);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogUtil.ErrorException(e, "Unable to load Configuration.");
                    log.Error("Please correct the invalid values in your configuration file and restart the Applicaton.");
                    return false;
                }
            }
            return true;
        }

        internal static void OnError(string msg)
        {
            log.Warn("<Config>" + msg);
        }

        internal static void OnError(string msg, params object[] args)
        {
            log.Warn("<Config>" + String.Format(msg, args));
        }

        public static string GetFullPath(string fileName)
        {
            return Path.IsPathRooted(fileName) 
                ? fileName 
                : Path.Combine(Environment.CurrentDirectory, fileName);
        }
    }
}
