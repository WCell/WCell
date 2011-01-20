using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WCell.Constants.World;
using WCell.Util.Graphics;

namespace WCell.Collision
{
    public class WorldModels
    {
        private const string fileType = "m2x";
        private static readonly Dictionary<string, bool> tileLoaded = new Dictionary<string, bool>();
        private static readonly Dictionary<string, bool> noTileExists = new Dictionary<string, bool>();
        private static readonly Dictionary<MapId, bool> mapExists = new Dictionary<MapId, bool>();
        private static readonly Dictionary<MapId, TreeReference<M2>> worldModels =
            new Dictionary<MapId, TreeReference<M2>>();


        internal static float? GetModelHeight(MapId map, Vector3 pos)
        {
            // Start at the character's head and send a ray pointed at the floor
            var startPos = pos + Vector3.Up * 2.0f; // what should the scalar be?
            var endPos = startPos + Vector3.Down * 2000.0f; // what should the scalar be?

            var models = GetPotentialColliders(map, ref startPos, ref endPos);
            if (models.Count == 0) return null;

            float tMax;
            var ray = CollisionHelper.CreateRay(startPos, endPos, out tMax);

            var dist = ShortestDistanceTo(models, tMax, ray);
            if (dist == null) return null;

            return (startPos.Z + Vector3.Down.Z * dist.Value);
        }

        internal static bool HasLOS(MapId map, Vector3 startPos, Vector3 endPos)
        {
            return !CheckIfCollides(map, startPos, endPos);
        }

        private static bool CheckIfCollides(MapId map, Vector3 startPos, Vector3 endPos)
        {
            var models = GetPotentialColliders(map, ref startPos, ref endPos);

            // No models intersect this footprint
            if (models.Count == 0) return false;

            float tMax;
            var ray = CollisionHelper.CreateRay(startPos, endPos, out tMax);

            return CollidesWithRay(models, tMax, ray);
        }

        private static bool CollidesWithRay(IList<M2> models, float tMax, Ray ray)
        {
            for (var i = 0; i < models.Count; i++)
            {
                var collides = models[i].CollidesWithRay(ref ray, ref tMax);
                if (!collides) continue;
                return true;
            }

            return false;
        }

        internal static float? GetCollisionDistance(MapId map, Vector3 startPos, Vector3 endPos)
        {
            var models = GetPotentialColliders(map, ref startPos, ref endPos);

            // No models intersect this footprint
            if (models.Count == 0) return null;

            float tMax;
            var ray = CollisionHelper.CreateRay(startPos, endPos, out tMax);

            return ShortestDistanceTo(models, tMax, ray);
        }

        private static float? ShortestDistanceTo(IList<M2> models, float tMax, Ray ray)
        {
            float? result = tMax;
            for (var i = 0; i < models.Count; i++)
            {
                var newResult = models[i].ShortestDistanceTo(ref ray, ref tMax);
                if (newResult == null) continue;

                result = Math.Min(result.Value, newResult.Value);
            }

            return result < tMax ? result : null;
        }

        #region Load Data
        internal static QuadTree<M2> GetBuildingTree(MapId map, TileCoord tileCoord)
        {
            if (!HasMap(map))
                return null;

            TreeReference<M2> tree;

            lock (worldModels)
            {
                // map has tree?
                if (worldModels.TryGetValue(map, out tree))
                {
                    if (tree.Tree != null)
                        return tree.Tree;
                }
                else
                {
                    // Create map tree
                    var box = MapBoundaries.GetMapBoundaries()[(int)map];
                    worldModels[map] = tree = new TreeReference<M2>(new QuadTree<M2>(box));
                }
            }

            lock (tree.LoadLock)
            {
                EnsureGroupLoaded(tree, map, tileCoord);
                return tree.Tree;
            }
        }

        private static List<M2> GetPotentialColliders(MapId map, ref Vector3 startPos, ref Vector3 endPos)
        {
            var startCoord = LocationHelper.GetTileXYForPos(startPos);
            var endCoord = LocationHelper.GetTileXYForPos(endPos);

            var tree = GetBuildingTree(map, startCoord);
            if (startCoord != endCoord)
            {
                tree = GetBuildingTree(map, endCoord);
            }

            var footPrint = MakeBounds(ref startPos, ref endPos);
            return tree.Query(footPrint);
        }

        private static bool EnsureGroupLoaded(TreeReference<M2> tree, MapId map, TileCoord tileCoord)
        {
            var result = true;

            var tile = new TileCoord
            {
                TileX = tileCoord.TileX,
                TileY = tileCoord.TileY
            };
            result = EnsureTileLoaded(map, tile, tree);

            tile.TileX = tileCoord.TileX - 1;
            result = result && EnsureTileLoaded(map, tile, tree);

            tile.TileX = tileCoord.TileX + 1;
            result = result && EnsureTileLoaded(map, tile, tree);

            tile.TileX = tileCoord.TileX;
            tile.TileY = tileCoord.TileY - 1;
            result = result && EnsureTileLoaded(map, tile, tree);

            tile.TileY = tileCoord.TileY + 1;
            result = result && EnsureTileLoaded(map, tile, tree);

            return result;
        }

        private static bool EnsureTileLoaded(MapId map, TileCoord tile, TreeReference<M2> tree)
        {
            if (IsTileLoaded(map, tile) || noTileExists.ContainsKey(GenerateKey(map, tile))) return true;

            if (!LoadTile(tree, map, tile))
            {
                noTileExists.Add(GenerateKey(map, tile), true);
                return false;
            }
            return true;
        }

        private static bool LoadTile(TreeReference<M2> tree, MapId mapId, TileCoord tileCoord)
        {
			var dir = Path.Combine(WorldMap.HeightMapFolder, ((int)mapId).ToString());
            if (!Directory.Exists(dir)) return false;

            var fileName = String.Format("{0}{1}.fub", tileCoord.TileX, tileCoord.TileY);
            var fullPath = Path.Combine(dir, fileName);
            if (!File.Exists(fullPath)) return false;

            return LoadTileModels(tree, fullPath);
        }

        private static bool HasMap(MapId mapId)
        {
            lock (mapExists)
            {
                if (!IsLoaded(mapId))
                {
					var mapPath = Path.Combine(WorldMap.HeightMapFolder, ((uint)mapId).ToString());
                    var exists = Directory.Exists(mapPath);

                    mapExists[mapId] = exists;
                    return exists;
                }

                return mapExists[mapId];
            }
        }

        private static bool IsLoaded(MapId mapId)
        {
            return mapExists.ContainsKey(mapId);
        }

        private static bool IsTileLoaded(MapId map, TileCoord tileCoord)
        {
            return (tileLoaded.ContainsKey(GenerateKey(map, tileCoord)));
        }

        private static string GenerateKey(MapId map, TileCoord tileCoord)
        {
            return String.Format("{0}_{1}_{2}", (int)map, tileCoord.TileX, tileCoord.TileY);
        }

        private static BoundingBox MakeBounds(ref Vector3 startPos, ref Vector3 endPos)
        {
            var newMin = Vector3.Min(startPos, endPos);
            var newMax = Vector3.Max(startPos, endPos);

            return new BoundingBox(newMin, newMax);
        }

        private static bool LoadTileModels(TreeReference<M2> tree, string filePath)
        {
            using (var file = File.OpenRead(filePath))
            using (var br = new BinaryReader(file))
            {
                var key = br.ReadString();
                if (key != fileType)
                {
                    Console.WriteLine("Invalid file format, suckah!");
                }

                ReadModels(br, tree);

                br.Close();
            }


            return true;
        }

        private static void ReadModels(BinaryReader br, TreeReference<M2> tree)
        {
            var numModels = br.ReadInt32();
            for (var i = 0; i < numModels; i++)
            {
                var bounds = br.ReadBoundingBox();
                
                Vector3[] vertices;
                var numVertices = br.ReadInt32();
                if (numVertices > 0)
                {
                    vertices = new Vector3[numVertices];
                    for (var j = 0; j < numVertices; j++)
                    {
                        vertices[j] = br.ReadVector3();
                    }
                }
                else
                {
                    vertices = null;
                }

                Index3[] indices;
                var numIndices = br.ReadInt32();
                if (numIndices > 0)
                {
                    indices = new Index3[numIndices];
                    for (var j = 0; j < numIndices; j++)
                    {
                        indices[j] = br.ReadIndex3();
                    }
                }
                else
                {
                    indices = null;
                }

                var model = new M2
                {
                    Bounds = bounds,
                    Vertices = vertices,
                    Triangles = indices
                };

                tree.Tree.Insert(model);
            }
        }
        #endregion
    }


}