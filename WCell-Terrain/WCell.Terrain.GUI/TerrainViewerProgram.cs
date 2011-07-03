//test
using System;
using WCell.Constants;
using WCell.Terrain.GUI.Util;
using WCell.MPQTool.StormLibWrapper;
using WCell.Terrain.MPQ;
using WCell.Terrain.Recast;
using WCell.Terrain.Serialization;
using WCell.Util.Graphics;
using WCell.Util.NLog;

using Terrain = WCell.Terrain.Terrain;

namespace WCell.Terrain.GUI
{
	public static class TerrainViewerProgram
	{
	    public static Vector3 AvatarPosition = new Vector3(-100, 100, -100);

		/// <summary>
		/// Parallel loading is still experimental
		/// </summary>
		public static bool UseMultiThreadedLoading = false;

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
			new TerrainGUIConfig();
			TerrainGUIConfig.Initialize();
			LogUtil.SetupConsoleLogging();
			WCellTerrainSettings.Config = TerrainGUIConfig.Instance;

			// initialize StormLib
            NativeMethods.StormLibFolder = WCellTerrainSettings.LibDir;
		    NativeMethods.InitAPI();

            var defaultTileId = TileIdentifier.DefaultTileIdentifier;
			var useExtractedData = TerrainGUIConfig.UseExtractedData;


			var start = DateTime.Now;
			Terrain terrain;
			TerrainTile tile;

			Console.Write("Loading default tile...");
			if (useExtractedData)
			{
				throw new NotImplementedException("Extracted data reader is currently under re-construction");
			}
			else
			{
				tile = WDT.LoadTile(defaultTileId.MapId, defaultTileId);
			}
			Console.WriteLine("Done - Loading time: {0:0.000}s", (DateTime.Now - start).TotalSeconds);

			if (tile == null)
			{
				throw new ArgumentException(string.Format(
								"Could not read tile information - Map: {0} at ({1}, {2}) " +
									defaultTileId.MapId,
									defaultTileId.X,
									defaultTileId.Y));
			}
			terrain = tile.Terrain;

			var navMesh = terrain.GetOrCreateNavMesh(tile);

			AvatarPosition = new Vector3(TerrainConstants.CenterPoint - (defaultTileId.X + 1)*TerrainConstants.TileSize,
										  TerrainConstants.CenterPoint - (defaultTileId.Y)*TerrainConstants.TileSize,
										  100.0f);
			
			XNAUtil.TransformWoWCoordsToXNACoords(ref AvatarPosition);

			//new RecastRunner(TerrainManager).Start();
			StartDefaultViewer(tile);
		}

		private static void StartDefaultViewer(TerrainTile tile)
		{
			using (var game = new TerrainViewer(AvatarPosition.ToXna(), tile))
			{
				game.Run();
			}
		}
	}
}

