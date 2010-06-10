using Microsoft.Xna.Framework;

namespace MPQNav.MPQ.ADT.Components
{
    /// <summary>
    /// MODF Class - WMO Placement Information
    /// </summary>
    public class MapObjectDefinition
    {
        /// <summary>
        /// Filename of the WMO
        /// </summary>
        public string FilePath;
        /// <summary>
        /// Unique ID of the WMO in this ADT
        /// </summary>
        public uint UniqueId;
        /// <summary>
        /// Position of the WMO
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Rotation of the Z axis
        /// </summary>
        public float OrientationA;
        /// <summary>
        /// Rotation of the Y axis
        /// </summary>
        public float OrientationB;
        /// <summary>
        ///  Rotation of the X axis
        /// </summary>
        public float OrientationC;

        public BoundingBox Extents;
        public ushort Flags;
        public ushort DoodadSetId;
        public ushort NameSet;
    }
}