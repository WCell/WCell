using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WCell.Collision;
using WCell.Util.Graphics;


namespace TerrainDisplay.Collision
{
    public class QuadTree<T> where T : class, IQuadObject
    {
        private readonly bool sort;
        private readonly Size minLeafSize;
        private readonly int maxObjectsPerLeaf;
        private readonly Dictionary<T, QuadNode> objectToNodeLookup = new Dictionary<T, QuadNode>();
        private readonly Dictionary<T, int> objectSortOrder = new Dictionary<T, int>();
        public QuadNode Root { get; private set; }
        private readonly object syncLock = new object();
        private int objectSortId;

        public QuadTree(Size minLeafSize, int maxObjectsPerLeaf)
        {
            Root = null;
            this.minLeafSize = minLeafSize;
            this.maxObjectsPerLeaf = maxObjectsPerLeaf;
        }

        public int GetSortOrder(T quadObject)
        {
            lock (objectSortOrder)
            {
                if (!objectSortOrder.ContainsKey(quadObject))
                    return -1;
                return objectSortOrder[quadObject];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minLeafSize">The smallest size a leaf will split into</param>
        /// <param name="maxObjectsPerLeaf">Maximum number of objects per leaf before it forces a split into sub quadrants</param>
        /// <param name="sort">Whether or not queries will return objects in the order in which they were added</param>
        public QuadTree(Size minLeafSize, int maxObjectsPerLeaf, bool sort)
            : this(minLeafSize, maxObjectsPerLeaf)
        {
            this.sort = sort;
        }

        public void Insert(T quadObject)
        {
            lock (syncLock)
            {
                if (sort & !objectSortOrder.ContainsKey(quadObject))
                {
                    objectSortOrder.Add(quadObject, objectSortId++);
                }

                var bounds = quadObject.Bounds;
                if (Root == null)
                {
                    var rootSize = new Size((float) Math.Ceiling(bounds.Width/minLeafSize.Width),
                                            (float) Math.Ceiling(bounds.Height/minLeafSize.Height));
                    double multiplier = Math.Max(rootSize.Width, rootSize.Height);
                    rootSize = new Size((float) (minLeafSize.Width*multiplier), (float) (minLeafSize.Height*multiplier));
                    var center = new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
                    var rootOrigin = new Point(center.X - rootSize.Width / 2, center.Y - rootSize.Height / 2);
                    Root = new QuadNode(new Rect(rootOrigin, rootSize));
                }

                while (!Root.Bounds.Contains(bounds))
                {
                    ExpandRoot(bounds);
                }

                InsertNodeObject(Root, quadObject);
            }
        }

        public List<T> Query(Rect bounds)
        {
            lock (syncLock)
            {
                var results = new List<T>();
                if (Root != null)
                    Query(bounds, Root, results);
                if (sort)
                    results.Sort((a, b) => objectSortOrder[a].CompareTo(objectSortOrder[b]));
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
                if (sort)
                {
                    results.Sort((a,b) => objectSortOrder[a].CompareTo(objectSortOrder[b]));
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
                    Query(ray, Root, results);
                if (sort)
                    results.Sort((a, b) => objectSortOrder[a].CompareTo(objectSortOrder[b]));
                return results;
            }
        }

        private void Query(Rect bounds, QuadNode node, ICollection<T> results)
        {
            lock (syncLock)
            {
                if (node == null) return;
                if (!bounds.IntersectsWith(node.Bounds)) return;
                
                foreach (var quadObject in node.Objects)
                {
                    if (bounds.IntersectsWith(quadObject.Bounds))
                        results.Add(quadObject);
                }

                foreach (var childNode in node.Nodes)
                {
                    Query(bounds, childNode, results);
                }
            }
        }

        private void Query(Point point, QuadNode node, ICollection<T> results)
        {
            lock (syncLock)
            {
                if (node == null) return;
                if (!node.Bounds.Contains(point)) return;
                
                foreach (var quadObject in node.Objects)
                {
                    if (quadObject.Bounds.Contains(point))
                        results.Add(quadObject);
                }

                foreach (var childNode in node.Nodes)
                {
                    Query(point, childNode, results);
                }
            }
        }

        private void Query(Ray2D ray, QuadNode node, ICollection<T> results)
        {
            lock (syncLock)
            {
                if (node == null) return;
                if (!node.Bounds.IntersectsWith(ray)) return;

                foreach (var quadObject in node.Objects)
                {
                    if (!quadObject.Bounds.IntersectsWith(ray)) continue;

                    results.Add(quadObject);
                }

                foreach (var childNode in node.Nodes)
                {
                    Query(ray, childNode, results);
                }
            }
        }

        private void ExpandRoot(Rect newChildBounds)
        {
            lock (syncLock)
            {
                var isNorth = Root.Bounds.Y < newChildBounds.Y;
                var isWest = Root.Bounds.X < newChildBounds.X;

                Direction rootDirection;
                if (isNorth)
                {
                    rootDirection = isWest ? Direction.NW : Direction.NE;
                }
                else
                {
                    rootDirection = isWest ? Direction.SW : Direction.SE;
                }

                var newX = (rootDirection == Direction.NW || rootDirection == Direction.SW)
                                  ? Root.Bounds.X
                                  : Root.Bounds.X - Root.Bounds.Width;
                var newY = (rootDirection == Direction.NW || rootDirection == Direction.NE)
                                  ? Root.Bounds.Y
                                  : Root.Bounds.Y - Root.Bounds.Height;
                var newRootBounds = new Rect(newX, newY, Root.Bounds.Width * 2, Root.Bounds.Height * 2);
                var newRoot = new QuadNode(newRootBounds);
                SetupChildNodes(newRoot);
                newRoot[rootDirection] = Root;
                Root = newRoot;
            }
        }

        private void InsertNodeObject(QuadNode node, T quadObject)
        {
            lock (syncLock)
            {
                if (!node.Bounds.Contains(quadObject.Bounds))
                    throw new Exception("This should not happen, child does not fit within node bounds");

                if (!node.HasChildNodes() && node.Objects.Count + 1 > maxObjectsPerLeaf)
                {
                    SetupChildNodes(node);

                    var childObjects = new List<T>(node.Objects);
                    var childrenToRelocate = new List<T>();

                    foreach (var childObject in childObjects)
                    {
                        foreach (var childNode in node.Nodes)
                        {
                            if (childNode == null)
                                continue;

                            if (childNode.Bounds.Contains(childObject.Bounds))
                            {
                                childrenToRelocate.Add(childObject);
                            }
                        }
                    }

                    foreach (var childObject in childrenToRelocate)
                    {
                        RemoveQuadObjectFromNode(childObject);
                        InsertNodeObject(node, childObject);
                    }
                }

                foreach (var childNode in node.Nodes)
                {
                    if (childNode != null)
                    {
                        if (childNode.Bounds.Contains(quadObject.Bounds))
                        {
                            InsertNodeObject(childNode, quadObject);
                            return;
                        }
                    }
                }

                AddQuadObjectToNode(node, quadObject);
            }
        }

/*
        private void ClearQuadObjectsFromNode(QuadNode node)
        {
            lock (syncLock)
            {
                var quadObjects = new List<T>(node.Objects);
                foreach (var quadObject in quadObjects)
                {
                    RemoveQuadObjectFromNode(quadObject);
                }
            }
        }
*/

        private void RemoveQuadObjectFromNode(T quadObject)
        {
            lock (syncLock)
            {
                var node = objectToNodeLookup[quadObject];
                node.quadObjects.Remove(quadObject);
                objectToNodeLookup.Remove(quadObject);
                quadObject.BoundsChanged -= quadObject_BoundsChanged;
            }
        }

        private void AddQuadObjectToNode(QuadNode node, T quadObject)
        {
            lock (syncLock)
            {
                node.quadObjects.Add(quadObject);
                objectToNodeLookup.Add(quadObject, node);
                quadObject.BoundsChanged += quadObject_BoundsChanged;
            }
        }

        void quadObject_BoundsChanged(object sender, EventArgs e)
        {
            lock (syncLock)
            {
                var quadObject = sender as T;
                if (quadObject == null) return;

                var node = objectToNodeLookup[quadObject];
                if (node.Bounds.Contains(quadObject.Bounds) && !node.HasChildNodes()) return;

                RemoveQuadObjectFromNode(quadObject);
                Insert(quadObject);
                if (node.Parent == null) return;

                CheckChildNodes(node.Parent);
            }
        }

        private void SetupChildNodes(QuadNode node)
        {
            lock (syncLock)
            {
                if (minLeafSize.Width > node.Bounds.Width/2 || minLeafSize.Height > node.Bounds.Height/2) return;

                node[Direction.NW] = new QuadNode(node.Bounds.X, node.Bounds.Y, node.Bounds.Width / 2,
                                                  node.Bounds.Height / 2);
                node[Direction.NE] = new QuadNode(node.Bounds.X + node.Bounds.Width / 2, node.Bounds.Y,
                                                  node.Bounds.Width / 2,
                                                  node.Bounds.Height / 2);
                node[Direction.SW] = new QuadNode(node.Bounds.X, node.Bounds.Y + node.Bounds.Height / 2,
                                                  node.Bounds.Width / 2,
                                                  node.Bounds.Height / 2);
                node[Direction.SE] = new QuadNode(node.Bounds.X + node.Bounds.Width / 2,
                                                  node.Bounds.Y + node.Bounds.Height / 2,
                                                  node.Bounds.Width / 2, node.Bounds.Height / 2);
            }
        }

        public void Remove(T quadObject)
        {
            lock (syncLock)
            {
                if (sort && objectSortOrder.ContainsKey(quadObject))
                {
                    objectSortOrder.Remove(quadObject);
                }

                if (!objectToNodeLookup.ContainsKey(quadObject))
                    throw new KeyNotFoundException("QuadObject not found in dictionary for removal");

                var containingNode = objectToNodeLookup[quadObject];
                RemoveQuadObjectFromNode(quadObject);

                if (containingNode.Parent != null)
                    CheckChildNodes(containingNode.Parent);
            }
        }



        private void CheckChildNodes(QuadNode node)
        {
            lock (syncLock)
            {
                if (GetQuadObjectCount(node) > maxObjectsPerLeaf) return;

                // Move child objects into this node, and delete sub nodes
                var subChildObjects = GetChildObjects(node);
                foreach (T childObject in subChildObjects)
                {
                    if (node.Objects.Contains(childObject)) continue;

                    RemoveQuadObjectFromNode(childObject);
                    AddQuadObjectToNode(node, childObject);
                }
                if (node[Direction.NW] != null)
                {
                    node[Direction.NW].Parent = null;
                    node[Direction.NW] = null;
                }
                if (node[Direction.NE] != null)
                {
                    node[Direction.NE].Parent = null;
                    node[Direction.NE] = null;
                }
                if (node[Direction.SW] != null)
                {
                    node[Direction.SW].Parent = null;
                    node[Direction.SW] = null;
                }
                if (node[Direction.SE] != null)
                {
                    node[Direction.SE].Parent = null;
                    node[Direction.SE] = null;
                }

                if (node.Parent != null)
                    CheckChildNodes(node.Parent);
                else
                {
                    // Its the root node, see if we're down to one quadrant, with none in local storage - if so, ditch the other three
                    var numQuadrantsWithObjects = 0;
                    QuadNode nodeWithObjects = null;
                    foreach (QuadNode childNode in node.Nodes)
                    {
                        if (childNode == null || GetQuadObjectCount(childNode) <= 0) continue;

                        numQuadrantsWithObjects++;
                        nodeWithObjects = childNode;
                        if (numQuadrantsWithObjects > 1) break;
                    }
                    if (numQuadrantsWithObjects == 1)
                    {
                        foreach (QuadNode childNode in node.Nodes)
                        {
                            if (childNode != nodeWithObjects)
                                childNode.Parent = null;
                        }
                        Root = nodeWithObjects;
                    }
                }
            }
        }


        private List<T> GetChildObjects(QuadNode node)
        {
            lock (syncLock)
            {
                var results = new List<T>();
                results.AddRange(node.quadObjects);
                foreach (var childNode in node.Nodes)
                {
                    if (childNode != null)
                        results.AddRange(GetChildObjects(childNode));
                }
                return results;
            }
        }

        public int GetQuadObjectCount()
        {
            lock (syncLock)
            {
                if (Root == null)
                    return 0;
                int count = GetQuadObjectCount(Root);
                return count;
            }
        }

        private int GetQuadObjectCount(QuadNode node)
        {
            lock (syncLock)
            {
                var count = node.Objects.Count;
                foreach (var childNode in node.Nodes)
                {
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
                if (Root == null)
                    return 0;
                var count = GetQuadNodeCount(Root, 1);
                return count;
            }
        }

        private int GetQuadNodeCount(QuadNode node, int count)
        {
            lock (syncLock)
            {
                if (node == null) return count;

                foreach (var childNode in node.Nodes)
                {
                    if (childNode != null)
                        count++;
                }
                return count;
            }
        }

        public List<QuadNode> GetAllNodes()
        {
            lock (syncLock)
            {
                var results = new List<QuadNode>();
                if (Root != null)
                {
                    results.Add(Root);
                    GetChildNodes(Root, results);
                }
                return results;
            }
        }

        private void GetChildNodes(QuadNode node, ICollection<QuadNode> results)
        {
            lock (syncLock)
            {
                foreach (QuadNode childNode in node.Nodes)
                {
                    if (childNode == null) continue;

                    results.Add(childNode);
                    GetChildNodes(childNode, results);
                }
            }
        }

        public class QuadNode
        {
            private static int _id;
            public readonly int ID = _id++;

            public QuadNode Parent { get; internal set; }

            private readonly QuadNode[] _nodes = new QuadNode[4];
            public QuadNode this[Direction direction]
            {
                get
                {
                    switch (direction)
                    {
                        case Direction.NW:
                            return _nodes[0];
                        case Direction.NE:
                            return _nodes[1];
                        case Direction.SW:
                            return _nodes[2];
                        case Direction.SE:
                            return _nodes[3];
                        default:
                            return null;
                    }
                }
                set
                {
                    switch (direction)
                    {
                        case Direction.NW:
                            _nodes[0] = value;
                            break;
                        case Direction.NE:
                            _nodes[1] = value;
                            break;
                        case Direction.SW:
                            _nodes[2] = value;
                            break;
                        case Direction.SE:
                            _nodes[3] = value;
                            break;
                    }
                    if (value != null)
                        value.Parent = this;
                }
            }

            public ReadOnlyCollection<QuadNode> Nodes;

            internal List<T> quadObjects = new List<T>();
            public ReadOnlyCollection<T> Objects;

            public Rect Bounds { get; internal set; }

            public bool HasChildNodes()
            {
                return _nodes[0] != null;
            }

            public QuadNode(Rect bounds)
            {
                Bounds = bounds;
                Nodes = new ReadOnlyCollection<QuadNode>(_nodes);
                Objects = new ReadOnlyCollection<T>(quadObjects);
            }

            public QuadNode(float x, float y, float width, float height)
                : this(new Rect(x, y, width, height))
            {

            }
        }
    }

    public enum Direction
    {
        NW = 0,
        NE = 1,
        SW = 2,
        SE = 3
    }
}