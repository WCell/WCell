using Microsoft.Xna.Framework;

namespace MPQNav.MPQ.WMO.Components
{
    /// <summary>
    /// WMO Group Header. MOGP chunk
    /// </summary>
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
}
