using System;
using System.Runtime.InteropServices;

namespace WCell.Util.Graphics
{
	/// <summary>
	/// Defines a vector with two components.
	/// </summary>
	[StructLayout(LayoutKind.Explicit, Size = 8)]
	public struct Vector2 : IEquatable<Vector2>
	{
		public static readonly Vector2 Zero = new Vector2(0, 0);

		/// <summary>
		/// The X component of the vector.
		/// </summary>
		[FieldOffset(0)]
		public float X;

		/// <summary>
		/// The Y component of the vector.
		/// </summary>
		[FieldOffset(4)]
		public float Y;

		/// <summary>
		/// Creates a new <see cref="Vector2" /> with the given X and Y components.
		/// </summary>
		/// <param name="x">the X component</param>
		/// <param name="y">the Y component</param>
		public Vector2(float x, float y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
		/// Clamps the values of the vector to be within a specified range.
		/// </summary>
		/// <param name="min">the minimum value</param>
		/// <param name="max">the maximum value</param>
		/// <returns>a new <see cref="Vector2" /> that has been clamped within the specified range</returns>
		public Vector2 Clamp(ref Vector2 min, ref Vector2 max)
		{
			float x = X;
			x = (x > max.X) ? max.X : x;
			x = (x < min.X) ? min.X : x;
			float y = Y;
			y = (y > max.Y) ? max.Y : y;
			y = (y < min.Y) ? min.Y : y;

			return new Vector2(x, y);
		}

		/// <summary>
		/// Calculates the distance from this vector to another.
		/// </summary>
		/// <param name="point">the second <see cref="Vector2" /></param>
		/// <returns>the distance between the vectors</returns>
		public float GetDistance(ref Vector2 point)
		{
			float x = point.X - X;
			float y = point.Y - Y;
			float dist = (x * x) + (y * y);

			return (float)Math.Sqrt(dist);
		}

		/// <summary>
		/// Calculates the distance squared from this vector to another.
		/// </summary>
		/// <param name="point">the second <see cref="Vector2" /></param>
		/// <returns>the distance squared between the vectors</returns>
		public float GetDistanceSquared(ref Vector2 point)
		{
			float x = point.X - X;
			float y = point.Y - Y;

			return (x * x) + (y * y);
		}

		/// <summary>
		/// Turns the current vector into a unit vector.
		/// </summary>
		/// <remarks>The vector becomes one unit in length and points in the same direction of the original vector.</remarks>
		public void Normalize()
		{
			float length = ((X * X) + (Y * Y));
			float normFactor = 1f / ((float)Math.Sqrt(length));

			X *= normFactor;
			Y *= normFactor;
		}

		/// <summary>
		/// Checks equality of two vectors.
		/// </summary>
		/// <param name="other">the other vector to compare with</param>
		/// <returns>true if both vectors are equal; false otherwise</returns>
		public bool Equals(Vector2 other)
		{
			return ((X == other.X) && (Y == other.Y));
		}

		/// <summary>
		/// Checks equality with another object.
		/// </summary>
		/// <param name="obj">the object to compare</param>
		/// <returns>true if the object is <see cref="Vector2" /> and is equal; false otherwise</returns>
		public override bool Equals(object obj)
		{
			return obj is Vector2 && Equals((Vector2) obj);
		}

		public override int GetHashCode()
		{
			return (X.GetHashCode() + Y.GetHashCode());
		}

		public static bool operator ==(Vector2 a, Vector2 b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Vector2 a, Vector2 b)
		{
			return (a.X != b.X) || a.Y != b.Y;
		}

		public override string ToString()
		{
			return string.Format("(X:{0}, Y:{1})", X, Y);
		}
	}
}