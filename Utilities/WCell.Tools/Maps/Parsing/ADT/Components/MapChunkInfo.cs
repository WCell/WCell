namespace WCell.Tools.Maps.Parsing.ADT.Components
{
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
}