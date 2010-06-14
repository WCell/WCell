//test
using System;
using Microsoft.Xna.Framework;
using TerrainDisplay.Extracted;
using TerrainDisplay.MPQ;
using TerrainDisplay.MPQ.ADT;
using TerrainDisplay.Recast;

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
            var defaultTileId = Config.DefaultTileIdentifier;
            var useExtractedData = Config.UseExtractedData;

			string mpqPath;
			if (useExtractedData)
			{
				TerrainManager = new ExtractedTerrain(Config.ExtractedDataPath, defaultTileId);
			}
			else
			{
				TerrainManager = new MpqTerrainManager(Config.MpqPath, defaultTileId);
			}

			TerrainManager.LoadTile(defaultTileId);

			AvatarPosition = new Vector3(TerrainConstants.CenterPoint - (defaultTileId.TileX + 1)*TerrainConstants.TileSize,
                                          TerrainConstants.CenterPoint - (defaultTileId.TileY + 1)*TerrainConstants.TileSize,
                                          100.0f);
			
			PositionUtil.TransformWoWCoordsToXNACoords(ref AvatarPosition);
            //using (var game = new Game1(_avatarPosition))
            //{
            //    game.Run();
            //}
            new RecastRunner(TerrainManager).Start();
        }
    }
}

