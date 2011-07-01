//using System;
//using System.Collections.Generic;
//using System.IO;
//using WCell.Addons.Collision.Components;
//using WCell.Constants;
//using WCell.Constants.World;
//using WCell.RealmServer.Global;
//using WCell.Terrain;
//using WCell.Terrain.Collision.QuadTree;
//using WCell.Util.Graphics;

//namespace WCell.Addons.Collision
//{
//    /// <summary>
//    /// Represents all models within a single map
//    /// </summary>
//    public class MapModelCollection
//    {
//        private const string FileType = "m2x";

//        private readonly bool[,] tileLoaded;
//        private readonly bool[,] tileInvalid;
		
//        private readonly QuadTree<M2> Tree;

//        public readonly MapId MapId;
//        private readonly BoundingBox BoundingBox;

//        public MapModelCollection(MapId map)
//        {
//            MapId = map;
//            BoundingBox = World.GetMapBoundingBox(MapId);
//            Tree = new QuadTree<M2>(BoundingBox);
//            tileLoaded = new bool[TerrainConstants.TilesPerMapSide,TerrainConstants.TilesPerMapSide];
//            tileInvalid = new bool[TerrainConstants.TilesPerMapSide, TerrainConstants.TilesPerMapSide];
//        }

//        public float? GetModelHeight(Vector3 pos)
//        {
//            // Start at the character's head and send a ray pointed at the floor
//            var startPos = pos + Vector3.Up * 2.0f; // what should the scalar be?
//            var endPos = startPos + Vector3.Down * 2000.0f; // what should the scalar be?

//            var models = GetPotentialColliders(ref startPos, ref endPos);
//            if (models.Count == 0) return null;

//            float tMax;
//            var ray = CollisionUtil.CreateRay(startPos, endPos, out tMax);

//            var dist = ShortestDistanceTo(models, tMax, ray);
//            if (dist == null) return null;

//            return (startPos.Z + Vector3.Down.Z * dist.Value);
//        }

//        internal bool HasLOS(Vector3 startPos, Vector3 endPos)
//        {
//            return !CheckIfCollides(startPos, endPos);
//        }

//        private bool CheckIfCollides(Vector3 startPos, Vector3 endPos)
//        {
//            var models = GetPotentialColliders(ref startPos, ref endPos);

//            // No models intersect this footprint
//            if (models.Count == 0) return false;

//            float tMax;
//            var ray = CollisionUtil.CreateRay(startPos, endPos, out tMax);

//            return CollidesWithRay(models, tMax, ray);
//        }

//        private static bool CollidesWithRay(IList<M2> models, float tMax, Ray ray)
//        {
//            for (var i = 0; i < models.Count; i++)
//            {
//                var collides = models[i].CollidesWithRay(ref ray, ref tMax);
//                if (!collides) continue;
//                return true;
//            }

//            return false;
//        }

//        internal float? GetCollisionDistance(Vector3 startPos, Vector3 endPos)
//        {
//            var models = GetPotentialColliders(ref startPos, ref endPos);

//            // No models intersect this footprint
//            if (models.Count == 0) return null;

//            float tMax;
//            var ray = CollisionUtil.CreateRay(startPos, endPos, out tMax);

//            return ShortestDistanceTo(models, tMax, ray);
//        }

//        private static float? ShortestDistanceTo(IList<M2> models, float tMax, Ray ray)
//        {
//            float? result = tMax;
//            for (var i = 0; i < models.Count; i++)
//            {
//                var newResult = models[i].ShortestDistanceTo(ref ray, ref tMax);
//                if (newResult == null) continue;

//                result = Math.Min(result.Value, newResult.Value);
//            }

//            return result < tMax ? result : null;
//        }

//        #region Load Data
//        private List<M2> GetPotentialColliders(ref Vector3 startPos, ref Vector3 endPos)
//        {
//            var startCoord = LocationHelper.GetTileXYForPos(startPos);
//            var endCoord = LocationHelper.GetTileXYForPos(endPos);

//            EnsureGroupLoaded(startCoord);
//            if (startCoord != endCoord)
//            {
//                EnsureGroupLoaded(endCoord);
//            }

//            var footPrint = MakeBounds(ref startPos, ref endPos);
//            return Tree.Query(footPrint);
//        }

//        /// <summary>
//        /// Makes sure that the given tile and all it's neighbors are loaded
//        /// </summary>
//        private bool EnsureGroupLoaded(Point2D tileCoord)
//        {
//            var result = true;

//            var tile = new Point2D
//            {
//                X = tileCoord.X,
//                Y = tileCoord.Y
//            };
//            result = EnsureTileLoaded(tile);

//            tile.X = tileCoord.X - 1;
//            result = result && EnsureTileLoaded(tile);

//            tile.X = tileCoord.X + 1;
//            result = result && EnsureTileLoaded(tile);

//            tile.X = tileCoord.X;
//            tile.Y = tileCoord.Y - 1;
//            result = result && EnsureTileLoaded(tile);

//            tile.Y = tileCoord.Y + 1;
//            result = result && EnsureTileLoaded(tile);

//            return result;
//        }

//        private bool EnsureTileLoaded(Point2D tile)
//        {
//            if (IsTileLoaded(tile) || NoTile(tile)) return true;
			
//            if (!LoadTile(tile))
//            {
//                tileInvalid[tile.X, tile.Y] = true;
//                return false;
//            }
//            return true;
//        }

//        private bool LoadTile(Point2D tileCoord)
//        {
//            var dir = Path.Combine(WorldMap.HeightMapFolder, ((int)MapId).ToString());
//            if (!Directory.Exists(dir)) return false;

//            var fileName = String.Format("{0}{1}.fub", tileCoord.X, tileCoord.Y);
//            var fullPath = Path.Combine(dir, fileName);
//            if (!File.Exists(fullPath)) return false;

//            return LoadTileModels(fullPath);
//        }

//        private bool IsTileLoaded(Point2D tileCoord)
//        {
//            return tileLoaded[tileCoord.X, tileCoord.Y];
//        }

//        private bool NoTile(Point2D tileCoord)
//        {
//            return tileInvalid[tileCoord.X, tileCoord.Y];
//        }

//        private static BoundingBox MakeBounds(ref Vector3 startPos, ref Vector3 endPos)
//        {
//            var newMin = Vector3.Min(startPos, endPos);
//            var newMax = Vector3.Max(startPos, endPos);

//            return new BoundingBox(newMin, newMax);
//        }
//        #endregion
//    }


//}