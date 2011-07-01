//using System;
//using System.Collections.Generic;
//using System.IO;
//using WCell.Addons.Collision;
//using WCell.Addons.Collision.Components;
//using WCell.Constants.World;
//using WCell.RealmServer.Global;
//using WCell.Terrain;
//using WCell.Terrain.Collision.QuadTree;
//using WCell.Util.Graphics;
//using WCell.Constants;

//namespace WCell.Addons.Collision
//{
//    public class MapObjectCollection
//    {
//        private const float RadiansPerDegree = 0.0174532925f;

//        private const string FileType = "wmo";

//        private readonly bool[,] tileLoaded;
//        private readonly bool[,] tileInvalid;

//        private readonly QuadTree<WMO> Tree;

//        public readonly MapId MapId;
//        private readonly BoundingBox BoundingBox;

//        public MapObjectCollection(MapId map)
//        {
//            MapId = map;
//            BoundingBox = World.GetMapBoundingBox(MapId);
//            Tree = new QuadTree<M2>(BoundingBox);
//            tileLoaded = new bool[TerrainConstants.TilesPerMapSide, TerrainConstants.TilesPerMapSide];
//            tileInvalid = new bool[TerrainConstants.TilesPerMapSide, TerrainConstants.TilesPerMapSide];
//        }


//        /// <summary>
//        /// Returns the height of the floor directly beneath a position on the world map.
//        /// </summary>
//        /// <param name="map">The MapId of the map the position is on</param>
//        /// <param name="pos">The position to check the floor height at</param>
//        public float? GetWMOHeight(Vector3 pos)
//        {
//            // Start at the character's head and send a ray pointed at the floor
//            var startPos = pos + Vector3.Up*2.0f; // how tall is each character? working with 2 yards at this point.
//            var endPos = startPos + Vector3.Down*2000.0f; // how far down should we go in checking for floor?
            
//            var buildings = GetPotentialColliders(ref startPos, ref endPos);
//            if (buildings.Count == 0) return null;

//            float tMax;
//            var ray = CollisionUtil.CreateRay(startPos, endPos, out tMax);

//            var dist = CollideWithRay(buildings, tMax, ray);
//            if (dist == null) return null;

//            return (startPos.Z + Vector3.Down.Z*dist.Value);
//        }

//        /// <summary>
//        /// Whether there is a clear line of sight between two points.
//        /// </summary>
//        /// <param name="map">The MapId of the map in question</param>
//        /// <param name="startPos">Vector in World Coords of the start position</param>
//        /// <param name="endPos">Vector in World Coords of the end position</param>
//        /// <returns>True of a clear line of sight exists</returns>
//        public bool HasLOS(Vector3 startPos, Vector3 endPos)
//        {
//            return (CheckCollision(startPos, endPos) == null);
//        }

//        /// <summary>
//        /// Whether the line from startPos to endPos intersects a Building on this Map
//        /// </summary>
//        /// <param name="map">The MapId of the Map in question</param>
//        /// <param name="startPos">Vector in World Coords of the start point</param>
//        /// <param name="endPos">Vector in World Coords of the end point</param>
//        /// <returns>Null if no collision occurs, else the earliest time of collision. 
//        /// To get the point of collision, a vector calculation of (startPos + (endPos - startPos)*returnValue) is required.</returns>
//        public float? CheckCollision(Vector3 startPos, Vector3 endPos)
//        {
//            var buildings = GetPotentialColliders(ref startPos, ref endPos);

//            // No buildings intersect this footprint
//            if (buildings.Count == 0) return null;

//            float tMax;
//            var ray = CollisionUtil.CreateRay(startPos, endPos, out tMax);

//            return CollideWithRay(buildings, tMax, ray);
//        }


//        private float? CollideWithRay(IList<WMO> buildings, float tMax, Ray ray)
//        {
//            float? result = tMax;
//            for(var i = 0; i < buildings.Count; i++)
//            {
//                var newResult = buildings[i].IntersectsWith(ref ray, ref tMax);
//                if (newResult == null) continue;

//                result = Math.Min(result.Value, newResult.Value);
//            }

//            return result < tMax ? result : null;
//        }

//        private List<WMO> GetPotentialColliders(ref Vector3 startPos, ref Vector3 endPos)
//        {
//            var startCoord = PositionUtil.GetTileXYForPos(startPos);
//            var endCoord = PositionUtil.GetTileXYForPos(endPos);

//            EnsureGroupLoaded(startCoord);
//            if (startCoord != endCoord)
//            {
//                EnsureGroupLoaded(endCoord);
//            }

//            var footPrint = MakeBounds(ref startPos, ref endPos);
//            return Tree.Query(footPrint);
//        }

//        #region Load Data
//        private bool EnsureGroupLoaded(Point2D tileCoord)
//        {
//            var result = true;

//            result = EnsureTileLoaded(tileCoord);

//            tileCoord.X = tileCoord.X - 1;
//            result = result && EnsureTileLoaded(tileCoord);

//            tileCoord.X = tileCoord.X + 1;
//            result = result && EnsureTileLoaded(tileCoord);

//            tileCoord.X = tileCoord.X;
//            tileCoord.Y = tileCoord.Y - 1;
//            result = result && EnsureTileLoaded(tileCoord);

//            tileCoord.Y = tileCoord.Y + 1;
//            result = result && EnsureTileLoaded(tileCoord);

//            return result;
//        }

//        private bool LoadTile(Point2D tileCoord)
//        {
//            var dir = Path.Combine(WCellTerrainSettings.MapDir, ((int)MapId).ToString());
//            if (!Directory.Exists(dir)) return false;

//            var fileName = TerrainConstants.GetWMOFile(tileCoord.X, tileCoord.Y);
//            var fullPath = Path.Combine(dir, fileName);
//            if (!File.Exists(fullPath)) return false;

//            return LoadTileBuildings(fullPath);
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
        
//        private bool LoadTileBuildings(string filePath)
//        {
//            using(var file = File.OpenRead(filePath))
//            using(var br = new BinaryReader(file))
//            {
//                var key = br.ReadString();
//                if (key != FileType)
//                {
//                    throw new ArgumentException("Invalid " + FileType + " file:" + filePath);
//                }

//                ReadBuildings(br);
//            }


//            return true;
//        }

//        private void ReadBuildings(BinaryReader reader)
//        {
//            var numBuildings = reader.ReadInt32();
//            for (var i = 0; i < numBuildings; i++)
//            {
//                var bounds = reader.ReadBoundingBox();
//                var invRot = reader.ReadSingle();
//                var matrix = Matrix.CreateRotationY(((-1.0f*(invRot - 90)))*RadiansPerDegree);
//                var center = reader.ReadVector3();

//                WMOGroup[] groups;
//                var numGroups = reader.ReadInt32();
//                if (numGroups > 0)
//                {
//                    groups = new WMOGroup[numGroups];
//                    for (var j = 0; j < numGroups; j++)
//                    {
//                        groups[j] = ReadBuildingGroup(reader);
//                    }
//                }
//                else
//                {
//                    groups = null;
//                }

//                var building = new WMO {
//                    Bounds = bounds,
//                    Center = center,
//                    InverseRotation = matrix,
//                    WmoGroups = groups
//                };

//                Tree.Insert(building);
//            }
//        }

//        private static WMOGroup ReadBuildingGroup(BinaryReader br)
//        {
//            var group = new WMOGroup {
//                Bounds = br.ReadBoundingBox()
//            };

//            Vector3[] vertices;
//            var numVerts = br.ReadInt32();
//            if (numVerts > 0)
//            {
//                vertices = new Vector3[numVerts];
//                for (var j = 0; j < numVerts; j++)
//                {
//                    vertices[j] = br.ReadVector3();
//                }
//            }
//            else
//            {
//                vertices = null;
//            }
//            group.Vertices = vertices;
//            group.Tree = ReadBSPTree(br);

//            return group;
//        }

//        private static BSPTree ReadBSPTree(BinaryReader br)
//        {
//            var rootId = br.ReadInt16();
//            BSPNode[] nodes;

//            var numNodes = br.ReadInt32();
//            if (numNodes > 0)
//            {
//                nodes = new BSPNode[numNodes];
//                for (var i = 0; i < numNodes; i++)
//                {
//                    nodes[i] = ReadBSPNode(br);
//                }
//            }
//            else
//            {
//                nodes = null;
//            }

//            return new BSPTree(rootId, nodes);
//        }

//        private static BSPNode ReadBSPNode(BinaryReader br)
//        {
//            var node = new BSPNode
//            {
//                flags = (BSPNodeFlags)br.ReadByte(),
//                negChild = br.ReadInt16(),
//                posChild = br.ReadInt16(),
//                planeDist = br.ReadSingle()
//            };

//            var numIndices = br.ReadInt32();
//            if (numIndices > 0)
//            {
//                var indices = new Index3[numIndices];
//                for (var i = 0; i < numIndices; i++)
//                {
//                    indices[i] = br.ReadIndex3();
//                }
//                node.TriIndices = indices;
//            }
//            else
//            {
//                node.TriIndices = null;
//            }

//            return node;
//        }
//        #endregion

//    }
//}