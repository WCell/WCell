//test
using System;
using Microsoft.Xna.Framework;
using MPQNav.MPQ;
using MPQNav.MPQ.ADT;
using TerrainDisplay.Extracted;
using TerrainDisplay.Recast;

namespace MPQNav
{
    public static class TerrainProgram
	{
		public static Vector3 _avatarPosition = new Vector3(-100, 100, -100);
		public static ITerrainManager _terrainManager;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
			var settingsReader = new System.Configuration.AppSettingsReader();

			var defaultContinent = (string)settingsReader.GetValue("defaultContinent", typeof(string));
			var mapId = (int)settingsReader.GetValue("mapId", typeof(int));		// unused
			var defaultMapX = (int)settingsReader.GetValue("defaultMapX", typeof(int));
			var defaultMapY = (int)settingsReader.GetValue("defaultMapY", typeof(int));
			var continent = (ContinentType)Enum.Parse(typeof(ContinentType), defaultContinent, true);
			var useExtractedData = (bool)settingsReader.GetValue("useExtractedData", typeof(bool));

			string mpqPath;
			if (useExtractedData)
			{
				mpqPath = (string)settingsReader.GetValue("extractedDataPath", typeof(string));
				_terrainManager = new ExtractedTerrain(mpqPath, mapId);
			}
			else
			{
				mpqPath = (string)settingsReader.GetValue("mpqPath", typeof(string));
				_terrainManager = new MpqTerrainManager(mpqPath, continent, mapId);
			}

			_terrainManager.LoadTile(defaultMapX, defaultMapY);

			//_terrainManager.ADTManager.LoadTile(defaultMapX - 1, defaultMapY);

			//_avatarPosition = _terrainManager.ADTManager.MapTiles[0].Vertices[0].Position;
			_avatarPosition = new Vector3(TerrainConstants.CenterPoint - (defaultMapX + 1) * TerrainConstants.TileSize, TerrainConstants.CenterPoint - (defaultMapY + 1) * TerrainConstants.TileSize, 100.0f);
			//_avatarPosition = new Vector3(0.0f);
			PositionUtil.TransformWoWCoordsToXNACoords(ref _avatarPosition);
			//using (var game = new Game1(_avatarPosition))
			//{
			//    game.Run();
			//}
			new RecastRunner(_terrainManager).Start();
        }
    }
}

