using System;
using System.Collections.Generic;
using WCell.Util.Graphics;

namespace TerrainDisplay
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
            return ((box.Min + box.Max)*0.5f);
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

        public static Microsoft.Xna.Framework.Vector3 ToXna(this Vector3 vec)
        {
            return new Microsoft.Xna.Framework.Vector3
            {
                X = vec.X,
                Y = vec.Y,
                Z = vec.Z
            };
        }
    }

    public static class MatrixExtensions
    {
        public static Microsoft.Xna.Framework.Matrix ToXna(this Matrix mat)
        {
            return new Microsoft.Xna.Framework.Matrix
            {
                M11 = mat.M11,
                M12 = mat.M12,
                M13 = mat.M13,
                M14 = mat.M14,
                M21 = mat.M21,
                M22 = mat.M22,
                M23 = mat.M23,
                M24 = mat.M24,
                M31 = mat.M31,
                M32 = mat.M32,
                M33 = mat.M33,
                M34 = mat.M34,
                M41 = mat.M41,
                M42 = mat.M42,
                M43 = mat.M43,
                M44 = mat.M44
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