using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Terrain.MPQ;
using WCell.Util.Graphics;

namespace WCell.Terrain
{
    public static class MeshClipper
    {
        public static ClipperResponse ClipAgainst(WMORoot wmo, Rect bounds, out List<Vector3> vertices, out List<int> indices)
        {
            vertices = new List<Vector3>();
            indices = new List<int>();
            return ClipperResponse.AllClipped;
        }
    }

    public enum ClipperResponse
    {
        NoneClipped,
        SomeClipped,
        AllClipped
    }
}
