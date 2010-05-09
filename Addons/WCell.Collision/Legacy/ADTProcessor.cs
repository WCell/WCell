//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.IO;
//using System.Runtime.InteropServices;
//using Microsoft.Xna.Framework;

//namespace WCell.Collision
//{
//    public class ADTImporter : ChunkedFileProcessor
//    {
//        private ADT m_adt;

//        public ADTImporter(string fileName)
//            : base(fileName)
//        {
//            m_adt = new ADT
//                        {
//                            FileName = fileName
//                        };
//            /*
//            StreamWriter sw = new StreamWriter("heightStuff.txl", true);
//            sw.WriteLine(fileName);
//            sw.Close();*/
//        }

//        public ADT ADT
//        {
//            get { return m_adt; }
//        }
    

//        protected override void ProcessChunk(string chunkSignature, ref int chunkSize)
//        {
//            switch (chunkSignature)
//            {
//                case "MVER":
//                    {
//                        ReadMVER(m_binReader, m_adt);
//                        break;
//                    }
//                case "MHDR":
//                    {
//                        ReadMHDR(m_binReader, m_adt);
//                        break;
//                    }
//                case "MCIN":
//                    {
//                        ReadMCIN(m_binReader, m_adt);
//                        break;
//                    }
//            }
//        }

//        protected override void PostProcess()
//        {
//            base.PostProcess();
//        }

//        private static void ReadMVER(BinaryReader binReader, ADT adt)
//        {
//            adt.Version = binReader.ReadInt32();
//        }

//        private static void ReadMHDR(BinaryReader br, ADT adt)
//        {
//            adt.Header = br.ReadStruct<MapTileHeader>();
//        }

//        private void ReadMCIN(BinaryReader br, ADT mapTile)
//        {
//            mapTile.MapChunks = new MapChunk[16,16];

            
//            for (int indexX = 0; indexX < 16; indexX++)
//            {
//                for (int indexY = 0; indexY < 16; indexY++)
//                {
//                    MapChunkInformation mci = br.ReadStruct<MapChunkInformation>();
//                    mapTile.MapChunks[indexX, indexY] = LoadMapChunk(indexX, indexY, mci);
//                }
//            }
//        }

//        private MapChunk LoadMapChunk(int x, int y, MapChunkInformation mapChunkInfo)
//        {
//            long start = m_binReader.BaseStream.Position;

//            m_binReader.BaseStream.Position = mapChunkInfo.Offset + 8;

//            MapChunk mapChunk = new MapChunk();

//            mapChunk.LoadFrom(x, y, mapChunkInfo, m_binReader);

//            m_binReader.BaseStream.Position = start;

//            return mapChunk;

//        }

//    }


//    /// <summary>
//    /// Map Tile
//    /// </summary>
//    public class ADT
//    {
//        public string FileName;

//        #region MVER
//        public int Version;
//        #endregion

//        #region MHDR

//        public MapTileHeader Header;

//        #endregion

//        #region MCIN

//        //public MapChunkInformation[] MapChunkInformation;

//        #endregion

//        #region MCNK

//        public MapChunk[,] MapChunks;

//        #endregion


//    }
    
//    /// <summary>
//    /// Represents the MH2O section of the ADT map file. MH2O = 0x4D48324F
//    /// </summary>
//    public class LiquidMap
//    {
//        private struct ChunkInfo
//        {
//            public int ofsChunkHeader;
//            public int ChunkHasLiquids;
//            public int ofsData2;
//        }

//        private readonly int startingOffset;

//        public LiquidMap(BinaryReader binReader)
//        {
//            startingOffset = (int)binReader.BaseStream.Position;

//            var headers = ReadChunkInfo(binReader);
//            ReadLiquidChunks(binReader, headers);
//        }

//        public LiquidMapChunk[] Chunks;

//        private static ChunkInfo[] ReadChunkInfo(BinaryReader binaryReader)
//        {
//            var headers = new ChunkInfo[256];

//            for (int i=0;i<headers.Length;i++)
//            {
//                headers[i] = binaryReader.ReadStruct<ChunkInfo>();
//            }
//            return headers;
//        }

//        private void ReadLiquidChunks(BinaryReader binaryReader, ChunkInfo[] chunkInfos)
//        {
//            Chunks = new LiquidMapChunk[256];
//            for (int i = 0; i < chunkInfos.Length; i++)
//            {
//                var chunkInfo = chunkInfos[i];
//                var chunk = Chunks[i];

//                binaryReader.BaseStream.Position = startingOffset + chunkInfo.ofsChunkHeader;

//                chunk.ChunkHeader = binaryReader.ReadStruct<LiquidMapChunkHeader>();

//                var liquidMapChunkHeader = chunk.ChunkHeader;

//                if (liquidMapChunkHeader.ofsPlacementMask > 0)
//                {
//                    // Warp to the placement mask
//                    binaryReader.BaseStream.Position = startingOffset + liquidMapChunkHeader.ofsPlacementMask;

//                    chunk.PlacementMask = binaryReader.ReadBytes(liquidMapChunkHeader.Height);
//                }

//                if (liquidMapChunkHeader.ofsWaterLevels > 0)
//                {
//                    // Time to read the water levels
//                    binaryReader.BaseStream.Position = startingOffset + liquidMapChunkHeader.ofsWaterLevels;

//                    // We need to read a number of floats equal to (data1.Width * data1.Height) quads
//                    // So we add 1 to both to get the number of floats
//                    chunk.LiquidMap = new float[liquidMapChunkHeader.Height + 1][];
//                    for (int x = 0; i < chunk.LiquidMap.Length; i++)
//                    {
//                        chunk.LiquidMap[x] = new float[liquidMapChunkHeader.Width + 1];
//                        for (int y = 0; y < chunk.LiquidMap[x].Length; y++)
//                        {
//                            chunk.LiquidMap[x][y] = binaryReader.ReadSingle();
//                        }
//                    }
//                }
//            }
//        }

//        public struct LiquidMapChunk
//        {
//            public LiquidMapChunkHeader ChunkHeader;

//            public byte[] PlacementMask;
//            public float[][] LiquidMap;
//        }

//        [StructLayout(LayoutKind.Sequential)]
//        public struct LiquidMapChunkHeader
//        {
//            /// <summary>
//            /// Mostly 5
//            /// </summary>
//            public ushort Flags;
//            /// <summary>
//            /// Seems to be the type of liquid (0 for normal "lake" water, 1 for lava and 2 for ocean water)
//            /// </summary>
//            public ushort Type;

//            /// <summary>
//            /// The global liquid-height of this chunk.
//            /// </summary>
//            public float HeightLevel1;
//            /// <summary>
//            /// Which is always in there twice. Blizzard knows why. 
//            /// (Actually these values are not always identical, I think they define the highest and lowest points in the heightmap)
//            /// </summary>
//            public float HeightLevel2;

//            /// <summary>
//            /// The X offset of the liquid square (0-7)
//            /// </summary>
//            public byte XOffset;
//            /// <summary>
//            /// The Y offset of the liquid square (0-7)
//            /// </summary>
//            public byte YOffset;
//            /// <summary>
//            /// The width of the liquid square (1-8)
//            /// </summary>
//            public byte Width;
//            /// <summary>
//            /// The height of the liquid square (1-8)
//            /// </summary>
//            public byte Height;

//            /// <summary>
//            /// Offset to the byte array of <see>ChunkHeader.Height</see> length where each byte represents a row of quads.
//            /// For each of the 8 bits, if a bit is set, water is rendered in that location.
//            /// </summary>
//            /// <example>
//            /// ++++++++
//            /// xxxxxx++
//            /// ++xxxxxx
//            /// ++++++++
//            /// In this example, we have a height of 2, so there would be 2 bytes starting at <see cref="ofsPlacementMask"/>.
//            /// The first line would be 0xFC (binary 11111100)
//            /// The second line would be 0x3F (binary 00111111)
//            /// </example>
//            public int ofsPlacementMask;
//            /// <summary>
//            /// Offset to the 9x9 heightmap.
//            /// Note that there will only be ((height+1) * (width+1)) floats here, 9x9 is the maximum number of floats.
//            /// (height * width) gives you the number of quads, and to get the number of floats you do ((height+1) * (width+1))
//            /// </summary>
//            public int ofsWaterLevels;

            
//        }
//    }


//    public class MapChunk
//    {
//        public MapChunkHeader Header;
//        public Vector3[,] HeightMap;
//        public float[,] WaterMap;
//        public Vector3[,] Normals;
//        private int m_indexX;
//        private int m_indexY;
//        private MapChunkInformation mci;

//        private static float TILELENGTH = 533.3333f; //scaler for tile width in the rendering system
//        private static float CHUNKLENGTH = TILELENGTH / 16.0f; //16x16 chunks per tile
//        private static float UNITLENGTH = CHUNKLENGTH / 8.0f;
        

//        public void LoadFrom(int indexX, int indexY, MapChunkInformation mapChunkInfo, BinaryReader binReader)
//        {            
//            long end = mapChunkInfo.Offset + mapChunkInfo.Size;            

//            mci = mapChunkInfo;

//            m_indexX = indexX;
//            m_indexY = indexY;

//            Header = binReader.ReadStruct<MapChunkHeader>();
            
//            //Shuffle offset values to their proper members;
//            float xPrime = Header.Position.Y;
//            float yBase = Header.Position.Z;
//            float zPrime = Header.Position.X;

//            Header.Position.X = (TILELENGTH * 32.0f) - xPrime;
//            Header.Position.Z = (TILELENGTH * 32.0f) - zPrime;
//            Header.Position.Y = yBase;

//            ReadHeightMap(binReader);
//            //ReadWaterData(binReader);
//            //ReadNormals(binReader);
//        }

//        //private void ReadWaterData(BinaryReader binReader)
//        //{
//        //    if (Header.sizeLiquid > 0)
//        //    {
//        //        binReader.BaseStream.Position = Header.ofsLiquid + mci.Offset + 4 + 132;                

//        //        int waterShiftX = m_indexX * 8;
//        //        int waterShiftY = m_indexY * 8;

//        //        WaterMap = new float[9, 9];

//        //        float waterLevel = binReader.ReadSingle();
//        //        float magmaLevel = binReader.ReadSingle();
//        //        float[,] points = new float[9, 9];

//        //        for (int waterXIndex = 0; waterXIndex < 9; waterXIndex++)
//        //        {
//        //            for (int waterYIndex = 0; waterYIndex < 9; waterYIndex++)
//        //            {
//        //                binReader.BaseStream.Position += 4;
//        //                float zpos = binReader.ReadSingle();
//        //                points[waterXIndex, waterYIndex] = zpos < 100000 ? zpos : waterLevel;
//        //            }
//        //        }

//        //        for (int ni = 0; ni < 8; ni++)
//        //        {
//        //            for (int nj = 0; nj < 8; nj++)
//        //            {
//        //                byte flag = binReader.ReadByte();
//        //                if ((flag & 8) == 0)
//        //                {
//        //                    WaterMap[ni + waterShiftX, (nj + waterShiftY)] = (points[ni, nj] +
//        //                                                             points[ni + 1, nj] +
//        //                                                             points[ni, nj + 1] +
//        //                                                             points[ni + 1, nj + 1]) / 4;
//        //                }
//        //            }
//        //        }
//        //    }
//        //}

//        private void ReadHeightMap(BinaryReader binReader)
//        {
//            long start = binReader.BaseStream.Position;

//            binReader.BaseStream.Position = mci.Offset + 132 + 8;// ofsHeight is always zero because it immediately proceeds the header.
            
//            /*
//            StreamWriter sw = new StreamWriter("heightStuff.txl", true);
//            if (this.m_indexY == 0)
//                sw.Write("\n");
//            //if (this.m_indexY % 2 == 1)
//                //sw.Write("\t");
//            sw.Write("Tile Offset: " + this.Header.Position.X.ToString() +  "," + this.Header.Position.Y.ToString() + "," + this.Header.Position.Z + "\t");*/
 

//            //int shiftX = m_indexX * 16;
//            //int shiftY = m_indexY * 16;

//            HeightMap = new Vector3[9+8, 9];

//            for (int indexX = 0; indexX < 17; indexX++)
//            {
//                for (int indexZ = 0; indexZ < ((indexX % 2) == 1 ? 8 : 9); indexZ++)
//                {
//                    //Reads in height data and positions each chunk vertex relative to the chunk base point
//                    float yPos = binReader.ReadSingle() + Header.Position.Y;
//                    float xPos = (indexX * UNITLENGTH) + Header.Position.X;
//                    if (indexX % 2 == 1) xPos += UNITLENGTH * 0.5f;
//                    float zPos = (indexZ * UNITLENGTH * 0.5f)+Header.Position.Z;                                       
//                    HeightMap[indexX, indexZ] = new Vector3(xPos,yPos,zPos);
//                }
//            }

//            /*
//            if (m_indexY == 15 && m_indexX == 15)
//                sw.Write("\nEND TILE\n");
//            sw.Close();*/

//        }

//        private void ReadNormals(BinaryReader binReader)
//        {
//            binReader.BaseStream.Position = mci.Offset + 132 + 8 + Header.ofsNormal;

//            StreamWriter sw = new StreamWriter("heightStuff.txl", true);
//            sw.WriteLine("NORMALS\n");
//            Normals = new Vector3[9 + 8, 9];
//            for (int x = 0; x < 9 + 8; x++)
//            {
//                sw.Write("\n");
//                for(int z=0; z< ((x%2)==1 ?8 : 9); z++)
//                {
//                    sbyte normX = binReader.ReadSByte();
//                    sbyte normZ = binReader.ReadSByte();
//                    sbyte normY = binReader.ReadSByte();
//                    Normals[x, z].X = normX / 127.0f;
//                    Normals[x, z].Z = normZ / 127.0f;
//                    Normals[x, z].Y = normY / 127.0f;
//                    sw.Write(Normals[x, z].X + "," + Normals[x, z].Y + "," + Normals[x, z].Z + " | ");
//                }
//            }
//            sw.Close();
//        }

//        private float GetZTransform()
//        {
//            return 0.0f;
//        }
//    }


//    [StructLayout(LayoutKind.Sequential, Size = 64)]
//    public struct MapTileHeader
//    {
//        public uint pad;
//        public uint offsInfo;
//        public uint offsTex;
//        public uint offsModels;
//        public uint offsModelsIds;
//        public uint offsMapObejcts;
//        public uint offsMapObejctsIds;
//        public uint offsDoodsDef;
//        public uint offsObjectsDef;
//        public uint pad1;
//        public uint pad2;
//        public uint pad3;
//        public uint pad4;
//        public uint pad5;
//        public uint pad6;
//        public uint pad7;
//    }

//    [StructLayout(LayoutKind.Sequential, Size = 16)]
//    public struct MapChunkInformation
//    {
//        public uint Offset;
//        public uint Size;
//        public MapChunkInfoFlags Flags;
//        public uint AsyncId;
//    }

//    [Flags]
//    public enum MapChunkInfoFlags : int
//    {

//    }

//    [StructLayout(LayoutKind.Sequential, Size = 128)]
//    public struct MapChunkHeader
//    {
//        public uint flags;//0x00
//        public int IndexX;//0x04
//        public int IndexY;//0x08
//        public uint nLayers;//0xC
//        public uint nDoodadRefs;//0x10
//        public uint ofsHeight;//0x14
//        public uint ofsNormal;//0x18
//        public uint ofsLayer;//0x1C
//        public uint ofsRefs;//0x20
//        public uint ofsAlpha;//0x24
//        public uint sizeAlpha;//0x28
//        public uint ofsShadow;//0x2C
//        public uint sizeShadow;//0x30
//        public int AreaId;//0x34
//        public uint nMapObjRefs;//0x38
//        /// <summary>
//        /// This is a bitmapped field, the least significant 16 bits are used row-wise in the following arrangement with a 1 bit meaning that the map chunk has a hole in that part of its area: 
//        /// </summary>
//        public uint HoleMask;//0x3C
//        public ushort Field_40;//0x40
//        public ushort Field_42;//0x42
//        public uint Field_44;//0x44
//        public uint Field_48;//0x48
//        public uint Field_4C;//0x4C
//        public uint predTex;//0x50
//        public uint nEffectDoodad;//0x54
//        public uint ofsSndEmitters;//0x58
//        public uint nSndEmitters;//0x5C
//        public uint ofsLiquid;//0x60
//        public uint sizeLiquid;//0x64
//        public Vector3 Position;//0x68
//        public uint textureId;//0x74
//        public uint props;//0x78
//        public uint effectId;//0x7C
//    }
    


//}
