using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.Graphics
{
    public struct Size
    {
        public static readonly Size Empty;

        static Size()
        {
            Empty = new Size(0.0f, 0.0f);
        }

        public Size(float width, float height)
        {
            this.width = width;
            this.height = height;
        }

        public float width;
        public float Width
        {
            get { return width; }
            set { width = value; }
        }

        public float height;
        public float Height
        {
            get { return height; }
            set { height = value; }
        }

        public bool IsEmpty
        {
            get { return ((Width == 0.0f) && (Height == 0)); }
        }

        public static Size Add(Size sz1, Size sz2)
        {
            return new Size((sz1.Width + sz2.Width), (sz1.Height + sz2.Height));
        }

        public static Size operator +(Size sz1, Size sz2)
        {
            return Add(sz1, sz2);
        }

        public static Size Subtract(Size sz1, Size sz2)
        {
            return new Size((sz1.Width - sz2.Width), (sz1.Height - sz2.Height));
        }

        public static Size operator -(Size sz1, Size sz2)
        {
            return Subtract(sz1, sz2);
        }

        public Point ToPoint()
        {
            return new Point(width, height);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Size)) return false;
            return Equals((Size) obj);
        }

        public bool Equals(Size other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.width.Equals(width) && other.height.Equals(height);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (width.GetHashCode()*397) ^ height.GetHashCode();
            }
        }

        public static bool operator ==(Size left, Size right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Size left, Size right)
        {
            return !Equals(left, right);
        }
    }
}
