using System;

namespace MPQNav.MPQ.WMO.Components
{
    // Not sure what this is for yet
    enum MaterialEnum1
    {
        MAPID_DIFFUSE = 0x0,
        MAPID_ENV = 0x1,
        MAPID_COUNT = 0x2,
    }
    [Flags]
    public enum MaterialFlags
    {
        F_UNLIT = 0x1,
        F_UNFOGGED = 0x2,
        F_UNCULLED = 0x4,
        F_EXTLIGHT = 0x8,
        /// <summary>
        /// SidnColor will be nonzero
        /// </summary>
        F_SIDN = 0x10,
        F_WINDOW = 0x20,
        /// <summary>
        /// Probably does something similar to
        /// glTexParameteri(image->target, GL_TEXTURE_WRAP_S, (flags & F_CLAMP_S) ? GL_CLAMP_TO_EDGE : GL_REPEAT);
        /// </summary>
        F_CLAMP_S = 0x40,
        /// <summary>
        /// Probably does something similar to
        /// glTexParameteri(image->target, GL_TEXTURE_WRAP_T, (flags & F_CLAMP_T) ? GL_CLAMP_TO_EDGE : GL_REPEAT);
        /// </summary>
        F_CLAMP_T = 0x80,
    }
    /// <summary>
    /// MOMT
    /// </summary>
    public class Material
    {
        /// <summary>
        /// 0x0
        /// </summary>
        public MaterialFlags Flags;
        /// <summary>
        /// 0x4
        /// Flags2? havent ever seen it nonzero
        /// </summary>
        public uint Int_1;
        /// <summary>
        /// 0x8
        /// Blending: 0 for opaque, 1 for transparent
        /// </summary>
        public int BlendMode;
        /// <summary>
        /// 0xC
        /// old: diffuseNameIndex
        /// </summary>
        public int TextureNameStart;
        /// <summary>
        /// 0x10
        /// </summary>
        public Color4 SidnColor;
        /// <summary>
        /// 0x14
        /// </summary>
        public Color4 FrameSidnColor;
        /// <summary>
        /// 0x18
        /// old: envNameIndex
        /// </summary>
        public int TextureNameEnd;
        /// <summary>
        /// 0x1C
        /// </summary>
        public Color4 DiffColor;
        /// <summary>
        /// 0x20
        /// TerrainType.dbc
        /// NOTE: check this!
        /// </summary>
        public int GroundType;
        /// <summary>
        /// 0x24
        /// </summary>
        public float Float_1;
        /// <summary>
        /// 0x28
        /// </summary>
        public float Float_2;
        /// <summary>
        /// 0x2C
        /// </summary>
        public int Int_2;
        /// <summary>
        /// 0x30
        /// </summary>
        public int Int_3;
        /// <summary>
        /// 0x34
        /// </summary>
        public int Int_4;
    }
}
