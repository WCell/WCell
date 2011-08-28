using System.IO;
using System.Xml.Serialization;
using NLog;
using WCell.Constants;
using WCell.MPQTool;
using WCell.PacketAnalysis.Logs;
using WCell.RealmServer.Debugging;
using WCell.Tools.PATools;
using WCell.Util;
using WCell.Util.Variables;

namespace WCell.Tools
{
	public class ToolConfig : XmlFile<ToolConfig>
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		[NotVariable]
		public static string ToolsRoot = Path.GetFullPath("../../Utilities/WCell.Tools/");
		public static readonly string ConfigFileName = "Tools.xml";

		public static string WCellRoot { get { return ToolsRoot + "../../"; } }
		public static string CoreDir { get { return WCellRoot + "Core/"; } }
		public static string WCellCoreRoot { get { return CoreDir + "WCell.Core/"; } }
		public static string WCellConstantsRoot { get { return CoreDir + "WCell.Constants/"; } }
		public static string WCellConstantsUpdates { get { return WCellConstantsRoot + "Updates/"; } }
		public static string ServicesRoot { get { return WCellRoot + "Services/"; } }
		public static string RealmServerRoot { get { return ServicesRoot + "WCell.RealmServer/"; } }
		public static string AuthServerRoot { get { return ServicesRoot + "WCell.AuthServer/"; } }
		public static string RunDir { get { return WCellRoot + "Run/"; } }
		public static string ContentDir { get { return RunDir + "Content/"; } }
		public static string MapDir { get { return ContentDir + "Maps/"; } }
		public static string WMODir { get { return ContentDir + "Maps/"; } }
		public static string M2Dir { get { return ContentDir + "Maps/"; } }

		public static string RealmServerRunDir { get { return RunDir + "Debug/"; } }
		public static string RealmServerDebugDir { get { return RunDir + "Debug/"; } }
		/// <summary>
		/// The location of WCell's executable in the Debug/ folder
		/// </summary>
		public static string WCellRealmServerConsoleExe { get { return  RealmServerDebugDir + "WCell.RealmServerConsole.exe"; } }

		public static string AddonDir { get { return "RealmServerAddons/"; } }
		public static string AddonSourceDir { get { return WCellRoot + "Addons/"; } }
		public static string DefaultAddonSourceDir { get { return AddonSourceDir + "WCell.DefaultAddon/"; } }

		public static string LogFolder { get { return  WCellRoot + "../Logs/"; } }
		public static string LogOutputFolder { get { return  WCellRoot + "../Logs/Converted/"; } }
		public static string PAToolLogFile { get { return  LogFolder + "PATool.xml"; } }
		public static string PAToolDefaultDir { get { return  LogFolder; } }
		public static string PAToolDefaultOutputFile { get { return  LogFolder + "PAToolOutput.txt"; } }

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

		public static void InitCfg()
		{
			if (s_instance == null)
			{
				var file = ToolsRoot + ConfigFileName;
				if (File.Exists(file))
				{
					log.Info("Loading ToolConfig from {0}...", Path.GetFullPath(ConfigFileName));
					s_instance = Load(file);
					InitPhase2();

					if (s_instance.PATool != null)
					{
						return;
					}
				}
				else
				{
					InitPhase2();
					s_instance = new ToolConfig
					{
						m_filename = file
					};
				}

				s_instance.PATool = new PATool();
				s_instance.PATool.Init(s_instance,
									   PAToolLogFile, new DirectoryInfo(PAToolDefaultDir),
									   new FileStreamTarget(PAToolDefaultOutputFile),
									   LogConverter.GetParser(LogParserType.Sniffitzt));
				s_instance.Save();
			}
		}

		private static void InitPhase2()
		{
			DebugUtil.LoadDefinitions();
		}

		public static ToolConfig Instance
		{
			get
			{
				InitCfg();
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