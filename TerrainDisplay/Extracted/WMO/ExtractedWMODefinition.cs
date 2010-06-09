using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MPQNav.MPQ.ADT.Components;

namespace TerrainDisplay.Extracted.WMO
{
    public class ExtractedWMODefinition : MapObjectDefinition
    {
        public Matrix WMOToWorld;
        public Matrix WorldToWMO;
    }
}
