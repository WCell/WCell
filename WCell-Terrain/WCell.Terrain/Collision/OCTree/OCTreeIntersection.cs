using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using WCell.Util.Graphics;

namespace WCell.Terrain.GUI.Collision.OCTree
{
    [StructLayout(LayoutKind.Sequential)]
    public struct OCTreeIntersection : IComparable<OCTreeIntersection>
    {
        public OCTreeNode Node;
        public int PolygonIndex;
        public Vector3 IntersectionPoint;
        public Vector3 IntersectionNormal;
        public float DistanceSquared;
        public float IntersectionDepth;
        public OCTreeIntersectionType IntersectType;
        public int CompareTo(OCTreeIntersection other)
        {
            if (this.IntersectionDepth < other.IntersectionDepth)
            {
                return 1;
            }
            if (this.IntersectionDepth > other.IntersectionDepth)
            {
                return -1;
            }
            return 0;
        }
    }
}
