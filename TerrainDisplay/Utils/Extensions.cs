using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TerrainDisplay.MPQ;
using TerrainDisplay.MPQ.WMO;
using WCell.Util.Graphics;

namespace TerrainDisplay.Util
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

        public static bool IsNullOrEmpty(this Array array)
        {
            if (array == null) return true;
            return (array.Length == 0);
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

        public static Vector3 ToWCell(this Microsoft.Xna.Framework.Vector3 vec)
        {
            return new Vector3(vec.X, vec.Y, vec.Z);
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
        public const float Epsilon = 0.00001f;

        public static bool NearlyZero(this float val)
        {
            return (val < Epsilon) && (val > -Epsilon);
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
            if (!(v2.X - v1.X).NearlyZero()) return false;
            if (!(v2.Y - v1.Y).NearlyZero()) return false;
            return (v2.Z - v1.Z).NearlyZero();
        }

        public static bool NearlyEquals(this float f1, float f2)
        {
            return (f2 - f1).NearlyZero();
        }

        public static Vector3 ABS(this Vector3 v)
        {
            return new Vector3(Math.Abs(v.X), Math.Abs(v.Y), Math.Abs(v.Z));
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

    public static class IOExtensions
    {
        public static OffsetLocation ReadOffsetLocation(this BinaryReader br)
        {
            return new OffsetLocation
            {
                Count = br.ReadInt32(),
                Offset = br.ReadInt32()
            };
        }

        public static void ReadOffsetLocation(this BinaryReader br, ref OffsetLocation offsetLoc)
        {
            offsetLoc = new OffsetLocation
            {
                Count = br.ReadInt32(),
                Offset = br.ReadInt32()
            };
        }

        public static Vector2 ReadVector2(this BinaryReader br)
        {
            return new Vector2(br.ReadSingle(), br.ReadSingle());
        }

        public static Vector3 ReadVector3(this BinaryReader br)
        {
            return new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }

        public static Vector3 ReadWMOVector3(this BinaryReader br)
        {
            var X = br.ReadSingle();
            var Y = br.ReadSingle();
            var Z = br.ReadSingle();
            return new Vector3(X, Y, Z);
        }

        public static Index3 ReadIndex3(this BinaryReader br)
        {
            return new Index3
            {
                Index0 = br.ReadInt16(),
                Index1 = br.ReadInt16(),
                Index2 = br.ReadInt16()
            };
        }

        public static Quaternion ReadQuaternion(this BinaryReader br)
        {
            return new Quaternion(br.ReadVector3(), br.ReadSingle());
        }

        public static BoundingBox ReadBoundingBox(this BinaryReader br)
        {
            return new BoundingBox(br.ReadVector3(), br.ReadVector3());
        }

        public static Plane ReadPlane(this BinaryReader br)
        {
            return new Plane(br.ReadVector3(), br.ReadSingle());
        }

        public static Color4 ReadColor4(this BinaryReader br)
        {
            return new Color4
            {
                B = br.ReadByte(),
                G = br.ReadByte(),
                R = br.ReadByte(),
                A = br.ReadByte()
            };
        }

        public static Matrix ReadMatrix(this BinaryReader br)
        {
            return new Matrix(
                br.ReadSingle(),
                br.ReadSingle(),
                br.ReadSingle(),
                br.ReadSingle(),
                br.ReadSingle(),
                br.ReadSingle(),
                br.ReadSingle(),
                br.ReadSingle(),
                br.ReadSingle(),
                br.ReadSingle(),
                br.ReadSingle(),
                br.ReadSingle(),
                br.ReadSingle(),
                br.ReadSingle(),
                br.ReadSingle(),
                br.ReadSingle()
                );
        }

        public static List<int> ReadInt32List(this BinaryReader br)
        {
            var count = br.ReadInt32();
            var list = new List<int>(count);
            for (var i = 0; i < count; i++)
            {
                list.Add(br.ReadInt32());
            }
            return list;
        }

        public static int[] ReadInt32Array(this BinaryReader br)
        {
            var length = br.ReadInt32();
            if (length == 0) return null;

            var array = new int[length];
            for (var i = 0; i < length; i++)
            {
                array[i] = br.ReadInt32();
            }
            return array;
        }

        public static List<Vector3> ReadVector3List(this BinaryReader br)
        {
            var count = br.ReadInt32();
            var list = new List<Vector3>(count);
            for (var i = 0; i < count; i++)
            {
                list.Add(br.ReadVector3());
            }
            return list;
        }

        public static List<Index3> ReadIndex3List(this BinaryReader br)
        {
            var count = br.ReadInt32();
            var list = new List<Index3>(count);
            for (var i = 0; i < count; i++)
            {
                list.Add(br.ReadIndex3());
            }
            return list;
        }

        public static Rect ReadRect(this BinaryReader br)
        {
            var x = br.ReadSingle();
            var y = br.ReadSingle();
            var width = br.ReadSingle();
            var height = br.ReadSingle();

            return new Rect(x, y, width, height);
        }

        /// <summary>
        /// Reads a C-style null-terminated string from the current stream.
        /// </summary>
        /// <param name="binReader">the extended <see cref="BinaryReader" /> instance</param>
        /// <returns>the string being reader</returns>
        public static string ReadCString(this BinaryReader binReader)
        {
            StringBuilder sb = new StringBuilder();
            byte c;

            while ((c = binReader.ReadByte()) != 0)
            {
                sb.Append((char)c);
            }

            return sb.ToString();
        }

        public static byte PeekByte(this BinaryReader binReader)
        {
            byte b = binReader.ReadByte();
            binReader.BaseStream.Position -= 1;
            return b;
        }

        public static string ReadFixedString(this BinaryReader br, int size)
        {
            var bytes = br.ReadBytes(size);

            for (int i = 0; i < size; i++)
            {
                if (bytes[i] == 0)
                {
                    return Encoding.ASCII.GetString(bytes, 0, i);
                }
            }

            return Encoding.ASCII.GetString(bytes);
        }

        public static bool HasData(this BinaryReader br)
        {
            return br.BaseStream.Position < br.BaseStream.Length;
        }

        public static void Write(this BinaryWriter writer, Index3 idx)
        {
            writer.Write(idx.Index0);
            writer.Write(idx.Index1);
            writer.Write(idx.Index2);
        }

        public static void Write(this BinaryWriter writer, Vector3 vector3)
        {
            writer.Write(vector3.X);
            writer.Write(vector3.Y);
            writer.Write(vector3.Z);
        }

        public static void Write(this BinaryWriter writer, BoundingBox box)
        {
            writer.Write(box.Min);
            writer.Write(box.Max);
        }

        public static void Write(this BinaryWriter writer, Matrix mat)
        {
            writer.Write(mat.M11);
            writer.Write(mat.M12);
            writer.Write(mat.M13);
            writer.Write(mat.M14);
            writer.Write(mat.M21);
            writer.Write(mat.M22);
            writer.Write(mat.M23);
            writer.Write(mat.M24);
            writer.Write(mat.M31);
            writer.Write(mat.M32);
            writer.Write(mat.M33);
            writer.Write(mat.M34);
            writer.Write(mat.M41);
            writer.Write(mat.M42);
            writer.Write(mat.M43);
            writer.Write(mat.M44);
        }

        public static void Write(this BinaryWriter writer, ICollection<int> list)
        {
            if (list == null)
            {
                writer.Write(0);
                return;
            }

            writer.Write(list.Count);
            foreach (var item in list)
            {
                writer.Write(item);
            }
        }

        public static void Write(this BinaryWriter writer, ICollection<Vector3> list)
        {
            writer.Write(list.Count);
            foreach (var item in list)
            {
                writer.Write(item);
            }
        }

        public static void Write(this BinaryWriter writer, ICollection<Index3> list)
        {
            writer.Write(list.Count);
            foreach (var item in list)
            {
                writer.Write(item);
            }
        }

        public static void Write(this BinaryWriter writer, Rect rect)
        {
            writer.Write(rect.X);
            writer.Write(rect.Y);
            writer.Write(rect.Width);
            writer.Write(rect.Height);
        }
    }
}