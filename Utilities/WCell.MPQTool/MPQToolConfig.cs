using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using WCell.Constants;
using WCell.Util;

namespace WCell.MPQTool
{
	public class Config : XmlFile<Config>
	{
		public static readonly string ConfigFile = "WCell.MPQTool.Config.xml";

		public static string DBCDirPrefix = new DirectoryInfo(string.Format(@"../Content/dbc")).FullName;
		public static string DefaultDBCOutputDir
		{
			get { return DBCDirPrefix + WCellInfo.RequiredVersion.BasicString + "/"; }
		}

		private static Config s_instance;

		public static Config Instance
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
						s_instance = new Config
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