using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerrainAnalysis.TerrainObjects
{
    public static class TerrainConstants
    {
        public struct RegionBounds
        {
            public static int TilesPerRow = 64;
            public static int TilesPerColumn = 64;
        }

        public struct Tile
        {
            public static float TileSize = 533.3333f;
            public static int ChunksPerRow = 16;
            public static int ChunksPerColumn = 16;
        }

        public struct HighResChunk
        {
            public static float ChunkSize = Tile.TileSize / 16.0f;
            public static int VertexRows = 17;
            public static int VertexColums = 9;
            public int ColumnsThisRow(int row) { return (row % 2 == 0 ? 9 : 8); }
            public static float UnitSizeX = ChunkSize / (float)(VertexColums-1);
            public static float UnitSizeZ = ChunkSize / (float)(VertexRows-1);
        }

        public struct LowResChunk
        {
            public static float ChunkSize = Tile.TileSize / 16.0f;
            public static int VertexRows = 9;
            public static int VertexColums = 9;
            public static float UnitSizeX = ChunkSize / (float)(VertexColums-1);
            public static float UnitSizeZ = ChunkSize / (float)(VertexRows-1);
        }
    }
}
