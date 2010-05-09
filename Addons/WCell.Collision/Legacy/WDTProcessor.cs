//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.IO;
//using Microsoft.Xna.Framework;
//using System.Runtime.InteropServices;

//namespace WCell.Collision
//{
//    public static class WDTProcessor
//    {
//        public static WDT Process(string fileName)
//        {
//            var wdt = new WDT {FileName = fileName};

//            using (var fs = File.Open(fileName, FileMode.Open))
//            {
//                using (var br = new BinaryReader(fs))
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
//                                    ReadMVER(br, ref size, wdt);
//                                    break;
//                                }
//                            case "MPHD":
//                                {
//                                    ReadMPHD(br, ref size, wdt);
//                                    break;
//                                }

//                            case "MAIN":
//                                {
//                                    ReadMAIN(br, ref size, wdt);
//                                    break;
//                                }

//                            case "MWMO":
//                                {
//                                    ReadMWMO(br, ref size, wdt);
//                                    break;
//                                }

//                            case "MODF":
//                                {
//                                    ReadMODF(br, ref size, wdt);
//                                    break;
//                                }
//                        }

//                        br.BaseStream.Seek(start + size, SeekOrigin.Begin);
//                    }
//                }
//            }

//            return wdt;
//        }

//        private static void ReadMVER(BinaryReader binReader, ref int size, WDT wdt)
//        {
//            wdt.Version = binReader.ReadInt32();
//        }

//        private static void ReadMPHD(BinaryReader binReader, ref int size, WDT wdt)
//        {
//            wdt.Header = new int[8];

//            for (int i = 0; i < 8; i++)
//            {
//                wdt.Header[i] = binReader.ReadInt32();
//            }
//        }

//        private static void ReadMAIN(BinaryReader binReader, ref int size, WDT wdt)
//        {
//            wdt.MapTileTable = new long[64][];

//            for (int x = 0; x < 64; x++)
//            {
//                wdt.MapTileTable[x] = new long[64];
//                for (int y = 0; y < 64; y++)
//                {
//                    wdt.MapTileTable[x][y] = binReader.ReadInt64();
//                }
//            }
//        }

//        private static void ReadMWMO(BinaryReader binReader, ref int size, WDT wdt)
//        {
//            wdt.WMOFileNames = new List<string>();

//            int i = 0;
//            while (i < size)
//            {
//                string str = binReader.ReadCString();
//                i += str.Length + 1;
//                wdt.WMOFileNames.Add(str);
//            }
//        }

//        private static void ReadMODF(BinaryReader binReader, ref int size, WDT wdt)
//        {

//        }
//    }

//    public class WDT
//    {
//        public string FileName;

//        #region MVER
//        public int Version;
//        #endregion

//        #region MPHD
//        /// <summary>
//        /// The first integer is 1 for maps without any terrain, 0 otherwise. The rest are always 0. 
//        /// </summary>
//        public int[] Header;
//        #endregion

//        #region MAIN

//        /// <summary>
//        /// Contains 64x64 = 4096 records of 8 bytes each. Each record can be considered a 64-bit integer: 1 if a tile is present, 0 otherwise.
//        /// </summary>
//        public long[][] MapTileTable;

//        #endregion

//        #region MWMO
//        /// <summary>
//        /// List of filenames for WMOs (world map objects) that appear in this map tile. A contiguous block of zero-terminated strings.
//        /// </summary>
//        public List<string> WMOFileNames;

//        #endregion

//        #region MODF

//        public MapObjectDefinition[] MapObjectDefinition;

//        #endregion
//    }    
//}
