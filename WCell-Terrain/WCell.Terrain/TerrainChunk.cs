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

		public bool IsFlat;

		private Rect _bounds = Rect.Empty;
		private int _nodeId = -1;

		private bool[,] holesMap;

		public bool[,] HolesMap
		{
			get
			{
				if (holesMap == null)
				{
					holesMap = new bool[4, 4];
					for (var i = 0; i < 16; i++)
					{
						holesMap[i / 4, i % 4] = (((HolesMask >> (i)) & 1) == 1);
					}
				}
				return holesMap;
			}
		}

		public TerrainChunk()
		{
		}


		public Rect Bounds
		{
			get { return _bounds; }
			set { _bounds = value; }
		}

		public int NodeId
		{
			get { return _nodeId; }
			set { _nodeId = value; }
		}

		public abstract int X { get; }

		public abstract int Y { get; }

		public abstract bool IsLiquid { get; }

		public abstract float[,] LiquidHeights { get; }

		public abstract FluidType LiquidType { get; }

		public abstract bool[,] LiquidMap { get; }

		public abstract RectInt32 LiquidBounds { get; }

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
