using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Graphics;

namespace WCell.Tools.Maps
{
    public class WMORoot
    {
        public readonly string FilePath;

        public WMOGroup[] Groups;

        /// <summary>
        /// MVER
        /// </summary>
        public int Version;

        /// <summary>
        /// MOHD
        /// </summary>
        public readonly RootHeader Header = new RootHeader();

        /// <summary>
        /// MOGI
        /// </summary>
        public GroupInformation[] GroupInformation;
        /// <summary>
        /// MOPT chunks
        /// </summary>
        public PortalInformation[] PortalInformation;
        /// <summary>
        /// MOPR chunks
        /// </summary>
        public PortalRelation[] PortalRelations;
        /// <summary>
        /// MOPV
        /// </summary>
        public Vector3[] PortalVertices;
        /// <summary>
        /// MODN
        /// </summary>
        public Dictionary<int, string> DoodadFiles;
        /// <summary>
        /// MODS
        /// </summary>
        public DoodadSet[] DoodadSets;
        /// <summary>
        /// MODD
        /// </summary>
        public DoodadDefinition[] DoodadDefinitions;
        /// <summary>
        /// MOTX
        /// </summary>
        public Dictionary<int, string> Textures;
        /// <summary>
        /// MOGN
        /// </summary>
        public Dictionary<int, string> GroupNames;
        /// <summary>
        /// MCVP
        /// </summary>
        public Plane[] ConvexVolumePlanes;
        /// <summary>
        /// MOVV
        /// </summary>
        public Vector3[] VisibleVertices;
        /// <summary>
        /// MOVB
        /// </summary>
        public VertexSpan[] VisibleBlocks;

        /// <summary>
        /// The Orientated Bounding Box for this WMO
        /// </summary>
        //public OBB OrientatedBoundingBox;

        public BoundingBox AABB;

        
        public WMORoot(string filePath)
        {
            FilePath = filePath;
        }

        public void ClearCollisionData()
        {
            //OrientatedBoundingBox = new OBB();
        }

        public void CreateAABB()
        {
            AABB = new BoundingBox(Header.BoundingBox.Min, Header.BoundingBox.Max);
        }
    }

    public class GroupInformation
    {
        /// <summary>
        /// Seems to be a limited version of the full group flags
        /// </summary>
        public WMOGroupFlags Flags;
        public BoundingBox BoundingBox;
        // -1 for no name?
        public int NameIndex;
    }

    public class RootHeader
    {
        /// <summary>
        /// MOHD +0x0
        /// </summary>
        public uint TextureCount;
        /// <summary>
        /// MOHD +0x4
        /// </summary>
        public uint GroupCount;
        /// <summary>
        /// MOHD +0x8
        /// </summary>
        public uint PortalCount;
        /// <summary>
        /// MOHD +0xC
        /// </summary>
        public uint LightCount;
        /// <summary>
        /// MOHD +0x10
        /// number of M2 models imported
        /// </summary>
        public uint ModelCount;
        /// <summary>
        /// MOHD +0x14
        /// </summary>
        public uint DoodadCount;
        /// <summary>
        /// MOHD +0x18
        /// </summary>
        public uint DoodadSetCount;
        /// <summary>
        /// MOHD +0x1C
        /// </summary>
        public uint AmbientColor;
        /// <summary>
        /// MOHD +0x20
        /// </summary>
        public uint WMOId;
        /// <summary>
        /// MOHD +0x24
        /// </summary>
        public BoundingBox BoundingBox;

        public WMORootHeaderFlags Flags;
    }

    [Flags]
    public enum WMORootHeaderFlags
    {
        Flag_0x1,
        /// <summary>
        /// If this is set, it adds the AmbientColor RGB components to MOCV1
        /// </summary>
        Flag_0x2,
        /// <summary>
        /// LiquidType related
        /// If this is set, it will take the variable in MOGP's var_0x34. If not, it checks var_0x34 for 15 (green lava). 
        /// If that is set, it will take 0, else it will take var_0x34 + 1. 
        /// </summary>
        Flag_0x4,
    }
}
