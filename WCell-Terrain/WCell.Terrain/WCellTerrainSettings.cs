using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
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
	/// 
	/// </summary>
	public static class WCellTerrainSettings
	{
		private static ITerrainConfiguration config;

		public static ITerrainConfiguration Config
		{
			get { return config; }
			set
			{
				config = value;

				LibDir = RootFolder + "Lib/";
				RunDir = RootFolder + "Run/";
				ContentDir = RunDir + "Content/";
				WMODir = ContentDir + "Maps/";
				M2Dir = ContentDir + "Maps/";
				LogFolder = ContentDir + "Logs/";
				MapDir = ContentDir + "Maps/";
				DBCDir = ContentDir + "dbc/";
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

		public static string LibDir;
		public static string RunDir;
		public static string ContentDir;
		public static string WMODir;
		public static string M2Dir;
		public static string LogFolder;
		public static string MapDir;
		public static string DBCDir;

		public static string MapDBCName = "Map.dbc";
		public static string WoWPath = @"D:\Games\WoW";

		public static ClientLocale DefaultLocale = ClientLocale.English;

		public static Encoding DefaultEncoding = Encoding.UTF8;
	}
}
