using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MPQNav.MPQ.WMO.Components;

namespace TerrainDisplay.Extracted.WMO
{
    public class ExtractedWMOGroup
    {
        public WMOGroupFlags Flags;
        public BoundingBox Bounds;
        public uint GroupId;
        public List<int> ModelRefs;
        public List<Vector3> Vertices;
        public BSPTree Tree;
    }
}
