using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TerrainDisplay.Extracted.WMO
{
    public class ExtractedWMO
    {
        public BoundingBox Extents;
        public uint WMOId;
        public List<Dictionary<int, ExtractedWMOM2Definition>> WMOM2Defs;
        public List<ExtractedWMOGroup> Groups;
    }
}
