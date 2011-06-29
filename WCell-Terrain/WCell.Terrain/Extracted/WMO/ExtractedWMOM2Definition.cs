using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Graphics;
using WCell.Terrain.MPQ.WMO.Components;

namespace TerrainDisplay.Extracted.WMO
{
    public class ExtractedWMOM2Definition : DoodadDefinition
    {
        public BoundingBox Extents;
        public Matrix WMOToModel;
        public Matrix ModeltoWMO;
    }
}
