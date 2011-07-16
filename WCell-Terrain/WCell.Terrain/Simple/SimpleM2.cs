using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Terrain.MPQ;
using WCell.Util.Graphics;

namespace WCell.Terrain.Simple
{
    public class SimpleM2
    {
        public BoundingBox Extents;
        public List<Vector3> BoundingVertices;
        public List<Index3> BoundingTriangles;
    }
}
