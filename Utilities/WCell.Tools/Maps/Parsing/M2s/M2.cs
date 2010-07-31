using System.Collections.Generic;
using WCell.Tools.Maps.Structures;
using WCell.Util.Graphics;

namespace WCell.Tools.Maps.Parsing.M2s
{
    public class M2
    {
        /// <summary>
        /// Model-space bounding box for the M2
        /// </summary>
        public BoundingBox ModelBounds;
        
        /// <summary>
        /// List of vertices used for rendering this M2 in World Space
        /// </summary>
        public List<Vector3> Vertices = new List<Vector3>();
        
        /// <summary>
        /// The Triangles formed from the vertices and indices.
        /// </summary>
        public List<Index3> Triangles = new List<Index3>();
        
    }
}