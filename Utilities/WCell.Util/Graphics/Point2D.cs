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
	}
}
