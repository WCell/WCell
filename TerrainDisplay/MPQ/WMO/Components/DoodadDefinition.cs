using Microsoft.Xna.Framework;

namespace MPQNav.MPQ.WMO.Components
{
    /// <summary>
    /// Represents the MODD chunk
    /// </summary>
    public class DoodadDefinition
    {
        public int NameIndex;
        public Vector3 Position;
        public Quaternion Rotation;
        public float Scale;
        /// <summary>
        /// BGRA
        /// </summary>
        public Color4 Color;

        public string FilePath;
    }
}
