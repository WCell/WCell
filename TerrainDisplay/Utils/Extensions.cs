using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using MPQNav.MPQ;
using MPQNav.MPQ.WMO;

namespace MPQNav.Util
{
    public static class Extensions
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
            return new Index3 {
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

            for (int i = 0; i < size;i++ )
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
    }
}
