using WCell.Tools.Maps.Structures;
using WCell.Util.Graphics;

namespace WCell.Tools.Maps.Parsing.WMO.Components
{
    public enum LightType
    {
        OMNI_LGT = 0x0,
        SPOT_LGT = 0x1,
        DIRECT_LGT = 0x2,
        AMBIENT_LGT = 0x3,
    }

    public class LightInformation
    {
        /// <summary>
        /// LightType?
        /// </summary>
        public byte Byte_1;
        /// <summary>
        /// type?
        /// </summary>
        public byte Byte_2;
        /// <summary>
        /// useAtten?
        /// </summary>
        public byte Byte_3;
        public byte Byte_4;
        public Color4 Color;
        /// <summary>
        /// Position (X,Z,-Y)
        /// </summary>
        public Vector3 Position;
        public float Intensity;
        public float AttenStart;
        public float AttenEnd;
        public float Float_4;
        public float Float_5;
        public float Float_6;
        public float Float_7;
    }
}