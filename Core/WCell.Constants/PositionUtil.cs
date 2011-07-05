using System;
using System.IO;
using WCell.Util.Graphics;

namespace WCell.Constants
{
	public static class PositionUtil
	{
		/// <summary>
		/// Calculates which Tile the given position belongs to on a Map.
		/// </summary>
		/// <param name="worldPos">Calculate the Tile coords for this position.</param>
		/// <param name="tileX">Set to the X coordinate of the tile.</param>
		/// <param name="tileY">Set to the Y coordinate of the tile.</param>
		/// <returns>True if the tile (X, Y) is valid.</returns>
		public static bool GetTileXYForPos(Vector3 worldPos, out int tileX, out int tileY)
		{
			tileX = (int)GetTileFraction(worldPos.Y);
			tileY = (int)GetTileFraction(worldPos.X);

			return VerifyTileCoords(tileX, tileY);
		}

		public static void GetChunkXYForPos(Vector3 worldPos, out int chunkX, out int chunkY)
		{
			var tileFractionX = GetTileFraction(worldPos.X);
			var tileFractionY = GetTileFraction(worldPos.Y);

			chunkX = (int)GetChunkFraction(tileFractionX);
			chunkY = (int)GetChunkFraction(tileFractionY);
		}

		public static Rect GetTileBoundingRect(int tileX, int tileY)
		{
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

		public static bool VerifyTileCoords(int tileX, int tileY)
		{
			var result = true;
			if (tileX < 0)
			{
				//log.Error(String.Format("WorldPos: {0} is off the map. tileX < 0.", worldPos));
				result = false;
			}
			if (tileX >= TerrainConstants.TilesPerMapSide)
			{
				//log.Error(String.Format("WorldPos: {0} is off the map. tileX >= {1}.", worldPos, TerrainConstants.TilesPerMapSide));
				result = false;
			}
			if (tileY < 0)
			{
				//log.Error(String.Format("WorldPos: {0} is off the map. tileY < 0.", worldPos));
				result = false;
			}
			if (tileX >= TerrainConstants.TilesPerMapSide)
			{
				//log.Error(String.Format("WorldPos: {0} is off the map. tileX >= {1}.", worldPos, TerrainConstants.TilesPerMapSide));
				result = false;
			}
			return result;
		}

		public static Point2D GetTileXYForPos(Vector3 worldPos)
		{
			var tileX = (int)GetTileFraction(worldPos.Y);
			var tileY = (int)GetTileFraction(worldPos.X);

			VerifyTileCoords(tileX, tileY);

			return new Point2D
			{
				X = tileX,
				Y = tileY
			};
		}

		public static Point2D GetXYForPos(Vector3 worldPos, out Point2D tileCoord)
		{
			var tileXFraction = GetTileFraction(worldPos.Y);
			var tileYFraction = GetTileFraction(worldPos.X);
			var tileX = (int)tileXFraction;
			var tileY = (int)tileYFraction;

			VerifyTileCoords(tileX, tileY);

			tileCoord = new Point2D
			{
				X = tileX,
				Y = tileY
			};

			var chunkX = (int)GetChunkFraction(tileXFraction);
			var chunkY = (int)GetChunkFraction(tileYFraction);

			VerifyPoint2D(worldPos, (int)tileXFraction, (int)tileYFraction, chunkX, chunkY);

			return new Point2D
			{
				X = chunkX,
				Y = chunkY
			};
		}

		public static Point2D GetHeightMapXYForPos(Vector3 worldPos, out Point2D tileCoord, out Point2D chunkCoord)
		{
			var tileXFraction = GetTileFraction(worldPos.Y);
			var tileYFraction = GetTileFraction(worldPos.X);
			var tileX = (int)tileXFraction;
			var tileY = (int)tileYFraction;

			VerifyTileCoords(tileX, tileY);

			tileCoord = new Point2D
			{
				X = tileX,
				Y = tileY
			};

			var chunkXFraction = GetChunkFraction(tileXFraction);
			var chunkYFraction = GetChunkFraction(tileYFraction);
			var chunkX = (int)chunkXFraction;
			var chunkY = (int)chunkYFraction;

			VerifyPoint2D(worldPos, tileX, tileY, chunkX, chunkY);

			chunkCoord = new Point2D
			{
				X = chunkX,
				Y = chunkY
			};

			var heightMapX = (int)GetHeightMapFraction(chunkXFraction);
			var heightMapY = (int)GetHeightMapFraction(chunkYFraction);

			VerifyHeightMapCoord(worldPos, (int)tileXFraction, (int)tileYFraction, (int)chunkXFraction,
								 (int)chunkYFraction, heightMapX, heightMapY);

			return new Point2D
			{
				X = heightMapX,
				Y = heightMapY
			};
		}

		public static HeightMapFraction GetHeightMapFraction(Vector3 worldPos, 
			out Point2D tileCoord, out Point2D chunkCoord, out Point2D unitCoord)
		{
			var tileXFraction = GetTileFraction(worldPos.Y);
			var tileYFraction = GetTileFraction(worldPos.X);
			var tileX = (int)tileXFraction;
			var tileY = (int)tileYFraction;

			VerifyTileCoords(tileX, tileY);

			tileCoord = new Point2D
			{
				X = tileX,
				Y = tileY
			};

			var chunkXFraction = GetChunkFraction(tileXFraction);
			var chunkYFraction = GetChunkFraction(tileYFraction);
			var chunkX = (int)chunkXFraction;
			var chunkY = (int)chunkYFraction;

			VerifyPoint2D(worldPos, tileX, tileY, chunkX, chunkY);

			chunkCoord = new Point2D
			{
				X = chunkX,
				Y = chunkY
			};

			var heightMapXFraction = GetHeightMapFraction(chunkXFraction);
			var heightMapYFraction = GetHeightMapFraction(chunkYFraction);

			var heightMapX = (int)heightMapXFraction;
			var heightMapY = (int)heightMapYFraction;

			VerifyHeightMapCoord(worldPos, tileX, tileY, chunkX, chunkY, heightMapX, heightMapY);

			unitCoord = new Point2D
			{
				X = heightMapX,
				Y = heightMapY
			};

			return new HeightMapFraction
			{
				FractionX = (heightMapXFraction - heightMapX),
				FractionY = (heightMapYFraction - heightMapY)
			};
		}

		private static float GetHeightMapFraction(float chunkLocFraction)
		{
			return ((chunkLocFraction - ((int)chunkLocFraction)) * TerrainConstants.UnitsPerChunkSide);
		}

		public static void VerifyPoint2D(Vector3 worldPos, int tileX, int tileY, int chunkX, int chunkY)
		{
			if (chunkX < 0)
				throw new InvalidDataException(
					String.Format("WorldPos: {0} does not correspond to a valid chunk in Tile:[{1}, {2}]. chunkX < 0.",
								  worldPos, tileX, tileY));
			if (chunkX >= TerrainConstants.ChunksPerTileSide)
				throw new InvalidDataException(
					String.Format("WorldPos: {0} does not correspond to a valid chunk in Tile:[{1}, {2}]. chunkX >= {3}.",
								  worldPos, tileX, tileY, TerrainConstants.ChunksPerTileSide));
			if (chunkY < 0)
				throw new InvalidDataException(
					String.Format("WorldPos: {0} does not correspond to a valid chunk in Tile:[{1}, {2}]. chunkY < 0.",
								  worldPos, tileX, tileY));
			if (chunkY >= TerrainConstants.ChunksPerTileSide)
				throw new InvalidDataException(
					String.Format("WorldPos: {0} does not correspond to a valid chunk in Tile:[{1}, {2}]. chunkY >= {3}.",
								  worldPos, tileX, tileY, TerrainConstants.ChunksPerTileSide));
		}

		public static void VerifyHeightMapCoord(Vector3 worldPos, int tileX, int tileY, int chunkX, int chunkY, int heightMapX, int heightMapY)
		{
			if (heightMapX < 0)
				throw new InvalidDataException(
					String.Format("WorldPos: {0} does not correspond to a valid chunk in Tile:[{1}, {2}], Chunk:[{3}, {4}]. heightMapX < 0.",
								  worldPos, tileX, tileY, chunkX, chunkY));
			if (heightMapX > TerrainConstants.UnitsPerChunkSide)
				throw new InvalidDataException(
					String.Format("WorldPos: {0} does not correspond to a valid chunk in Tile:[{1}, {2}], Chunk:[{3}, {4}]. chunkX >= {5}.",
								  worldPos, tileX, tileY, chunkX, chunkY, TerrainConstants.UnitsPerChunkSide));
			if (heightMapY < 0)
				throw new InvalidDataException(
					String.Format("WorldPos: {0} does not correspond to a valid chunk in Tile:[{1}, {2}], Chunk:[{3}, {4}]. chunkY < 0.",
								  worldPos, tileX, tileY, chunkX, chunkY));
			if (heightMapY > TerrainConstants.UnitsPerChunkSide)
				throw new InvalidDataException(
					String.Format("WorldPos: {0} does not correspond to a valid chunk in Tile:[{1}, {2}], Chunk:[{3}, {4}]. chunkY >= {5}.",
								  worldPos, tileX, tileY, chunkX, chunkY, TerrainConstants.ChunksPerTileSide));
		}
	}

	public class HeightMapFraction : IEquatable<HeightMapFraction>
	{
		public float FractionX;
		public float FractionY;

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof(HeightMapFraction)) return false;
			return Equals((HeightMapFraction)obj);
		}

		public bool Equals(HeightMapFraction obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj.FractionX == FractionX && obj.FractionY == FractionY;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (FractionX.GetHashCode() * 397) ^ FractionY.GetHashCode();
			}
		}

		public static bool operator ==(HeightMapFraction left, HeightMapFraction right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(HeightMapFraction left, HeightMapFraction right)
		{
			return !Equals(left, right);
		}
	}
}
