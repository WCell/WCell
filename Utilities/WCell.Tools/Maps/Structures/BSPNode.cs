using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WCell.Util.Graphics;

namespace WCell.Tools.Maps.Structures
{
    public class BSPTree
    {
        public readonly short rootId;
        public readonly BSPNode[] nodes;

        public BSPNode Root
        {
            get { return nodes[rootId]; }
        }
        
        public BSPTree(IList<BSPNode> nodes)
        {
            this.nodes = nodes.ToArray();
            rootId = FindRootNode(nodes);
            
            if (rootId == short.MinValue)
            {
                throw new InvalidDataException("No root node found for this BSP tree.");
            }
        }

        public BSPTree(short rootId, BSPNode[] nodes)
        {
            this.rootId = rootId;
            this.nodes = nodes;
        }

        private static short FindRootNode(IList<BSPNode> nodes)
        {
            for (short rootId = 0; rootId < nodes.Count; rootId++)
            {
                var node = nodes[rootId];
                if (node == null) continue;

                var found = false;
                for (var j = 0; j < nodes.Count; j++)
                {
                    // don't check a node against itself
                    if (rootId == j) continue;

                    var parent = nodes[j];
                    if (parent == null) continue;

                    // this node is the child to another node
                    if (parent.posChild != rootId && parent.negChild != rootId) continue;

                    found = true;
                    break;
                }
                if (!found) return rootId;
            }
            return short.MinValue;
        }

        //private static void VisitNodes(BSPNode node, Ray ray, float tMax, Action<BSPNode> callback)
        //{
        //    if (node == null) return;
        //    if ((node.flags & BSPNodeFlags.Flag_Leaf) != 0)
        //    {
        //        // Do stuff here
        //        callback(node);
        //    }

        //    //Figure out which child to recurse into first
        //    float startVal;
        //    float dirVal;
        //    if ((node.flags & BSPNodeFlags.Flag_XAxis) != 0)
        //    {
        //        startVal = ray.Position.X;
        //        dirVal = ray.Direction.X;
        //    }
        //    else if ((node.flags & BSPNodeFlags.Flag_YAxis) != 0)
        //    {
        //        startVal = ray.Position.Y;
        //        dirVal = ray.Direction.Y;
        //    }
        //    else
        //    {
        //        startVal = ray.Position.Z;
        //        dirVal = ray.Direction.Z;
        //    }


        //    BSPNode first;
        //    BSPNode last;
        //    if (startVal > node.planeDist)
        //    {
        //        first = node.Positive;
        //        last = node.Negative;
        //    }
        //    else
        //    {
        //        first = node.Negative;
        //        last = node.Positive;
        //    }

        //    if (dirVal == 0.0)
        //    {
        //        // Segment is parallel to the splitting plane, visit the near side only.
        //        VisitNodes(first, ray, tMax, callback);
        //        return;
        //    }

        //    // The t-value for the intersection with the boundary plane
        //    var tIntersection = (node.planeDist - startVal) / dirVal;

        //    VisitNodes(first, ray, tMax, callback);

        //    // Test if line segment straddles the boundary plane
        //    if (0.0f > tIntersection || tIntersection > tMax) return;

        //    // It does, visit the far side
        //    VisitNodes(last, ray, tMax, callback);
        //}
    }

    public class BSPNode
    {
        public BSPNodeFlags flags;
        public short negChild;
        public short posChild;
        /// <summary>
        /// num of triangle faces in  MOBR
        /// </summary>
        public ushort nFaces;
        /// <summary>
        /// index of the first triangle index(in  MOBR)
        /// </summary>
        public uint faceStart;
        public float planeDist;
        public Index3[] TriIndices;
    }

    [Flags]
    public enum BSPNodeFlags : ushort
    {
        Flag_XAxis = 0x0,
        Flag_YAxis = 0x1,
        Flag_ZAxis = 0x2,
        Flag_AxisMask = Flag_XAxis | Flag_YAxis | Flag_ZAxis,
        Flag_Leaf = 0x4,
        Flag_NoChild = 0xffff,
    }
}