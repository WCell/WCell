using System;

namespace WCell.Constants
{
	public static class TerrainConstants
	{
		/// <summary>
		/// The width/height of 1 of the 64x64 tiles that compose a full map
		/// </summary>
		public const float TileSize = (1600.0f / 3.0f);
		/// <summary>
		/// The width/height of 1 of the 16x16 chunks that compose a tile
		/// </summary>
		public const float ChunkSize = TileSize / 16.0f;
		/// <summary>
		/// The width/height of 1 of the 8x8 units that compose a chunk
		/// </summary>
		public const float UnitSize = ChunkSize / 8.0f;

		public const int TilesPerMapSide = 64;
		public const int ChunksPerTileSide = 16;
		public const int UnitsPerChunkSide = 8;

		/// <summary>
		/// The Center of a full 64x64 map
		/// </summary>
		public const float CenterPoint = (TilesPerMapSide / 2f) * TileSize;
		//public const float CenterPoint = 0;

		/// <summary>
		/// The highest possible Z component on a Map
		/// </summary>
		public const float MaxHeight = 2048;

		/// <summary>
		/// The length of a side of the 64x64 map
		/// </summary>
		public const float MapLength = ((TilesPerMapSide * ChunksPerTileSide) * ((150.0f / 36.0f) * UnitsPerChunkSide));

		/// <summary>
		/// The lowest X/Y value possible
		/// </summary>
		public const float MinPlain = -(TilesPerMapSide / 2) * TileSize;

		/// <summary>
		/// The highest X/Y value possible
		/// </summary>
		public const float MaxPlain = (TilesPerMapSide / 2f) * TileSize;

		public const string HeightMapFileExtension = "map";
		public const string HeightMapFileFormat = "{0:00}_{1:00}." + HeightMapFileExtension;
		public static string GetHeightMapFile(int x, int y)
		{
			return string.Format(HeightMapFileFormat, x, y);
		}

		public const string WMOFileExtension = "wmo";
		public const string WMOFileFormat = "{0:00}_{1:00}." + WMOFileExtension;
		public static string GetWMOFile(int x, int y)
		{
			return string.Format(WMOFileFormat, x, y);
		}

		public const string M2FileExtension = "m2x";
		public const string M2FileFormat = "{0:00}_{1:00}." + M2FileExtension;
		public static string GetM2File(int x, int y)
		{
			return string.Format(M2FileFormat, x, y);
		}

		public static string GetADTFile(string wdtName, int tileX, int tileY)
		{
			return string.Format("{0}_{1:00}_{2:00}", wdtName, tileX, tileY);
		}
	}
     
	public enum FluidType : byte
	{
		Water = 0x00,
		Lava,
		OceanWater,
        None = 0xFF
	}

	[Flags]
	public enum MH2OFlags
	{
		/// <summary>
		/// If this flag is set, skip the water heightmap. The offset to the heightmap will be nonzero, but skip it anyway
		/// </summary>
		Ocean = 0x2,
	}
}
