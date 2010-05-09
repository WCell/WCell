using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WCell.Util.Graphics;

namespace WCell.Tools.Maps
{
    public class WMOGroup
    {
        public uint Version;
        public readonly GroupHeader Header = new GroupHeader();

        /// <summary>
        /// MOVT - Verticies
        /// </summary>
        public readonly List<Vector3> Verticies = new List<Vector3>();
        /// <summary>
        /// Normals
        /// </summary>
        public readonly List<Vector3> Normals = new List<Vector3>();

        /// <summary>
        /// MOVI - Triangle Indices.
        /// Rendering Indices information for the Group.
        /// </summary>
        public readonly List<Index3> Indicies = new List<Index3>();

        /// <summary>
        /// Flags that affect the Triangles in this Group.
        /// </summary>
        public MOPY[] TriangleMaterials;

        /// <summary>
        /// The index into the WMOs GroupList that refers to this WMOGroup.
        /// </summary>
        public int Index;

        public BSPNode[] BSPNodes;
        public ushort[] MOBR;

        public uint TriangleCount;
        public uint VertexCount;

        public readonly WMORoot Root;
        public string Name;
        public string DescriptiveName;

        public LiquidInfo LiquidInfo;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="root"></param>
        /// <param name="index">The index into the WMOs GroupList that refers to this WMOGroup.</param>
        public WMOGroup(WMORoot root, int index)
        {
            Root = root;
            Index = index;
        }

        //internal void DumpBSPNodes()
        //{
        //    using (var f = new StreamWriter(Name + ".bspnodes.txt"))
        //    {
        //        for (var i = 0; i < BSPNodes.Length; i++)
        //        {
        //            var node = BSPNodes[i];
        //            f.WriteLine("Node: " + i);
        //            node.Dump(f);
        //        }
        //    }
        //}
    }

    [Flags]
    public enum WMOGroupFlags
    {
        HasBSPNodes = 0x1,
        Flag_0x2 = 0x2,
        /// <summary>
        /// An MOCV chunk is in this group
        /// </summary>
        HasVertexColors = 0x4,
        Outdoor = 0x8,
        Flag_0x10 = 0x10,
        Flag_0x20 = 0x20,
        Flag_0x40 = 0x40,
        Flag_0x80 = 0x80,
        Flag_0x100 = 0x100,
        /// <summary>
        /// A MOLR chunk is in this group
        /// </summary>
        HasLights = 0x200,
        /// <summary>
        /// Has MPBV, MPBP, MPBI, MPBG
        /// </summary>
        HasMPChunks = 0x400,
        /// <summary>
        /// A MODR chunk is in this group
        /// </summary>
        HasDoodads = 0x800,
        /// <summary>
        /// A MLIQ chunk is in this group
        /// real name: SMOGroup::LIQUIDSURFACE
        /// </summary>
        HasWater = 0x1000,
        /// <summary>
        /// Indoor lighting. The client uses local lights from this wmo instead of the global lights
        /// Client uses the MOCV for this group
        /// </summary>
        Indoor = 0x2000,

        Flag_0x4000 = 0x4000,
        Flag_0x8000 = 0x8000,
        Flag_0x10000 = 0x10000,
        /// <summary>
        /// Has MORI/MORB
        /// MORI - 2 byte indices
        /// </summary>
        HasMORChunks = 0x20000,
        /// <summary>
        /// Has MOSB chunk
        /// </summary>
        HasSkybox = 0x40000,
        /// <summary>
        /// Water is Ocean. The liquidtype for this object will resolve to "WMO Ocean"
        /// </summary>
        Flag_0x80000 = 0x80000,

        /// <summary>
        /// Has a second MOCV chunk
        /// </summary>
        HasMOCV2 = 0x1000000,

        /// <summary>
        /// Has a second MOTV chunk
        /// </summary>
        HasMOTV2 = 0x2000000,
    }

    public class GroupHeader
    {
        /// <summary>
        /// MOGP +0x0
        /// </summary>
        public int NameStart;
        /// <summary>
        /// MOGP +0x4
        /// </summary>
        public int DescriptiveNameStart;
        /// <summary>
        /// MOGP +0x8
        /// </summary>
        public WMOGroupFlags Flags;
        /// <summary>
        /// MOGP +0xC, 24 bytes (2 * Vector3)
        /// </summary>
        public BoundingBox BoundingBox;
        /// <summary>
        /// MOGP +0x24
        /// </summary>
        public ushort PortalStart;
        /// <summary>
        /// MOGP +0x26
        /// </summary>
        public ushort PortalCount;
        /// <summary>
        /// MOGP +0x28
        /// </summary>
        public ushort BatchesA;
        /// <summary>
        /// MOGP +0x2A
        /// </summary>
        public ushort BatchesB;
        /// <summary>
        /// MOGP +0x2C
        /// </summary>
        public ushort BatchesC;
        /// <summary>
        /// MOGP +0x2E
        /// </summary>
        public ushort BatchesD;
        /// <summary>
        /// MOGP +0x30
        /// Fog stuff?
        /// </summary>
        public byte[] Fogs;
        /// <summary>
        /// MOGP +0x34
        /// </summary>
        public uint LiquidType;
        /// <summary>
        /// MOGP +0x38
        /// </summary>
        public uint WMOGroupId;
        /// <summary>
        /// MOGP +0x3C
        /// </summary>
        public uint MOGP_0x3C;
        /// <summary>
        /// MOGP +0x40
        /// </summary>
        public uint MOGP_0x40;


        public bool HasBSPInfo
        {
            get { return (Flags & WMOGroupFlags.HasBSPNodes) != 0; }
        }

        public bool HasMOCV1
        {
            get { return (Flags & WMOGroupFlags.HasVertexColors) != 0; }
        }

        public bool HasMOLR
        {
            get { return (Flags & WMOGroupFlags.HasLights) != 0; }
        }

        public bool HasMPChunks
        {
            get { return (Flags & WMOGroupFlags.HasMPChunks) != 0; }
        }

        public bool HasMODR
        {
            get { return (Flags & WMOGroupFlags.HasDoodads) != 0; }
        }

        public bool HasMLIQ
        {
            get { return (Flags & WMOGroupFlags.HasWater) != 0; }
        }

        public bool HasMORChunks
        {
            get { return (Flags & WMOGroupFlags.HasMORChunks) != 0; }
        }

        public bool HasMOTV2
        {
            get { return (Flags & WMOGroupFlags.HasMOTV2) != 0; }
        }

        public bool HasMOCV2
        {
            get { return (Flags & WMOGroupFlags.HasMOCV2) != 0; }
        }
    }

    public class LiquidInfo
    {
        /// <summary>
        /// number of X vertices (xverts)
        /// </summary>
        public int XVertexCount;
        /// <summary>
        /// number of Y vertices (yverts)
        /// </summary>
        public int YVertexCount;
        /// <summary>
        /// number of X tiles (xtiles = xverts-1)
        /// </summary>
        public int XTileCount;
        /// <summary>
        /// number of Y tiles (ytiles = yverts-1)
        /// </summary>
        public int YTileCount;

        public Vector3 BaseCoordinates;
        public ushort MaterialId;

        public int VertexCount
        {
            get { return XVertexCount * YVertexCount; }
        }
        public int TileCount
        {
            get { return XTileCount * YTileCount; }
        }

        /// <summary>
        /// VertexCount group of float[2]
        /// </summary>
        public Vector2[] UnkFloatPairs;
        /// <summary>
        /// TileCount group of bytes
        /// They are often masked with 0xF
        /// They seem to determine the liquid type?
        /// </summary>
        public byte[] LiquidTileFlags;
    }

    public struct Index3
    {
        public short Index0;
        public short Index1;
        public short Index2;
    }
}
