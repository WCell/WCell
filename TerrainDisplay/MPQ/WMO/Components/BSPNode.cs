using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using TerrainDisplay.Collision._3D;

namespace TerrainDisplay.MPQ.WMO.Components
{
    public class BSPTree
    {
        public short rootId;
        public List<BSPNode> nodes;
        private BoundingBox bounds;

        public BSPTree(List<BSPNode> nodes)
        {
            var id = FindRootNodeId(nodes);
            if (id == short.MinValue)
            {
                throw new InvalidDataException("No root node found for this BSP tree.");
            }
            rootId = id;
            this.nodes = nodes;

            //root.HookUpChildren(nodes);
            //Min = root.GetTreeMin(new Vector3(float.MaxValue));
            //Max = root.GetTreeMax(new Vector3(float.MinValue));
            //bounds = new BoundingBox(Min, Max);
        }

        public BSPTree(List<BSPNode> nodes, short rootId)
        {
            this.rootId = rootId;
            this.nodes = nodes;
        }

        //public List<Vector3> Vertices
        //{
        //    get
        //    {
        //        var newList = new List<Vector3>();
        //        root.GetVertices(newList);
        //        return newList;
        //    }
        //}

        public List<int> Indices
        {
            get
            {
                var newList = new List<int>();
                foreach (var node in nodes)
                {
                    if (node.TriIndices == null) continue;
                    foreach (var index3 in node.TriIndices)
                    {
                        newList.Add(index3.Index0);
                        newList.Add(index3.Index1);
                        newList.Add(index3.Index2);
                    }
                }
                return newList;
            }
        }

        //public Vector3 UnitNormal
        //{
        //    get { return Vector3.Up; }
        //}

        //public Vector3 Min
        //{
        //    get;
        //    private set;
        //}

        //public Vector3 Max
        //{
        //    get;
        //    private set;
        //}

        //public float? IntersectsWith(Ray ray)
        //{
        //    var minDist = float.MaxValue;
        //    // Check the stored triangles for intersection and return the smallest distance.
        //    VisitNodes(root, ray, float.MaxValue, (node) =>{
        //        if (node.PolygonSet == null) return;
        //        foreach (var triangle in node.PolygonSet)
        //        {
        //            var dist = triangle.IntersectsWith(ray);
        //            if (!dist.HasValue) continue;
        //            minDist = Math.Min(minDist, dist.Value);
        //        }
        //    });
        //    if (minDist == float.MaxValue) return null;
        //    return minDist;
        //}

        private static short FindRootNodeId(IList<BSPNode> nodes)
        {
            for(var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node == null) continue;

                var found = false;
                for (var j = 0; j < nodes.Count; j++)
                {
                    // don't check a node against itself
                    if (i == j) continue;

                    var parent = nodes[j];
                    if (parent == null) continue;
                    // this node is the child to another node
                    if (parent.posChild != i && parent.negChild != i) continue;
                    found = true;
                    break;
                }
                if (!found) return (short)i;
            }
            return short.MinValue;
        }

        private static void VisitNodes(BSPNode node, Ray ray, float tMax, Action<BSPNode> callback)
        {
            if (node == null) return;
            if ((node.flags & BSPNodeFlags.Flag_Leaf) != 0)
            {
                // Do stuff here
                callback(node);
            }

            //Figure out which child to recurse into first
            float startVal; 
            float dirVal;
            if ((node.flags & BSPNodeFlags.Flag_XAxis) != 0)
            {
                startVal = ray.Position.X;
                dirVal = ray.Direction.X;
            }
            else if((node.flags & BSPNodeFlags.Flag_YAxis) != 0)
            {
                startVal = ray.Position.Y;
                dirVal = ray.Direction.Y;
            }
            else
            {
                startVal = ray.Position.Z;
                dirVal = ray.Direction.Z;
            }
            

            BSPNode first;
            BSPNode last;
            if (startVal > node.planeDist)
            {
                first = node.Positive;
                last = node.Negative;
            }
            else
            {
                first = node.Negative;
                last = node.Positive;
            }

            if (dirVal == 0.0)
            {
                // Segment is parallel to the splitting plane, visit the near side only.
                VisitNodes(first, ray, tMax, callback);
                return;
            }

            // The t-value for the intersection with the boundary plane
            var tIntersection = (node.planeDist - startVal) / dirVal;

            VisitNodes(first, ray, tMax, callback);

            // Test if line segment straddles the boundary plane
            if (0.0f > tIntersection || tIntersection > tMax) return;

            // It does, visit the far side
            VisitNodes(last, ray, tMax, callback);
        }
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

        public BSPNode Positive;
        public BSPNode Negative;
        public List<Index3> TriIndices;


        //public void Dump(StreamWriter file)
        //{
        //    file.WriteLine("Flags: " + flags);
        //    file.WriteLine("negChild: " + negChild);
        //    file.WriteLine("posChild: " + posChild);
        //    file.WriteLine("nFaces: " + nFaces);
        //    file.WriteLine("faceStart: " + faceStart);
        //    file.WriteLine("planeDist: " + planeDist);

        //    if (PolygonSet != null)
        //    {
        //        file.WriteLine("PolygonSet");
        //        for (var i = 0; i < PolygonSet.Length; i++)
        //        {
        //            file.WriteLine("\tVertex[{0}]:", i);
        //            file.WriteLine("\t\tPoint1: {0}", PolygonSet[i].Point1);
        //            file.WriteLine("\t\tPoint2: {0}", PolygonSet[i].Point2);
        //            file.WriteLine("\t\tPoint3: {0}", PolygonSet[i].Point3);
        //        }
        //    }

        //    file.WriteLine();
        //}

        //internal void HookUpChildren(IList<BSPNode> nodes)
        //{
        //    if ((flags & BSPNodeFlags.Flag_Leaf) != 0 ||
        //        (flags & BSPNodeFlags.Flag_NoChild) != 0) return;

        //    var positiveChild = nodes[posChild];
        //    var negativeChild = nodes[negChild];
        //    if (positiveChild == null || negativeChild == null) return;

        //    Positive = positiveChild;
        //    Negative = negativeChild;

        //    Positive.HookUpChildren(nodes);
        //    Negative.HookUpChildren(nodes);
        //}
        
        //internal void GetVertices(List<Vector3> vertices)
        //{
        //    if (TriIndices != null)
        //    {
        //        foreach (var index in TriIndices)
        //        {
        //            vertices.Add(triangle.Point1);
        //            vertices.Add(triangle.Point2);
        //            vertices.Add(triangle.Point3);
        //        }
        //    }

        //    if (Positive != null) Positive.GetVertices(vertices);
        //    if (Negative != null) Negative.GetVertices(vertices);
        //}

        internal void GetIndices(List<int> indices)
        {
            if (TriIndices != null)
            {
                foreach (var triangle in TriIndices)
                {
                    indices.Add(triangle.Index0);
                    indices.Add(triangle.Index1);
                    indices.Add(triangle.Index2);
                }
            }

            if (Positive != null) Positive.GetIndices(indices);
            if (Negative != null) Negative.GetIndices(indices);
        }

        //internal Vector3 GetTreeMin(Vector3 min)
        //{
        //    if (TriIndices != null)
        //    {
        //        foreach (var triangle in TriIndices)
        //        {
        //            min = Vector3.Min(min, triangle.Min);
        //        }
        //    }

        //    if (Positive != null) min = Vector3.Min(Positive.GetTreeMin(min), min);
        //    if (Negative != null) min = Vector3.Min(Negative.GetTreeMin(min), min);
        //    return min;
        //}

        //internal Vector3 GetTreeMax(Vector3 max)
        //{
        //    if (TriIndices != null)
        //    {
        //        foreach (var triangle in TriIndices)
        //        {
        //            max = Vector3.Max(max, triangle.Max);
        //        }
        //    }

        //    if (Positive != null) max = Vector3.Max(Positive.GetTreeMin(max), max);
        //    if (Negative != null) max = Vector3.Max(Negative.GetTreeMin(max), max);
        //    return max;
        //}
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
