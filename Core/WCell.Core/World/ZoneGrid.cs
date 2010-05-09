using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.World;

namespace WCell.Core.World
{
	public class ZoneGrid
	{
		/// <summary>
		/// 
		/// </summary>
		public readonly ZoneId[,] ZoneIds = new ZoneId[TerrainConstants.ChunksPerTileSide, TerrainConstants.ChunksPerTileSide];
	}
}
