using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Core.Graphics;

namespace WCell.Core.World
{
	public class ZoneTileSet
	{
		public readonly ZoneGrid[,] ZoneGrids = new ZoneGrid[TerrainConstants.TilesPerMapSide, TerrainConstants.TilesPerMapSide];

		public ZoneId GetZoneId(ref Vector3 pos)
		{
			var x = (int)(pos.X / TerrainConstants.TileSize);
			var y = (int)(pos.Y / TerrainConstants.TileSize);

			if (x > TerrainConstants.TilesPerMapSide || y > TerrainConstants.TilesPerMapSide)
			{
				return ZoneId.None;
			}

			var grid = ZoneGrids[x, y];
			if (grid == null)
			{
				// hole in the grid
				return ZoneId.None;
			}

			x = (int)(pos.X / TerrainConstants.ChunkSize);
			y = (int)(pos.Y / TerrainConstants.ChunkSize);

			if (x < TerrainConstants.ChunksPerTileSide && y < TerrainConstants.ChunksPerTileSide)
			{
				return grid.ZoneIds[x, y];
			}

			//TODO: Finish
			return ZoneId.None;
		}
	}
}
