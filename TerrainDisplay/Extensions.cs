using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MPQNav.Collision
{
    ///<summary>
    /// Adds functonality to the BoundingBox class
    ///</summary>
    public static class BoxExtensions
    {
        /// <summary>
        /// Calculates the vector at the center of the box
        /// </summary>
        /// <returns>A Vector3 that points to the center of the BoundingBox.</returns>
        public static Vector3 Center(this BoundingBox box)
        {
            return (0.5f * (box.Min + box.Max));
        }

        /// <summary>
        /// Returns Box.Max in Box coordinates
        /// </summary>
        public static Vector3 Extents(this BoundingBox box)
        {
            return (box.Max - box.Center());
        }
    }

    public static class CollectionExtensions
    {
        public static void AddUnique<T>(this ICollection<T> collection, T item)
        {
            if (collection.Contains(item)) return;
            collection.Add(item);
        }
    }

    public static class VectorExtensions
    {
        //public static double[] ToDoubleArray(this Vector3 vector)
        //{
        //    return new double[] {
        //        vector.X, vector.Y, vector.Z
        //    };
        //}

        public static float[] ToFloatArray(this Vector3 vector)
        {
            return new[] {
                vector.X, vector.Y, vector.Z
            };
        }
    }

    public static class MathExtensions
    {
        public static float Epsilon = 0.0001f;

        public static bool NearlyZero(this float val)
        {
            return val < float.Epsilon &&
                   val > -float.Epsilon;
        }

        public static float Cos(this float angle)
        {
            return (float)Math.Cos(angle);
        }

        public static float CosABS(this float angle)
        {
            return (float)Math.Abs(Math.Cos(angle));
        }

        public static float Sin(this float angle)
        {
            return (float)Math.Sin(angle);
        }

        public static bool NearlyEqual(this Vector3 v1,
                                       Vector3 v2, float delta)
        {
            return (v2 - v1).LengthSquared() < delta;
        }

        public static Vector3 ABS(this Vector3 v)
        {
            return new Vector3((float)Math.Abs(v.X), (float)Math.Abs(v.Y), (float)Math.Abs(v.Z));
        }

        public static Plane PlaneFromPointNormal(Vector3 point, Vector3 normal)
        {
            return new Plane(normal, -Vector3.Dot(normal, point));
        }

        public static Vector3 NormalFromPoints(Vector3 a, Vector3 b, Vector3 c)
        {
            return Vector3.Normalize(Vector3.Cross(a - b, c - b));
        }

    }
}