using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Graphics;

namespace Terra.Geometry
{
    internal class Triangle : ILabelled
    {
        public Triangle(Edge edge) : this(edge, 0)
        {
        }

        public Triangle(Edge edge, int t)
        {
            Token = t;
            Reshape(edge);
        }


        public int Token { get; set; }

        public Triangle NextFace { get; private set; }

        public Edge Anchor { get; private set; }

        public Vector2 Point1
        {
            get { return Anchor.Orig; }
        }

        public Vector2 Point2
        {
            get { return Anchor.Dest; }
        }

        public Vector2 Point3
        {
            get { return Anchor.LPrev.Orig; }
        }

        public Triangle LinkTo(Triangle triangle)
        {
            NextFace = triangle;
            return this;
        }

        public void DontAnchor(Edge edge)
        {
            if (Anchor != edge) return;
            Anchor = edge.LNext;
        }

        public void Reshape(Edge edge)
        {
            Anchor = edge;

            edge.LFace = this;
            edge.LNext.LFace = this;
            edge.LPrev.LFace = this;
        }

        public virtual void Update(Subdivision sub)
        {
        }

        public override string ToString()
        {
            return String.Format("Triangle({0} {1} {2})", Point1, Point2, Point3);
        }
    }
}
