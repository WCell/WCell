using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.Graphics
{
	/// <summary>
	/// Fixed 2D point (using integers for components)
	/// </summary>
	public class Point2D : IEquatable<Point2D>
	{
        public static Point2D NorthWest = new Point2D(-1, -1);
        public static Point2D North = new Point2D(-1, 0);
        public static Point2D NorthEast = new Point2D(-1, 1);
        public static Point2D East = new Point2D(0, 1);
        public static Point2D SouthEast = new Point2D(1, 1);
        public static Point2D South = new Point2D(1, 0);
        public static Point2D SouthWest = new Point2D(1, -1);
        public static Point2D West = new Point2D(0, -1);
        
        public int X;
		public int Y;

		public Point2D()
		{
		}

		public Point2D(int x, int y)
		{
			X = x;
			Y = y;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof(Point2D)) return false;
			return Equals((Point2D)obj);
		}

		public bool Equals(Point2D obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj.X == X && obj.Y == Y;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (X * 397) ^ Y;
			}
		}

		public static bool operator ==(Point2D left, Point2D right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Point2D left, Point2D right)
		{
			return !Equals(left, right);
		}

        public static Point2D operator +(Point2D left, Point2D right)
        {
            return new Point2D(left.X + right.X, left.Y + right.Y);
        }

        public static Point2D operator -(Point2D left, Point2D right)
        {
            return new Point2D(left.X - right.X, left.Y - right.Y);
        }
	}
}
