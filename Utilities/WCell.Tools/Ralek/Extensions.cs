/*************************************************************************
 *
 *   file		: Extensions.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-07 07:58:12 +0100 (lø, 07 mar 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 784 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace WCell.Tools.Ralek
{
    public static class Extensions
    {
        /*
         public static unsafe T ReadStruct<T>(this BinaryReader binReader) where T : struct
         {
             byte[] rawData = binReader.ReadBytes(Marshal.SizeOf(typeof(T)));
             GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
             T returnObject = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
             handle.Free();
             return returnObject;
         }*/

        public static void Swap<T>(this T[] array, int index1, int index2)
        {
            T temp = array[index1];
            array[index1] = array[index2];
            array[index2] = temp;
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

        public static unsafe T ReadStruct<T>(this BinaryReader binReader) where T : struct
        {
            byte[] rawData = binReader.ReadBytes(Marshal.SizeOf(typeof(T)));
            GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
            T returnObject = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return returnObject;
        }

        public static T ReadStruct<T>(this Stream stream) where T : struct
        {
            if (!stream.CanRead)
                return default(T);

            int size = Marshal.SizeOf(typeof(T));
            byte[] rawData = new byte[size];
            stream.Read(rawData, (int)stream.Position, size);

            GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
            T returnObject = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return returnObject;
        }

        public static string ReadCString(this BinaryReader binReader)
        {
            StringBuilder str = new StringBuilder();
            char c;
            while ((c = binReader.ReadChar()) != '\0')
            {
                str.Append(c);
            }

            return str.ToString();
        }

        public static byte PeekByte(this BinaryReader binReader)
        {
            byte b = binReader.ReadByte();
            binReader.BaseStream.Position -= 1;
            return b;
        }

        public static ulong ReadPackedGUID(this BinaryReader binReader)
        {
            ulong fullGUID = 0;
            byte guidMask = binReader.ReadByte();

            for (int i = 0; i < 8; i++)
            {
                if ((guidMask & (1 << i)) != 0)
                {
                    fullGUID |= ((ulong)binReader.ReadByte() << (i * 8));
                }
            }

            return fullGUID;
        }
    }
}