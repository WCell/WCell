using System.IO;
using WCell.Util.Graphics;

namespace WCell.Tools.Maps.Parsing.ADT.Components
{
    /// <summary>
    /// MDDF Chunk Class - Placement information for M2 Models
    /// </summary>
    public class MapDoodadDefinition
    {
        /// <summary>
        /// Filename of the M2
        /// </summary>
        private string _filePath;

        /// <summary>
        /// Unique ID of the M2 in this ADT
        /// </summary>
        public uint UniqueId;

        /// <summary>
        /// Position of the M2
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Rotation around the Z axis
        /// </summary>
        public float OrientationA;

        /// <summary>
        /// Rotation around the Y axis
        /// </summary>
        public float OrientationB;

        /// <summary>
        /// Rotation around the X axis
        /// </summary>
        public float OrientationC;

        /// <summary>
        ///  Scale factor of the M2
        /// </summary>
        public float Scale;

        /// <summary>
        /// Flags for the M2
        /// </summary>
        public ushort Flags;

        public BoundingBox Extents;
        public Matrix WorldToModel;
        public Matrix ModelToWorld;

        /// <summary>
        /// Filename of the M2
        /// </summary>
        public string FilePath
        {
            get
            {
                if (Path.GetExtension(_filePath).ToLower().Equals(".mdx") ||
                    Path.GetExtension(_filePath).ToLower().Equals(".mdl"))
                {
                    return Path.ChangeExtension(_filePath, ".m2");
                }
                return _filePath;
            }
            set { _filePath = value; }
        }
    }
}