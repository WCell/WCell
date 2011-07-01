using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Terrain.MPQ.WMOs;
using WCell.Util.Graphics;

namespace WCell.Terrain.Simple.WMO
{
    public class SimpleWMOGroup
    {
        public WMOGroupFlags Flags;
        public BoundingBox Bounds;
        public uint GroupId;
        public List<int> ModelRefs;

        public bool HasLiquid;
        public Vector3 LiquidBaseCoords;
        public int LiqTileCountX;
        public int LiqTileCountY;
        public bool[,] LiquidTileMap;
        public float[,] LiquidHeights;

        public List<Vector3> WmoVertices;
        public List<Vector3> LiquidVertices;
        public BSPTree Tree;
    }
}
