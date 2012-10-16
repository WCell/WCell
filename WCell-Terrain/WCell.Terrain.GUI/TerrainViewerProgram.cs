//test
using System;
using System.Collections.Generic;
using System.IO;
using WCell.Constants;
using WCell.Constants.World;

using WCell.MPQTool.StormLibWrapper;
using WCell.Terrain.GUI.Util;
using WCell.Terrain.MPQ;
using WCell.Terrain.Recast;
using WCell.Terrain.Serialization;
using WCell.Util.Graphics;
using WCell.Util.NLog;

namespace WCell.Terrain.GUI
{
	public static class TerrainViewerProgram
	{
	    public static Vector3 AvatarPosition = new Vector3(-100, 100, -100);

		/// <summary>
		/// Parallel loading is still experimental
		/// </summary>
		public static bool UseMultiThreadedLoading = false;

		private static World world;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			// parse arguments
			if (args.Length > 0)
			{
				int num;
				if (!int.TryParse(args[0], out num))
				{
					throw new Exception("Invalid argument for ParallelLoading: " + args[0]);
				}
				UseMultiThreadedLoading = num != 0;
			}

			// initialize config & logging
			TerrainGUIConfig.Initialize();
			LogUtil.SetupConsoleLogging();

			// initialize StormLib
			NativeMethods.StormLibFolder = WCellTerrainSettings.LibDir;
			NativeMethods.InitAPI();

			var defaultTileId = TileIdentifier.DefaultTileIdentifier;
		    
            world = new World();

			LoadInitialData(defaultTileId);


			Console.WriteLine("All data has been loaded - Starting GUI...");

			//new RecastRunner(TerrainManager).Start();
			StartDefaultViewer(world, defaultTileId);
		}

		public static List<Vector3[]> v = new List<Vector3[]>();
		private static void LoadInitialData(TileIdentifier defaultTileId)
		{
			var terrain = new SimpleWDTTerrain(defaultTileId.MapId, false);
			world.WorldTerrain.Add(defaultTileId.MapId, terrain);

			terrain.GetOrCreateTile(defaultTileId.MapId, defaultTileId.X, defaultTileId.Y);

			AvatarPosition = new Vector3(TerrainConstants.CenterPoint - (defaultTileId.X + 1) * TerrainConstants.TileSize,
										 TerrainConstants.CenterPoint - (defaultTileId.Y) * TerrainConstants.TileSize,
										 100.0f);
		}


		private static void StartDefaultViewer(World world, TileIdentifier tileId)
		{
			using (var game = new TerrainViewer(AvatarPosition.ToXna(), world, tileId))
			{
				game.Run();
			}
		}
	}
}

