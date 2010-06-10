using System;

namespace MPQNav.MPQ.WMO.Components
{
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
}
