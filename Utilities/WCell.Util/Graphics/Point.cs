using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.Graphics
{
    public struct Point
    {
        public static readonly Point Empty;

        static Point()
        {
            Empty = new Point(0.0f, 0.0f);
        }

        public Point(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        private float x;
        public float X
        {
            get { return x; }
            set { x = value; }
        }

        private float y;
        public float Y
        {
            get { return y; }
            set { y = value; }
        }

        public bool IsEmpty
        {
            get { return ((x == 0.0f) && (y == 0.0f)); }
        }

        public static Point Add(Point pt, Size sz)
        {
            return new Point(pt.X + sz.Width, pt.Y + sz.Height);
        }

        public static Point operator +(Point pt, Size sz)
        {
            return Add(pt, sz);
        }

        public static Point Subtract(Point pt, Size sz)
        {
            return new Point((pt.X - sz.Width), (pt.Y - sz.Height));
        }

        public static Point operator -(Point pt, Size sz)
        {
            return Subtract(pt, sz);
        }

        public Size ToSize()
        {
            return new Size(X, Y);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Point)) return false;
            return Equals((Point) obj);
        }


        public bool Equals(Point other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.x.Equals(x) && other.y.Equals(y);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (x.GetHashCode()*397) ^ y.GetHashCode();
            }
        }

        public static bool operator ==(Point left, Point right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Point left, Point right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return string.Format("X = {0}, Y = {1}", X, Y);
        }
    }
}
