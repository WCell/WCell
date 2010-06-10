using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MPQNav.MPQ.ADT.Components;

namespace TerrainDisplay.Extracted.M2
{
    public class ExtractedMapM2Definition : MapDoodadDefinition
    {
        public BoundingBox Extents;
        public Matrix ModelToWorld;
        public Matrix WorldToModel;
    }
}
