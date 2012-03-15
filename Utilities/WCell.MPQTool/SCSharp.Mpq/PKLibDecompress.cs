/*************************************************************************
 *
 *   file		: PKLibDecompress.cs
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
    internal enum CompressionType
    {
        Binary = 0,
        Ascii = 1
    }

    /// <summary>
    /// A decompressor for PKLib implode/explode
    /// </summary>
    public class PKLibDecompress
    {
        private readonly BitStream mStream;

        private readonly CompressionType mCType;
        private readonly int mDSizeBits;	// Dictionary size in bits

        private static readonly byte[] sPosition1;
        private static readonly byte[] sPosition2;

        private static readonly byte[] sLenBits =
		{
			3, 2, 3, 3, 4, 4, 4, 5, 5, 5, 5, 6, 6, 6, 7, 7
		};

        private static readonly byte[] sLenCode =
		{
			5, 3, 1, 6, 10, 2, 12, 20, 4, 24, 8, 48, 16, 32, 64, 0
		};

        private static readonly byte[] sExLenBits =
		{
			0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8
		};

        private static readonly UInt16[] sLenBase =
		{
			0x0000, 0x0001, 0x0002, 0x0003, 0x0004, 0x0005, 0x0006, 0x0007,
			0x0008, 0x000A, 0x000E, 0x0016, 0x0026, 0x0046, 0x0086, 0x0106
		};

        private static readonly byte[] sDistBits =
		{
			2, 4, 4, 5, 5, 5, 5, 6, 6, 6, 6, 6, 6, 6, 6, 6,
			6, 6, 6, 6, 6, 6, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8
		};

        private static readonly byte[] sDistCode =
		{
		    0x03, 0x0D, 0x05, 0x19, 0x09, 0x11, 0x01, 0x3E, 0x1E, 0x2E, 0x0E, 0x36, 0x16, 0x26, 0x06, 0x3A,
		    0x1A, 0x2A, 0x0A, 0x32, 0x12, 0x22, 0x42, 0x02, 0x7C, 0x3C, 0x5C, 0x1C, 0x6C, 0x2C, 0x4C, 0x0C,
		    0x74, 0x34, 0x54, 0x14, 0x64, 0x24, 0x44, 0x04, 0x78, 0x38, 0x58, 0x18, 0x68, 0x28, 0x48, 0x08,
		    0xF0, 0x70, 0xB0, 0x30, 0xD0, 0x50, 0x90, 0x10, 0xE0, 0x60, 0xA0, 0x20, 0xC0, 0x40, 0x80, 0x00
		};

        static PKLibDecompress()
        {
            sPosition1 = GenerateDecodeTable(sDistBits, sDistCode);
            sPosition2 = GenerateDecodeTable(sLenBits, sLenCode);
        }

        public PKLibDecompress(Stream Input)
        {
            mStream = new BitStream(Input);

            mCType = (CompressionType)Input.ReadByte();
            if (mCType != CompressionType.Binary && mCType != CompressionType.Ascii)
                throw new MpqParserException("Invalid compression type: " + mCType);

            mDSizeBits = Input.ReadByte();
            // This is 6 in test cases
            if (4 > mDSizeBits || mDSizeBits > 6)
                throw new MpqParserException("Invalid dictionary size: " + mDSizeBits);
        }

        public byte[] Explode(int ExpectedSize)
        {
            var outputbuffer = new byte[ExpectedSize];
            Stream outputstream = new MemoryStream(outputbuffer);

            int instruction;
            while ((instruction = DecodeLit()) != -1)
            {
                if (instruction < 0x100)
                {
                    outputstream.WriteByte((byte)instruction);
                }
                else
                {
                    // If instruction is greater than 0x100, it means "Repeat n - 0xFE bytes"
                    int copylength = instruction - 0xFE;
                    int moveback = DecodeDist(copylength);
                    if (moveback == 0) break;

                    int source = (int)outputstream.Position - moveback;
                    // We can't just outputstream.Write the section of the array
                    // because it might overlap with what is currently being written
                    while (copylength-- > 0)
                        outputstream.WriteByte(outputbuffer[source++]);
                }
            }

            if (outputstream.Position == ExpectedSize)
            {
                return outputbuffer;
            }

            // Resize the array
            var result = new byte[outputstream.Position];
            Array.Copy(outputbuffer, 0, result, 0, result.Length);
            return result;
        }

        // Return values:
        // 0x000 - 0x0FF : One byte from compressed file.
        // 0x100 - 0x305 : Copy previous block (0x100 = 1 byte)
        // -1            : EOF
        private int DecodeLit()
        {
            switch (mStream.ReadBits(1))
            {
                case -1:
                    return -1;

                case 1:
                    // The next bits are position in buffers
                    int pos = sPosition2[mStream.PeekByte()];

                    // Skip the bits we just used
                    if (mStream.ReadBits(sLenBits[pos]) == -1) return -1;

                    int nbits = sExLenBits[pos];
                    if (nbits != 0)
                    {
                        // TODO: Verify this conversion
                        int val2 = mStream.ReadBits(nbits);
                        if (val2 == -1 && (pos + val2 != 0x10e)) return -1;

                        pos = sLenBase[pos] + val2;
                    }
                    return pos + 0x100; // Return number of bytes to repeat

                case 0:
                    if (mCType == CompressionType.Binary)
                        return mStream.ReadBits(8);

                    // TODO: Text mode
                    throw new NotImplementedException("Text mode is not yet implemented");
                default:
                    return 0;
            }
        }

        private int DecodeDist(int Length)
        {
            if (mStream.EnsureBits(8) == false) return 0;
            int pos = sPosition1[mStream.PeekByte()];
            byte skip = sDistBits[pos];     // Number of bits to skip

            // Skip the appropriate number of bits
            if (mStream.ReadBits(skip) == -1) return 0;

            if (Length == 2)
            {
                if (mStream.EnsureBits(2) == false) return 0;
                pos = (pos << 2) | mStream.ReadBits(2);
            }
            else
            {
                if (mStream.EnsureBits(mDSizeBits) == false) return 0;
                pos = ((pos << mDSizeBits)) | mStream.ReadBits(mDSizeBits);
            }

            return pos + 1;
        }

        private static byte[] GenerateDecodeTable(byte[] Bits, byte[] Codes)
        {
            var result = new byte[256];

            for (int i = Bits.Length - 1; i >= 0; i--)
            {
                UInt32 idx1 = Codes[i];
                UInt32 idx2 = (UInt32)1 << Bits[i];

                do
                {
                    result[idx1] = (byte)i;
                    idx1 += idx2;
                } while (idx1 < 0x100);
            }
            return result;
        }
    }
}