using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Tools.Maps
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
