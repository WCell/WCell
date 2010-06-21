using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using TerrainDisplay.Util;
using TerrainDisplay.Collision._3D;
using TerrainDisplay.MPQ.WMO.Components;

namespace TerrainDisplay.MPQ.WMO
{
    public static class WMOGroupParser
    {
        /// <summary>
        /// Gets a WMOGroup from the WMO Group file
        /// </summary>
        /// <param name="filePath">File path to the WMOGroup</param>
        /// <param name="fileName">Filename for the WMOGroup</param>
        /// <param name="groupIndex">Current index in the WMO Group</param>
        /// <returns>A WMOGroup from the WMO Group file</returns>
        public static WMOGroup Process(string filePath, string fileName, WMORoot root, int groupIndex)
        {
            var fullFilePath = Path.Combine(filePath, fileName);
            if (!File.Exists(fullFilePath))
            {
                throw new Exception("File does not exist: " + fullFilePath);
            }

            var currentWMOGroup = new WMOGroup(root, groupIndex);

            using (var fs = File.OpenRead(fullFilePath))
            using (var br = new BinaryReader(fs))
            {
                ReadRequiredChunks(br, currentWMOGroup);
                ReadOptionalChunks(br, currentWMOGroup);
            }

            return currentWMOGroup;
        }

        static long AdvanceToNextChunk(BinaryReader br, long curPos, ref uint type, ref uint size)
        {
            br.BaseStream.Seek(curPos + size, SeekOrigin.Begin);
            if (br.BaseStream.Position == br.BaseStream.Length)
            {
                return br.BaseStream.Length;
            }

            type = br.ReadUInt32();
            size = br.ReadUInt32();
            long newCurPos = br.BaseStream.Position;
            return newCurPos;
        }

        private static void ReadRequiredChunks(BinaryReader br, WMOGroup currentWMOGroup)
        {
            uint type = 0;
            uint size = 0;

            var curPos = AdvanceToNextChunk(br, br.BaseStream.Position, ref type, ref size);
            if (type == Signatures.MVER)
            {
                ReadMVER(br, currentWMOGroup);
            }
            else
            {
                Console.WriteLine("ERROR: WMO Group did not have required chunk MVER");
            }

            curPos = AdvanceToNextChunk(br, curPos, ref type, ref size);
            if (type == Signatures.MOGP)
            {
                size = 68; // MOGP is stupid and wants to use the entire rest of the file as its size
                ReadMOGP(br, currentWMOGroup);
            }
            else
            {
                Console.WriteLine("ERROR: WMO Group did not have required chunk MOGP");
            }

            curPos = AdvanceToNextChunk(br, curPos, ref type, ref size);
            if (type == Signatures.MOPY)
            {
                ReadMOPY(br, currentWMOGroup, size);
            }
            else
            {
                Console.WriteLine("ERROR: WMO Group did not have required chunk MOPY");
            }

            curPos = AdvanceToNextChunk(br, curPos, ref type, ref size);
            if (type == Signatures.MOVI)
            {
                ReadMOVI(br, currentWMOGroup);
            }
            else
            {
                Console.WriteLine("ERROR: WMO Group did not have required chunk MOVI");
            }

            curPos = AdvanceToNextChunk(br, curPos, ref type, ref size);
            if (type == Signatures.MOVT)
            {
                ReadMOVT(br, currentWMOGroup, size);
            }
            else
            {
                Console.WriteLine("ERROR: WMO Group did not have required chunk MOVT");
            }

            curPos = AdvanceToNextChunk(br, curPos, ref type, ref size);
            if (type == Signatures.MONR)
            {
                ReadMONR(br, currentWMOGroup);
            }
            else
            {
                Console.WriteLine("ERROR: WMO Group did not have required chunk MONR");
            }

            curPos = AdvanceToNextChunk(br, curPos, ref type, ref size);
            if (type == Signatures.MOTV)
            {
                ReadMOTV1(br, currentWMOGroup, size);
            }
            else
            {
                Console.WriteLine("ERROR: WMO Group did not have required chunk MOTV1");
            }

            curPos = AdvanceToNextChunk(br, curPos, ref type, ref size);
            if (type == Signatures.MOBA)
            {
                ReadMOBA(br, currentWMOGroup, size);
            }
            else
            {
                Console.WriteLine("ERROR: WMO Group did not have required chunk MOBA");
            }

            br.BaseStream.Seek(curPos + size, SeekOrigin.Begin); // seek to the end of the required fields
        }

        private static void ReadOptionalChunks(BinaryReader br, WMOGroup currentWMOGroup)
        {
            // We start here at the end of the required chunks
            uint type = 0;
            uint size = 0;
            var curPos = AdvanceToNextChunk(br, br.BaseStream.Position, ref type, ref size);

            if (currentWMOGroup.Header.HasMOLR)
            {
                if (type == Signatures.MOLR)
                {
                    ReadMOLR(br, currentWMOGroup, size);
                }
                else
                {
                    Console.WriteLine(
                        "ERROR: WMO Group did not have optional chunk MOLR, but was specified in the flags");
                }

                curPos = AdvanceToNextChunk(br, curPos, ref type, ref size);
            }

            if (currentWMOGroup.Header.HasMODR)
            {
                if (type == Signatures.MODR)
                {
                    ReadMODR(br, currentWMOGroup, size);
                }
                else
                {
                    Console.WriteLine(
                        "ERROR: WMO Group did not have optional chunk MODR, but was specified in the flags");
                }

                curPos = AdvanceToNextChunk(br, curPos, ref type, ref size);
            }

            if (currentWMOGroup.Header.HasBSPInfo)
            {
                if (type == Signatures.MOBN)
                {
                    ReadMOBN(br, currentWMOGroup, size);
                }
                else
                {
                    Console.WriteLine(
                        "ERROR: WMO Group did not have optional chunk MOBN, but was specified in the flags");
                }

                curPos = AdvanceToNextChunk(br, curPos, ref type, ref size);

                if (type == Signatures.MOBR)
                {
                    ReadMOBR(br, currentWMOGroup, size);
                }
                else
                {
                    Console.WriteLine(
                        "ERROR: WMO Group did not have optional chunk MOBR, but was specified in the flags");
                }

                curPos = AdvanceToNextChunk(br, curPos, ref type, ref size);
            }

            if (currentWMOGroup.Header.HasMPChunks)
            {
                if (type == Signatures.MPBV)
                {
                    ReadMPBV(br, currentWMOGroup);
                }
                else
                {
                    Console.WriteLine(
                        "ERROR: WMO Group did not have optional chunk MPBV, but was specified in the flags");
                }
                curPos = AdvanceToNextChunk(br, curPos, ref type, ref size);

                if (type == Signatures.MPBP)
                {
                    ReadMPBP(br, currentWMOGroup);
                }
                else
                {
                    Console.WriteLine(
                        "ERROR: WMO Group did not have optional chunk MPBP, but was specified in the flags");
                }
                curPos = AdvanceToNextChunk(br, curPos, ref type, ref size);

                if (type == Signatures.MPBI)
                {
                    ReadMPBI(br, currentWMOGroup);
                }
                else
                {
                    Console.WriteLine(
                        "ERROR: WMO Group did not have optional chunk MPBI, but was specified in the flags");
                }
                curPos = AdvanceToNextChunk(br, curPos, ref type, ref size);

                if (type == Signatures.MPBG)
                {
                    ReadMPBG(br, currentWMOGroup);
                }
                else
                {
                    Console.WriteLine(
                        "ERROR: WMO Group did not have optional chunk MPBG, but was specified in the flags");
                }
                curPos = AdvanceToNextChunk(br, curPos, ref type, ref size);
            }

            if (currentWMOGroup.Header.HasMOCV1)
            {
                if (type == Signatures.MOCV)
                {
                    ReadMOCV1(br, currentWMOGroup, size);
                }
                else
                {
                    Console.WriteLine(
                        "ERROR: WMO Group did not have optional chunk MOCV 1, but was specified in the flags");
                }
                curPos = AdvanceToNextChunk(br, curPos, ref type, ref size);
            }

            if (currentWMOGroup.Header.HasMLIQ)
            {
                if (type == Signatures.MLIQ)
                {
                    ReadMLIQ(br, currentWMOGroup);
                }
                else
                {
                    Console.WriteLine(
                        "ERROR: WMO Group did not have optional chunk MLIQ, but was specified in the flags");
                }
                curPos = AdvanceToNextChunk(br, curPos, ref type, ref size);
            }

            if (currentWMOGroup.Header.HasMORChunks)
            {
                if (type == Signatures.MORI)
                {
                    ReadMORI(br, currentWMOGroup);
                }
                else
                {
                    Console.WriteLine(
                        "ERROR: WMO Group did not have optional chunk MORI, but was specified in the flags");
                }
                curPos = AdvanceToNextChunk(br, curPos, ref type, ref size);

                if (type == Signatures.MORB)
                {
                    ReadMORB(br, currentWMOGroup);
                }
                else
                {
                    Console.WriteLine(
                        "ERROR: WMO Group did not have optional chunk MORB, but was specified in the flags");
                }
                curPos = AdvanceToNextChunk(br, curPos, ref type, ref size);
            }

            if (currentWMOGroup.Header.HasMOTV2)
            {
                if (type == Signatures.MOTV)
                {
                    ReadMOTV2(br, currentWMOGroup, size);
                }
                else
                {
                    Console.WriteLine(
                        "ERROR: WMO Group did not have optional chunk MOTV2, but was specified in the flags");
                }

                curPos = AdvanceToNextChunk(br, curPos, ref type, ref size);
            }

            if (currentWMOGroup.Header.HasMOCV2)
            {
                if (type == Signatures.MOCV)
                {
                    ReadMOCV2(br, currentWMOGroup, size);
                }
                else
                {
                    Console.WriteLine(
                        "ERROR: WMO Group did not have optional chunk MOCV2, but was specified in the flags");
                }

                // This is the final chunk in the file (or should be...)
                //curPos = AdvanceToNextChunk(br, curPos, ref type, ref size);
            }
        }

        private static void ReadMORI(BinaryReader reader, WMOGroup group)
        {
        }

        private static void ReadMORB(BinaryReader reader, WMOGroup group)
        {
        }

        private static void ReadMPBV(BinaryReader br, WMOGroup group)
        {
        }

        private static void ReadMPBP(BinaryReader br, WMOGroup group)
        {
        }

        private static void ReadMPBI(BinaryReader br, WMOGroup group)
        {
        }

        private static void ReadMPBG(BinaryReader br, WMOGroup group)
        {
        }
        
        static void ReadMVER(BinaryReader br, WMOGroup group)
        {
            group.Version = br.ReadUInt32();
        }

        static void ReadMOGP(BinaryReader file, WMOGroup group)
        {
            group.Header.NameStart = file.ReadInt32();
            if (group.Header.NameStart != -1)
            {
                group.Name = group.Root.GroupNames[group.Header.NameStart];
            }

            group.Header.DescriptiveNameStart = file.ReadInt32();
            if (group.Header.DescriptiveNameStart > 0)
            {
                group.DescriptiveName = group.Root.GroupNames[group.Header.DescriptiveNameStart];
            }

            group.Header.Flags = (WMOGroupFlags)file.ReadUInt32();

            group.Header.BoundingBox = new BoundingBox(file.ReadWMOVector3(), file.ReadWMOVector3());

            group.Header.PortalStart = file.ReadUInt16();
            group.Header.PortalCount = file.ReadUInt16();
            group.Header.BatchesA = file.ReadUInt16();
            group.Header.BatchesB = file.ReadUInt16();
            group.Header.BatchesC = file.ReadUInt16();
            group.Header.BatchesD = file.ReadUInt16();
            group.Header.Fogs = file.ReadBytes(4);
            group.Header.LiquidType = file.ReadUInt32();
            group.Header.WMOGroupId = file.ReadUInt32();
            group.Header.MOGP_0x3C = file.ReadUInt32();
            group.Header.MOGP_0x40 = file.ReadUInt32();
        }

        static void ReadMOPY(BinaryReader file, WMOGroup group, uint size)
        {
            group.TriangleCount = size / 2;
            // materials
            /*  0x01 - inside small houses and paths leading indoors
			 *  0x02 - ???
			 *  0x04 - set on indoor things and ruins
			 *  0x08 - ???
			 *  0x10 - ???
			 *  0x20 - Always set?
			 *  0x40 - sometimes set- 
			 *  0x80 - ??? never set
			 *  
			 */
            group.TriangleMaterials = new MOPY[group.TriangleCount];

            for (var i = 0; i < group.TriangleCount; i++)
            {
                var t = new MOPY
                {
                    Flags = (MOPY.MaterialFlags)file.ReadByte(),
                    MaterialIndex = file.ReadByte()
                };
                group.TriangleMaterials[i] = t;
            }
        }

        /// <summary>
        /// Reads the WMO Vertex Index List
        /// </summary>
        /// <param name="br"></param>
        /// <param name="group"></param>
        static void ReadMOVI(BinaryReader br, WMOGroup group)
        {
            //group.Triangles = new ushort[group.TriangleCount * 3];
            for (uint i = 0; i < group.TriangleCount; i++)
            {
                var one = br.ReadInt16();
                var two = br.ReadInt16();
                var three = br.ReadInt16();

                var ind = new Index3 {Index0 = three, Index1 = two, Index2 = one};
                //var ind = new Index3 { Index0 = one, Index1 = two, Index2 = three };
                group.Indices.Add(ind);
            }
        }

        /// <summary>
        /// Reads the vertices for this wmo group
        /// </summary>
        /// <param name="file"></param>
        /// <param name="group"></param>
        /// <param name="size"></param>
        static void ReadMOVT(BinaryReader file, WMOGroup group, uint size)
        {
            group.VertexCount = size/(sizeof (float)*3);
            // let's hope it's padded to 12 bytes, not 16...
            
            for (uint i = 0; i < group.VertexCount; i++)
            {
                group.Vertices.Add(file.ReadWMOVector3());
            }
        }

        /// <summary>
        /// Reads the Normal vectors for this group
        /// </summary>
        /// <param name="file"></param>
        /// <param name="group"></param>
        static void ReadMONR(BinaryReader file, WMOGroup group)
        {
            // Read the vector normals
            for (var i=0;i<group.VertexCount;i++)
            {
                group.Normals.Add(file.ReadWMOVector3());
            }
        }

        static void ReadMOTV1(BinaryReader br, WMOGroup group, uint size)
        {
            // Texture coordinates, 2 floats per vertex in (X,Y) order. The values range from 0.0 to 1.0.
            // Vertices, normals and texture coordinates are in corresponding order, of course. 
            group.TextureVertices1 = new Vector2[size/8];

            for (int i = 0; i < group.TextureVertices1.Length; i++)
            {
                group.TextureVertices1[i] = br.ReadVector2();
            }
        }

        static void ReadMOTV2(BinaryReader br, WMOGroup group, uint size)
        {
            // Texture coordinates, 2 floats per vertex in (X,Y) order. The values range from 0.0 to 1.0.
            // Vertices, normals and texture coordinates are in corresponding order, of course. 
            group.TextureVertices2 = new Vector2[size / 8];

            for (int i = 0; i < group.TextureVertices2.Length; i++)
            {
                group.TextureVertices2[i] = br.ReadVector2();
            }
        }

        static void ReadMOBA(BinaryReader br, WMOGroup group, uint size)
        {
            // Render batches. Records of 24 bytes.
            group.Batches = new RenderBatch[size/24];

            for (int i = 0; i < group.Batches.Length; i++)
            {
                var batch = new RenderBatch
                                {
                                    BottomX = br.ReadInt16(),
                                    BottomY = br.ReadInt16(),
                                    BottomZ = br.ReadInt16(),
                                    TopX = br.ReadInt16(),
                                    TopY = br.ReadInt16(),
                                    TopZ = br.ReadInt16(),
                                    StartIndex = br.ReadInt32(),
                                    IndexCount = br.ReadUInt16(),
                                    VertexStart = br.ReadUInt16(),
                                    VertexEnd = br.ReadUInt16(),
                                    Byte_13 = br.ReadByte(),
                                    TextureIndex = br.ReadByte()
                                };

                group.Batches[i] = batch;
            }
        }

        static void ReadMOLR(BinaryReader br, WMOGroup group, uint size)
        {
            // Light references, one 16-bit integer per light reference. 
            // This is basically a list of lights used in this WMO group, 
            // the numbers are indices into the WMO root file's MOLT table.
            group.LightReferences = new ushort[size/2];

            for (int i = 0; i < group.LightReferences.Length; i++)
            {
                group.LightReferences[i] = br.ReadUInt16();
            }
        }

        static void ReadMODR(BinaryReader br, WMOGroup group, uint size)
        {
            // Doodad references, one 16-bit integer per doodad. 
            // The numbers are indices into the doodad instance table (MODD chunk) of the WMO root file. 
            // These have to be filtered to the doodad set being used in any given WMO instance.
            group.DoodadReferences = new ushort[size / 2];

            for (int i = 0; i < group.DoodadReferences.Length; i++)
            {
                group.DoodadReferences[i] = br.ReadUInt16();
            }
        }

        static void ReadMOBN(BinaryReader file, WMOGroup group, uint size)
        {
            uint count = size/0x10;

            group.BSPNodes = new BSPNode[count];
            for (var i=0;i<count;i++)
            {
                var node = new BSPNode
                               {
                                   flags = (BSPNodeFlags) file.ReadUInt16(),
                                   negChild = file.ReadInt16(),
                                   posChild = file.ReadInt16(),
                                   nFaces = file.ReadUInt16(),
                                   faceStart = file.ReadUInt32(),
                                   planeDist = file.ReadSingle()
                               };

                group.BSPNodes[i] = node;
            }
        }

        static void ReadMOBR(BinaryReader file, WMOGroup group, uint size)
        {
            uint count = size / 2;
            group.MOBR = new ushort[count];

            for (int i = 0; i < count; i++)
            {
                group.MOBR[i] = file.ReadUInt16();
            }

            LinkBSPNodes(group);
        }

        static void LinkBSPNodes(WMOGroup group)
        {
            for (int i = 0; i < group.BSPNodes.Length; i++)
            {
                var n = group.BSPNodes[i];

                if (n.posChild != -1)
                {
                    n.Positive = group.BSPNodes[n.posChild];
                }
                if (n.negChild != -1)
                {
                    n.Negative = group.BSPNodes[n.negChild];
                }

                if (n.faceStart == 0 && n.nFaces == 0)
                {
                    // empty?
                    continue;
                }

                //var faces = new ushort[n.nFaces];
                n.TriIndices = new List<Index3>(n.nFaces);
                for (int j = 0; j < n.nFaces; j++)
                {
                    var triIndex = group.Indices[group.MOBR[n.faceStart + j]];
                    n.TriIndices.Add(triIndex);
                }
            }

            //group.DumpBSPNodes();
        }


        static void ReadMOCV1(BinaryReader br, WMOGroup group, uint size)
        {
            // Vertex colors, 4 bytes per vertex (BGRA), for WMO groups using indoor lighting.
            group.VertexColors1 = new Color4[size/4];

            for (int i = 0; i < group.VertexColors1.Length; i++)
            {
                group.VertexColors1[i] = br.ReadColor4();
            }
        }

        static void ReadMOCV2(BinaryReader br, WMOGroup group, uint size)
        {
            // Vertex colors, 4 bytes per vertex (BGRA), for WMO groups using indoor lighting.
            group.VertexColors2 = new Color4[size / 4];

            for (int i = 0; i < group.VertexColors2.Length; i++)
            {
                group.VertexColors2[i] = br.ReadColor4();
            }
        }

        static void ReadMLIQ(BinaryReader file, WMOGroup group)
        {
            group.LiquidInfo = new LiquidInfo
                                   {
                                       XVertexCount = file.ReadInt32(),
                                       YVertexCount = file.ReadInt32(),
                                       XTileCount = file.ReadInt32(),
                                       YTileCount = file.ReadInt32(),
                                       BaseCoordinates = file.ReadVector3(),
                                       MaterialId = file.ReadUInt16()
                                   };

            // The following is untested, but is what the client appears to be doing
            // These are probably similar to the 2 floats in the old adt water chunk
            group.LiquidInfo.HeightMapMax = new float[group.LiquidInfo.XVertexCount, group.LiquidInfo.YVertexCount];
            group.LiquidInfo.HeightMapMin = new float[group.LiquidInfo.XVertexCount, group.LiquidInfo.YVertexCount];
            
            // This is different from the other ADT files, these are stored as [row, col] instead of [col, row]
            for (var y = 0; y < group.LiquidInfo.YVertexCount; y++) 
            {
                for (var x = 0; x < group.LiquidInfo.XVertexCount; x++)
                {
                    // The first value is never used ...
                    group.LiquidInfo.HeightMapMin[x, y] = file.ReadSingle();
                    group.LiquidInfo.HeightMapMax[x, y] = file.ReadSingle();
                }
            }

            group.LiquidInfo.LiquidTileFlags = new byte[group.LiquidInfo.XTileCount, group.LiquidInfo.YTileCount];
            for (var y = 0; y < group.LiquidInfo.YTileCount; y++) 
            {
                for (var x = 0; x < group.LiquidInfo.XTileCount; x++)
                {
                    group.LiquidInfo.LiquidTileFlags[x, y] = file.ReadByte();
                }
            }
        }
    }
}
