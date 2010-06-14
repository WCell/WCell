using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerrainDisplay.MPQ.ADT.Components;

namespace TerrainDisplay.Extracted.ADT
{
    public class ExtractedADTChunk
    {
        public bool IsFlat;
        public float MedianHeight;
        public List<int> M2References;
        public List<int> WMOReferences;
        
        public bool HasHoles;
        public bool[,] HolesMap;

        public float[,] HeightMap;

        public bool HasLiquid;
        public MH2OFlags LiquidFlags;
        public FluidType LiquidType;
        public bool IsLiquidFlat;
        public float MedianLiquidHeight1;
        public float MedianLiquidHeight2;
        public bool[,] LiquidMap;
        public float[,] LiquidHeights;
    }
}
