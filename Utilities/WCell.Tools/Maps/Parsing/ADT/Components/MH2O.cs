using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;

namespace WCell.Tools.Maps
{
    public class MH2O
    {
        public bool Used;
        public MH2OFlags Flags;
        public FluidType Type;
        public float HeightLevel1;
        public float HeightLevel2;

        public byte XOffset;
        public byte YOffset;
        public byte Width;
        public byte Height;

        public float[,] Heights;
    }

    public class MH2OHeader
    {
        public uint ofsData1;
        public uint LayerCount;
        public uint ofsData2;
    }
}
