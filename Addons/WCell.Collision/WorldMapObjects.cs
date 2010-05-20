using System;
using System.Collections.Generic;
using System.IO;
using WCell.Constants.World;
using WCell.RealmServer;
using WCell.Util.Graphics;
using WCell.Constants;

namespace WCell.Collision
{
    public class WorldMapObjects
	{
        private const float RadiansPerDegree = 0.0174532925f;
        private const string fileType = "wmo";
        private static readonly Dictionary<string, bool> tileLoaded = new Dictionary<string, bool>();
        private static readonly Dictionary<string, bool> noTileExists = new Dictionary<string, bool>();
        private static readonly Dictionary<MapId, bool> mapData = new Dictionary<MapId, bool>();
        private static readonly Dictionary<MapId, TreeReference<Building>> worldBuildings =
            new Dictionary<MapId, TreeReference<Building>>();


        /// <summary>
        /// Returns the height of the floor directly beneath a position on the world map.
        /// </summary>
        /// <param name="map">The MapId of the map the position is on</param>
        /// <param name="pos">The position to check the floor height at</param>
        public static float? GetWMOHeight(MapId map, Vector3 pos)
        {
            // Start at the character's head and send a ray pointed at the floor
            var startPos = pos + Vector3.Up*2.0f; // how tall is each character? working with 2 yards at this point.
            var endPos = startPos + Vector3.Down*2000.0f; // how far down should we go in checking for floor?
            
            var buildings = GetPotentialColliders(map, ref startPos, ref endPos);
            if (buildings.Count == 0) return null;

            float tMax;
            var ray = CollisionHelper.CreateRay(startPos, endPos, out tMax);

            var dist = CollideWithRay(buildings, tMax, ray);
            if (dist == null) return null;

            return (startPos.Z + Vector3.Down.Z*dist.Value);
        }

        /// <summary>
        /// Whether there is a clear line of sight between two points.
        /// </summary>
        /// <param name="map">The MapId of the map in question</param>
        /// <param name="startPos">Vector in World Coords of the start position</param>
        /// <param name="endPos">Vector in World Coords of the end position</param>
        /// <returns>True of a clear line of sight exists</returns>
        public static bool HasLOS(MapId map, Vector3 startPos, Vector3 endPos)
        {
            return (CheckCollision(map, startPos, endPos) == null);
        }

        /// <summary>
        /// Whether the line from startPos to endPos intersects a Building on this Map
        /// </summary>
        /// <param name="map">The MapId of the Map in question</param>
        /// <param name="startPos">Vector in World Coords of the start point</param>
        /// <param name="endPos">Vector in World Coords of the end point</param>
        /// <returns>Null if no collision occurs, else the earliest time of collision. 
        /// To get the point of collision, a vector calculation of (startPos + (endPos - startPos)*returnValue) is required.</returns>
        public static float? CheckCollision(MapId map, Vector3 startPos, Vector3 endPos)
        {
            var buildings = GetPotentialColliders(map, ref startPos, ref endPos);

            // No buildings intersect this footprint
            if (buildings.Count == 0) return null;

            float tMax;
            var ray = CollisionHelper.CreateRay(startPos, endPos, out tMax);

            return CollideWithRay(buildings, tMax, ray);
        }


        private static float? CollideWithRay(IList<Building> buildings, float tMax, Ray ray)
        {
            float? result = tMax;
            for(var i = 0; i < buildings.Count; i++)
            {
                var newResult = buildings[i].IntersectsWith(ref ray, ref tMax);
                if (newResult == null) continue;

                result = Math.Min(result.Value, newResult.Value);
            }

            return result < tMax ? result : null;
        }

        #region Load Data
        internal static QuadTree<Building> GetBuildingTree(MapId mapId, TileCoord tileCoord)
        {
            if (!MapDataExists(mapId))
                return null;

            TreeReference<Building> treeRef;

            lock (worldBuildings)
            {
                // map has tree?
                if (worldBuildings.TryGetValue(mapId, out treeRef))
                {
                    if (treeRef.Tree != null)
                        return treeRef.Tree;
                }
                else
                {
                    // Create map tree
                    var box = RegionBoundaries.GetRegionBoundaries()[(int)mapId];
                    worldBuildings[mapId] = treeRef = new TreeReference<Building>(new QuadTree<Building>(box));
                }
            }

            lock (treeRef.LoadLock)
            {
                EnsureGroupLoaded(treeRef, mapId, tileCoord);
                return treeRef.Tree;
            }
        }

        private static List<Building> GetPotentialColliders(MapId map, ref Vector3 startPos, ref Vector3 endPos)
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

        private static bool EnsureGroupLoaded(TreeReference<Building> tree, MapId map, TileCoord tileCoord)
        {
            var result = true;

            var tile = new TileCoord {
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

        private static bool EnsureTileLoaded(MapId map, TileCoord tile, TreeReference<Building> tree)
        {
            if (IsTileLoaded(map, tile) || noTileExists.ContainsKey(GenerateKey(map, tile))) return true;

            if (!LoadTile(tree, map, tile))
            {
                noTileExists.Add(GenerateKey(map, tile), true);
                return false;
            }
            return true;
        }

        private static bool LoadTile(TreeReference<Building> tree, MapId mapId, TileCoord tileCoord)
        {
			var dir = Path.Combine(WorldMap.HeightMapFolder, ((int)mapId).ToString());
            if (!Directory.Exists(dir)) return false;

            var fileName = TerrainConstants.GetWMOFile(tileCoord.TileX, tileCoord.TileY);
            var fullPath = Path.Combine(dir, fileName);
            if (!File.Exists(fullPath)) return false;

            return LoadTileBuildings(tree, fullPath);
        }

        private static bool MapDataExists(MapId mapId)
        {
            lock (mapData)
            {
                if (!IsLoaded(mapId))
                {
					var mapPath = Path.Combine(WorldMap.HeightMapFolder, ((uint)mapId).ToString());
                    var exists = Directory.Exists(mapPath);

                    mapData[mapId] = exists;
                    return exists;
                }

                return mapData[mapId];
            }
        }

        private static bool IsLoaded(MapId mapId)
        {
            return mapData.ContainsKey(mapId);
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
        
        private static bool LoadTileBuildings(TreeReference<Building> tree, string filePath)
        {
            using(var file = File.OpenRead(filePath))
            using(var br = new BinaryReader(file))
            {
                var key = br.ReadString();
                if (key != fileType)
                {
                    Console.WriteLine(String.Format("{0} has an invalid file format, suckah!", filePath));
                }

                ReadBuildings(br, tree);
                
                br.Close();
            }


            return true;
        }

        private static void ReadBuildings(BinaryReader br, TreeReference<Building> tree)
        {
            var numBuildings = br.ReadInt32();
            for (var i = 0; i < numBuildings; i++)
            {
                var bounds = br.ReadBoundingBox();
                var invRot = br.ReadSingle();
                var matrix = Matrix.CreateRotationY(((-1.0f*(invRot - 90)))*RadiansPerDegree);
                var center = br.ReadVector3();

                BuildingGroup[] groups;
                var numGroups = br.ReadInt32();
                if (numGroups > 0)
                {
                    groups = new BuildingGroup[numGroups];
                    for (var j = 0; j < numGroups; j++)
                    {
                        groups[j] = ReadBuildingGroup(br);
                    }
                }
                else
                {
                    groups = null;
                }

                var building = new Building {
                    Bounds = bounds,
                    Center = center,
                    InverseRotation = matrix,
                    BuildingGroups = groups
                };

                tree.Tree.Insert(building);
            }
        }

        private static BuildingGroup ReadBuildingGroup(BinaryReader br)
        {
            var group = new BuildingGroup {
                Bounds = br.ReadBoundingBox()
            };

            Vector3[] vertices;
            var numVerts = br.ReadInt32();
            if (numVerts > 0)
            {
                vertices = new Vector3[numVerts];
                for (var j = 0; j < numVerts; j++)
                {
                    vertices[j] = br.ReadVector3();
                }
            }
            else
            {
                vertices = null;
            }
            group.Vertices = vertices;
            group.Tree = ReadBSPTree(br);

            return group;
        }

        private static BSPTree ReadBSPTree(BinaryReader br)
        {
            var rootId = br.ReadInt16();
            BSPNode[] nodes;

            var numNodes = br.ReadInt32();
            if (numNodes > 0)
            {
                nodes = new BSPNode[numNodes];
                for (var i = 0; i < numNodes; i++)
                {
                    nodes[i] = ReadBSPNode(br);
                }
            }
            else
            {
                nodes = null;
            }

            return new BSPTree(rootId, nodes);
        }

        private static BSPNode ReadBSPNode(BinaryReader br)
        {
            var node = new BSPNode
            {
                flags = (BSPNodeFlags)br.ReadByte(),
                negChild = br.ReadInt16(),
                posChild = br.ReadInt16(),
                planeDist = br.ReadSingle()
            };

            var numIndices = br.ReadInt32();
            if (numIndices > 0)
            {
                var indices = new Index3[numIndices];
                for (var i = 0; i < numIndices; i++)
                {
                    indices[i] = br.ReadIndex3();
                }
                node.TriIndices = indices;
            }
            else
            {
                node.TriIndices = null;
            }

            return node;
        }
        #endregion

    }
}
