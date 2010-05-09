//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Microsoft.Xna.Framework;
//using System.IO;
//using System.Runtime.InteropServices;

//namespace WCell.Collision
//{
//    public static class WMOGroupProcessor
//    {
//        public static WMOGroup Import(string fileName)
//        {
//            WMOGroup group = new WMOGroup();
//            group.FileName = fileName;

//            using (FileStream fs = File.Open(fileName, FileMode.Open))
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
//                                    ReadMVER(br, ref size, group);
//                                    break;
//                                }
//                            case "MOGP":
//                                {
//                                    ReadMOGP(br, ref size, group);
//                                    break;
//                                }
//                            case "MOPY":
//                                {
//                                    ReadMOPY(br, ref size, group);
//                                    break;
//                                }
//                        }

//                        br.BaseStream.Seek(start + size, SeekOrigin.Begin);
//                    }
//                }
//            }

//            return group;
//        }

//        private static void ReadMVER(BinaryReader binReader, ref int size, WMOGroup group)
//        {
//            group.Version = binReader.ReadInt32();
//        }

//        private static void ReadMOGP(BinaryReader binReader, ref int size, WMOGroup group)
//        {
//            /*group.GroupNameOffset = binReader.ReadInt32();
//            group.DescriptiveGroupNameOffset = binReader.ReadInt32();

//            group.Flags = (GroupFlags)binReader.ReadUInt32();

//            group.BoundingBox1 = binReader.ReadVector3();
//            group.BoundingBox2 = binReader.ReadVector3();

//            group.MOPRChunkOffset = binReader.ReadUInt16();
//            group.MOPRItemCount = binReader.ReadUInt16();
//            group.ItemBatchesA = binReader.ReadUInt16();
//            group.ItemBatchesB = binReader.ReadUInt16();
//            group.ItemBatchesC = binReader.ReadInt32();

//            group.FogIndices = binReader.ReadBytes(4);
//            group.Field_34 = binReader.ReadInt32();
//            group.WMOGroupId = binReader.ReadInt32();
//            group.Field_3C = binReader.ReadInt32();
//            group.Field_40 = binReader.ReadInt32();*/

//            group.Header = binReader.ReadStruct<GroupHeader>();

//            size = 68;
//        }

//        private static void ReadMOPY(BinaryReader binReader, ref int size, WMOGroup group)
//        {
//            group.MapObjectPolys = new MapObjectPoly[size / 2];

//            for (int i = 0; i < group.MapObjectPolys.Length; i++)
//            {
//                group.MapObjectPolys[i].Flags = (MapObjectPolyFlags)binReader.ReadByte();
//                group.MapObjectPolys[i].MaterialId = binReader.ReadByte();
//            }
//        }
//    }

//    public class WMOGroup
//    {
//        public string FileName;

//        #region MVER
//        public int Version;
//        #endregion

//        #region MOGP
//        public GroupHeader Header;        
//        #endregion

//        #region MOPY
//        public MapObjectPoly[] MapObjectPolys;
//        #endregion

//        public void DumpInfo(TextWriter writer)
//        {

//            writer.WriteLine("WMO Group File: {0}", FileName);
//            writer.WriteLine(" Version: {0}", Version);
//            writer.WriteLine(" Header");
//            Header.DumpInfo(writer, "\t");

//            writer.WriteLine("\tMOPY Count {0}", MapObjectPolys.Length);
//            /*for (int i=0;i<MapObjectPolys.Length;i++)
//            {
//                writer.WriteLine("\t\tMOPY {0}", i);
//                MapObjectPolys[i].DumpInfo(writer, "\t\t");
//            }*/

//        }

//    }

    
//}
