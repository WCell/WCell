using System.Collections.Generic;
using WCell.Tools.Maps.Parsing.ADT.Components;

namespace WCell.Tools.Maps.Parsing.ADT
{
    public class ADTChunk
    {
        public MCNK Header = new MCNK();

        /// <summary>
        /// MCTV Chunk (Height values for the MCNK)
        /// </summary>
        public MapVertices Heights = new MapVertices();
        /// <summary>
        /// MCNR Chunk (Normals for the MCNK)
        /// </summary>
        //public MapNormals Normals = new MapNormals();

        /// <summary>
        /// MCRF - (Header.nDoodadRefs + Header.nMapObjRefs) indices into the MDDF and MODF
        /// chunks that tell which M2's and WMO's (respectively) are drawn over this chunk.
        /// When a player is on a chunk, only those objects referenced are checked for collision.
        /// </summary>
        public List<int> DoodadRefs = new List<int>();
        public List<int> ObjectRefs = new List<int>();

        /// <summary>
        /// MH20 Chunk (Water information for the MCNK)
        /// </summary>
        public MH2O WaterInfo = new MH2O();

        public bool IsFlat;
    }
}