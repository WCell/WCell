using System.Collections.Generic;
using WCell.Constants;
using WCell.Terrain.MPQ.ADTs;
using WCell.Util.Graphics;

namespace WCell.Terrain.MPQ
{
	public class ADTChunk : TerrainChunk
	{
		public MCNK Header = new MCNK();

		public MH2O WaterInfo;

		/// <summary>
		/// MCRF - (Header.nDoodadRefs + Header.nMapObjRefs) indices into the MDDF and MODF
		/// chunks that tell which M2's and WMO's (respectively) are drawn over this chunk.
		/// When a player is on a chunk, only those objects referenced are checked for collision.
		/// </summary>
		public List<int> DoodadRefs = new List<int>();
		public List<int> ObjectRefs = new List<int>();

		public override int X
		{
			get { return Header.IndexX; }
		}

		public override int Y
		{
			get { return Header.IndexY; }
		}

		public override bool IsLiquid
		{
			get { return WaterInfo != null && WaterInfo.Header.Used; }
		}

		public override float[,] LiquidHeights
		{
			get { return WaterInfo.Heights; }
		}

		public override bool[,] LiquidMap
		{
			get { return WaterInfo.GetRenderBitMapMatrix(); }
		}

		public override RectInt32 LiquidBounds
		{
			get { return new RectInt32(WaterInfo.Header.XOffset, WaterInfo.Header.YOffset, 
									WaterInfo.Header.Width, WaterInfo.Header.Height); }
		}

		public override float MedianHeight
		{
			get { return Header.Z; }
		}

		public override FluidType LiquidType
		{
			get { return WaterInfo.Header.Type; }
		}


		public override ushort HolesMask
		{
			get { return Header.Holes; }
		}
	}
}
