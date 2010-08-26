//test
using System;
using TerrainDisplay.Extracted;
using TerrainDisplay.MPQ;
using TerrainDisplay.MPQ.ADT;
using TerrainDisplay.Recast;
using TerrainDisplay.Util;
using WCell.Util.Graphics;

namespace TerrainDisplay
{
	public static class TerrainProgram
	{
	    public static Vector3 AvatarPosition = new Vector3(-100, 100, -100);
		public static ITerrainManager TerrainManager;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
            TerrainDisplayConfig.Initialize();
            
            var defaultTileId = TileIdentifier.DefaultTileIdentifier;
			var useExtractedData = TerrainDisplayConfig.UseExtractedData;

			if (useExtractedData)
			{
			    TerrainManager = new ExtractedTerrainManager(TerrainDisplayConfig.ExtractedDataPath, defaultTileId);
			}
			else
			{
				TerrainManager = new MpqTerrainManager(defaultTileId);
			}

			TerrainManager.LoadTile(defaultTileId);

			AvatarPosition = new Vector3(TerrainConstants.CenterPoint - (defaultTileId.TileX + 1)*TerrainConstants.TileSize,
										  TerrainConstants.CenterPoint - (defaultTileId.TileY + 1)*TerrainConstants.TileSize,
										  100.0f);
			
			PositionUtil.TransformWoWCoordsToXNACoords(ref AvatarPosition);
			//new RecastRunner(TerrainManager).Start();

			using (var game = new Game1(AvatarPosition.ToXna()))
			{
				game.Run();
			}
		}

        static TerrainProgram()
        {
            new TerrainDisplayConfig();
            new TileIdentifier();
        }
	}
}

