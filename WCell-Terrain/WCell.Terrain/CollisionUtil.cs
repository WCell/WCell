using System;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.Terrain
{
    public class CollisionUtil
    {
        /// <summary>
        /// Point of intersection = (1 - u - v)*vert0 + u*vert1 + v*vert2.
        /// t is the distance from ray.Position to the point of intersection.
        /// </summary>
        public static float? RayTriangleIntersect(ref Vector3 vert0, ref Vector3 vert1, ref Vector3 vert2, ref Ray ray, out float u, out float v)
        {
            u = Single.NaN;
            v = Single.NaN;

            var edge1 = vert1 - vert0;
            var edge2 = vert2 - vert0;

            var pVec = Vector3.Cross(ray.Direction, edge2);
            var det = Vector3.Dot(edge1, pVec);

            if (det.IsWithinEpsilon(0.0f)) return null;

            var invDet = 1.0f/det;
            var tVec = ray.Position - vert0;

            u = Vector3.Dot(tVec, pVec)*invDet;
            if (u < 0.0f) return null;
            if (u > 1.0f) return null;

            var qVec = Vector3.Cross(tVec, edge1);
            v = Vector3.Dot(ray.Direction, qVec)*invDet;
            if (v < 0.0f) return null;
            if ((u + v) > 1.0f) return null;

            var t = Vector3.Dot(edge2, qVec)*invDet;
            return t;
        }
    }
}