using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using TerrainAnalysis.TerrainObjects;
using Microsoft.DirectX;

namespace WCell.Collision
{
    public class ADTImporter : ChunkedFileProcessor
    {
        private ADT m_adt;
        private bool highRes;

        public ADTImporter(string fileName, bool highRes)
            : base(fileName)
        {
            m_adt = new ADT();
            m_adt.FileName = fileName;
            m_adt.HighRes = highRes;
            this.highRes = highRes;
            /*
            StreamWriter sw = new StreamWriter("heightstuff.txl", true);
            sw.Write("\n" + fileName + "\n");
            sw.Close();*/
        }

        public ADT ADT
        {
            get { return m_adt; }
        }
    

        protected override void ProcessChunk(string chunkSignature, ref int chunkSize)
        {
            switch (chunkSignature)
            {
                case "MVER":
                    {
                        ReadMVER(m_binReader, m_adt);
                        break;
                    }
                case "MHDR":
                    {
                        ReadMHDR(m_binReader, m_adt);
                        break;
                    }
                case "MCIN":
                    {
                        ReadMCIN(m_binReader, m_adt);
                        break;
                    }
            }
        }

        protected override void PostProcess()
        {
            base.PostProcess();
        }

        private static void ReadMVER(BinaryReader binReader, ADT adt)
        {
            adt.Version = binReader.ReadInt32();
        }

        private static void ReadMHDR(BinaryReader br, ADT adt)
        {
            adt.Header = br.ReadStruct<MapTileHeader>();
        }

        private void ReadMCIN(BinaryReader br, ADT mapTile)
        {
            mapTile.MapChunks = new MapChunk[16,16];

            
            for (int indexX = 0; indexX < 16; indexX++)
            {
                for (int indexY = 0; indexY < 16; indexY++)
                {
                    var mci = br.ReadStruct<MapChunkInformation>();
                    mapTile.MapChunks[indexX, indexY] = LoadMapChunk(indexX, indexY, mci);
                }
            }
        }

        private MapChunk LoadMapChunk(int x, int y, MapChunkInformation mapChunkInfo)
        {
            var start = m_binReader.BaseStream.Position;

            m_binReader.BaseStream.Position = mapChunkInfo.Offset + 8;

            var mapChunk = new MapChunk();

            mapChunk.LoadFrom(x, y, mapChunkInfo, m_binReader, highRes);

            m_binReader.BaseStream.Position = start;

            return mapChunk;

        }

    }


    /// <summary>
    /// Map Tile
    /// </summary>
    public class ADT
    {
        public string FileName;
        public bool HighRes;

        #region MVER
        public int Version;
        #endregion

        #region MHDR

        public MapTileHeader Header;

        #endregion

        #region MCIN

        //public MapChunkInformation[] MapChunkInformation;

        #endregion

        #region MCNK
        public MapChunk[,] MapChunks;
        #endregion
    }


    public class MapChunk
    {
        public MapChunkHeader Header;
        public float[,] HeightMap;
        public float[,] WaterMap;
        public sbyte[,][] Normals;
        private int m_indexX;
        private int m_indexY;
        private MapChunkInformation mci;

        private static float CENTERPOINT = 32.0f * TerrainConstants.Tile.TileSize;

        public uint Holes { get { return Header.HoleMask; } }

        public void LoadFrom(int indexX, int indexY, MapChunkInformation mapChunkInfo, BinaryReader binReader, bool highRes)
        {            
            long end = mapChunkInfo.Offset + mapChunkInfo.Size;            

            mci = mapChunkInfo;

            m_indexX = indexX;
            m_indexY = indexY;

            Header = binReader.ReadStruct<MapChunkHeader>();

            //throw new Exception(Header.ofsNormal.ToString());
            //Normals offset is 724

            //Shuffle offset values to their proper members;
            float xPrime = Header.Position.Y;
            float yBase = Header.Position.Z;
            float zPrime = Header.Position.X;

            Header.Position.X = CENTERPOINT - xPrime;
            Header.Position.Z = CENTERPOINT - zPrime;
            Header.Position.Y = yBase;
                

            /*
            StreamWriter sw = new StreamWriter("heightstuff.txl", true);
            if (indexY == 0)
                sw.Write("\n");

            sw.Write(Header.Position.X.ToString() + "," + Header.Position.Y.ToString() + "," + Header.Position.Z.ToString() + "\t");
            sw.Close();*/

            
            ReadHeightMap(binReader, highRes);
            ReadWaterData(binReader);
            ReadNormals(binReader, highRes);
        }

        private void ReadWaterData(BinaryReader binReader)
        {
            if (Header.sizeLiquid > 8)
            {
                binReader.BaseStream.Position = Header.ofsLiquid + mci.Offset + 8;                

                int waterShiftX = m_indexX * 8;
                int waterShiftY = m_indexY * 8;

                WaterMap = new float[9, 9];

                float waterLevel = binReader.ReadSingle();
                float magmaLevel = binReader.ReadSingle();
                float[,] points = new float[9, 9];

                for (int waterXIndex = 0; waterXIndex < 9; waterXIndex++)
                {
                    for (int waterYIndex = 0; waterYIndex < 9; waterYIndex++)
                    {
                        binReader.BaseStream.Position += 4;
                        float zpos = binReader.ReadSingle();
                        points[waterXIndex, waterYIndex] = zpos < 100000 ? zpos : waterLevel;
                    }
                }

                for (int ni = 0; ni < 8; ni++)
                {
                    for (int nj = 0; nj < 8; nj++)
                    {
                        byte flag = binReader.ReadByte();
                        if ((flag & 8) == 0)
                        {
                            WaterMap[ni + waterShiftX, (nj + waterShiftY)] = (points[ni, nj] +
                                                                     points[ni + 1, nj] +
                                                                     points[ni, nj + 1] +
                                                                     points[ni + 1, nj + 1]) / 4;
                        }
                    }
                }
            }
        }

        private void ReadHeightMap(BinaryReader binReader, bool highRes)
        {
            long start = binReader.BaseStream.Position;

            binReader.BaseStream.Position = mci.Offset + 132 + 12;

            
            HeightMap = new float[(highRes ? 9+8:9), 9];

            for (int row = 0; row < 17; row++)
            {
                for (int col = 0; col < ((row % 2) == 1 ? 8 : 9); col++)
                {                    
                    float yPos = binReader.ReadSingle() + Header.Position.Y;
                    if (highRes)
                        HeightMap[row, col] = yPos;
                    else
                    {
                        if (row % 2 == 0)
                            HeightMap[row / 2, col] = yPos;
                    }
                }
            }
        }

        private void ReadNormals(BinaryReader binReader, bool highRes)
        {
            binReader.BaseStream.Position = mci.Offset + Header.ofsNormal + 8;

            Normals = new sbyte[(highRes ? 9 + 8 : 9), 9][];
            for (int x = 0; x < 9 + 8; x++)
            {                
                for(int z=0; z< ((x%2)==1 ?8 : 9); z++)
                {
                    sbyte normX, normZ, normY;
                    
                    normZ = binReader.ReadSByte();
                    normX = binReader.ReadSByte();
                    normY = binReader.ReadSByte();

                    if (highRes)
                        Normals[x, z] = new sbyte[] { normX, normY, normZ };
                    else
                    {
                        if (x % 2 == 0)
                            Normals[x / 2, z] = new sbyte[] { normX, normY, normZ };
                    }
                }
            }            
        }

        private float GetZTransform()
        {
            return 0.0f;
        }
    }


    [StructLayout(LayoutKind.Sequential, Size = 64)]
    public struct MapTileHeader
    {
        public uint pad;
        public uint offsInfo;
        public uint offsTex;
        public uint offsModels;
        public uint offsModelsIds;
        public uint offsMapObjects;
        public uint offsMapObjectIds;
        public uint offsDoodsDef;
        public uint offsObjectsDef;
        public uint offsFlightBoundary;
        public uint offsMH20;
        public uint pad3;
        public uint pad4;
        public uint pad5;
        public uint pad6;
        public uint pad7;
    }

    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public struct MapChunkInformation
    {
        public uint Offset;
        public uint Size;
        public MapChunkInfoFlags Flags;
        public uint AsyncId;
    }

    [Flags]
    public enum MapChunkInfoFlags : int
    {

    }

    [StructLayout(LayoutKind.Sequential, Size = 128)]
    public struct MapChunkHeader
    {
        public uint flags;//0x00
        public int IndexX;//0x04
        public int IndexY;//0x08
        public uint nLayers;//0xC
        public uint nDoodadRefs;//0x10
        public uint ofsHeight;//0x14
        public uint ofsNormal;//0x18
        public uint ofsLayer;//0x1C
        public uint ofsRefs;//0x20
        public uint ofsAlpha;//0x24
        public uint sizeAlpha;//0x28
        public uint ofsShadow;//0x2C
        public uint sizeShadow;//0x30
        public int AreaId;//0x34
        public uint nMapObjRefs;//0x38
        /// <summary>
        /// This is a bitmapped field, the least significant 16 bits are 
        /// used row-wise in the following arrangement with a 1 bit 
        /// meaning that the map chunk has a hole in that part of its area: 
        /// </summary>
        public uint HoleMask;//0x3C
        public ushort Field_40;//0x40
        public ushort Field_42;//0x42
        public uint Field_44;//0x44
        public uint Field_48;//0x48
        public uint Field_4C;//0x4C
        public uint predTex;//0x50
        public uint nEffectDoodad;//0x54
        public uint ofsSndEmitters;//0x58
        public uint nSndEmitters;//0x5C
        public uint ofsLiquid;//0x60
        public uint sizeLiquid;//0x64
        public Vector3 Position;//0x68
        public uint textureId;//0x74
        public uint props;//0x78
        public uint effectId;//0x7C
    }
    


}
