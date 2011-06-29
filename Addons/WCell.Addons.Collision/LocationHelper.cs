using System;
using System.IO;
using WCell.Constants;
using WCell.Util.Graphics;

namespace WCell.Collision
{
    internal static class LocationHelper
    {
        internal static Point2D GetTileXYForPos(Vector3 worldPos)
        {
            var tileX = (int)GetTileFraction(worldPos.Y);
            var tileY = (int)GetTileFraction(worldPos.X);

            VerifyPoint2D(worldPos, tileX, tileY);

            return new Point2D
            {
                X = tileX,
                Y = tileY
            };
        }

        internal static Point2D GetXYForPos(Vector3 worldPos, out Point2D tileCoord)
        {
            var tileXFraction = GetTileFraction(worldPos.Y);
            var tileYFraction = GetTileFraction(worldPos.X);
            var tileX = (int)tileXFraction;
            var tileY = (int)tileYFraction;

            VerifyPoint2D(worldPos, tileX, tileY);

            tileCoord = new Point2D {
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

        internal static Point2D GetHeightMapXYForPos(Vector3 worldPos, out Point2D tileCoord, out Point2D chunkCoord)
        {
            var tileXFraction = GetTileFraction(worldPos.Y);
            var tileYFraction = GetTileFraction(worldPos.X);
            var tileX = (int)tileXFraction;
            var tileY = (int)tileYFraction;

            VerifyPoint2D(worldPos, tileX, tileY);

            tileCoord = new Point2D {
                X = tileX,
                Y = tileY
            };

            var chunkXFraction = GetChunkFraction(tileXFraction);
            var chunkYFraction = GetChunkFraction(tileYFraction);
            var chunkX = (int)chunkXFraction;
            var chunkY = (int)chunkYFraction;

            VerifyPoint2D(worldPos, tileX, tileY, chunkX, chunkY);

            chunkCoord = new Point2D {
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

        internal static HeightMapFraction GetFullInfoForPos(Vector3 worldPos, 
			out Point2D tileCoord, out Point2D chunkCoord, out Point2D unitCoord)
        {
            var tileXFraction = GetTileFraction(worldPos.Y);
            var tileYFraction = GetTileFraction(worldPos.X);
            var tileX = (int)tileXFraction;
            var tileY = (int)tileYFraction;

            VerifyPoint2D(worldPos, tileX, tileY);

            tileCoord = new Point2D {
                X = tileX,
                Y = tileY
            };

            var chunkXFraction = GetChunkFraction(tileXFraction);
            var chunkYFraction = GetChunkFraction(tileYFraction);
            var chunkX = (int)chunkXFraction;
            var chunkY = (int)chunkYFraction;

            VerifyPoint2D(worldPos, tileX, tileY, chunkX, chunkY);

            chunkCoord = new Point2D {
                X = chunkX,
                Y = chunkY
            };

            var heightMapXFraction = GetHeightMapFraction(chunkXFraction);
            var heightMapYFraction = GetHeightMapFraction(chunkYFraction);

            var heightMapX = (int)heightMapXFraction;
            var heightMapY = (int)heightMapYFraction;

            VerifyHeightMapCoord(worldPos, tileX, tileY, chunkX, chunkY, heightMapX, heightMapY);

            unitCoord =  new Point2D
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

        private static void VerifyPoint2D(Vector3 worldPos, int tileX, int tileY)
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

        private static void VerifyPoint2D(Vector3 worldPos, int tileX, int tileY, int chunkX, int chunkY)
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