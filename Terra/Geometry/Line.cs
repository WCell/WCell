using System;
using System.Diagnostics;
using WCell.Util.Graphics;


namespace Terra.Geometry
{
    /// <summary>
    /// A Barycentric representation of a Line in 2D space
    /// </summary>
    internal class Line
    {
        public float A;
        public float B;
        public float C;

        /// <summary>
        /// Creates a new Line connecting the points p, q
        /// </summary>
        public Line(Vector2 p, Vector2 q)
        {
            Vector2 t;
            Vector2.Subtract(ref q, ref p, out t);

            var len = t.Length();
            if (len == 0)
            {
                throw new DivideByZeroException("Cannot create a line from zero length segment.");
            }

            A = t.Y / len;
            B = t.X / len;
            C = (A * p.X + B * p.Y) * -1;
        }

        public float Eval(ref Vector2 p)
        {
            return (A * p.X + B * p.Y + C);
        }

        public Side Classify(ref Vector2 p)
        {
            var d = Eval(ref p);

            if (d < (-1 * GeometryHelpers.Epsilon)) return Side.Left;
            if (d > GeometryHelpers.Epsilon) return Side.Right;
            return Side.On;
        }

        public Vector2 Intersect(Line l)
        {
            Vector2 p;
            Intersect(l, out p);
            return p;
        }

        public void Intersect(Line l, out Vector2 p)
        {
            var denom = A * l.B - B * l.A;
            Debug.Assert(denom != 0.0f);

            p.X = (B * l.C - C * l.B) / denom;
            p.Y = (C * l.A - A * l.C) / denom;
        }

        public override string ToString()
        {
            return string.Format("Line: (A: {0}, B: {1}, C: {2})", A, B, C);
        }
    }
}