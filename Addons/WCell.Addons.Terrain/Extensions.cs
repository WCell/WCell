using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using WCell.Util.Graphics;

namespace WCell.Addons.Terrain
{
    internal static class Extensions
    {
        internal static BoundingBox ReadPackedBoundingBox(this BinaryReader reader)
        {
            return new BoundingBox(reader.ReadPackedVector3(), reader.ReadPackedVector3());
        }

        internal static Vector3 ReadPackedVector3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadPackedFloat(), reader.ReadPackedFloat(), reader.ReadPackedFloat());
        }

        internal static PackedVector3 ReadPackedVector3AsPacked(this BinaryReader reader)
        {
            return new PackedVector3 {
                X = reader.ReadUInt16(),
                Y = reader.ReadUInt16(),
                Z = reader.ReadUInt16()
            };
        }

        internal static OBB ReadPackedOBB(this BinaryReader reader)
        {
            return new OBB(reader.ReadPackedBoundingBox(), reader.ReadPackedMatrix());
        }

        internal static Matrix ReadPackedMatrix(this BinaryReader reader)
        {
            return new Matrix(reader.ReadPackedFloat(), reader.ReadPackedFloat(), reader.ReadPackedFloat(),
                              reader.ReadPackedFloat(), reader.ReadPackedFloat(), reader.ReadPackedFloat(),
                              reader.ReadPackedFloat(), reader.ReadPackedFloat(), reader.ReadPackedFloat(),
                              reader.ReadPackedFloat(), reader.ReadPackedFloat(), reader.ReadPackedFloat(),
                              reader.ReadPackedFloat(), reader.ReadPackedFloat(), reader.ReadPackedFloat(),
                              reader.ReadPackedFloat());
        }

        internal static float ReadPackedFloat(this BinaryReader reader)
        {
            return HalfUtils.Unpack(reader.ReadUInt16());
        }

        internal static Index3 ReadIndex3(this BinaryReader reader)
        {
            return new Index3 {
                Index0 = reader.ReadInt16(),
                Index1 = reader.ReadInt16(),
                Index2 = reader.ReadInt16()
            };
        }

        internal static Vector3 ReadVector3(this BinaryReader reader)
        {
            var X = reader.ReadSingle();
            var Y = reader.ReadSingle();
            var Z = reader.ReadSingle();

            return new Vector3(X, Y, Z);
        }

        internal static BoundingBox ReadBoundingBox(this BinaryReader reader)
        {
            var min = reader.ReadVector3();
            var max = reader.ReadVector3();

            return new BoundingBox(min, max);
        }

        //public static T ReadStruct<T>(this BinaryReader binReader) where T : struct
        //{
        //    byte[] rawData = binReader.ReadBytes(Marshal.SizeOf(typeof(T)));
        //    GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
        //    T returnObject = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        //    handle.Free();
        //    return returnObject;
        //}

        //public static string ReadCString(this BinaryReader binReader)
        //{
        //    StringBuilder str = new StringBuilder();
        //    char c;
        //    while ((c = binReader.ReadChar()) != '\0')
        //    {
        //        str.Append(c);
        //    }

        //    return str.ToString();
        //}

        //public static byte PeekByte(this BinaryReader binReader)
        //{
        //    byte b = binReader.ReadByte();
        //    binReader.BaseStream.Position -= 1;
        //    return b;
        //}

        //public static Vector3 ReadVector3(this BinaryReader binReader)
        //{
        //    return new Vector3(binReader.ReadSingle(), binReader.ReadSingle(), binReader.ReadSingle());
        //}

        //public static Plane ReadPlane(this BinaryReader binReader)
        //{
        //    return new Plane(binReader.ReadVector3(), binReader.ReadSingle());
        //}

        //public static string ReadFixedReversedAsciiString(this BinaryReader binReader, int size)
        //{
        //    char[] chars = new char[size];//binReader.ReadChars(size);
        //    for (int i = 0; i < size; i++)
        //    {
        //        chars[i] = (char)binReader.ReadByte();
        //    }
        //    Reverse(chars);
        //    return new string(chars);
        //}

        //public static void Reverse<T>(this T[] array)
        //{
        //    int length = array.Length;
        //    for (int i = 0; i < length / 2; i++)
        //    {
        //        T temp = array[i];
        //        array[i] = array[length - i - 1];
        //        array[length - i - 1] = temp;
        //    }
        //}
    }
}