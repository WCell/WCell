using System;
using Microsoft.Xna.Framework;

namespace TerrainDisplay.MPQ.WMO.Components
{
    [Flags]
    public enum WMORootHeaderFlags
    {
        Flag_0x1 = 0x1,
        /// <summary>
        /// If this is set, it adds the AmbientColor RGB components to MOCV1
        /// </summary>
        Flag_0x2 = 0x2,
        /// <summary>
        /// LiquidType related
        /// If this is set, it will take the variable in MOGP's var_0x34. If not, it checks var_0x34 for 15 (green lava). 
        /// If that is set, it will take 0, else it will take var_0x34 + 1. 
        /// </summary>
        Flag_0x4 = 0x4,

        /// <summary>
        /// Changes the vertex colors if this flag is set and 0x4 is set in the group
        /// </summary>
        Flag_0x8 = 0x8,
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
        public Color4 AmbientColor;
        /// <summary>
        /// MOHD +0x20
        /// </summary>
        public uint WMOId;
        /// <summary>
        /// MOHD +0x24
        /// </summary>
        public BoundingBox BoundingBox;
        /// <summary>
        /// MOHD +0x3C
        /// </summary>
        public WMORootHeaderFlags Flags;
    }
}
