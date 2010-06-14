using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TerrainDisplay.MPQ.WMO.Components;

namespace TerrainDisplay.Extracted.WMO
{
    public class ExtractedWMOM2Definition : DoodadDefinition
    {
        public BoundingBox Extents;
        public Matrix WMOToModel;
        public Matrix ModeltoWMO;
    }
}
