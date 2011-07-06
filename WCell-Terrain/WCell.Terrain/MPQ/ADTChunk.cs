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

		public bool IsFlat;

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

		public override bool HasLiquid
		{
			get { return WaterInfo != null && WaterInfo.Header.Used && LiquidType != Constants.LiquidType.None; }
		}

		public override float[,] LiquidHeights
		{
			get { return WaterInfo.Heights; }
		}

		public override bool[,] LiquidMap
		{
			get { return WaterInfo.GetRenderBitMapMatrix(); }
		}

		public override float MedianHeight
		{
			get { return Header.Z; }
		}

		public override LiquidType LiquidType
		{
			get { return WaterInfo.Header.Type; }
		}


		public override ushort HolesMask
		{
			get { return Header.Holes; }
		}
	}
}
