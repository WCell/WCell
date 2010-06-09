namespace WCell.Tools.Maps.Parsing.WMO.Components
{
    /// <summary>
    /// MOBA 24 bytes
    /// </summary>
    public class RenderBatch
    {
        private const int F_RENDERED = 0xF0;
        /// <summary>
        /// 0x0
        /// </summary>
        public short BottomX;
        /// <summary>
        /// 0x2
        /// </summary>
        public short BottomY;
        /// <summary>
        /// 0x4
        /// </summary>
        public short BottomZ;
        /// <summary>
        /// 0x6
        /// </summary>
        public short TopX;
        /// <summary>
        /// 0x8
        /// </summary>
        public short TopY;
        /// <summary>
        /// 0xA
        /// </summary>
        public short TopZ;
        /// <summary>
        /// 0xC
        /// </summary>
        public int StartIndex;
        /// <summary>
        /// 0x10
        /// </summary>
        public ushort IndexCount;
        /// <summary>
        /// 0x12
        /// </summary>
        public ushort VertexStart;
        /// <summary>
        /// 0x14
        /// </summary>
        public ushort VertexEnd;
        /// <summary>
        /// 0x16
        /// </summary>
        public byte Byte_13;
        /// <summary>
        /// 0x17
        /// </summary>
        public byte TextureIndex;
    }
}
