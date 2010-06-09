using System.IO;
using WCell.Tools.Maps.Structures;
using WCell.Util.Graphics;

namespace WCell.Tools.Maps.Parsing.WMO.Components
{
    public class DoodadDefinition
    {
        public int NameIndex;
        /// <summary>
        /// Position of the model relative to the WMOs center {0, 0, 0}
        /// </summary>
        public Vector3 Position;
        public Quaternion Rotation;
        public float Scale;
        /// <summary>
        /// BGRA
        /// </summary>
        public Color4 Color;

        private string _filePath;

        public BoundingBox Extents;
        public Matrix WMOToModel;
        public Matrix ModelToWMO;

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
