using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using MPQNav.MPQ.ADT.Components;

namespace MPQNav.MPQ.ADT
{
    public class ADTChunk
    {
        public MCNK Header = new MCNK();

        /// <summary>
        /// MCTV Chunk (Height values for the MCNK)
        /// </summary>
        public MapVertices Heights = new MapVertices();
        /// <summary>
        /// MCNR Chunk (Normals for the MCNK)
        /// </summary>
        public MapNormals Normals = new MapNormals();

        /// <summary>
        /// MCRF - (Header.nDoodadRefs + Header.nMapObjRefs) indices into the MDDF and MODF
        /// chunks that tell which M2's and WMO's (respectively) are drawn over this chunk.
        /// When a player is on a chunk, only those objects referenced are checked for collision.
        /// </summary>
        public List<int> DoodadRefs = new List<int>();
        public List<int> ObjectRefs = new List<int>();

        /// <summary>
        /// MH20 Chunk (Water information for the MCNK)
        /// </summary>
        public MH2O WaterInfo = new MH2O();
    }

    public static class ADTChunkParser
    {
        public static ADTChunk Process(BinaryReader fileReader, uint mcnkOffset, ADT parent)
        {
            var chunk = new ADTChunk();

            fileReader.BaseStream.Position = mcnkOffset;

            // Read the header
            ReadMCNK(fileReader, chunk);

            if (chunk.Header.offsColorValues != 0)
            {
                fileReader.BaseStream.Position = mcnkOffset + chunk.Header.offsColorValues;
                ReadMCCV(fileReader, chunk);
            }
            if (chunk.Header.ofsHeight != 0)
            {
                fileReader.BaseStream.Position = mcnkOffset + chunk.Header.ofsHeight;
                ReadMCVT(fileReader, chunk);
            }
            if (chunk.Header.ofsNormal != 0)
            {
                fileReader.BaseStream.Position = mcnkOffset + chunk.Header.ofsNormal;
                ReadMCNR(fileReader, chunk);
            }
            if (chunk.Header.ofsLayer != 0)
            {
                fileReader.BaseStream.Position = mcnkOffset + chunk.Header.ofsLayer;
                ReadMCLY(fileReader, chunk);
            }
            if (chunk.Header.ofsRefs != 0)
            {
                fileReader.BaseStream.Position = mcnkOffset + chunk.Header.ofsRefs;
                ReadMCRF(fileReader, chunk);
            }
            if (chunk.Header.ofsShadow != 0)
            {
                fileReader.BaseStream.Position = mcnkOffset + chunk.Header.ofsShadow;
                ReadMCSH(fileReader, chunk);
            }
            if (chunk.Header.ofsAlpha != 0)
            {
                fileReader.BaseStream.Position = mcnkOffset + chunk.Header.ofsAlpha;
                ReadMCAL(fileReader, chunk);
            }
            if (chunk.Header.ofsSndEmitters != 0)
            {
                fileReader.BaseStream.Position = mcnkOffset + chunk.Header.ofsSndEmitters;
                ReadMCSE(fileReader, chunk);
            }

            return chunk;
        }

        static void ReadMCNK(BinaryReader fileReader, ADTChunk chunk)
        {
            var sig = fileReader.ReadUInt32();
            if (sig != Signatures.MCNK)
            {
                Console.WriteLine();
            }

            var mcnkSize = fileReader.ReadUInt32();

            var h = chunk.Header;
            h.Flags = fileReader.ReadInt32();
            // Column Index
            h.IndexY = fileReader.ReadInt32();
            //Row Index
            h.IndexX = fileReader.ReadInt32();
            h.nLayers = fileReader.ReadUInt32(); //0xC
            h.nDoodadRefs = fileReader.ReadUInt32(); //0x10
            h.ofsHeight = fileReader.ReadUInt32(); //0x14
            h.ofsNormal = fileReader.ReadUInt32(); //0x18
            h.ofsLayer = fileReader.ReadUInt32(); //0x1C
            h.ofsRefs = fileReader.ReadUInt32(); //0x20
            h.ofsAlpha = fileReader.ReadUInt32(); //0x24
            h.sizeAlpha = fileReader.ReadUInt32(); //0x28
            h.ofsShadow = fileReader.ReadUInt32(); //0x2C
            h.sizeShadow = fileReader.ReadUInt32(); //0x30
            h.AreaId = fileReader.ReadInt32(); //0x34
            h.nMapObjRefs = fileReader.ReadUInt32(); //0x38

            // Bitmap to which height values are ignored
            h.Holes = fileReader.ReadUInt16();
            fileReader.ReadUInt16(); // pad

            //if (h.Holes > 0)
            //{
            //    Console.WriteLine(Convert.ToString(h.Holes, 2));
            //}


            h.predTex = new ushort[8];
            for (var i = 0; i < 8; i++)
            {
                h.predTex[i] = fileReader.ReadUInt16();
            }

            h.nEffectDoodad = new byte[8];
            for (var i = 0; i < 8; i++)
            {
                h.nEffectDoodad[i] = fileReader.ReadByte();
            }
            h.ofsSndEmitters = fileReader.ReadUInt32(); //0x58
            h.nSndEmitters = fileReader.ReadUInt32(); //0x5C
            h.ofsLiquid = fileReader.ReadUInt32(); //0x60
            h.sizeLiquid = fileReader.ReadUInt32(); //0x64
            h.X = fileReader.ReadSingle();
            h.Y = fileReader.ReadSingle();
            h.Z = fileReader.ReadSingle();
            h.offsColorValues = fileReader.ReadInt32();
            h.props = fileReader.ReadInt32();
            h.effectId = fileReader.ReadInt32();
        }

        /// <summary>
        /// Reads the chunk's raw height-map with both inner and outer sets
        /// </summary>
        /// <param name="fileReader"></param>
        /// <param name="chunk"></param>
        static void ReadMCVT(BinaryReader fileReader, ADTChunk chunk)
        {
            var sig = fileReader.ReadUInt32();
            var size = fileReader.ReadUInt32();
            for (int i = 0; i < 145; i++)
            {
                chunk.Heights.Heights[i] = fileReader.ReadSingle();
            }
        }

        static void ReadMCCV(BinaryReader br, ADTChunk chunk)
        {
            var sig = br.ReadUInt32();
            var size = br.ReadUInt32();
            // vertex colors
        }

        static void ReadMCNR(BinaryReader fileReader, ADTChunk chunk)
        {
            var sig = fileReader.ReadUInt32();
            var size = fileReader.ReadUInt32();

            for (int i = 0; i < 145; i++)
            {
                var normalZ = fileReader.ReadSByte();
                var normalX = fileReader.ReadSByte();
                var normalY = fileReader.ReadSByte();
                chunk.Normals.Normals[i] = new Vector3(-(float)normalX / 127.0f, normalY / 127.0f,
                                                             -(float)normalZ / 127.0f);
            }
            
        }

        static void ReadMCLY(BinaryReader fileReader, ADTChunk chunk)
        {
            var sig = fileReader.ReadUInt32();
            var size = fileReader.ReadUInt32();
        }

        /// <summary>
        /// A list of indices into the Tile's referenced M2s and WMOs that tells which ones need to be checked for collision
        /// when doing collision checks on this chunk.
        /// </summary>
        /// <param name="fileReader"></param>
        /// <param name="chunk"></param>
        static void ReadMCRF(BinaryReader fileReader, ADTChunk chunk)
        {
            var sig = fileReader.ReadUInt32();
            var size = fileReader.ReadUInt32();

            for (var i = 0; i < chunk.Header.nDoodadRefs; i++)
            {
                chunk.DoodadRefs.Add(fileReader.ReadInt32());
            }

            for (var i = 0; i < chunk.Header.nMapObjRefs; i++)
            {
                chunk.ObjectRefs.Add(fileReader.ReadInt32());
            }
        }

        static void ReadMCSH(BinaryReader fileReader, ADTChunk chunk)
        {
            var sig = fileReader.ReadUInt32();
            var size = fileReader.ReadUInt32();
        }

        static void ReadMCAL(BinaryReader fileReader, ADTChunk chunk)
        {
            var sig = fileReader.ReadUInt32();
            var size = fileReader.ReadUInt32();
        }

        static void ReadMCSE(BinaryReader fileReader, ADTChunk chunk)
        {
            var sig = fileReader.ReadUInt32();
            var size = fileReader.ReadUInt32();
        }
    }
}
