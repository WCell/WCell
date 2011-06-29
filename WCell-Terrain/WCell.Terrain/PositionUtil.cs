using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using TerrainDisplay;
using WCell.Util.Graphics;

namespace WCell.Terrain
{
	public static class PositionUtil
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public static void TransformWoWCoordsToRecastCoords(ref Vector3 vertex)
		{
			var temp = vertex.X;
			vertex.X = vertex.Y * -1;
			vertex.Y = vertex.Z;
			vertex.Z = temp;
		}

		internal static void TransformRecastCoordsToWoWCoords(ref Vector3 vertex)
		{
			var temp = vertex.Z;
			vertex.Z = vertex.Y;
			vertex.Y = temp * -1;
			//vertex.X = temp;
		}

		/// <summary>
		/// Calculates which Tile the given position belongs to on a Map.
		/// </summary>
		/// <param name="worldPos">Calculate the Tile coords for this position.</param>
		/// <param name="tileX">Set to the X coordinate of the tile.</param>
		/// <param name="tileY">Set to the Y coordinate of the tile.</param>
		/// <returns>True if the tile (X, Y) is valid.</returns>
		internal static bool GetTileXYForPos(Vector3 worldPos, out int tileX, out int tileY)
		{
			tileX = (int)GetTileFraction(worldPos.Y);
			tileY = (int)GetTileFraction(worldPos.X);

			return VerifyPoint2D(worldPos, tileX, tileY);
		}

		internal static void GetXYForPos(Vector3 worldPos, out int chunkX, out int chunkY)
		{
			var tileFractionX = GetTileFraction(worldPos.X);
			var tileFractionY = GetTileFraction(worldPos.Y);

			chunkX = (int)GetChunkFraction(tileFractionX);
			chunkY = (int)GetChunkFraction(tileFractionY);
		}

		internal static Rect GetTileBoundingRect(TileIdentifier tileId)
		{
			var tileX = tileId.X;
			var tileY = tileId.Y;
			var x = TerrainConstants.CenterPoint - (tileX * TerrainConstants.TileSize);
			var botX = x - TerrainConstants.TileSize;
			var y = TerrainConstants.CenterPoint - (tileY * TerrainConstants.TileSize);
			var botY = y - TerrainConstants.TileSize;

			return new Rect(new Point(x, y), new Point(TerrainConstants.TileSize, TerrainConstants.TileSize));
		}

		private static float GetTileFraction(float loc)
		{
			return ((TerrainConstants.CenterPoint - loc) / TerrainConstants.TileSize);
		}

		private static float GetChunkFraction(float tileFraction)
		{
			return (tileFraction - (int)tileFraction) * TerrainConstants.ChunksPerTileSide;
		}

		private static bool VerifyPoint2D(Vector3 worldPos, int tileX, int tileY)
		{
			var result = true;
			if (tileX < 0)
			{
				log.Error(String.Format("WorldPos: {0} is off the map. tileX < 0.", worldPos));
				result = false;
			}
			if (tileX >= TerrainConstants.TilesPerMapSide)
			{
				log.Error(String.Format("WorldPos: {0} is off the map. tileX >= {1}.", worldPos,
										TerrainConstants.TilesPerMapSide));
				result = false;
			}
			if (tileY < 0)
			{
				log.Error(String.Format("WorldPos: {0} is off the map. tileY < 0.", worldPos));
				result = false;
			}
			if (tileX >= TerrainConstants.TilesPerMapSide)
			{
				log.Error(String.Format("WorldPos: {0} is off the map. tileX >= {1}.", worldPos,
										TerrainConstants.TilesPerMapSide));
				result = false;
			}
			return result;
		}
	}
}
