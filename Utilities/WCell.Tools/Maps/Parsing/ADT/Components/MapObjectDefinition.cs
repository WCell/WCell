using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Graphics;

namespace WCell.Tools.Maps
{
    public class MapObjectDefinition
    {
        /// <summary>
        /// Filename of the WMO
        /// </summary>
        public string FileName;
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
        public ushort DoodadSet;
        public ushort NameSet;
    }
}
