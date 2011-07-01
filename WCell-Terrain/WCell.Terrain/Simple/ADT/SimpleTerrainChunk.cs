using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Terrain.MPQ;
using WCell.Terrain.MPQ.ADTs;
using WCell.Util.Graphics;

namespace WCell.Terrain.Simple.ADT
{
	/// <summary>
	/// Similar to <see cref="ADTChunk"/> but only with a subset of the data
	/// </summary>
	public class SimpleTerrainChunk : TerrainChunk
	{
		private int x, y;

		public List<int> M2References;
		public List<int> WMOReferences;

		//public bool HasHoles;
		//public bool[,] HolesMap;

		//public float[,] HeightMap;

		public MH2OFlags LiquidFlags;
		public bool IsLiquidFlat;

		public SimpleTerrainChunk(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public override int X
		{
			get { return x; }
		}

		public override int Y
		{
			get { return y; }
		}

		public override bool IsLiquid
		{
			get { throw new NotImplementedException(); }
		}

		public override float[,] LiquidHeights
		{
			get { throw new NotImplementedException(); }
		}

		public override RectInt32 LiquidBounds
		{
			get { throw new NotImplementedException(); }
		}

		public override float MedianHeight
		{
			get { throw new NotImplementedException(); }
		}

		public override FluidType LiquidType
		{
			get { throw new NotImplementedException(); }
		}

		public override bool[,] LiquidMap
		{
			get { throw new NotImplementedException(); }
		}

		public override ushort HolesMask
		{
			get { throw new NotImplementedException(); }
		}
	}
}
