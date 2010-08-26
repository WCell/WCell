using System.Collections.Generic;
using TerrainDisplay.Collision._3D;
using WCell.Util.Graphics;

namespace TerrainDisplay.MPQ.M2
{
    public class M2
    {
        /// <summary>
        /// AABB For the M2
        /// </summary>
        public BoundingBox Bounds;
        /// <summary>
        /// The Orientated Bounding Box for this M2
        /// </summary>
        public Collision._3D.OBB OBB;

        #region Rendering Variables
        /// <summary>
        /// List of vertices used for rendering this M2 in World Space
        /// </summary>
        public List<Vector3> Vertices = new List<Vector3>();
        /// <summary>
        /// List of indicies used for rendering this M2 in World Space
        /// </summary>
        public List<int> Indices = new List<int>();
        /// <summary>
        /// The Triangles formed from the vertices and indices.
        /// </summary>
        public List<Triangle> Triangles = new List<Triangle>();
        #endregion
    }
}