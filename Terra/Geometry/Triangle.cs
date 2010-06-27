using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Graphics;

namespace Terra.Geometry
{
    public delegate void EdgeCallback(Edge edge, object closure);
    public delegate void FaceCallback(Triangle tri, object closure);

    public class Triangle : ILabelled
    {
        private Edge anchor;
        private Triangle nextFace;


        public Triangle(Edge edge) : this(edge, 0)
        {
        }

        public Triangle(Edge edge, int t)
        {
            Token = t;
            Reshape(edge);
        }


        public int Token { get; set; }

        public Triangle Link
        {
            get { return nextFace; }
        }

        public Edge Anchor
        {
            get { return anchor; }
        }

        public Vector2 Point1
        {
            get { return anchor.Orig; }
        }

        public Vector2 Point2
        {
            get { return anchor.Dest; }
        }

        public Vector2 Point3
        {
            get { return anchor.LPrev.Orig; }
        }

        public Triangle LinkTo(Triangle triangle)
        {
            nextFace = triangle;
            return this;
        }

        public void DontAnchor(Edge edge)
        {
            if (anchor != edge) return;
            anchor = edge.LNext;
        }

        public void Reshape(Edge edge)
        {
            anchor = edge;

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
