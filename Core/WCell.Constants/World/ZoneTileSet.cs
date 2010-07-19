using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.World;

namespace WCell.Constants.World
{
	public class ZoneTileSet
	{
		public readonly IZoneGrid[,] ZoneGrids;

		public ZoneTileSet()
		{
			ZoneGrids = new IZoneGrid[TerrainConstants.TilesPerMapSide, TerrainConstants.TilesPerMapSide];
		}

		public ZoneTileSet(IZoneGrid[,] grids)
		{
			ZoneGrids = grids;
		}

		public ZoneId GetZoneId(float x, float y)
		{
			x = TerrainConstants.TilesPerMapSide / 2 - x / TerrainConstants.TileSize;
			y = TerrainConstants.TilesPerMapSide / 2 - y / TerrainConstants.TileSize;

			if (x >= TerrainConstants.TilesPerMapSide || y >= TerrainConstants.TilesPerMapSide ||
				x < 0 || y < 0)
			{
				return ZoneId.None;
			}

			var grid = ZoneGrids[(int)y, (int)x];
			if (grid == null)
			{
				// hole in the terrain
				return ZoneId.None;
			}

			// take away the whole part and set the range from 0-0.999 to 0-(ChunksPerTileSide-1)
			x = (x - (int)x) * TerrainConstants.ChunksPerTileSide;
			y = (y - (int)y) * TerrainConstants.ChunksPerTileSide;

			if (x < TerrainConstants.ChunksPerTileSide && y < TerrainConstants.ChunksPerTileSide &&
				x >= 0 && y >= 0)
			{
				return grid.GetZoneId((int)y, (int)x);
			}

			//TODO: Finish
			return ZoneId.None;
		}
	}
}