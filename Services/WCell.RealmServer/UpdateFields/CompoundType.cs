/*************************************************************************
 *
 *   file		: UpdateValue.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-02-16 12:30:51 +0800 (Mon, 16 Feb 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 757 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace WCell.RealmServer.UpdateFields
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct CompoundType
    {
        [FieldOffset(0)]
        public byte Byte1;
        [FieldOffset(1)]
        public byte Byte2;
        [FieldOffset(2)]
        public byte Byte3;
        [FieldOffset(3)]
        public byte Byte4;

        [FieldOffset(0)]
        public float Float;

        [FieldOffset(0)]
        public short Int16Low;
        [FieldOffset(2)]
        public short Int16High;

        [FieldOffset(0)]
        public int Int32;

        [FieldOffset(0)]
        public ushort UInt16Low;
        [FieldOffset(2)]
        public ushort UInt16High;
        [FieldOffset(0)]
        public uint UInt32;

        //public UpdateValue(float val)
        //{
        //    Int32 = Int16Low = Int16High = 0;
        //    UInt32 = UInt16Low = UInt16High = 0;
        //    Float = val;
        //}

        //public UpdateValue(uint val)
        //{
        //    Float = 0f;
        //    UInt16Low = UInt16High = 0;
        //    Int32 = Int16Low = Int16High = 0;
        //    UInt32 = val;
        //}

        //public UpdateValue(byte[] val)
        //{
        //    Float = 0f;
        //    UInt32 = UInt16Low = UInt16High = 0;
        //    Int32 = Int16Low = Int16High = 0;
        //    ByteArray = val;
        //}

        public unsafe byte[] ByteArray
        {
            get
            {
                var buffer = new byte[4];
                fixed (byte* pBuffer = buffer)
                {
                    *(uint*)pBuffer = UInt32;
                }
                return buffer;
            }
            set
            {
                fixed (byte* pBuffer = &value[0])
                {
                    UInt32 = *(uint*)pBuffer;
                }
            }
        }

        public void SetByte(int index, byte value)
        {
            // clear the old value
            UInt32 &= ~(uint)(0xFF << (index * 8));
            // add the new value
            UInt32 |= (uint)(value << (index * 8));
        }

        public unsafe void SetByteUnsafe(int index, byte value)
        {
            *(((byte*)UInt32) + index) = value;
        }

        public byte GetByte(int index)
        {
            return (byte)(UInt32 >> (index * 8));
        }

        public unsafe byte GetByteUnsafe(int index)
        {
            return *(((byte*)UInt32) + index);
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is CompoundType && Equals((CompoundType)obj);
        }

        private bool Equals(CompoundType obj)
        {
            return UInt32 == obj.UInt32;
        }

        public override int GetHashCode()
        {
            return Int32;
        }

        public static bool operator ==(CompoundType lhs, CompoundType rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(CompoundType lhs, CompoundType rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}