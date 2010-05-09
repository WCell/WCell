using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.DirectX;

namespace WCell.Collision
{
    public static class Extensions
    {

        public static T ReadStruct<T>(this BinaryReader binReader) where T : struct
        {
            var rawData = binReader.ReadBytes(Marshal.SizeOf(typeof(T)));
            var handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
			try
			{
				var returnObject = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
				return returnObject;
			}
			finally
			{
				handle.Free();
			}
        }

        public static string ReadCString(this BinaryReader binReader)
        {
            var str = new StringBuilder();
            char c;
            while ((c = binReader.ReadChar()) != '\0')
            {
                str.Append(c);
            }

            return str.ToString();
        }

        public static byte PeekByte(this BinaryReader binReader)
        {
            var b = binReader.ReadByte();
            binReader.BaseStream.Position -= 1;
            return b;
        }

        public static Vector3 ReadVector3(this BinaryReader binReader)
        {
            return new Vector3(binReader.ReadSingle(), binReader.ReadSingle(), binReader.ReadSingle());
        }

        /*
        public static Plane ReadPlane(this BinaryReader binReader)
        {
            return new Plane(binReader.ReadVector3(), binReader.ReadSingle());
        }*/

        public static string ReadFixedReversedAsciiString(this BinaryReader binReader, int size)
        {
            char[] chars = new char[size];//binReader.ReadChars(size);
            for (int i = 0; i < size; i++)
            {
                chars[i] = (char)binReader.ReadByte();
            }
            Reverse(chars);
            return new string(chars);
        }

        public static void Reverse<T>(this T[] array)
        {
            int length = array.Length;
            for (int i = 0; i < length / 2; i++)
            {
                T temp = array[i];
                array[i] = array[length - i - 1];
                array[length - i - 1] = temp;
            }
        }
    }
}
