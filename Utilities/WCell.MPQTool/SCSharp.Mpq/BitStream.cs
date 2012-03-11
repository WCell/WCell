/*************************************************************************
 *
 *   file		: BitStream.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-01-31 19:35:36 +0800 (Thu, 31 Jan 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 87 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.IO;

namespace MpqReader
{
    /// <summary>
    /// A utility class for reading groups of bits from a stream
    /// </summary>
    internal class BitStream
    {
        private readonly Stream mStream;
        private int mCurrent;
        private int mBitCount;

        public BitStream(Stream SourceStream)
        {
            mStream = SourceStream;
        }

        public Stream BaseStream
        { get { return mStream; } }

        public int ReadBits(int BitCount)
        {
            if (BitCount > 16)
                throw new ArgumentOutOfRangeException("BitCount", "Maximum BitCount is 16");
            if (EnsureBits(BitCount) == false) return -1;
            int result = mCurrent & (0xffff >> (16 - BitCount));
            WasteBits(BitCount);
            return result;
        }

        public int PeekByte()
        {
            if (EnsureBits(8) == false) return -1;
            return mCurrent & 0xff;
        }

        public bool EnsureBits(int BitCount)
        {
            if (BitCount <= mBitCount) return true;

            if (mStream.Position >= mStream.Length) return false;
            int nextvalue = mStream.ReadByte();
            mCurrent |= nextvalue << mBitCount;
            mBitCount += 8;
            return true;
        }

        private bool WasteBits(int BitCount)
        {
            mCurrent >>= BitCount;
            mBitCount -= BitCount;
            return true;
        }
    }
}