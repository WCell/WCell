using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using WCell.PacketAnalysis.Logs;
using WCell.Util;
using WCell.Tools.PATools;
using System.IO;
using System.Xml.Serialization;
using WCell.RealmServer.Debugging;
using WCell.Constants;
using WCell.Util.Variables;
using WCell.MPQTool;

namespace WCell.Tools
{
	public class ToolConfig : XmlConfig<ToolConfig>
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();
		public static readonly string ConfigFile = "Tools.xml";

		public static string ToolsRoot = "../../Utilities/WCell.Tools/";
		public static string WCellRoot = ToolsRoot + "../../";
		public static string CoreDir = WCellRoot + "Core/";
		public static string WCellCoreRoot = CoreDir + "WCell.Core/";
		public static string WCellConstantsRoot = CoreDir + "WCell.Constants/";
		public static string WCellConstantsUpdates = WCellConstantsRoot + "Updates/";
		public static string ServicesRoot = WCellRoot + "Services/";
		public static string RealmServerRoot = ServicesRoot + "WCell.RealmServer/";
		public static string AuthServerRoot = ServicesRoot + "WCell.AuthServer/";
		public static string RunDir = WCellRoot + "Run/";
		public static string ContentDir = RunDir + "Content/";
		public static string MapDir = ContentDir + "Maps/";
		public static string WMODir = ContentDir + "Maps/";
		public static string M2Dir = ContentDir + "Maps/";
        public static string AddonDir = "RealmServerAddons/";
		public static string AddonSourceDir = WCellRoot + "Addons/";
		public static string DefaultAddonSourceDir = AddonSourceDir + "WCell.DefaultAddon/";
		public static string RealmServerRunDir = RunDir + "Debug/";
		public static string RealmServerDebugDir = RunDir + "Debug/";

		/// <summary>
		/// The location of WCell's executable in the Debug/ folder
		/// </summary>
		public static string WCellRealmServerConsoleExe = RealmServerDebugDir + "WCell.RealmServerConsole.exe";

		public static string LogFolder = WCellRoot + "../Logs/";
		public static string LogOutputFolder = WCellRoot + "../Logs/Converted/";
		public static string PAToolLogFile = LogFolder + "PATool.xml";
		public static string PAToolDefaultDir = LogFolder;
		public static string PAToolDefaultOutputFile = LogFolder + "PAToolOutput.txt";
	    
		public static string OldWoWFileLocation;

		public static ClientLocale Locale = ClientLocale.English;


		public string wowDir;
		public string GetWoWDir()
		{
			return wowDir ?? (wowDir = DBCTool.FindWowDir());
		}

		public string WoWFileLocation
		{
			get { return Path.Combine(GetWoWDir(), "wow.exe"); }
		}

		[XmlElement]
		public static string OutputDir = Path.GetFullPath(ToolsRoot + "output/");

		public PATool PATool;

		private static ToolConfig s_instance;

		public static ToolConfig Instance
		{
			get
			{
				if (s_instance == null)
				{
					DebugUtil.LoadDefinitions();
					if (File.Exists(ConfigFile))
					{
						log.Info("Loading ToolConfig from {0}...", Path.GetFullPath(ConfigFile));
						s_instance = Load(ConfigFile);
						if (s_instance.PATool != null)
						{
							return s_instance;
						}
					}
					else
					{
						s_instance = new ToolConfig {
							m_filename = ConfigFile
						};
					}
					s_instance.PATool = new PATool();
					s_instance.PATool.Init(s_instance,
										   PAToolLogFile, new DirectoryInfo(PAToolDefaultDir),
										   new FileStreamTarget(PAToolDefaultOutputFile),
										   LogConverter.GetParser(LogParserType.Sniffitzt));
					s_instance.Save();
				}
				return s_instance;
			}
		}

		public ToolConfig()
		{
		}

		protected override void OnLoad()
		{
			PATool._OnLoad(this);
		}
	}
}