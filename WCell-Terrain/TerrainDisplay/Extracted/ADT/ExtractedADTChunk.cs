using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerrainDisplay.MPQ.ADT.Components;
using TerrainDisplay.MPQ.WMO;
using WCell.Collision;
using WCell.Util.Graphics;

namespace TerrainDisplay.Extracted.ADT
{
    public class ExtractedADTChunk : IQuadObject
    {
        public bool IsFlat;
        public float MedianHeight;
        public List<int> M2References;
        public List<int> WMOReferences;
        public List<Index3> TerrainTris;

        //public bool HasHoles;
        //public bool[,] HolesMap;

        //public float[,] HeightMap;

        public bool HasLiquid;
        public MH2OFlags LiquidFlags;
        public FluidType LiquidType;
        public bool IsLiquidFlat;
        public float MedianLiquidHeight1;
        public float MedianLiquidHeight2;
        public bool[,] LiquidMap;
        public float[,] LiquidHeights;

        public Rect Bounds { get; set; }
        public int NodeId { get; set; }
    }
}
