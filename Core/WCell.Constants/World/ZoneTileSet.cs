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
		    x = ((TerrainConstants.CenterPoint - x)/TerrainConstants.TileSize);
		    y = ((TerrainConstants.CenterPoint - y)/TerrainConstants.TileSize);

		    var tileX = (int) x;
		    var tileY = (int) y;
			if (tileX >= TerrainConstants.TilesPerMapSide || tileY >= TerrainConstants.TilesPerMapSide ||
				tileX < 0 || tileY < 0)
			{
				return ZoneId.None;
			}

            // ZoneGrids are per tile
			var grid = ZoneGrids[tileY, tileX];
			if (grid == null)
			{
				// hole in the terrain
				return ZoneId.None;
			}

			// take away the whole part and set the range from 0-0.999 to 0-(ChunksPerTileSide-1)
		    x = (x - tileX)*TerrainConstants.ChunksPerTileSide;
		    y = (y - tileY)*TerrainConstants.ChunksPerTileSide;

		    var chunkX = (int) x;
		    var chunkY = (int) y;
		    if (chunkX >= TerrainConstants.ChunksPerTileSide || chunkY >= TerrainConstants.ChunksPerTileSide || 
                chunkX < 0 || chunkY < 0)
		    {
		        return ZoneId.None;
		    }

		    //TODO: Finish
		    return grid.GetZoneId(chunkY, chunkX);
		}
	}
}