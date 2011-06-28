using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerrainDisplay.MPQ.M2;
using WCell.Collision;
using WCell.Util.Graphics;

namespace TerrainDisplay.Collision.QuadTree
{
    public class M2Holder : IQuadObject, ICollidable
    {
        public M2 Model;
        public event EventHandler BoundsChanged;

        public Rect Bounds
        {
            get; 
            private set;
        }

        public int NodeId
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public M2Holder(M2 model)
        {
            Model = model;

            var min = model.Bounds.Min;
            var max = model.Bounds.Max;
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
