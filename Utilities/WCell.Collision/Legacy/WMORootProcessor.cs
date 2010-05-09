//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Content;
//using System.IO;
//using System.Runtime.InteropServices;

//namespace WCell.Collision
//{
//    public class WMORootImporter
//    {
//        public static WMORoot Import(string filename)
//        {
//            WMORoot wmo = new WMORoot();
//            wmo.FileName = filename;
//            using (FileStream fs = File.Open(filename, FileMode.Open))
//            {
//                using (BinaryReader br = new BinaryReader(fs))
//                {
//                    while (br.BaseStream.Position < br.BaseStream.Length)
//                    {
//                        string chunk = br.ReadFixedReversedAsciiString(4);
//                        // sometimes this is wrong :/ so we pass it to the readers so they can fix it
//                        int size = br.ReadInt32();
//                        long start = br.BaseStream.Position;

//                        switch (chunk)
//                        {
//                            case "MVER":
//                                {
//                                    ReadMVER(br, ref size, wmo);
//                                    break;
//                                }
//                            case "MOHD":
//                                {
//                                    ReadMOHD(br, ref size, wmo);
//                                    break;
//                                }
//                            /*case "MOTX":
//                                {
//                                    ReadMOTX(br, size, wmo);
//                                    break;
//                                }*/
//                            case "MOGI":
//                                {
//                                    ReadMOGI(br, ref size, wmo);
//                                    break;
//                                }
//                            case "MOPT":
//                                {
//                                    ReadMOPT(br, ref size, wmo);
//                                    break;
//                                }
//                            case "MOPR":
//                                {
//                                    ReadMOPR(br, ref size, wmo);
//                                    break;
//                                }
//                            default:
//                                break;
//                        }

//                        br.BaseStream.Seek(start + size, SeekOrigin.Begin);
//                    }
//                }
//            }

//            return wmo; 
//        }

//        private static void ReadMVER(BinaryReader binReader, ref int size, WMORoot wmoRoot)
//        {
//            wmoRoot.Version = binReader.ReadInt32();
//        }

//        private static void ReadMOHD(BinaryReader binReader, ref int size, WMORoot wmoRoot)
//        {
//            /*wmoRoot.TextureCount = binReader.ReadInt32();
//            wmoRoot.GroupCount = binReader.ReadInt32();
//            wmoRoot.PortalCount = binReader.ReadInt32();
//            wmoRoot.LightCount = binReader.ReadInt32();
//            wmoRoot.ModelCount = binReader.ReadInt32();
//            wmoRoot.DoodadCount = binReader.ReadInt32();
//            wmoRoot.SetCount = binReader.ReadInt32();
//            wmoRoot.colR = binReader.ReadByte();
//            wmoRoot.colG = binReader.ReadByte();
//            wmoRoot.colB = binReader.ReadByte();
//            wmoRoot.colX = binReader.ReadByte();
//            wmoRoot.Id = binReader.ReadInt32();
//            wmoRoot.BoundingBoxCorner1 = binReader.ReadVector3();
//            wmoRoot.BoundingBoxCorner2 = binReader.ReadVector3();
//            binReader.ReadInt32(); // padding*/
//            wmoRoot.Header = binReader.ReadStruct<RootHeader>();
//        }

//        private static void ReadMOTX(BinaryReader binReader, ref int size, WMORoot wmoRoot)
//        {
//            /*wmoRoot.Textures = new string[wmoRoot.TextureCount];
//            for (int i = 0; i < wmoRoot.TextureCount; i++)
//            {
//                wmoRoot.Textures[i] = binReader.ReadCString();
//            }*/
//        }

//        private static void ReadMOGI(BinaryReader binReader, ref int size, WMORoot wmoRoot)
//        {
//            wmoRoot.GroupInformation = new GroupInformation[wmoRoot.Header.GroupCount];

//            for (int i = 0; i < wmoRoot.Header.GroupCount; i++)
//            {
//                wmoRoot.GroupInformation[i].Flags = (GroupFlags)binReader.ReadUInt32();
//                wmoRoot.GroupInformation[i].BoundingBox1 = binReader.ReadVector3();
//                wmoRoot.GroupInformation[i].BoundingBox2 = binReader.ReadVector3();
//                wmoRoot.GroupInformation[i].NameIndex = binReader.ReadInt32();
//            }
//        }

//        private static void ReadMOPT(BinaryReader binReader, ref int size, WMORoot wmoRoot)
//        {
//            wmoRoot.PortialInformation = new PortalInformation[wmoRoot.Header.PortalCount];

//            for (int i = 0; i < wmoRoot.Header.PortalCount; i++)
//            {
//                wmoRoot.PortialInformation[i].StartVertex = binReader.ReadUInt16();
//                wmoRoot.PortialInformation[i].VertexCount = binReader.ReadUInt16();
//                wmoRoot.PortialInformation[i].Plane = binReader.ReadPlane();
//            }
//        }

//        private static void ReadMOPR(BinaryReader binReader, ref int size, WMORoot wmoRoot)
//        {
//            wmoRoot.PortalRelations = new PortalRelation[wmoRoot.Header.PortalCount * 2];

//            for (int i = 0; i < wmoRoot.PortalRelations.Length; i++)
//            {
//                wmoRoot.PortalRelations[i].PortalIndex = binReader.ReadUInt16();
//                wmoRoot.PortalRelations[i].GroupIndex = binReader.ReadUInt16();
//                wmoRoot.PortalRelations[i].Side = binReader.ReadInt16();
//                wmoRoot.PortalRelations[i].Filler = binReader.ReadUInt16();
//            }
//        }
//    }

    

//    public class WMORoot
//    {
//        public string FileName;

//        #region MVER
//        public int Version;
//        #endregion

//        #region MOHD
//        public RootHeader Header;
//        #endregion

//        #region MOTX
//        public string[] Textures;
//        #endregion

//        #region MOGI
//        /// <summary>
//        /// Array of GroupCount size
//        /// </summary>
//        public GroupInformation[] GroupInformation;
//        #endregion

//        #region MOPT
//        /// <summary>
//        /// Array of PortalCount size
//        /// </summary>
//        public PortalInformation[] PortialInformation;
//        #endregion

//        #region MOPR
//        public PortalRelation[] PortalRelations;
//        #endregion

//        public void DumpInfo(TextWriter writer)
//        {
//            writer.WriteLine("WMO Root File: {0}", FileName);
//            Header.DumpInfo(writer, "\t");
//            //writer.WriteLine(" Version: {0}", Version);
//            //writer.WriteLine(" TextureCount: {0}", Header.TextureCount);
//            writer.WriteLine(" GroupCount: {0}", Header.GroupCount);
//            if (Header.GroupCount > 0)
//            {
//                writer.WriteLine("\tGroup Information");
//                for (int i = 0; i < Header.GroupCount; i++)
//                {
//                    writer.WriteLine("\t\tGroup {0}", i);
//                    GroupInformation[i].DumpInfo(writer, "\t\t ");
//                }
//            }
//            writer.WriteLine(" PortalCount: {0}", Header.PortalCount);
//            if (Header.PortalCount > 0)
//            {
//                writer.WriteLine("\tPortal Information");
//                for (int i = 0; i < Header.PortalCount; i++)
//                {
//                    writer.WriteLine("\t\tPortal {0}", i);
//                    PortialInformation[i].DumpInfo(writer, "\t\t");
//                }
//            }
//        }
//    }

    
//}
