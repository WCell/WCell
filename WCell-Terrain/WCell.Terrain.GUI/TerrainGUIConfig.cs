using System;
using System.IO;
using NLog;
using WCell.Terrain;
using WCell.Util.NLog;
using WCell.Util.Variables;

//using WCell.MPQTool;

namespace WCell.Terrain.GUI
{
    public class TerrainGUIConfig : TerrainConfiguration<TerrainGUIConfig>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public static readonly string ConfigFileName = "TerrainDisplay.Config.xml";

        public static readonly string WCellRoot = "../../../";

    	public override string RootFolder
    	{
    		get { return WCellRoot;  }
    	}


        public static bool UseExtractedData = false;

        public TerrainGUIConfig()
        {
        	ErrorHandler = OnError;
			RootNodeName = "TerrainDisplayConfig";
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
                Instance.AddVariablesOfAsm<VariableAttribute>(typeof(TerrainGUIConfig).Assembly);
                
                try
                {
                    if (!Instance.Load())
                    {
                        Instance.Save(true, false);
                        log.Warn("Config-file \"{0}\" not found - Created new file.", Instance.FilePath);
                        log.Warn("Please take a little time to configure your server and then restart the Application.");
                        log.Warn("See http://wiki.wcell.org/index.php/Configuration for more information.");
                        return false;
                    }
                    else
                    {
                        if (Instance.AutoSave)
                        {
                            Instance.Save(true, true);
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
