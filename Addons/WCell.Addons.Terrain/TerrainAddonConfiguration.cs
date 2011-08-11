using System;
using System.IO;
using NLog;
using WCell.Terrain;
using WCell.Util.NLog;
using WCell.Util.Variables;

//using WCell.MPQTool;

namespace WCell.Addons.Terrain
{
	public class TerrainAddonConfiguration : TerrainConfiguration<TerrainAddonConfiguration>
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();
		public static readonly string ConfigFileName = "WCell.Addons.Terrain.Config.xml";

		public static readonly string WCellRoot = "../../";

		public override string RootFolder
		{
			get { return WCellRoot; }
		}

		public TerrainAddonConfiguration()
		{
			ErrorHandler = OnError;
			RootNodeName = "TerrainAddonConfiguration";
		}

		[NotVariable]
		public static bool Loaded { get; private set; }

		public override bool Load()
		{
			if (!Loaded)
			{
				Loaded = true;
				FilePath = Path.Combine(CollisionAddon.Instance.Context.File.DirectoryName,
					"TerrainAddonConfig.xml");
				AutoSave = true;
				AddVariablesOfAsm<VariableAttribute>(typeof(TerrainAddonConfiguration).Assembly);

				try
				{
					if (!base.Load())
					{
						Save(true, false);
						log.Warn("Config-file \"{0}\" not found - Created new file.", Instance.FilePath);
						log.Warn("Please take a little time to configure your server and then restart the Application.");
						log.Warn("See http://wiki.wcell.org/index.php/Configuration for more information.");
						return false;
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
	}
}
