using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using TerrainDisplay.Util;
using TerrainDisplay.MPQ.WMO.Components;

namespace TerrainDisplay.MPQ.WMO
{
    public static class WMORootParser
    {
        public static WMORoot Process(string filePath)
        {
            var root = new WMORoot(filePath);

            using (var file = File.OpenRead(filePath))
            using (var fileReader = new BinaryReader(file))
            {
                uint type = 0;
                uint size = 0;
                long curPos = AdvanceToNextChunk(fileReader, 0, ref type, ref size);

                if (type == Signatures.MVER)
                {
                    // Version
                    ReadMVER(fileReader, root);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MVER");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MOHD)
                {
                    // Root Header
                    ReadMOHD(fileReader, root);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MOHD");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MOTX)
                {
                    // Texture Names
                    ReadMOTX(fileReader, root, size);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MOTX");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MOMT)
                {
                    // Materials
                    ReadMOMT(fileReader, root);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MOMT");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MOGN)
                {
                    ReadMOGN(fileReader, root, size);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MOGN");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MOGI)
                {
                    // Group Information
                    ReadMOGI(fileReader, root);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MOGI");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MOSB)
                {
                    // Skybox (always 0 now, its no longer handled in WMO)
                    ReadMOSB(fileReader, root);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MOSB");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MOPV)
                {
                    // Portal Vertices
                    ReadMOPV(fileReader, root);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MOPV");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MOPT)
                {
                    // Portal Information
                    ReadMOPT(fileReader, root);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MOPT");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MOPR)
                {
                    // Portal Relations
                    ReadMOPR(fileReader, root);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MOPR");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MOVV)
                {
                    // Visible Vertices
                    ReadMOVV(fileReader, root, size);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MOVV");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MOVB)
                {
                    // Visible Blocks
                    ReadMOVB(fileReader, root, size);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MOVB");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MOLT)
                {
                    // Lights
                    ReadMOLT(fileReader, root);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MOLT");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MODS)
                {
                    // Doodad Set
                    ReadMODS(fileReader, root);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MODS");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MODN)
                {
                    // Doodad Names
                    ReadMODN(fileReader, root, size);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MODN");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MODD)
                {
                    // Doodad Definitions
                    ReadMODD(fileReader, root, size);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MODD");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MFOG)
                {
                    // Fog info
                    ReadMFOG(fileReader, root, size);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MFOG");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                // Theres only 1 optional chunk in a WMO root
                if (fileReader.BaseStream.Position < fileReader.BaseStream.Length)
                {
                    if (type == Signatures.MCVP)
                    {
                        ReadMCVP(fileReader, root, size);
                    }

                    //curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);
                }

            }


            return root;
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

        static void ReadMVER(BinaryReader br, WMORoot wmo)
        {
            wmo.Version = br.ReadInt32();
        }

        /// <summary>
        /// Reads the header for the root file
        /// </summary>
        static void ReadMOHD(BinaryReader br, WMORoot wmo)
        {
            wmo.Header.TextureCount = br.ReadUInt32();
            wmo.Header.GroupCount = br.ReadUInt32();
            wmo.Header.PortalCount = br.ReadUInt32();
            wmo.Header.LightCount = br.ReadUInt32();
            wmo.Header.ModelCount = br.ReadUInt32();
            wmo.Header.DoodadCount = br.ReadUInt32();
            wmo.Header.DoodadSetCount = br.ReadUInt32();

            wmo.Header.AmbientColor = br.ReadColor4();
            wmo.Header.WMOId = br.ReadUInt32();

            wmo.Header.BoundingBox = new BoundingBox(br.ReadWMOVector3(), br.ReadWMOVector3());

            wmo.Header.Flags = (WMORootHeaderFlags)br.ReadUInt32();

            wmo.Groups = new WMOGroup[wmo.Header.GroupCount];
        }

        static void ReadMOTX(BinaryReader br, WMORoot wmo, uint size)
        {
            wmo.Textures = new Dictionary<int, string>();

            long endPos = br.BaseStream.Position + size;
            while (br.BaseStream.Position < endPos)
            {
                if (br.PeekByte() == 0)
                {
                    br.BaseStream.Position++;
                }
                else
                {
                    wmo.Textures.Add((int) (size - (endPos - br.BaseStream.Position)), br.ReadCString());
                }
            }
        }

        static void ReadMOMT(BinaryReader br, WMORoot wmo)
        {
            wmo.Materials = new Material[wmo.Header.TextureCount];

            for (int i = 0; i < wmo.Materials.Length; i++)
            {
                var mt = new Material
                             {
                                 Flags = (MaterialFlags)br.ReadUInt32(),
                                 Int_1 = br.ReadUInt32(),
                                 BlendMode = br.ReadInt32(),
                                 TextureNameStart = br.ReadInt32(),
                                 SidnColor = br.ReadColor4(),
                                 FrameSidnColor = br.ReadColor4(),
                                 TextureNameEnd = br.ReadInt32(),
                                 DiffColor = br.ReadColor4(),
                                 GroundType = br.ReadInt32(),
                                 Float_1 = br.ReadSingle(),
                                 Float_2 = br.ReadSingle(),
                                 Int_2 = br.ReadInt32(),
                                 Int_3 = br.ReadInt32(),
                                 Int_4 = br.ReadInt32()
                             };

                // these 2 are set in RAM in the client to the associated HTEXTUREs
                br.ReadUInt32();// 0x38
                br.ReadUInt32();// 0x3C

                wmo.Materials[i] = mt;

                if (mt.Flags != 0)
                    Console.WriteLine();
            }
        }

        static void ReadMOGN(BinaryReader br, WMORoot wmo, uint size)
        {
            wmo.GroupNames = new Dictionary<int, string>();

            long endPos = br.BaseStream.Position + size;
            while (br.BaseStream.Position < endPos)
            {
                if (br.PeekByte() == 0)
                {
                    br.BaseStream.Position++;
                }
                else
                {
                    wmo.GroupNames.Add((int)(size - (endPos - br.BaseStream.Position)), br.ReadCString());
                }
            }
        }

        static void ReadMOGI(BinaryReader br, WMORoot wmo)
        {
            wmo.GroupInformation = new GroupInformation[wmo.Header.GroupCount];

            for (int i = 0; i < wmo.GroupInformation.Length; i++)
            {
                var g = new GroupInformation {
                                                 Flags = (WMOGroupFlags) br.ReadUInt32(),
                                                 BoundingBox = new BoundingBox(br.ReadWMOVector3(), br.ReadWMOVector3()),
                                                 NameIndex = br.ReadInt32()
                                             };

                wmo.GroupInformation[i] = g;
            }
        }

        static void ReadMOSB(BinaryReader br, WMORoot wmo)
        {
            // skyboxes moved to light.dbc
        }

        static void ReadMOPV(BinaryReader br, WMORoot wmo)
        {
            // PortalCount of 4 x Vector3 to form a rectangle for the doorway
            uint vertexCount = wmo.Header.PortalCount*4;
            wmo.PortalVertices = new Vector3[vertexCount];

            for (var i=0; i<vertexCount; i++)
            {
                wmo.PortalVertices[i] = br.ReadVector3();
            }
        }

        static void ReadMOPT(BinaryReader br, WMORoot wmo)
        {
            wmo.PortalInformation = new PortalInformation[wmo.Header.PortalCount];

            for (var i = 0; i < wmo.Header.PortalCount; i++)
            {
                var p = new PortalInformation
                            {
                                Vertices = new VertexSpan
                                               {
                                                   StartVertex = br.ReadInt16(),
                                                   VertexCount = br.ReadInt16(),
                                               },
                                Plane = br.ReadPlane()
                            };
                wmo.PortalInformation[i] = p;
            }
        }

        static void ReadMOPR(BinaryReader br, WMORoot wmo)
        {
            wmo.PortalRelations = new PortalRelation[wmo.Header.PortalCount];

            for (int i = 0; i < wmo.PortalRelations.Length; i++)
            {
                var r = new PortalRelation
                {
                    PortalIndex = br.ReadUInt16(),
                    GroupIndex = br.ReadUInt16(),
                    Side = br.ReadInt16()
                };
                br.ReadInt16(); // filler

                wmo.PortalRelations[i] = r;
            }
        }

        static void ReadMOVV(BinaryReader br, WMORoot wmo, uint size)
        {
            wmo.VisibleVertices = new Vector3[size/12]; // 12 = sizeof(Vector3), but we can't use sizeof with Vector3

            for (int i = 0; i < wmo.VisibleVertices.Length; i++)
            {
                wmo.VisibleVertices[i] = br.ReadVector3();
            }
        }

        static void ReadMOVB(BinaryReader br, WMORoot wmo, uint size)
        {
            uint blockCount = size/4;
            wmo.VisibleBlocks = new VertexSpan[blockCount];

            for (int i = 0; i < blockCount; i++)
            {
                wmo.VisibleBlocks[i] = new VertexSpan
                                           {
                                               StartVertex = br.ReadInt16(),
                                               VertexCount = br.ReadInt16()
                                           };
            }
        }
        
        static void ReadMOLT(BinaryReader br, WMORoot wmo)
        {
            wmo.LightInfo = new LightInformation[wmo.Header.LightCount];

            for (int i = 0; i < wmo.LightInfo.Length; i++)
            {
                var light = new LightInformation
                                {
                                    Byte_1 = br.ReadByte(),
                                    Byte_2 = br.ReadByte(),
                                    Byte_3 = br.ReadByte(),
                                    Byte_4 = br.ReadByte(),
                                    Color = br.ReadColor4(),
                                    Position = br.ReadVector3(),
                                    Intensity = br.ReadSingle(),
                                    AttenStart = br.ReadSingle(),
                                    AttenEnd = br.ReadSingle(),
                                    Float_4 = br.ReadSingle(),
                                    Float_5 = br.ReadSingle(),
                                    Float_6 = br.ReadSingle(),
                                    Float_7 = br.ReadSingle()
                                };

                //FixVector3(ref light.position);

                wmo.LightInfo[i] = light;
            }
        }

        /// <summary>
        /// Translates a Vector3 read by the ReadVector3 extension method 
        /// into a vector in the coordinate space used by WoW
        /// (X, Y, Z) -> (X, Z, -Y)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="final"></param>
        static void FixVector3(ref Vector3 source)
        {
            var y = source.Y;
            source.Y = source.Z;
            source.Z = -y;
        }

        static void ReadMODS(BinaryReader br, WMORoot wmo)
        {
            wmo.DoodadSets = new DoodadSet[wmo.Header.DoodadSetCount];

            for (int i = 0; i < wmo.Header.DoodadSetCount; i++)
            {
                var d = new DoodadSet
                {
                    SetName = br.ReadFixedString(20),
                    FirstInstanceIndex = br.ReadUInt32(),
                    InstanceCount = br.ReadUInt32()
                };
                br.ReadInt32(); // padding

                wmo.DoodadSets[i] = d;
            }
        }

        static void ReadMODN(BinaryReader br, WMORoot wmo, uint size)
        {
            wmo.DoodadFiles = new Dictionary<int, string>();

            long endPos = br.BaseStream.Position + size;
            while (br.BaseStream.Position < endPos)
            {
                if (br.PeekByte() == 0)
                {
                    br.BaseStream.Position++;
                }
                else
                {
                    //doodadNames.Add(size - (endPos - br.BaseStream.Position), br.ReadCString());
                    wmo.DoodadFiles.Add((int) (size - (endPos - br.BaseStream.Position)), br.ReadCString());
                }
            }
        }

        static void ReadMODD(BinaryReader br, WMORoot wmo, uint size)
        {
            // Why oh why is wmo.Header.DoodadCount wrong sometimes
            // 40 is the size of DoodadDefinition
            wmo.DoodadDefinitions = new DoodadDefinition[size / 40];

            for (var i = 0; i < wmo.DoodadDefinitions.Length; i++)
            {
                var dd = new DoodadDefinition
                             {
                                 NameIndex = br.ReadInt32(),
                                 Position = br.ReadVector3(),
                                 Rotation = br.ReadQuaternion(),
                                 Scale = br.ReadSingle(),
                                 Color = br.ReadColor4()
                             };

                if (dd.NameIndex != -1)
                {
                    dd.FilePath = wmo.DoodadFiles[dd.NameIndex];
                }

                wmo.DoodadDefinitions[i] = dd;
            }
        }

        static void ReadMFOG(BinaryReader br, WMORoot wmo, uint size)
        {
            wmo.Fogs = new Fog[size/48];

            for (int i = 0; i < wmo.Fogs.Length; i++)
            {
                var fog = new Fog
                              {
                                  Flags = (FogFlags)br.ReadUInt32(),
                                  Position = br.ReadVector3(),
                                  Start = br.ReadSingle(),
                                  End = br.ReadSingle(),
                                  FogInfo_FOG = new FogInfo
                                                 {
                                                     End = br.ReadSingle(),
                                                     StartScalar = br.ReadSingle(),
                                                     Color = br.ReadColor4()
                                                 },
                                  FogInfo_UWFOG = new FogInfo
                                                 {
                                                     End = br.ReadSingle(),
                                                     StartScalar = br.ReadSingle(),
                                                     Color = br.ReadColor4()
                                                 }
                              };

                wmo.Fogs[i] = fog;
            }
        }

        static void ReadMCVP(BinaryReader br, WMORoot wmo, uint size)
        {
            wmo.ComplexVolumePlanes = new Plane[size/(4*sizeof (float))];

            for (int i = 0; i < wmo.ComplexVolumePlanes.Length; i++)
            {
                wmo.ComplexVolumePlanes[i] = br.ReadPlane();
            }
        }
    }

}
