using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Graphics;
using WCell.Terrain.MPQ.ADT.Components;

namespace TerrainDisplay.Extracted.WMO
{
    public class ExtractedWMODefinition : MapObjectDefinition
    {
        public Matrix WMOToWorld;
        public Matrix WorldToWMO;
    }
}
