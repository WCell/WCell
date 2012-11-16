using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.MPQTool;
using WCell.Terrain.Recast;
using WCell.Util.Variables;

namespace WCell.Terrain
{
	public interface ITerrainConfiguration
	{
		string RootFolder { get; }
	}

	public abstract class TerrainConfiguration<C> : VariableConfiguration<C, TypeVariableDefinition>, ITerrainConfiguration
		where C : VariableConfiguration<TypeVariableDefinition>, ITerrainConfiguration, new()
	{
		public readonly static C Instance = new C();

		public abstract string RootFolder { get; }
	}

	/// <summary>
	/// TODO: Make this a separate config
	/// </summary>
	public static class WCellTerrainSettings
	{
		private static ITerrainConfiguration config;

		public static MpqLibrarian GetDefaultMPQFinder()
		{
			return MpqLibrarian.GetDefaultFinder(WoWPath);
		}

		public static ITerrainConfiguration Config
		{
			get { return config; }
			set
			{
				config = value;

				WCellTerrainDir = RootFolder + "WCell-Terrain/";
				LibDir = WCellTerrainDir + "Lib/";
				RunDir = RootFolder + "Run/";
				ContentDir = RunDir + "Content/";
				LogFolder = ContentDir + "Logs/";
				DBCDir = ContentDir + "dbc3.3.5/";
				MapDir = ContentDir + "Maps/";

				SimpleMapDir = MapDir + "Simple/";

				RecastInputMeshFolder = MapDir + "RecastInput/";
				RecastNavMeshFolder = MapDir + "RecastNavMeshes/";

                WoWPath = ContentDir;
			}
		}

		public static string RootFolder
		{
			get
			{
				return Config.RootFolder;
			}
		}

		/// <summary>
		/// Experimental setting to allow multi-threaded loading (doesn't quite work yet)
		/// </summary>
		public static bool UseMultiThreadedLoading { get; set; }

		public static string WCellTerrainDir;
		public static string LibDir;
		public static string RunDir;
		public static string ContentDir;
		public static string WMODir;
		public static string M2Dir;
		public static string LogFolder;
		public static string MapDir;
		public static string RawMapDir;
		public static string SimpleMapDir;
		public static string DBCDir;

		public static string RecastInputMeshFolder;
		public static string RecastNavMeshFolder;

		public static string MapDBCName = "Map.dbc";
		public static string WoWPath;

		public static ClientLocale DefaultLocale = ClientLocale.English;

		public static Encoding DefaultEncoding = Encoding.UTF8;
	}
}
