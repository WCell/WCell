using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Graphics;

namespace WCell.Collision
{
    public class M2 : IBounded
    {
        public BoundingBox Bounds
        {
            get;
            set;
        }

        public Vector3[] Vertices;
        public Index3[] Triangles;


        /// <summary>
        /// Returns the earliest time at which a ray intersects the polys in this model.
        /// Notice that the ray should be in world coords.
        /// </summary>
        public float? ShortestDistanceTo(ref Ray ray, ref float tMax)
        {
            // Does the ray intersect the outer bounding box for this model?
            float? result;
            Bounds.Intersects(ref ray, out result);
            if (result == null) return null;

            // Get the first time this ray intersects any of the triangles in this model
            result = tMax;
            for (var i = 0; i < Triangles.Length; i++)
            {
                var triangle = Triangles[i];
                float u, v;
                var newResult = CollisionHelper.RayTriangleIntersect(ref Vertices[triangle.Index0],
                                                             ref Vertices[triangle.Index1],
                                                             ref Vertices[triangle.Index2], 
                                                             ref ray, out u, out v);
                if (newResult == null) continue;

                result = Math.Min(result.Value, newResult.Value);
            }

            return (result < tMax) ? result : null;
        }

        internal bool CollidesWithRay(ref Ray ray, ref float tMax)
        {
            float? result;
            Bounds.Intersects(ref ray, out result);
            if (result == null) return false;

            for (var i = 0; i < Triangles.Length; i++)
            {
                var triangle = Triangles[i];
                float u, v;
                var distance = CollisionHelper.RayTriangleIntersect(ref Vertices[triangle.Index0],
                                                             ref Vertices[triangle.Index1],
                                                             ref Vertices[triangle.Index2],
                                                             ref ray, out u, out v);
                if (distance == null) continue;
                if (distance < tMax)
                {
                    return true;
                }
            }

            return false;
        }
    }
}