using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Graphics;

namespace WCell.Terrain.Simple.WMO
{
    public class SimpleWMO
    {
        public BoundingBox Extents;
        public uint WMOId;
        public List<Dictionary<int, SimpleWMOM2Definition>> WMOM2Defs;
        public List<SimpleWMOGroup> Groups;
    }
}
