using System;
using System.Collections.Generic;
using WCell.Terrain.GUI;
using WCell.Terrain.Collision;
using WCell.Util.Graphics;
using OBB = WCell.Util.Graphics.OBB;

namespace WCell.Terrain.MPQ
{
    /// <summary>
    /// Class for the WMO Group File
    /// </summary>
    public class WMO
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
        //public List<VertexPositionNormalColored> Vertices = new List<VertexPositionNormalColored>();
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
            //Vertices.Clear();
            Indices.Clear();
        }
    }
}