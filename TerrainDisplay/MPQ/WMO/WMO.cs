using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MPQNav.Collision._3D;
using TerrainDisplay.Collision._3D;

namespace MPQNav.MPQ.WMO
{
    /// <summary>
    /// Class for the WMO Group File
    /// </summary>
    class WMO
    {
        /// <summary>
        /// AABB For the WMO
        /// </summary>
        public AABB AABB;

        /// <summary>
        /// Total number of groups for the WMO
        /// </summary>
        public int TotalGroups = 1;

        /// <summary>
        /// Name of the WMO Group file
        /// </summary>
        public string FileName;

        /// <summary>
        /// List containg all the WMO Sub-Chunks for this WMO Group File
        /// </summary>
        private readonly List<WMOGroup> _wmoGroups = new List<WMOGroup>();

        /// <summary>
        /// The Orientated Bounding Box for this WMO
        /// </summary>
        public OBB OrientatedBoundingBox;

        public int GroupCount
        {
            get { return _wmoGroups.Count; }
        }

        #region Rendering Variables
        /// <summary>
        /// List of vertices used for rendering this WMO in World Space
        /// </summary>
        public List<VertexPositionNormalColored> Vertices = new List<VertexPositionNormalColored>();
        /// <summary>
        /// List of indicies used for rendering this WMO in World Space
        /// </summary>
        public List<int> Indices = new List<int>();
        /// <summary>
        /// List of Triangles that are described by the indices/vertices
        /// </summary>
        public List<Triangle> Triangles = new List<Triangle>();
        #endregion

        public WMO()
        {

        }

        public WMO(String name)
        {
            FileName = name;
        }

        public void CreateAABB(Vector3 min, Vector3 max)
        {
            AABB = new AABB(min, max);
        }

        public void AddWMOGroup(WMOGroup wmoGroup)
        {
            _wmoGroups.Add(wmoGroup);
        }

        public WMOGroup GetWMOGroup(int index)
        {
            return _wmoGroups[index];                       
        }

        public void ClearCollisionData()
        {
            OrientatedBoundingBox = new OBB();
            Vertices.Clear();
            Indices.Clear();
        }

        public void AddVertex(Vector3 vec)
        {
            Vertices.Add(new VertexPositionNormalColored(vec, Color.Yellow, Vector3.Up));
        }

        public void AddIndex(int index)
        {
            Indices.Add(index);
        }

        public void AddIndex(short index)
        {
            Indices.Add(index);
        }
    }
}