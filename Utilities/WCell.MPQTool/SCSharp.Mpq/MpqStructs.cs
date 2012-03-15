/*************************************************************************
 *
 *   file		: MpqStructs.cs
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
    [Flags]
    public enum MpqFileFlags : uint
    {
        Changed = 1,
        Protected = 2,
        CompressedPK = 0x100,
        CompressedMulti = 0x200,
        Compressed = 0xff00,
        Encrypted = 0x10000,
        FixSeed = 0x20000,
        SingleUnit = 0x1000000,
        Unknown_02000000 = 0x02000000,  // The file is only 1 byte long and its name is a hash

        //Unknown_04000000 = 0x04000000, // Appears in WoW 1.10 or newer, dunno what does it mean
        FileHasMetadata = 0x04000000, // Indicates the file has associted metadata.

        Exists = 0x80000000
    }

    [Flags]
    public enum MpqFileCompression
    {
        Huffman = 1,
        ZLib = 2,
        PK = 8,

        OneChannelWav = 0x40,
        TwoChannelWav = 0x80
    }

    struct MpqHeader
    {
        public uint ID; // Signature.  Should be 0x1a51504d
        public uint DataOffset; // Offset of the first file
        public uint ArchiveSize;
        public ushort Offs0C; // Unknown
        public ushort BlockSize; // Size of file block is 0x200 << BlockSize
        public uint HashTablePos;
        public uint BlockTablePos;
        public uint HashTableSize;
        public uint BlockTableSize;

        public static readonly uint MpqId = 0x1a51504d;
        public static readonly uint Size = 32;

        public MpqHeader(BinaryReader br)
        {
            ID = br.ReadUInt32();
            DataOffset = br.ReadUInt32();
            ArchiveSize = br.ReadUInt32();
            Offs0C = br.ReadUInt16();
            BlockSize = br.ReadUInt16();
            HashTablePos = br.ReadUInt32();
            BlockTablePos = br.ReadUInt32();
            HashTableSize = br.ReadUInt32();
            BlockTableSize = br.ReadUInt32();
        }
    }

    struct MpqHash
    {
        public uint Name1;
        public uint Name2;
        public uint Locale;
        public uint BlockIndex;

        public static readonly uint Size = 16;

        public MpqHash(BinaryReader br)
        {
            Name1 = br.ReadUInt32();
            Name2 = br.ReadUInt32();
            Locale = br.ReadUInt32();
            BlockIndex = br.ReadUInt32();
        }

        public static MpqHash InvalidHash()
        {
            MpqHash invalid = new MpqHash();
            invalid.Name1 = uint.MaxValue;
            invalid.Name2 = uint.MaxValue;
            return invalid;
        }

        public bool IsValid
        {
            get
            {
                return Name1 != uint.MaxValue && Name2 != uint.MaxValue;
            }
        }
    }

    struct MpqBlock
    {
        public uint FilePos;
        public uint CompressedSize;
        public uint FileSize;
        public MpqFileFlags Flags;

        public static readonly uint Size = 16;

        public MpqBlock(BinaryReader br, uint HeaderOffset)
        {
            FilePos = br.ReadUInt32() + HeaderOffset;
            CompressedSize = br.ReadUInt32();
            FileSize = br.ReadUInt32();
            Flags = (MpqFileFlags)br.ReadUInt32();
        }

        public bool IsEncrypted
        {
            get
            {
                return (Flags & MpqFileFlags.Encrypted) != 0;
            }
        }

        public bool IsCompressed
        {
            get
            {
                return (Flags & MpqFileFlags.Compressed) != 0;
            }
        }
    }
}