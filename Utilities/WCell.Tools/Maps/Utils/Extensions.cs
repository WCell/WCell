using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WCell.Core;
using WCell.Tools.Maps.Structures;
using WCell.Util.Graphics;

namespace WCell.Tools.Maps.Utils
{
    public static class Extensions
    {
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

        public static Index3 ReadIndex3(this BinaryReader reader)
        {
            var idx0 = reader.ReadInt16();
            var idx1 = reader.ReadInt16();
            var idx2 = reader.ReadInt16();

            return new Index3
            {
                Index0 = idx0,
                Index1 = idx1,
                Index2 = idx2
            };
        }

        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            var X = reader.ReadSingle();
            var Y = reader.ReadSingle();
            return new Vector2(X, Y);
        }

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

        /// <summary>
        /// Reads a C-style null-terminated string from the current stream.
        /// </summary>
        /// <param name="binReader">the extended <see cref="BinaryReader" /> instance</param>
        /// <returns>the string being reader</returns>
        public static string ReadCString(this BinaryReader binReader)
        {
            var sb = new StringBuilder();
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

            for (var i = 0; i < size;i++ )
            {
                if (bytes[i] == 0)
                {
					return WCellConstants.DefaultEncoding.GetString(bytes, 0, i);
                }
            }

			return WCellConstants.DefaultEncoding.GetString(bytes);
        }

        public static bool HasData(this BinaryReader br)
        {
            return br.BaseStream.Position < br.BaseStream.Length;
        }

        public static bool IsNullOrEmpty(this Array array)
        {
            if (array == null) return true;
            return (array.Length == 0);
        }
    }
}