using System.IO;
using WCell.Constants;
using WCell.Util;

namespace WCell.MPQTool
{
    public class MPQToolConfig : XmlFile<MPQToolConfig>
    {
        public static readonly string ConfigFile = "WCell.MPQTool.Config.xml";

        public static string DBCDirPrefix = new DirectoryInfo(string.Format(@"../Content/dbc")).FullName;

        public static string DefaultDBCOutputDir
        {
            get { return DBCDirPrefix + WCellInfo.RequiredVersion.BasicString + "/"; }
        }

        private static MPQToolConfig s_instance;

        public static MPQToolConfig Instance
        {
            get
            {
                if (s_instance == null)
                {
                    if (File.Exists(ConfigFile))
                    {
                        //log.Info("Loading ToolConfig from {0}...", Path.GetFullPath(ConfigFile));
                        s_instance = Load(ConfigFile);
                    }
                    else
                    {
                        s_instance = new MPQToolConfig
                        {
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