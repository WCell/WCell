using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Graphics;

namespace WCell.Tools.Maps
{
    /// <summary>
    /// MOPI Chunk. Used in WMO Root files
    /// </summary>
    public struct PortalInformation
    {
        /// <summary>
        /// Vertex Span that says what vertices from MOPV are used
        /// </summary>
        public VertexSpan Vertices;
        /// <summary>
        /// a normal vector maybe? haven't checked.
        /// </summary>
        public Plane Plane;
    }

    /// <summary>
    /// MOPR Chunk. Used in WMO root files
    /// </summary>
    public struct PortalRelation
    {
        /// <summary>
        /// Portal index
        /// </summary>
        public ushort PortalIndex;
        /// <summary>
        /// WMO Group Index
        /// </summary>
        public ushort GroupIndex;
        /// <summary>
        /// 1 or -1
        /// </summary>
        public short Side;
    }

    public struct VertexSpan
    {
        /// <summary>
        /// The starting vertex in the respective vertex list
        /// </summary>
        public short StartVertex;
        /// <summary>
        /// How many vertices make up this span
        /// </summary>
        public short VertexCount;
    }
}
