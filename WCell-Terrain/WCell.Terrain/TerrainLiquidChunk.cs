using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;

namespace WCell.Terrain
{
	/// <summary>
	/// Liquid information of a single chunk within a tile
	/// </summary>
	public class TerrainLiquidChunk
	{
		/// <summary>
		/// The Liquid type of this chunk
		/// </summary>
		public LiquidType Type;

		public int OffsetX, OffsetY, Width, Height;

		/// <summary>
		/// The Liquid heights per unit
		/// </summary>
		public float[,] Heights;
	}
}
