using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Terrain.Collision;
using WCell.Terrain.Collision.QuadTree;
using WCell.Terrain.MPQ;
using WCell.Util.Graphics;

namespace WCell.Terrain.Collision.QuadTree
{
    public class WMOHolder : IQuadObject, ICollidable
    {
        private WMORoot WMO;

        public Rect Bounds { get; private set; }

        public int NodeId
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public event EventHandler BoundsChanged;

        public WMOHolder(WMORoot wmo)
        {
            WMO = wmo;

            var min = wmo.Bounds.Min;
            var max = wmo.Bounds.Max;
            var minPoint = new Point(min.X, min.Y);
            var maxPoint = new Point(max.X, max.Y);
            Bounds = new Rect(minPoint, maxPoint);
        }

        public bool IntersectWith(Ray ray, out float distance)
        {
            throw new NotImplementedException();
        }
    }
}
