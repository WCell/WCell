//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Runtime.InteropServices;
//using Microsoft.Xna.Framework;
//using System.IO;

//namespace WCell.Collision
//{
//    #region MODF

//    /// <summary>
//    /// MODF Chunk. Used in ADT and WDT Files
//    /// Placement information for WMO objects
//    /// </summary>
//    [StructLayout(LayoutKind.Sequential, Size = 64)]
//    public struct MapObjectDefinition
//    {
//        /// <summary>
//        /// index in the MWMO list
//        /// </summary>
//        public uint Index;//0x00 
//        /// <summary>
//        /// unique identifier for this instance
//        /// </summary>
//        public uint UniqueId;// 0x04
//        /// <summary>
//        /// Position (X,Y,Z)
//        /// now, if i have wmo which is 20 wide, 20 tall, 20 deep, and placement co-ords are 
//        /// 100,100,100.... my x2,y2,z2 will be 90,100,90 and x3,y3,z3 will be 110,120,110....
//        /// </summary>
//        public Vector3 Position;// 0x08
//        /// <summary>
//        /// Orientation (A,B,C)
//        /// </summary>
//        public Vector3 Rotation;// 0x14
//        /// <summary>
//        /// Upper Extents
//        /// </summary>
//        public Vector3 UpperExtents;// 0x20
//        /// <summary>
//        /// Lower Extents
//        /// </summary>
//        public Vector3 LowerExtents;// 0x2C
//        public MapObjectDefinitionFlags Flags;// 0x38
//        public short DoodadSetIndex;// 0x3C
//        public short NameSet; // 0x3E

//        public void DumpInfo(TextWriter writer, string indent)
//        {
//            ReflectionUtils.EnumFields(this, writer, indent);
//        }
//    }

//    [Flags]
//    public enum MapObjectDefinitionFlags : int
//    {
//        Flag_0x1 = 0x1,
//        Flag_0x2 = 0x2,
//        Flag_0x4 = 0x4,
//        Flag_0x8 = 0x8,
//        Flag_0x10 = 0x10,
//    }

//    #endregion

//    #region MDDF

//    /// <summary>
//    /// MDDF Chunk. Used in ADT files
//    /// Used to specify the M2 files in the tile
//    /// </summary>
//    [StructLayout(LayoutKind.Sequential, Size = 36)]
//    public struct DoodadPlacement
//    {
//        /*
//         * The instance information specifies the actual M2 model to use, its absolute position and orientation within the world. The orientation is defined by rotations (in degrees) about the 3 axes as such (this order of operations is for OpenGL, so the transformations "actually" happen in reverse):

//   1. Rotate around the Y axis by B-90
//   2. Rotate around the Z axis by -A
//   3. Rotate around the X axis by C 

//Finally the scale factor is given by an integer, multiplied by 1024. So the model needs to be scaled by scale/1024. I wonder why they didn't just use a float. 
//         * 
//         * */
//        // M2 Files
//        /// <summary>
//        /// ID (index in the MMDX list)
//        /// </summary>
//        public uint NameId;
//        /// <summary>
//        /// unique identifier for this instance
//        /// </summary>
//        public uint UniqueId;
//        /// <summary>
//        /// Position (X, Y, Z)
//        /// </summary>
//        public Vector3 Position;// X, Y, Z
//        /// <summary>
//        /// Orientation (A, B, C)
//        /// </summary>
//        public Vector3 Rotation;// A, B, C
//        public DoodadPlacementFlags Flags;
//        /// <summary>
//        /// scale factor * 1024
//        /// </summary>
//        public ushort Scale;

//        public void DumpInfo(TextWriter writer, string indent)
//        {
//            ReflectionUtils.EnumFields(this, writer, indent);
//        }
//    }

//    [Flags]
//    public enum DoodadPlacementFlags : ushort
//    {
//        Flag_0x1 = 0x1,
//    }

//    #endregion

//    #region MODS

//    public struct DoodadSet
//    {
//        /// <summary>
//        /// Set name
//        /// <remarks>char[20]</remarks>
//        /// </summary>
//        public string Name;

//        /*014h*/
//        /// <summary>
//        /// index of first doodad instance in this set
//        /// </summary>
//        public uint FirstDoodadInstanceIndex;
//        /*018h*/
//        /// <summary>
//        /// number of doodad instances in this set
//        /// </summary>
//        public uint DoodadCount;
        
//        /// <summary>
//        /// Unused? always 0
//        /// </summary>
//        public uint nulls;
//    }

//    #endregion

//    #region MODD

//    public struct DoodadInstancePlacement
//    {

//    }

//    #endregion

//    #region MOGI

//    /// <summary>
//    /// MOGI Chunk. Used in WMO Root files
//    /// </summary>
//    public struct GroupInformation
//    {
//        public GroupFlags Flags;
//        public Vector3 BoundingBox1;
//        public Vector3 BoundingBox2;
//        // -1 for no name?
//        public int NameIndex;

//        public void DumpInfo(TextWriter writer, string indent)
//        {
//            ReflectionUtils.EnumFields(this, writer, indent);
//        }
//    }

//    #endregion

//    #region MOPI

//    /// <summary>
//    /// MOPI Chunk. Used in WMO Root files
//    /// </summary>
//    public struct PortalInformation
//    {
//        /// <summary>
//        /// Base vertex index?
//        /// </summary>
//        public ushort StartVertex;
//        /// <summary>
//        /// Number of vertices (?), always 4 (?)
//        /// </summary>
//        public ushort VertexCount;
//        /// <summary>
//        /// a normal vector maybe? haven't checked.
//        /// </summary>
//        public Plane Plane;

//        public void DumpInfo(TextWriter writer, string indent)
//        {
//            ReflectionUtils.EnumFields(this, writer, indent);
//        }
//    }

//    #endregion

//    #region MOPR

//    /// <summary>
//    /// MOPR Chunk. Used in WMO root files
//    /// </summary>
//    [StructLayout(LayoutKind.Sequential)]
//    public struct PortalRelation
//    {
//        /// <summary>
//        /// Portal index
//        /// </summary>
//        public ushort PortalIndex;
//        /// <summary>
//        /// WMO Group Index
//        /// </summary>
//        public ushort GroupIndex;
//        /// <summary>
//        /// 1 or -1
//        /// </summary>
//        public short Side;
//        /// <summary>
//        /// Unknown, always 0?
//        /// </summary>
//        public ushort Filler;

//        public void DumpInfo(TextWriter writer, string indent)
//        {
//            ReflectionUtils.EnumFields(this, writer, indent);
//        }
//    }

//    #endregion

//    #region MOHD

//    /// <summary>
//    /// MOHD Chunk in WMO root
//    /// </summary>
//    [StructLayout(LayoutKind.Sequential)]
//    public struct RootHeader
//    {
//        /// <summary>
//        /// number of textures (BLP Files)
//        /// </summary>
//        public int TextureCount;
//        /// <summary>
//        /// number of WMO groups
//        /// </summary>
//        public int GroupCount;
//        /// <summary>
//        /// number of portals
//        /// </summary>
//        public int PortalCount;
//        /// <summary>
//        /// number of lights
//        /// </summary>
//        public int LightCount;
//        /// <summary>
//        /// number of M2 models imported
//        /// </summary>
//        public int ModelCount;
//        /// <summary>
//        /// number of doodads (M2 instances)
//        /// </summary>
//        public int DoodadCount;
//        /// <summary>
//        /// number of doodad sets
//        /// </summary>
//        public int SetCount;
//        public byte colR;
//        public byte colG;
//        public byte colB;
//        public byte colX;
//        /// <summary>
//        /// wmo id
//        /// </summary>
//        public int Id;
//        public Vector3 BoundingBoxCorner1;
//        public Vector3 BoundingBoxCorner2;

//        public void DumpInfo(TextWriter writer, string indent)
//        {
//            ReflectionUtils.EnumFields(this, writer, indent);
//        }
//    }

//    #endregion

//    #region MOGP

//    /// <summary>
//    /// Header used in WMO group files
//    /// </summary>
//    [StructLayout(LayoutKind.Sequential, Size = 68)]
//    public struct GroupHeader
//    {
//        /// <summary>
//        /// String offset into MOGN
//        /// </summary>
//        public int GroupNameOffset;// 0
//        /// <summary>
//        /// String offset into MOGN
//        /// </summary>
//        public int DescriptiveGroupNameOffset;// 4

//        public GroupFlags Flags;// 8

//        public Vector3 BoundingBox1;// C
//        public Vector3 BoundingBox2;// 18
//        /// <summary>
//        /// Index into the MOPR chunk
//        /// </summary>
//        public ushort PortalRelationChunkOffset;// 24
//        /// <summary>
//        /// Number of items used from the MOPR chunk
//        /// </summary>
//        public ushort PortalRelationItemCount;// 26

//        public ushort ItemBatchesA;// 28
//        public ushort ItemBatchesB;// 2A
//        public int ItemBatchesC;// 2C
//        /// <summary>
//        /// Up to four indices into the WMO fog list
//        /// </summary>
//        public byte[] FogIndices;// 30

//        public int Field_34;// 34

//        /// <summary>
//        /// WMO group ID (column 4 in WMOAreaTable.dbc)
//        /// </summary>
//        public int WMOGroupId;// 38

//        public int Field_3C;// 3C
//        public int Field_40;// 40

//        public void DumpInfo(TextWriter writer, string indent)
//        {
//            ReflectionUtils.EnumFields(this, writer, indent);
//        }
//    }

//    [Flags]
//    public enum GroupFlags
//    {
//        Flag_0x1 = 0x1,
//        Flag_0x2 = 0x2,
//        HasVertexColors = 0x4,
//        Outdoor = 0x8,
//        Flag_0x10 = 0x10,
//        Flag_0x20 = 0x20,
//        Flag_0x40 = 0x40,
//        Flag_0x80 = 0x80,
//        Flag_0x100 = 0x100,
//        HasLights = 0x200,
//        Flag_0x400 = 0x400,
//        HasDoodads = 0x800,
//        HasWater = 0x1000,
//        Indoor = 0x2000,
//        Flag_0x4000 = 0x4000,
//        Flag_0x8000 = 0x8000,
//        Flag_0x10000 = 0x10000,
//        Flag_0x20000 = 0x20000,
//        ShowSkybox = 0x40000,
//        Flag_0x80000 = 0x80000,
//    }

//    #endregion

//    #region MOPY

//    [StructLayout(LayoutKind.Sequential, Size = 2)]
//    public struct MapObjectPoly
//    {
//        public MapObjectPolyFlags Flags;
//        /// <summary>
//        /// Material ID specifies an index into the material table in the root WMO file's MOMT chunk
//        /// </summary>
//        public byte MaterialId;

//        public void DumpInfo(TextWriter writer, string indent)
//        {
//            ReflectionUtils.EnumFields(this, writer, indent);
//        }
//    }

//    [Flags]
//    public enum MapObjectPolyFlags : byte
//    {
//        Flag_0x1 = 0x1,
//        Flag_0x2 = 0x2,
//        Flag_0x4 = 0x4,
//        Flag_0x8 = 0x8,
//        Flag_0x10 = 0x10,
//        Flag_0x20 = 0x20,
//        Flag_0x40 = 0x40,
//        Flag_0x80 = 0x80,
//    }

//    #endregion

//public enum Worlds
//{
//    Azeroth = 0,
//    Kalimdor = 1,
//    test = 13,
//    ScottTest = 25,
//    Test = 29,
//    PVPZone01 = 30,
//    Shadowfang = 33,
//    StormwindJail = 34,
//    StormwindPrison = 35,
//    DeadminesInstance = 36,
//    PVPZone02 = 37,
//    Collin = 42,
//    WailingCaverns = 43,
//    Monastery = 44,
//    RazorfenKraulInstance = 47,
//    Blackfathom = 48,
//    Uldaman = 70,
//    GnomeragonInstance = 90,
//    SunkenTemple = 109,
//    RazorfenDowns = 129,
//    EmeraldDream = 169,
//    MonasteryInstances = 189,
//    TanarisInstance = 209,
//    BlackRockSpire = 229,
//    BlackrockDepths = 230,
//    OnyxiaLairInstance = 249,
//    CavernsOfTime = 269,
//    SchoolofNecromancy = 289,
//    Zulgurub = 309,
//    Stratholme = 329,
//    Mauradon = 349,
//    DeeprunTram = 369,
//    OrgrimmarInstance = 389,
//    MoltenCore = 409,
//    DireMaul = 429,
//    AlliancePVPBarracks = 449,
//    HordePVPBarracks = 450,
//    development = 451,
//    BlackwingLair = 469,
//    PVPZone03 = 489,
//    AhnQiraj = 509,
//    PVPZone04 = 529,
//    Expansion01 = 530,
//    AhnQirajTemple = 531,
//    Karazahn = 532,
//    StratholmeRaid = 533,
//    HyjalPast = 534,
//    HellfireMilitary = 540,
//    HellfireDemon = 542,
//    HellfireRampart = 543,
//    HellfireRaid = 544,
//    CoilfangPumping = 545,
//    CoilfangMarsh = 546,
//    CoilfangDraenei = 547,
//    CoilfangRaid = 548,
//    TempestKeepRaid = 550,
//    TempestKeepArcane = 552,
//    TempestKeepAtrium = 553,
//    TempestKeepFactory = 554,
//    AuchindounShadow = 555,
//    AuchindounDemon = 556,
//    AuchindounEthereal = 557,
//    AuchindounDraenei = 558,
//    PVPZone05 = 559,
//    HillsbradPast = 560,
//    bladesedgearena = 562,
//    BlackTemple = 564,
//}
//}
