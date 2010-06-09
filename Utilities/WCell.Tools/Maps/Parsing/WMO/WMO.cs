using System;
using System.Collections.Generic;
using WCell.Tools.Maps.Structures;
using WCell.Util.Graphics;

namespace WCell.Tools.Maps.Parsing.WMO
{
    /// <summary>
    /// Class for the WMO Group File
    /// </summary>
    class WMO
    {
        /// <summary>
        /// ModelSpace Bounds For the WMO
        /// </summary>
        public BoundingBox ModelBounds;

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

        public int GroupCount
        {
            get { return _wmoGroups.Count; }
        }

        #region Rendering Variables
        /// <summary>
        /// List of vertices used for rendering this WMO in World Space
        /// </summary>
        public List<Vector3> Vertices = new List<Vector3>();
        /// <summary>
        /// List of Triangles that are described by the indices/vertices
        /// </summary>
        public List<Index3> Triangles = new List<Index3>();
        #endregion

        public WMO()
        {

        }

        public WMO(String name)
        {
            FileName = name;
        }

        public void AddWMOGroup(WMOGroup wmoGroup)
        {
            _wmoGroups.Add(wmoGroup);
        }

        public WMOGroup GetWMOGroup(int index)
        {
            return _wmoGroups[index];                       
        }
    }
}