using System;
using WCell.Constants.World;
using WCell.Tools.Maps.Parsing.WDT;
using WCell.Util.Graphics;
using WCell.Constants;

namespace WCell.Tools.Maps
{
	public static class RegionBoundaryUtil
	{
		public static readonly BoundingBox[] Boundaries = new BoundingBox[(int)MapId.End];

		public static BoundingBox[] ExportBoundaries()
		{
			WDTParser.EnsureEventEmpty();
			WDTParser.Parsed += GetBoundaries;
			WDTParser.ProcessAll();
			return Boundaries;
		}

		public static void GetBoundaries(WDTFile wdt)
		{
			Vector3 min;
			Vector3 max;

			if ((wdt.Header.Header1 & WDTFlags.GlobalWMO) != 0)
			{
				const float maxDim = TerrainConstants.TileSize * (32.0f);

				// No terrain, get the boundaries from the MODF
				min = wdt.WmoDefinitions[0].Extents.Min;
				max = wdt.WmoDefinitions[0].Extents.Max;

				var diff = Math.Max(max.X - min.X, max.Y - min.Y);
				if ((min.X + diff) > maxDim)
				{
					var newdiff = maxDim - diff;
					if ((min.X - newdiff) < -maxDim)
					{
						throw new Exception("Can't square region: " + wdt.Name);
					}
					min.X = min.X - newdiff;
				}

				if ((min.Y + diff) > maxDim)
				{
					var newdiff = maxDim - diff;
					if ((min.Y - newdiff) < -maxDim)
					{
						throw new Exception("Can't square region: " + wdt.Name);
					}
					min.Y = min.Y - newdiff;
				}

				max.X = min.X + diff;
				max.Y = min.Y + diff;
			}
			else
			{
				var minX = TerrainConstants.TilesPerMapSide;
				var maxX = 0;
				var minY = TerrainConstants.TilesPerMapSide;
				var maxY = 0;
				for (var y = 0; y < TerrainConstants.TilesPerMapSide; y++)
				{
					for (var x = 0; x < TerrainConstants.TilesPerMapSide; x++)
					{
						if (!wdt.TileProfile[x, y]) continue;
						minX = Math.Min(minX, x);
						maxX = Math.Max(maxX, x);
						minY = Math.Min(minY, y);
						maxY = Math.Max(maxY, y);
					}
				}

				var diff = Math.Max(maxX - minX, maxY - minY);
				if ((minX + diff) > TerrainConstants.TilesPerMapSide)
				{
					var newdiff = TerrainConstants.TilesPerMapSide - diff;
					if ((minX - newdiff) < 0)
					{
						throw new Exception("Can't square this region.");
					}
					minX = minX - newdiff;
				}

				if ((minY + diff) > TerrainConstants.TilesPerMapSide)
				{
					var newdiff = TerrainConstants.TilesPerMapSide - diff;
					if ((minY - newdiff) < 0)
					{
						throw new Exception("Can't square this region.");
					}
					minY = minY - newdiff;
				}

				maxX = minX + diff;
				maxY = minY + diff;

				var maxXLoc = TerrainConstants.TileSize * (32 - minX);
				var maxYLoc = TerrainConstants.TileSize * (32 - minY);
				var minXLoc = TerrainConstants.TileSize * (31 - maxX);
				var minYLoc = TerrainConstants.TileSize * (31 - maxY);

				//var maxXLoc = TerrainConstants.TileSize * (maxX - 32);
				//var maxYLoc = TerrainConstants.TileSize * (maxY - 32);
				//var minXLoc = TerrainConstants.TileSize * (minX - 32);
				//var minYLoc = TerrainConstants.TileSize * (minY - 32);

				min = new Vector3(minXLoc, minYLoc, -2048.0f);
				max = new Vector3(maxXLoc, maxYLoc, 2048.0f);

				// Convert to in-game coordinates
				var temp = min.X;
				min.X = min.Y;
				min.Y = temp;

				temp = max.X;
				max.X = max.Y;
				max.Y = temp;
			}

			// TODO: Region bounds that in the file are a little off
			min.X -= 200;
			min.Y -= 200;
			max.X += 200;
			max.Y += 200;

			//text.WriteLine(dbcMapEntry.Id + "\t" + wdtName + "\tMin: " + min + "\tMax: " + max);
			Boundaries[wdt.Entry.Id] = new BoundingBox(min, max);
		}
	}
}