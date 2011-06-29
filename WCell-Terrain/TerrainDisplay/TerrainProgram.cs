//test
using System;
using TerrainDisplay.Collision;
using TerrainDisplay.Config;
using TerrainDisplay.Extracted;
using TerrainDisplay.Util;
using WCell.Constants;
using WCell.Terrain;
using WCell.Terrain.MPQ;
using WCell.Terrain.MPQ.ADT;
using TerrainDisplay.Recast;
using WCell.MPQTool.StormLibWrapper;
using WCell.Util.Graphics;
using WCell.Util.NLog;

namespace TerrainDisplay
{
	public static class TerrainProgram
	{
	    public static Vector3 AvatarPosition = new Vector3(-100, 100, -100);
		public static ITerrainManager TerrainManager;
		public static SelectedTriangleManager SelectedTriangleManager;

		/// <summary>
		/// Parallel loading is still experimental
		/// </summary>
		public static bool UseMultiThreadedLoading = false;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			if (args.Length > 0)
			{
				int num;
				if (!int.TryParse(args[0], out num))
				{
					throw new Exception("Invalid argument for ParallelLoading: " + args[0]);
				}
				UseMultiThreadedLoading = num != 0;
			}

			TerrainDisplayConfig.Initialize();
			LogUtil.SetupConsoleLogging();


			WCellTerrainSettings.Config = TerrainDisplayConfig.Instance;

            NativeMethods.StormLibFolder = WCellTerrainSettings.LibDir;
		    NativeMethods.InitAPI();

            var defaultTileId = TileIdentifier.DefaultTileIdentifier;
			var useExtractedData = TerrainDisplayConfig.UseExtractedData;

			if (useExtractedData)
			{
				TerrainManager = new ExtractedTerrainManager(WCellTerrainSettings.MapDir, defaultTileId);
			}
			else
			{
				TerrainManager = new MpqTerrainManager(defaultTileId);
			}
			SelectedTriangleManager = new SelectedTriangleManager(TerrainManager.ADTManager);

			var start = DateTime.Now;
			Console.Write("Loading default tile...");
			TerrainManager.LoadTile(defaultTileId);
			Console.WriteLine("Done - Loading time: {0:0.000}s", (DateTime.Now - start).TotalSeconds);

			AvatarPosition = new Vector3(TerrainConstants.CenterPoint - (defaultTileId.TileX + 1)*TerrainConstants.TileSize,
										  TerrainConstants.CenterPoint - (defaultTileId.TileY)*TerrainConstants.TileSize,
										  100.0f);
			
			XNAUtil.TransformWoWCoordsToXNACoords(ref AvatarPosition);

			//new RecastRunner(TerrainManager).Start();
			StartDefaultViewer();
		}

		private static void StartDefaultViewer()
		{
			using (var game = new TerrainRenderWindow(AvatarPosition.ToXna()))
			{
				game.Run();
			}
		}

		static TerrainProgram()
        {
            new TerrainDisplayConfig();
        }
	}
}

