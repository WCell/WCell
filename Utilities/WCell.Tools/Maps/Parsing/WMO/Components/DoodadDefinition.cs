using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Graphics;

namespace WCell.Tools.Maps
{
    public class DoodadDefinition
    {
        public int NameIndex;
        public Vector3 Position;
        public Quaternion Rotation;
        public float Scale;
        /// <summary>
        /// BGRA
        /// </summary>
        public uint Color;

        public string FilePath;
    }

    public struct DoodadSet
    {
        public string SetName;
        public uint FirstInstanceIndex;
        public uint InstanceCount;
    }
}
