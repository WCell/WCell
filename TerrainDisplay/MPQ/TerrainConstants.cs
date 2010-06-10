namespace MPQNav.MPQ
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

        public const float TerrainSimplificationConst = 0.005f;
        public const float H2OSimplificationConst = 0.005f;
        public const float WMOSimplificationConst = 0.0f;
        public const float M2SimplificationConst = 0.0f;
    }
}
