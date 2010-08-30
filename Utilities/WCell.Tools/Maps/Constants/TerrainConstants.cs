using WCell.Util.Graphics;

namespace WCell.Tools.Maps.Constants
{
    public static class TerrainConstants
    {
        /// <summary>
        /// The width/height of 1 of the 64x64 tiles that compose a full map
        /// ~533.333333333
        /// </summary>
        public const float TileSize = (1600.0f / 3.0f);
        /// <summary>
        /// The width/height of 1 of the 16x16 chunks that compose a tile
        /// ~33.33333333333
        /// </summary>
        public const float ChunkSize = TileSize / 16.0f;
        /// <summary>
        /// The width/height of 1 of the 8x8 units that compose a chunk
        /// ~4.1666666666667
        /// </summary>
        public const float UnitSize = ChunkSize / 8.0f;
        /// <summary>
        /// How many tiles wide is a full map
        /// </summary>
        public const int MapTileCount = 64;

        /// <summary>
        /// The middle of a full 64x64 map
        /// </summary>
        public const float CenterPoint = (MapTileCount / 2) * TileSize;

        /// <summary>
        /// The length of a side of the 64x64 map
        /// </summary>
        public const float MapLength = ((TilesPerMapSide*ChunksPerTileSide)*((150.0f/36.0f)*UnitsPerChunkSide));

        public const int TilesPerMapSide = 64;
        public const int ChunksPerTileSide = 16;
        public const int UnitsPerChunkSide = 8;

        public const float MinAllowedHeightDiff = 0.1f;
        public const float TerrainSimplificationConst = 0.005f;
        public const float H2OSimplificationConst = 0.005f;
        public const float WMOSimplificationConst = 0.0f;
        public const float M2SimplificationConst = 0.0f;

        public static void TilePositionToWorldPosition(ref Vector3 tilePosition)
        {
            tilePosition.X = (tilePosition.X - CenterPoint) * -1;
            tilePosition.Y = (tilePosition.Y - CenterPoint) * -1;
        }

        public static void TileExtentsToWorldExtents(ref BoundingBox tileExtents)
        {
            TilePositionToWorldPosition(ref tileExtents.Min);
            TilePositionToWorldPosition(ref tileExtents.Max);
        }

        public const string MapFileExtension = "map";
        public const string MapFilenameFormat = "{0:00}_{1:00}." + MapFileExtension;
        public static string GetMapFilename(int tileX, int tileY)
        {
            return string.Format(MapFilenameFormat, tileY, tileX);
        }

        public const string M2FileExtension = "m2x";
        public const string M2FileFormat = "{0:00}_{1:00}." + M2FileExtension;
        public static string GetM2File(int x, int y)
        {
            return string.Format(M2FileFormat, x, y);
        }

        public const string WMOFileExtension = "wmo";
        public const string WMOFileFormat = "{0:00}_{1:00}." + WMOFileExtension;
        public static string GetWMOFile(int x, int y)
        {
            return string.Format(WMOFileFormat, x, y);
        }
    }

    /// <summary>
    /// Enumeration of the different continents available.
    /// </summary>
    public enum ContinentType
    {
        Azeroth,
        Kalimdor,
        /// <summary>
        /// Outland, but this is the name the client uses for the files
        /// </summary>
        Expansion01,
        Northrend,
    }
}