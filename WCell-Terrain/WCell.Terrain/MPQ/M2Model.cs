using WCell.Terrain.MPQ.M2s;
using WCell.Util.Graphics;

namespace WCell.Terrain.MPQ
{
    public class M2Model
    {
        public ModelHeader Header;

        public string Name;

        /// <summary>
        /// A list of timestamps that act as 
        /// upper limits for global sequence ranges.
        /// </summary>
        public uint[] GlobalSequenceTimestamps;

        /// <summary>
        /// Models, too, use a Z-up coordinate systems, 
        /// so in order to convert to Y-up, the X, Y, Z 
        /// values become (X, -Z, Y). 
        /// </summary>
        public ModelVertices[] Vertices;


        public Vector3[] BoundingVertices;
        public Index3[] BoundingTriangles;
        public Vector3[] BoundingNormals;
    }
}
