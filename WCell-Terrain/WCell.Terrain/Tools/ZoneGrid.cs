using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Core.DBC;
using WCell.Core.World;
using WCell.MPQTool;
using WCell.RealmServer;
using WCell.Util.Toolshed;

namespace WCell.Tools.Maps
{
    public static class ZoneGrid
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        private static MpqManager manager;
        private const string baseDir = "WORLD\\maps\\";

        [Tool]
        public static ZoneTileSet[] GetZoneTileSets()
        {
            var wowRootDir = DBCTool.FindWowDir(null);
            manager = new MpqManager(wowRootDir);

            var entryList = GetMapEntries();
            if (entryList == null)
            {
                Console.WriteLine("Error retrieving Map Entries.");
                return null;
            }

			var tiles = new ZoneTileSet[(int)MapId.End];
			var count = 0;

            foreach (var dbcMapEntry in entryList)
            {
                var dir = dbcMapEntry.MapDirName;
                var wdtDir = Path.Combine(baseDir, dir);
                var wdtName = dir;

                var wdt = RegionBoundaries.Process(manager, wdtDir, wdtName + ".wdt");
                if (wdt == null) continue;

                if ((wdt.Header.Header1 & WDTFlags.GlobalWMO) == 0)
                {
                    var tileSet = new ZoneTileSet();

                    // Read in the Tiles
                    for (var y = 0; y < 64; y++)
                    {
                        for (var x = 0; x < 64; x++)
                        {
                            if (!wdt.TileProfile[x, y]) continue;
                        	++count;
                            var adtName = string.Format("{0}_{1:00}_{2:00}", wdtName, x, y);
                            var adt = Process(manager, wdtDir, adtName);
                            if (adt == null) continue;

                            // Read in the TileChunks 
                            for (var j = 0; j < 16; j++)
                            {
                                for (var i = 0; i < 16; i++)
                                {
                                    var areaId = adt.MapChunks[i, j].AreaId;
									tileSet.ZoneGrids[x, y].ZoneIds[i, j] = (ZoneId)areaId;
                                }
                            }
                        }
                    }

                    tiles[dbcMapEntry.Id] = tileSet;
                }
				else
                {
                	log.Info("Could not read Zones from WMO: " + (MapId)dbcMapEntry.Id);
                }
            }

        	log.Info("Exported {0} ZoneTileSets.", count);

            return tiles;
        }

        public static List<DBCMapEntry> GetMapEntries()
        {
            var wowRootDir = DBCTool.FindWowDir(null);
            if (!Directory.Exists(wowRootDir))
            {
                log.Error("Could not find WoW-directory: \"{0}\"", wowRootDir);
                return null;
            }

            var DBCMapEntryReader = new DBCReader<DBCMapEntry, DBCMapConverter>(
                RealmServerConfiguration.GetDBCFile("Map.dbc"));

            return DBCMapEntryReader.EntryList;
        }

        public static ADTFile Process(MpqManager manager, string dataDirectory, string adtName)
        {
            var adtFilePath = Path.Combine(dataDirectory, adtName + ".adt");
            if (!manager.FileExists(adtFilePath)) return null;

            var fileReader = new BinaryReader(manager.OpenFile(adtFilePath));
            var adt = new ADTFile() {
                Name = adtName,
                Path = dataDirectory
            };

            ReadMVER(fileReader, adt);
            ReadMHDR(fileReader, adt);
            
            if (adt.Header.offsInfo != 0)
            {
                fileReader.BaseStream.Position = adt.Header.Base + adt.Header.offsInfo;
                ReadMCIN(fileReader, adt);
            }
            
            ReadMCNKs(fileReader, adt);
            
            if (adt.Header.offsMH2O != 0)
            {
                fileReader.BaseStream.Position = adt.Header.Base + adt.Header.offsMH2O;
                ReadMH2O(fileReader, adt);
            }

            fileReader.Close();

            return adt;

        }

        static void ReadMVER(BinaryReader fileReader, ADTFile adt)
        {
            var type = fileReader.ReadUInt32();
            var size = fileReader.ReadUInt32();

            adt.Version = fileReader.ReadInt32();
        }

        static void ReadMHDR(BinaryReader fileReader, ADTFile adt)
        {
            var type = fileReader.ReadUInt32();
            var size = fileReader.ReadUInt32();

            adt.Header.Base = (uint)fileReader.BaseStream.Position;

            var pad = fileReader.ReadUInt32();

            adt.Header.offsInfo = fileReader.ReadUInt32();
            adt.Header.offsTex = fileReader.ReadUInt32();
            adt.Header.offsModels = fileReader.ReadUInt32();
            adt.Header.offsModelIds = fileReader.ReadUInt32();
            adt.Header.offsMapObjects = fileReader.ReadUInt32();
            adt.Header.offsMapObjectIds = fileReader.ReadUInt32();
            adt.Header.offsDoodadDefinitions = fileReader.ReadUInt32();
            adt.Header.offsObjectDefinitions = fileReader.ReadUInt32();
            adt.Header.offsFlightBoundary = fileReader.ReadUInt32();
            adt.Header.offsMH2O = fileReader.ReadUInt32();

            var pad3 = fileReader.ReadUInt32();
            var pad4 = fileReader.ReadUInt32();
            var pad5 = fileReader.ReadUInt32();
            var pad6 = fileReader.ReadUInt32();
            var pad7 = fileReader.ReadUInt32();
        }

        static void ReadMCIN(BinaryReader fileReader, ADTFile adt)
        {
            var type = fileReader.ReadUInt32();
            var size = fileReader.ReadUInt32();

            adt.MapChunkInfo = new MapChunkInfo[256];
            for (var i = 0; i < 256; i++)
            {
                var mcin = new MapChunkInfo
                {
                    Offset = fileReader.ReadUInt32(),
                    Size = fileReader.ReadUInt32(),
                    Flags = fileReader.ReadUInt32(),
                    AsyncId = fileReader.ReadUInt32()
                };

                adt.MapChunkInfo[i] = mcin;
            }
        }

        static void ReadMCNKs(BinaryReader br, ADTFile adt)
        {
            for (var i = 0; i < 256; i++)
            {
                var chunk = ProcessChunk(br, adt.MapChunkInfo[i].Offset);
                adt.MapChunks[chunk.IndexX, chunk.IndexY] = chunk;

                if (chunk.ofsHeight != 0)
                {
                    adt.HeightMaps[chunk.IndexX, chunk.IndexY] = ProcessHeights(br, adt.MapChunkInfo[i].Offset + chunk.ofsHeight);
                }
            }
        }

        static void ReadMH2O(BinaryReader fileReader, ADTFile adt)
        {
            var sig = fileReader.ReadUInt32();
            var size = fileReader.ReadUInt32();

            var ofsMH2O = fileReader.BaseStream.Position;
            var mh2oHeader = new MH2OHeader[256];

            for (var i = 0; i < 256; i++)
            {
                mh2oHeader[i] = new MH2OHeader {
                    ofsData1 = fileReader.ReadUInt32(),
                    LayerCount = fileReader.ReadUInt32(),
                    ofsData2 = fileReader.ReadUInt32()
                };
            }

            for (var y = 0; y < 16; y++)
            {
                for (var x = 0; x < 16; x++)
                {
                    adt.LiquidMaps[x, y] = ProcessMH2O(fileReader, mh2oHeader[y * 16 + x], ofsMH2O);
                }
            }
        }

        private static MH2O ProcessMH2O(BinaryReader fileReader, MH2OHeader header, long waterSegmentBase)
        {
            var water = new MH2O();

            if (header.LayerCount == 0)
            {
                water.Used = false;
                return water;
            }

            water.Used = true;

            fileReader.BaseStream.Position = waterSegmentBase + header.ofsData1;

            water.Flags = (MH2OFlags)fileReader.ReadUInt16();
            
            water.Type = (FluidType)fileReader.ReadUInt16();
            water.HeightLevel1 = fileReader.ReadSingle();
            water.HeightLevel2 = fileReader.ReadSingle();
            water.XOffset = fileReader.ReadByte();
            water.YOffset = fileReader.ReadByte();
            water.Width = fileReader.ReadByte();
            water.Height = fileReader.ReadByte();

            var ofsWaterFlags = fileReader.ReadUInt32();
            var ofsWaterHeightMap = fileReader.ReadUInt32();

            var heightMapLen = (water.Width + 1) * (water.Height + 1);
            var heights = new float[heightMapLen];

            // If flags is 2, the chunk is for an ocean, and there is no heightmap
            if (ofsWaterHeightMap != 0 && (water.Flags & MH2OFlags.Ocean) == 0)
            {
                fileReader.BaseStream.Position = waterSegmentBase + ofsWaterHeightMap;
                
                for (var i = 0; i < heightMapLen; i++)
                {
                    heights[i] = fileReader.ReadSingle();
                    if (heights[i] == 0)
                    {
                        heights[i] = water.HeightLevel1;
                    }
                }
                
            }
            else
            {
                for (var i = 0; i < heightMapLen; i++)
                {
                    heights[i] = water.HeightLevel1;
                }
            }

            water.Heights = new float[water.Height + 1, water.Width + 1];
            for (var r = 0; r <= water.Height; r++)
            {
                for (var c = 0; c <= water.Width; c++)
                {

                    water.Heights[r, c] = heights[c + r * c];
                }
            }

            return water;
        }

        static MCNK ProcessChunk(BinaryReader fileReader, uint mcnkOffset)
        {
            var mcnk = new MCNK();

            fileReader.BaseStream.Position = mcnkOffset;

            var ca = "MCNK".ToCharArray();
            var b0 = (uint)ca[0];
            var b1 = (uint)ca[1];
            var b2 = (uint)ca[2];
            var b3 = (uint)ca[3];
            var magic = b3 | (b2 << 8) | (b1 << 16) | (b0 << 24);

            var sig = fileReader.ReadUInt32();
            if (sig != magic)
            {
                Console.WriteLine("Invalid Chunk offset found.");
            }

            var mcnkSize = fileReader.ReadUInt32();

            mcnk.Flags = fileReader.ReadInt32();
            mcnk.IndexX = fileReader.ReadInt32();
            mcnk.IndexY = fileReader.ReadInt32();
            mcnk.nLayers = fileReader.ReadUInt32(); //0xC
            mcnk.nDoodadRefs = fileReader.ReadUInt32(); //0x10
            mcnk.ofsHeight = fileReader.ReadUInt32(); //0x14
            mcnk.ofsNormal = fileReader.ReadUInt32(); //0x18
            mcnk.ofsLayer = fileReader.ReadUInt32(); //0x1C
            mcnk.ofsRefs = fileReader.ReadUInt32(); //0x20
            mcnk.ofsAlpha = fileReader.ReadUInt32(); //0x24
            mcnk.sizeAlpha = fileReader.ReadUInt32(); //0x28
            mcnk.ofsShadow = fileReader.ReadUInt32(); //0x2C
            mcnk.sizeShadow = fileReader.ReadUInt32(); //0x30
            mcnk.AreaId = fileReader.ReadInt32(); //0x34
            mcnk.nMapObjRefs = fileReader.ReadUInt32(); //0x38

            mcnk.Holes = fileReader.ReadUInt16();
            fileReader.ReadUInt16(); // pad

            mcnk.predTex = new ushort[8];
            for (var i = 0; i < 8; i++)
            {
                mcnk.predTex[i] = fileReader.ReadUInt16();
            }

            mcnk.nEffectDoodad = new byte[8];
            for (var i = 0; i < 8; i++)
            {
                mcnk.nEffectDoodad[i] = fileReader.ReadByte();
            }
            mcnk.ofsSndEmitters = fileReader.ReadUInt32(); //0x58
            mcnk.nSndEmitters = fileReader.ReadUInt32(); //0x5C
            mcnk.ofsLiquid = fileReader.ReadUInt32(); //0x60
            mcnk.sizeLiquid = fileReader.ReadUInt32(); //0x64
            mcnk.Z = fileReader.ReadSingle();
            mcnk.X = fileReader.ReadSingle();
            mcnk.Y = fileReader.ReadSingle();
            mcnk.offsColorValues = fileReader.ReadInt32();
            mcnk.props = fileReader.ReadInt32();
            mcnk.effectId = fileReader.ReadInt32();

            return mcnk;
        }

        static MCVT ProcessHeights(BinaryReader fileReader, uint mcvtOffset)
        {
            fileReader.BaseStream.Position = mcvtOffset;
            var heights = new MCVT();
            var sig = fileReader.ReadUInt32();
            var size = fileReader.ReadUInt32();
            for (var i = 0; i < 145; i++)
            {
                heights.Heights[i] = fileReader.ReadSingle();
            }
            return heights;
        }

    }

    public class ADTFile
    {
        public string Name;
        public string Path;
        /// <summary>
        /// Version of the ADT
        /// </summary>
        /// <example></example>
        public int Version;

        public readonly MapHeader Header = new MapHeader();

        public MapChunkInfo[] MapChunkInfo;
        /// <summary>
        /// Array of MCNK chunks which give the ADT vertex information for this ADT
        /// </summary>
        public readonly MCNK[,] MapChunks = new MCNK[16, 16];

        public readonly MCVT[,] HeightMaps = new MCVT[16, 16];

        public readonly MH2O[,] LiquidMaps = new MH2O[16, 16];
    }

    public class MapHeader
    {
        public uint Base;

        public uint offsInfo;
        public uint offsTex;
        public uint offsModels;
        public uint offsModelIds;
        public uint offsMapObjects;
        public uint offsMapObjectIds;
        public uint offsDoodadDefinitions;
        public uint offsObjectDefinitions;
        public uint offsFlightBoundary; // tbc, wotlk	
        public uint offsMH2O;		// new in WotLK
    }

    public class MapChunkInfo
    {
        /// <summary>
        /// Offset to the start of the MCNK chunk, relative to the start of the file header data
        /// </summary>
        public uint Offset;
        public uint Size;
        public uint Flags;
        public uint AsyncId;
    }

    public class MCNK
    {
        public int Flags;
        public int IndexX;
        public int IndexY;
        public uint nLayers;
        public uint nDoodadRefs;
        /// <summary>
        /// Offset to the MCVT chunk
        /// </summary>
        public uint ofsHeight;
        /// <summary>
        /// Offset to the MCNR chunk
        /// </summary>
        public uint ofsNormal;
        /// <summary>
        /// Offset to the MCLY chunk
        /// </summary>
        public uint ofsLayer;
        /// <summary>
        /// Offset to the MCRF chunk
        /// </summary>
        public uint ofsRefs;
        /// <summary>
        /// Offset to the MCAL chunk
        /// </summary>
        public uint ofsAlpha;
        public uint sizeAlpha;
        /// <summary>
        /// Offset to the MCSH chunk
        /// </summary>
        public uint ofsShadow;
        public uint sizeShadow;
        public int AreaId;
        public uint nMapObjRefs;

        /// <summary>
        /// Holes of the MCNK
        /// </summary>
        public ushort Holes;

        public ushort[] predTex;
        public byte[] nEffectDoodad;

        /// <summary>
        /// Offset to the MCSE chunk
        /// </summary>
        public uint ofsSndEmitters;
        public uint nSndEmitters;
        /// <summary>
        /// Offset to the MCLQ chunk (deprecated)
        /// </summary>
        public uint ofsLiquid;
        public uint sizeLiquid;
        /// <summary>
        /// X position of the MCNK
        /// </summary>
        public float X;
        /// <summary>
        /// Y position of the MCNK
        /// </summary>
        public float Y;
        /// <summary>
        /// Z position of the MCNK
        /// </summary>
        public float Z;
        /// <summary>
        /// Offset to the MCCV chunk, if present
        /// </summary>
        public int offsColorValues; // formerly textureId
        public int props;
        public int effectId;

        public MCNK()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x">X position of the MCNK</param>
        /// <param name="y">Y position of the MCNK</param>
        /// <param name="z">Z position of the MCNK</param>
        /// <param name="holes">Holes in this MCNK</param>
        public MCNK(float x, float y, float z, UInt16 holes)
        {
            X = x;
            Y = y;
            Z = z;
            Holes = holes;
        }
        
    }

    public class MCVT
    {
        public float[] Heights = new float[145];
    }

    public class MH2O
    {
        public bool Used;
        public MH2OFlags Flags;
        public FluidType Type;
        public float HeightLevel1;
        public float HeightLevel2;

        public byte XOffset;
        public byte YOffset;
        public byte Width;
        public byte Height;

        public float[,] Heights;
    }

    public class MH2OHeader
    {
        public uint ofsData1;
        public uint LayerCount;
        public uint ofsData2;
    }
}