using System;
using System.Collections.Generic;
using System.IO;
using WCell.Collision;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.Terrain.Collision.QuadTree
{
    public class QuadTree<T> where T : class, IQuadObject
    {
        private object syncLock;
        private float minLeafSize;
        public List<QuadNode> Nodes { get; private set; }

        public QuadNode Root
        {
            get { return Nodes[0]; }
        }
        
        public QuadTree(Point basePosition, int numLeavesPerSide, float minLeafSize) : this()
        {
            this.minLeafSize = minLeafSize;
            var totalLeaves = numLeavesPerSide*numLeavesPerSide;
            

            var totalNodes = 0;
            for (var nodes = 1; nodes <= totalLeaves; nodes *= 4)
            {
                totalNodes += nodes;
            }

            Nodes = new List<QuadNode>(totalNodes);
            var length = (numLeavesPerSide*minLeafSize);
            var botRightX = basePosition.X - length;
            var botRightY = basePosition.Y - length;
            var rootBounds = new Rect(basePosition, new Point(botRightX, botRightY));
            var rootNode = new QuadNode
            {
                ParentId = -1,
                NodeId = Nodes.Count,
                Bounds = rootBounds
            };
            Nodes.Add(rootNode);

            Subdivide(rootNode);
        }

        private QuadTree()
        {
            syncLock = new object();
        }

        private void Subdivide(QuadNode node)
        {
            /*************************************************************************
             *                                               X
             *                                               ^
             * (Rect.X + width, Rect.Y + width)              |
             *                      ------------------------ (Rect.X + width, Rect.Y)
             *                      |           |          | |
             *                      |     NW    |    NE    | |
             *                      |           |          | |
             *                      |-----------+----------| |
             *                      |           |          | |
             *                      |     SW    |    SE    | |
             *                      |           |          | |
             *                      -----------------------o (Rect.X, Rect.Y)
             * (Rect.X, Rect.Y + Width)                      |
             *                Y<-----------------------------0
             *************************************************************************/

            if (node.Bounds.Width <= minLeafSize) return;

            var childWidth = node.Bounds.Width/2.0f;
            if (childWidth < minLeafSize) return;

            var basePos = node.Bounds.BottomRight;

            // NW corner
            node.ChildIds = new int[4];
            var topLeftX = basePos.X;
            var topLeftY = basePos.Y;
            var botRightX = topLeftX - childWidth;
            var botRightY = topLeftY - childWidth;
            var newBounds = new Rect(new Point(topLeftX, topLeftY), 
                            new Point(botRightX, botRightY));
            var newChild = new QuadNode
            {
                ParentId = node.NodeId,
                NodeId = Nodes.Count,
                Bounds = newBounds
            };
            node.ChildIds[(int) Direction.NW] = Nodes.Count;
            Nodes.Add(newChild);

            // NE corner
            topLeftX = basePos.X;
            topLeftY = basePos.Y - childWidth;
            botRightX = topLeftX - childWidth;
            botRightY = topLeftY - childWidth;
            newBounds = new Rect(new Point(topLeftX, topLeftY),
                                 new Point(botRightX, botRightY)); 
            newChild = new QuadNode
            {
                ParentId = node.NodeId,
                NodeId = Nodes.Count,
                Bounds = newBounds
            };
            node.ChildIds[(int)Direction.NE] = Nodes.Count;
            Nodes.Add(newChild);

            // SW corner
            topLeftX = basePos.X - childWidth;
            topLeftY = basePos.Y;
            botRightX = topLeftX - childWidth;
            botRightY = topLeftY - childWidth;
            newBounds = new Rect(new Point(topLeftX, topLeftY),
                                 new Point(botRightX, botRightY));
            newChild = new QuadNode
            {
                ParentId = node.NodeId,
                NodeId = Nodes.Count,
                Bounds = newBounds
            };
            node.ChildIds[(int)Direction.SW] = Nodes.Count;
            Nodes.Add(newChild);

            // SE corner
            topLeftX = basePos.X - childWidth;
            topLeftY = basePos.Y - childWidth;
            botRightX = topLeftX - childWidth;
            botRightY = topLeftY - childWidth;
            newBounds = new Rect(new Point(topLeftX, topLeftY),
                                 new Point(botRightX, botRightY));
            newChild = new QuadNode
            {
                ParentId = node.NodeId,
                NodeId = Nodes.Count,
                Bounds = newBounds
            };
            node.ChildIds[(int)Direction.SE] = Nodes.Count;
            Nodes.Add(newChild);

            // recurse into the child nodes
            foreach (var childId in node.ChildIds)
            {
                Subdivide(Nodes[childId]);
            }
            
        }

        public bool Insert(T quadObject)
        {
            lock (syncLock)
            {
                var nodeId = quadObject.NodeId;
                if (nodeId > -1)
                {
                    var node = Nodes[nodeId];
                    AddQuadObjectToNode(node, quadObject);
                    return true;
                }

                return (Root != null) && InsertNodeObject(Root, quadObject);
            }
        }

        private bool InsertNodeObject(QuadNode node, T quadObject)
        {
            lock (syncLock)
            {
                var contained = node.Bounds.Contains(quadObject.Bounds);
                if (!contained)
                    throw new Exception("This should not happen, child does not fit within node bounds");

                if (node.IsLeaf)
                {
                    // This node is a leaf, add the object here.
                    AddQuadObjectToNode(node, quadObject);
                    return true;
                }

                foreach (var childId in node.ChildIds)
                {
                    var childNode = Nodes[childId];

                    if (childNode == null) continue;
                    if (!childNode.Bounds.Contains(quadObject.Bounds)) 
                        continue;

                    return InsertNodeObject(childNode, quadObject);
                }

                // Could not add to this node nor any of the children
                return false;
            }
        }

        public List<T> Query(Rect bounds)
        {
            lock (syncLock)
            {
                var results = new List<T>();
                if (Root != null)
                {
                    Query(bounds, Root, results);
                }
                return results;
            }
        }

        public List<T> Query(Point point)
        {
            lock (syncLock)
            {
                var results = new List<T>();
                if (Root != null)
                {
                    Query(point, Root, results);
                }
                return results;
            }
        }

        public List<T> Query(Point point1, Point point2)
        {
            lock (syncLock)
            {
                var results = new List<T>();
                if (Root != null)
                {
                    Query(point1, point2, Root, results);
                }
                return results;
            }
        }

        public List<T> Query(Ray2D ray)
        {
            lock (syncLock)
            {
                var results = new List<T>();
                if (Root != null)
                {
                    Query(ray, Root, results);
                }
                return results;
            }
        }

        private void Query(Rect bounds, QuadNode node, ICollection<T> results)
        {
            lock (syncLock)
            {
                if (node == null) return;
                if (!bounds.IntersectsWith(node.Bounds)) return;

                if (!node.IsLeaf)
                {
                    foreach (var childId in node.ChildIds)
                    {
                        var childNode = Nodes[childId];
                        Query(bounds, childNode, results);
                    }
                    return;
                }

                foreach (var quadObject in node.Objects)
                {
                    if (quadObject.Bounds.IntersectsWith(bounds))
                        results.Add(quadObject);
                }
            }
        }

        private void Query(Point point, QuadNode node, ICollection<T> results)
        {
            lock (syncLock)
            {
                if (node == null) return;
                if (!node.Bounds.Contains(point)) return;
                
                if (!node.IsLeaf)
                {
                    foreach (var childId in node.ChildIds)
                    {
                        var childNode = Nodes[childId];
                        Query(point, childNode, results);
                    }
                    return;
                }
                
                foreach (var quadObject in node.Objects)
                {
                    if (quadObject.Bounds.Contains(point))
                        results.Add(quadObject);
                }
            }
        }

        private void Query(Point point1, Point point2, QuadNode node, ICollection<T> results)
        {
            lock (syncLock)
            {
                if (node == null) return;
                if (!node.Bounds.IntersectsWith(point1, point2)) return;

                if (!node.IsLeaf)
                {
                    foreach (var childId in node.ChildIds)
                    {
                        var childNode = Nodes[childId];
                        Query(point1, point2, childNode, results);
                    }
                    return;
                }

                foreach (var quadObject in node.Objects)
                {
                    if (quadObject.Bounds.IntersectsWith(point1, point2))
                        results.Add(quadObject);
                }
            }
        }

        private void Query(Ray2D ray, QuadNode node, ICollection<T> results)
        {
            lock (syncLock)
            {
                if (node == null) return;
                if (!node.Bounds.IntersectsWith(ray)) return;

                if (!node.IsLeaf)
                {
                    foreach (var childId in node.ChildIds)
                    {
                        var childNode = Nodes[childId];
                        Query(ray, childNode, results);
                    }
                    return;
                }

                foreach (var quadObject in node.Objects)
                {
                    if (quadObject.Bounds.IntersectsWith(ray))
                        results.Add(quadObject);
                }
            }
        }

        private void AddQuadObjectToNode(QuadNode node, T quadObject)
        {
            lock (syncLock)
            {
                if (node.Objects == null) node.Objects = new List<T>();
                node.Objects.AddUnique(quadObject);
                quadObject.NodeId = node.NodeId;
            }
        }

        public bool Remove(T quadObject)
        {
            lock (syncLock)
            {
                var nodeId = quadObject.NodeId;
                var node = Nodes[nodeId];
                if (node == null) return false;

                node.Objects.Remove(quadObject);
                return true;
            }
        }

        public int GetQuadObjectCount()
        {
            lock (syncLock)
            {
                if (Root == null)
                    return 0;
                var count = GetQuadObjectCount(Root);
                return count;
            }
        }

        private int GetQuadObjectCount(QuadNode node)
        {
            lock (syncLock)
            {
                if (node.IsLeaf) return node.Objects.Count;

                var count = 0;
                foreach (var childId in node.ChildIds)
                {
                    var childNode = Nodes[childId];
                    if (childNode != null)
                    {
                        count += GetQuadObjectCount(childNode);
                    }
                }
                return count;
            }
        }

        public int GetQuadNodeCount()
        {
            lock (syncLock)
            {
                return (Nodes != null) ? Nodes.Count : 0;
            }
        }

        public List<QuadNode> GetAllNodes()
        {
            lock (syncLock)
            {
                return (Nodes);
            }
        }

        public static QuadTree<T> LoadFromFile(BinaryReader br)
        {
            var tree = new QuadTree<T>();
            var nodesLength = br.ReadInt32();
            tree.Nodes = new List<QuadNode>(nodesLength);

            for (var nodeId = 0; nodeId < nodesLength; nodeId++)
            {
                var parentId = br.ReadInt32();

                var childIds = br.ReadInt32Array();
                
                var bounds = br.ReadRect();
                tree.Nodes.Add(new QuadNode
                {
                    ParentId = parentId,
                    NodeId = nodeId,
                    ChildIds = childIds,
                    Bounds = bounds
                });
            }

            return tree;
        }

        public class QuadNode
        {
            public int ParentId;
            public int NodeId;
            public int[] ChildIds;
            public Rect Bounds;
            public List<T> Objects;

            public bool HasChildNodes
            {
                get { return !ChildIds.IsNullOrEmpty(); }
            }

            public bool IsLeaf
            {
                get { return ((ParentId != -1) && !HasChildNodes); }
            }
        }
    }

    public enum Direction
    {
        /**************************************************************************
         *     WoW axis looks like
         *     
         *              X       (min + width) o---------o
         *              +                     | NW | NE |
         *              |                     |----+----|
         *              |                     | SW | SE |
         *              |                     o---------o  (min)
         *     Y +-------  
         *     So the Rect bounds stored as (Point min, Width, Height)
         *     actually point the bottom right corner as the min
         **************************************************************************/
        NW = 0,
        NE = 1,
        SW = 2,
        SE = 3
    }
}