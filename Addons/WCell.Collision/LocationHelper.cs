using System;
using System.IO;
using WCell.Constants;
using WCell.Util.Graphics;

namespace WCell.Collision
{
    internal static class LocationHelper
    {
        internal static TileCoord GetTileXYForPos(Vector3 worldPos)
        {
            var tileX = (int)GetTileFraction(worldPos.Y);
            var tileY = (int)GetTileFraction(worldPos.X);

            VerifyTileCoord(worldPos, tileX, tileY);

            return new TileCoord
            {
                TileX = tileX,
                TileY = tileY
            };
        }

        internal static ChunkCoord GetChunkXYForPos(Vector3 worldPos, out TileCoord tileCoord)
        {
            var tileXFraction = GetTileFraction(worldPos.Y);
            var tileYFraction = GetTileFraction(worldPos.X);
            var tileX = (int)tileXFraction;
            var tileY = (int)tileYFraction;

            VerifyTileCoord(worldPos, tileX, tileY);

            tileCoord = new TileCoord {
                TileX = tileX,
                TileY = tileY
            };

            var chunkX = (int)GetChunkFraction(tileXFraction);
            var chunkY = (int)GetChunkFraction(tileYFraction);

            VerifyChunkCoord(worldPos, (int)tileXFraction, (int)tileYFraction, chunkX, chunkY);

            return new ChunkCoord
            {
                ChunkX = chunkX,
                ChunkY = chunkY
            };
        }

        internal static PointX2D GetHeightMapXYForPos(Vector3 worldPos, out TileCoord tileCoord, out ChunkCoord chunkCoord)
        {
            var tileXFraction = GetTileFraction(worldPos.Y);
            var tileYFraction = GetTileFraction(worldPos.X);
            var tileX = (int)tileXFraction;
            var tileY = (int)tileYFraction;

            VerifyTileCoord(worldPos, tileX, tileY);

            tileCoord = new TileCoord {
                TileX = tileX,
                TileY = tileY
            };

            var chunkXFraction = GetChunkFraction(tileXFraction);
            var chunkYFraction = GetChunkFraction(tileYFraction);
            var chunkX = (int)chunkXFraction;
            var chunkY = (int)chunkYFraction;

            VerifyChunkCoord(worldPos, tileX, tileY, chunkX, chunkY);

            chunkCoord = new ChunkCoord {
                ChunkX = chunkX,
                ChunkY = chunkY
            };

            var heightMapX = (int)GetHeightMapFraction(chunkXFraction);
            var heightMapY = (int)GetHeightMapFraction(chunkYFraction);

            VerifyHeightMapCoord(worldPos, (int)tileXFraction, (int)tileYFraction, (int)chunkXFraction,
                                 (int)chunkYFraction, heightMapX, heightMapY);

            return new PointX2D
            {
                X = heightMapX,
                Y = heightMapY
            };
        }

        internal static HeightMapFraction GetFullInfoForPos(Vector3 worldPos, out TileCoord tileCoord, out ChunkCoord chunkCoord, out PointX2D pointX2D)
        {
            var tileXFraction = GetTileFraction(worldPos.Y);
            var tileYFraction = GetTileFraction(worldPos.X);
            var tileX = (int)tileXFraction;
            var tileY = (int)tileYFraction;

            VerifyTileCoord(worldPos, tileX, tileY);

            tileCoord = new TileCoord {
                TileX = tileX,
                TileY = tileY
            };

            var chunkXFraction = GetChunkFraction(tileXFraction);
            var chunkYFraction = GetChunkFraction(tileYFraction);
            var chunkX = (int)chunkXFraction;
            var chunkY = (int)chunkYFraction;

            VerifyChunkCoord(worldPos, tileX, tileY, chunkX, chunkY);

            chunkCoord = new ChunkCoord {
                ChunkX = chunkX,
                ChunkY = chunkY
            };

            var heightMapXFraction = GetHeightMapFraction(chunkXFraction);
            var heightMapYFraction = GetHeightMapFraction(chunkYFraction);

            var heightMapX = (int)heightMapXFraction;
            var heightMapY = (int)heightMapYFraction;

            VerifyHeightMapCoord(worldPos, tileX, tileY, chunkX, chunkY, heightMapX, heightMapY);

            pointX2D =  new PointX2D
            {
                X = heightMapX,
                Y = heightMapY
            };

            return new HeightMapFraction {
                FractionX = (heightMapXFraction - heightMapX),
                FractionY = (heightMapYFraction - heightMapY)
            };
        }

        private static float GetTileFraction(float loc)
        {
            return ((TerrainConstants.CenterPoint - loc)/TerrainConstants.TileSize);
        }

        private static float GetChunkFraction(float tileLocFraction)
        {
            return ((tileLocFraction - (int)tileLocFraction)*TerrainConstants.ChunksPerTileSide);
        }

        private static float GetHeightMapFraction(float chunkLocFraction)
        {
            return ((chunkLocFraction - ((int)chunkLocFraction))*TerrainConstants.UnitsPerChunkSide);
        }

        private static void VerifyTileCoord(Vector3 worldPos, int tileX, int tileY)
        {
            if (tileX < 0)
                throw new InvalidDataException(String.Format("WorldPos: {0} is off the map. tileX < 0.", worldPos));
            if (tileX >= TerrainConstants.TilesPerMapSide)
                throw new InvalidDataException(String.Format("WorldPos: {0} is off the map. tileX >= {1}.", worldPos,
                                                             TerrainConstants.TilesPerMapSide));
            if (tileY < 0)
                throw new InvalidDataException(String.Format("WorldPos: {0} is off the map. tileY < 0.", worldPos));
            if (tileX >= TerrainConstants.TilesPerMapSide)
                throw new InvalidDataException(String.Format("WorldPos: {0} is off the map. tileX >= {1}.", worldPos,
                                                             TerrainConstants.TilesPerMapSide));
        }

        private static void VerifyChunkCoord(Vector3 worldPos, int tileX, int tileY, int chunkX, int chunkY)
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

        private static void VerifyHeightMapCoord(Vector3 worldPos, int tileX, int tileY, int chunkX, int chunkY, int heightMapX, int heightMapY)
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

    internal class TileCoord : IEquatable<TileCoord>
    {
        public int TileX;
        public int TileY;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (TileCoord)) return false;
            return Equals((TileCoord)obj);
        }

        public bool Equals(TileCoord obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.TileX == TileX && obj.TileY == TileY;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (TileX*397) ^ TileY;
            }
        }

        public static bool operator ==(TileCoord left, TileCoord right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TileCoord left, TileCoord right)
        {
            return !Equals(left, right);
        }
    }

    internal class ChunkCoord : IEquatable<ChunkCoord>
    {
        public int ChunkX;
        public int ChunkY;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ChunkCoord)) return false;
            return Equals((ChunkCoord)obj);
        }

        public bool Equals(ChunkCoord obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.ChunkX == ChunkX && obj.ChunkY == ChunkY;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ChunkX*397) ^ ChunkY;
            }
        }

        public static bool operator ==(ChunkCoord left, ChunkCoord right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ChunkCoord left, ChunkCoord right)
        {
            return !Equals(left, right);
        }
    }

	/// <summary>
	/// Fixed 2D point (using integers for components)
	/// </summary>
    internal class PointX2D : IEquatable<PointX2D>
    {
        public int X;
        public int Y;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (PointX2D)) return false;
            return Equals((PointX2D)obj);
        }

        public bool Equals(PointX2D obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.X == X && obj.Y == Y;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X*397) ^ Y;
            }
        }

        public static bool operator ==(PointX2D left, PointX2D right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PointX2D left, PointX2D right)
        {
            return !Equals(left, right);
        }
    }

    internal class HeightMapFraction : IEquatable<HeightMapFraction>
    {
        public float FractionX;
        public float FractionY;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (HeightMapFraction)) return false;
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
                return (FractionX.GetHashCode()*397) ^ FractionY.GetHashCode();
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
