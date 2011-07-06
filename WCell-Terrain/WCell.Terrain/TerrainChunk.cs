using WCell.Constants;
using WCell.Terrain.MPQ.ADTs;
using WCell.Util.Graphics;

namespace WCell.Terrain
{
	public abstract class TerrainChunk
	{
		/// <summary>
		/// MCTV Chunk (Height values for the MCNK)
		/// </summary>
		public MapVertices Heights = new MapVertices();

		/// <summary>
		/// MCNR Chunk (Normals for the MCNK)
		/// </summary>
		public MapNormals Normals = new MapNormals();

		public TerrainChunk()
		{
		}

		public abstract bool HasLiquid { get; }

		public abstract float[,] LiquidHeights { get; }

		public abstract LiquidType LiquidType { get; }

		public abstract bool[,] LiquidMap { get; }

		public abstract float MedianHeight { get; }

		public abstract ushort HolesMask { get; }


		//internal void PrepareWaterInfo()
		//{
		//    if (!IsLiquid) return;

		//    var min = float.MaxValue;
		//    var max = float.MinValue;
		//    foreach (var height in LiquidHeights)
		//    {
		//        min = Math.Min(height, min);
		//        max = Math.Max(height, max);
		//    }
		//    IsFlatLiquid = (Math.Abs(max - min) < 1.0f);
		//}
	}
}
