using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Util.Graphics;

namespace WCell.Addons.Terrain
{
    public class zzQuadTree<T> where T : IBounded
    {
        /// <summary>
        /// The root zzQuadTreeNode
        /// </summary>
        private readonly zzQuadTreeNode<T> m_root;

        /// <summary>
        /// The bounds of this QuadTree
        /// </summary>
        private readonly BoundingBox m_rectangle;

        /// <summary>
        /// An delegate that performs an action on a zzQuadTreeNode
        /// </summary>
        /// <param name="obj"></param>
        public delegate void QTAction(zzQuadTreeNode<T> obj);

        public zzQuadTree(BoundingBox rectangle)
        {
            m_rectangle = rectangle;
            m_root = new zzQuadTreeNode<T>(m_rectangle);
        }

        /// <summary>
        /// Get the count of items in the QuadTree
        /// </summary>
        public int Count { get { return m_root.Count; } }

        /// <summary>
        /// Insert the feature into the QuadTree
        /// </summary>
        /// <param name="item"></param>
        public void Insert(T item)
        {
            m_root.Insert(item);
        }

        /// <summary>
        /// Query the QuadTree, returning the items that are in the given area
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public List<T> Query(BoundingBox area)
        {
            return m_root.Query(area);
        }

        /// <summary>
        /// Do the specified action for each item in the quadtree
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(QTAction action)
        {
            m_root.ForEach(action);
        }

    }

    public class zzQuadTreeNode<T> where T : IBounded
    {
        /// <summary>
        /// The contents of this node.
        /// Note that the contents have no limit: this is not the standard way to impement a QuadTree
        /// </summary>
        private readonly List<T> m_contents = new List<T>();

        /// <summary>
        /// The child nodes of the QuadTree
        /// </summary>
		private readonly List<zzQuadTreeNode<T>> m_nodes = new List<zzQuadTreeNode<T>>(4);

		/// <summary>
		/// Construct a quadtree node with the given bounds 
		/// </summary>
		public zzQuadTreeNode(BoundingBox bounds)
		{
			Bounds = bounds;
		}

        /// <summary>
        /// Is the node empty
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return (m_nodes.Count == 0 || Bounds.Height == 0 || Bounds.Width == 0);
            }
        }

        /// <summary>
        /// Area of the quadtree node
        /// </summary>
        public BoundingBox Bounds
        {
            get;
            private set;
        }

        /// <summary>
        /// Total number of nodes in this node and all SubNodes
        /// </summary>
        public int Count
        {
            get
            {
                var count = 0;

                foreach (var node in m_nodes)
                    count += node.Count;

                count += Contents.Count;

                return count;
            }
        }

        /// <summary>
        /// Return the contents of this node and all subnodes in the tree below this one.
        /// </summary>
        public List<T> SubTreeContents
        {
            get
            {
                var results = new List<T>();

                foreach (var node in m_nodes)
                    results.AddRange(node.SubTreeContents);

                results.AddRange(this.Contents);
                return results;
            }
        }

        public List<T> Contents { get { return m_contents; } }

        /// <summary>
        /// Query the QuadTree for items that are in the given area
        /// </summary>
        /// <param name="queryArea"></pasram>
        /// <returns></returns>
        public List<T> Query(BoundingBox queryArea)
        {
            // create a list of the items that are found
            var results = new List<T>();

            // this quad contains items that are not entirely contained by
            // it's four sub-quads. Iterate through the items in this quad 
            // to see if they intersect.
            foreach (var item in Contents)
            {
                var bounds = item.Bounds;
                if (queryArea.Intersects(ref bounds) != IntersectionType.NoIntersection)
                {
                    results.Add(item);
                }
            }

            foreach (var node in m_nodes)
            {
                if (node.IsEmpty)
                    continue;

                // Case 1: search area completely contained by sub-quad
                // if a node completely contains the query area, go down that branch
                // and skip the remaining nodes (break this loop)
                if (node.Bounds.Contains(ref queryArea))
                {
                    results.AddRange(node.Query(queryArea));
                    break;
                }

                // Case 2: Sub-quad completely contained by search area 
                // if the query area completely contains a sub-quad,
                // just add all the contents of that quad and it's children 
                // to the result set. You need to continue the loop to test 
                // the other quads
                var bounds = node.Bounds;
                if (queryArea.Contains(ref bounds))
                {
                    results.AddRange(node.SubTreeContents);
                    continue;
                }

                // Case 3: search area intersects with sub-quad
                // traverse into this quad, continue the loop to search other
                // quads
                if (node.Bounds.Intersects(ref queryArea) == IntersectionType.Intersects)
                {
                    results.AddRange(node.Query(queryArea));
                }
            }


            return results;
        }

        /// <summary>
        /// Insert an item to this node
        /// </summary>
        public void Insert(T item)
        {
            // if the item is not contained in this quad, there's a problem
            var bounds = item.Bounds;
            if (!Bounds.Contains(ref bounds))
            {
                Trace.TraceWarning("feature is out of the bounds of this quadtree node");
                return;
            }

            // if the subnodes are null create them. may not be sucessfull: see below
            // we may be at the smallest allowed size in which case the subnodes will not be created
            if (m_nodes.Count == 0)
                CreateSubNodes();

            // for each subnode:
            // if the node contains the item, add the item to that node and return
            // this recurses into the node that is just large enough to fit this item
            foreach (var node in m_nodes)
            {
                if (!node.Bounds.Contains(ref bounds)) continue;
                node.Insert(item);
                return;
            }

            // if we make it to here, either
            // 1) none of the subnodes completely contained the item. or
            // 2) we're at the smallest subnode size allowed 
            // add the item to this node's contents.
            Contents.Add(item);
        }

        /// <summary>
        /// Remove an item from this node
        /// </summary>
        public void Remove(T item)
        {
            // if the item is not contained in this quad, there's a problem
            var bounds = item.Bounds;
            if (!Bounds.Contains(ref bounds))
            {
                Trace.TraceWarning("feature is out of the bounds of this quadtree node");
                return;
            }

            // for each subnode:
            // if the node contains the item, add the item to that node and return
            // this recurses into the node that is just large enough to fit this item
            foreach (var node in m_nodes)
            {
                if (!node.Bounds.Contains(ref bounds)) continue;
                
                node.Remove(item);
                return;
            }

            // if we make it to here, either
            // 1) none of the subnodes completely contained the item. or
            // 2) we're at the smallest subnode size allowed 
            // remove the item from this node's contents.
            Contents.Remove(item);
        }

        public void ForEach(zzQuadTree<T>.QTAction action)
        {
            action(this);

            // draw the child quads
            foreach (var node in m_nodes)
                node.ForEach(action);
        }

        /// <summary>
        /// Internal method to create the subnodes (partitions space)
        /// </summary>
        private void CreateSubNodes()
        {
            // the smallest subnode will be chunk-sized 
            if (Bounds.Height <= TerrainConstants.ChunkSize)
                return;
            if (Bounds.Width <= TerrainConstants.ChunkSize)
                return;

            var halfWidth = (Bounds.Width / 2.0f);
            var halfHeight = (Bounds.Height / 2.0f);

            var centerPoint = new Vector2(Bounds.Min.X + halfWidth, Bounds.Min.Y + halfHeight);
            var bottomMiddle = new Vector2(Bounds.Min.X + halfWidth, Bounds.Min.Y);
            var rightMiddle = new Vector2(Bounds.Max.X, Bounds.Max.Y - halfHeight);
            var leftMiddle = new Vector2(Bounds.Min.X, Bounds.Min.Y + halfHeight);
            var topMiddle = new Vector2(Bounds.Max.X - halfWidth, Bounds.Max.Y);

            // Bottom Left
            m_nodes.Add(new zzQuadTreeNode<T>(new BoundingBox(Bounds.Min, new Vector3(centerPoint, Bounds.Max.Z))));

            // Bottom Right
            m_nodes.Add(
                new zzQuadTreeNode<T>(new BoundingBox(new Vector3(bottomMiddle, Bounds.Min.Z),
                                                    new Vector3(rightMiddle, Bounds.Max.Z))));

            // Top Left
            m_nodes.Add(
                new zzQuadTreeNode<T>(new BoundingBox(new Vector3(leftMiddle, Bounds.Min.Z),
                                                    new Vector3(topMiddle, Bounds.Max.Z))));

            // Top Right
            m_nodes.Add(new zzQuadTreeNode<T>(new BoundingBox(new Vector3(centerPoint, Bounds.Min.Z), Bounds.Max)));
        }

    }

    public interface IBounded
    {
        BoundingBox Bounds
        {
            get;
            set;
        }
    }
}