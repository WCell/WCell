using System;
using System.Runtime.InteropServices;

namespace WCell.Util.Graphics
{
	/// <summary>
	/// Defines a vector with three components.
	/// </summary>
	[StructLayout(LayoutKind.Explicit, Size = 12)]
	public struct Vector3 : IEquatable<Vector3>
	{
		public static readonly Vector3 Zero = new Vector3(0, 0, 0);
		public static readonly Vector3 Up = new Vector3(0f, 0f, 1f);
		public static readonly Vector3 Down = new Vector3(0f, 0f, -1f);
		public static readonly Vector3 Left = new Vector3(-1f, 0f, 0f);
		public static readonly Vector3 Right = new Vector3(1f, 0f, 0f);
		public static readonly Vector3 Backwards = new Vector3(0f, 0f, 1f);
		public static readonly Vector3 Forward = new Vector3(0f, 0f, -1f);

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
		/// The Z component of the vector.
		/// </summary>
		[FieldOffset(8)]
		public float Z;

        public Vector3(float val)
            : this(val, val, val)
        {
        }

		/// <summary>
		/// Creates a new <see cref="Vector3" /> with the given X and Y, and Z = 0.
		/// </summary>
		/// <param name="x">the X component</param>
		/// <param name="y">the Y component</param>
		public Vector3(float x, float y) : this(x, y, 0f)
		{
		}

		/// <summary>
		/// Creates a new <see cref="Vector3" /> with the given X, Y and Z components.
		/// </summary>
		/// <param name="x">the X component</param>
		/// <param name="y">the Y component</param>
		/// <param name="z">the Z component</param>
		public Vector3(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
		/// Creates a new <see cref="Vector3" /> with the given X, Y, and Z components.
		/// </summary>
		/// <param name="xy">the XY component</param>
		/// <param name="z">the Z component</param>
		public Vector3(Vector2 xy, float z)
		{
			X = xy.X;
			Y = xy.Y;
			Z = z;
		}

		public bool IsSet
		{
			get { return this != Zero; }
		}

		#region Distances
		/// <summary>
		/// Calculates the distance from this vector to another.
		/// </summary>
		/// <param name="point">the second <see cref="Vector3" /></param>
		/// <returns>the distance between the vectors</returns>
		public float GetDistance(Vector3 point)
		{
			float x = point.X - X;
			float y = point.Y - Y;
			float z = point.Z - Z;
			float dist = ((x * x) + (y * y)) + (z * z);

			return (float)Math.Sqrt(dist);
		}

		/// <summary>
		/// Calculates the distance from this vector to another.
		/// </summary>
		/// <param name="point">the second <see cref="Vector3" /></param>
		/// <returns>the distance between the vectors</returns>
		public float GetDistance(ref Vector3 point)
		{
			float x = point.X - X;
			float y = point.Y - Y;
			float z = point.Z - Z;
			float dist = ((x * x) + (y * y)) + (z * z);

			return (float)Math.Sqrt(dist);
		}

		/// <summary>
		/// Calculates the distance squared from this vector to another.
		/// </summary>
		/// <param name="point">the second <see cref="Vector3" /></param>
		/// <returns>the distance squared between the vectors</returns>
		public float GetDistanceSquared(Vector3 point)
		{
			float x = point.X - X;
			float y = point.Y - Y;
			float z = point.Z - Z;

			return ((x * x) + (y * y)) + (z * z);
		}

		/// <summary>
		/// Calculates the distance squared from this vector to another.
		/// </summary>
		/// <param name="point">the second <see cref="Vector3" /></param>
		/// <returns>the distance squared between the vectors</returns>
		public float GetDistanceSquared(ref Vector3 point)
		{
			float x = point.X - X;
			float y = point.Y - Y;
			float z = point.Z - Z;

			return ((x * x) + (y * y)) + (z * z);
		}

        public float LengthSquared()
        {
            return (X*X + Y*Y + Z*Z);
        }

        public float Length()
        {
            var sqrd = (X*X + Y*Y + Z*Z);
            return (float)Math.Sqrt(sqrd);
        }
		#endregion

		#region Angles
		/// <summary>
		/// Gets the angle between this object and the given position, in relation to the north-south axis
		/// </summary>
		public float GetAngleTowards(Vector3 v)
		{
			var angle = (float)Math.Atan2((v.Y - Y), (v.X - X));
			if (angle < 0)
			{
				angle += (2 * MathUtil.PI);
			}
			return angle;
		}

		/// <summary>
		/// Gets the angle between this object and the given position, in relation to the north-south axis
		/// </summary>
		public float GetAngleTowards(ref Vector3 v)
		{
			var angle = (float)Math.Atan2((v.Y - Y), (v.X - X));
			if (angle < 0)
			{
				angle += (2 * MathUtil.PI);
			}
			return angle;
		}
		#endregion

		/// <summary>
		/// Gets the Point that lies in the given angle and has dist from this Point (in the XY plane).
		/// </summary>
		public void GetPointXY(float angle, float dist, out Vector3 point)
		{
			point = new Vector3(X - (dist * (float)Math.Sin(angle)), Y + (dist * (float)Math.Cos(angle)));
		}

		/// <summary>
		/// Gets the Point that lies in the given angle and has dist from this Point 
		/// (in the weird WoW coordinate system: X -> South, Y -> West).
		/// </summary>
		public void GetPointYX(float angle, float dist, out Vector3 point)
		{
			point = new Vector3(X + (dist * (float)Math.Cos(angle)), Y + (dist * (float)Math.Sin(angle)));
		}

		#region Normalization
		/// <summary>
		/// Turns the current vector into a unit vector.
		/// </summary>
		/// <remarks>The vector becomes one unit in length and points in the same direction of the original vector.</remarks>
		public void Normalize()
		{
			float length = ((X * X) + (Y * Y)) + (Z * Z);
			float normFactor = 1f / ((float)Math.Sqrt(length));

			X *= normFactor;
			Y *= normFactor;
			Z *= normFactor;
		}

        /// <summary>
        /// Turns the current vector into a unit vector.
        /// </summary>
        /// <remarks>The vector becomes one unit in length and points in the same direction of the original vector.</remarks>
        /// <returns>The length of the original vector.</returns>
        public float NormalizeReturnLength()
        {
            var lengthSqrd = ((X * X) + (Y * Y)) + (Z * Z);
            var length = (float)Math.Sqrt(lengthSqrd);
            var normFactor = 1f / (length);

            X *= normFactor;
            Y *= normFactor;
            Z *= normFactor;

            return length;
        }

		/// <summary>
		/// Turns the current vector into a unit vector.
		/// </summary>
		/// <remarks>The vector becomes one unit in length and points in the same direction of the original vector.</remarks>
		public Vector3 NormalizedCopy()
		{
			var length = ((X * X) + (Y * Y)) + (Z * Z);
			var normFactor = 1f / ((float)Math.Sqrt(length));

			return new Vector3(
				X * normFactor,
				Y * normFactor,
				Z * normFactor);
		}
		#endregion

		#region Compression
		public static Vector3 FromPacked(uint packed)
		{
			Vector3 vector;
			vector.X = packed & 0x7FF;
			vector.Y = (packed >> 11) & 0x7FF;
			vector.Z = (packed >> 21) & 0x3FF;

			return vector;
		}

		public static Vector3 FromDeltaPacked(uint packed, Vector3 startingVector, Vector3 firstPoint)
		{
			float xPart = packed & 0x7FF;
			float yPart = (packed >> 11) & 0x7FF;
			float zPart = (packed >> 21) & 0x3FF;

			Vector3 dst;
			dst.X = (xPart / 4) - (startingVector.X + firstPoint.X) / 2;
			dst.Y = (yPart / 4) - (startingVector.Y + firstPoint.Y) / 2;
			dst.Z = (zPart / 4) - (startingVector.Z + firstPoint.Z) / 2;
			return dst;
		}

		public uint ToPacked()
		{
			uint packed = 0;
			packed |= ((uint)X) & 0x7FF;
			packed |= ((uint)Y & 0x7FF) << 11;
			packed |= ((uint)Z & 0x3FF) << 22;

			return packed;
		}

		/// <summary>
		/// TODO: Ensure this is the correct order of packing
		/// </summary>
		/// <param name="startingVector"></param>
		/// <param name="firstPoint"></param>
		/// <returns></returns>
		public uint ToDeltaPacked(Vector3 startingVector, Vector3 firstPoint)
		{
			float xPart = (X * 4) + ((startingVector.X - firstPoint.X) * 2);
			float yPart = (Y * 4) + ((startingVector.Y - firstPoint.Y) * 2);
			float zPart = (Z * 4) + ((startingVector.Z - firstPoint.Z) * 2);

			uint packed = 0;
			packed |= ((uint)xPart) & 0x7FF;
			packed |= ((uint)yPart & 0x7FF) << 11;
			packed |= ((uint)zPart & 0x3FF) << 22;

			return packed;
		}
		#endregion

		#region Vector Operations
		public static Vector3 operator -(Vector3 a, Vector3 b)
		{
			Vector3 vector;
			vector.X = a.X - b.X;
			vector.Y = a.Y - b.Y;
			vector.Z = a.Z - b.Z;

			return vector;
		}

		public static Vector3 operator -(Vector3 value)
		{
			Vector3 vector;
			vector.X = -value.X;
			vector.Y = -value.Y;
			vector.Z = -value.Z;
			return vector;
		}

		public static Vector3 operator +(Vector3 a, Vector3 b)
		{
			Vector3 vector;
			vector.X = a.X + b.X;
			vector.Y = a.Y + b.Y;
			vector.Z = a.Z + b.Z;

			return vector;
		}

		public static Vector3 operator *(Vector3 a, Vector3 b)
		{
			Vector3 vector;
			vector.X = a.X * b.X;
			vector.Y = a.Y * b.Y;
			vector.Z = a.Z * b.Z;

			return vector;
		}

		public static Vector3 operator *(Vector3 a, float scaleFactor)
		{
			Vector3 vector;
			vector.X = a.X * scaleFactor;
			vector.Y = a.Y * scaleFactor;
			vector.Z = a.Z * scaleFactor;

			return vector;
		}

		public static Vector3 operator /(Vector3 a, Vector3 b)
		{
			Vector3 vector;
			vector.X = a.X / b.X;
			vector.Y = a.Y / b.Y;
			vector.Z = a.Z / b.Z;

			return vector;
		}

		public static Vector3 operator /(Vector3 a, float scaleFactor)
		{
			Vector3 vector;
			float factor = 1f / scaleFactor;
			vector.X = a.X * factor;
			vector.Y = a.Y * factor;
			vector.Z = a.Z * factor;

			return vector;
		}

        public static Vector3 Cross(Vector3 vector1, Vector3 vector2)
        {
            Vector3 vector;
            vector.X = (vector1.Y * vector2.Z) - (vector1.Z * vector2.Y);
            vector.Y = (vector1.Z * vector2.X) - (vector1.X * vector2.Z);
            vector.Z = (vector1.X * vector2.Y) - (vector1.Y * vector2.X);
            return vector;
        }

        public static Vector3 Normalize(Vector3 value)
        {
            Vector3 vector;
            float num2 = ((value.X * value.X) + (value.Y * value.Y)) + (value.Z * value.Z);
            float num = 1f / ((float)Math.Sqrt((double)num2));
            vector.X = value.X * num;
            vector.Y = value.Y * num;
            vector.Z = value.Z * num;
            return vector;
        }

        public static float Dot(Vector3 vector1, Vector3 vector2)
        {
            return (((vector1.X * vector2.X) + (vector1.Y * vector2.Y)) + (vector1.Z * vector2.Z));
		}

        public static void Add(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.X = value1.X + value2.X;
            result.Y = value1.Y + value2.Y;
            result.Z = value1.Z + value2.Z;
        }

        public static void Subtract(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.X = value1.X - value2.X;
            result.Y = value1.Y - value2.Y;
            result.Z = value1.Z - value2.Z;
        }
		#endregion

		public static bool operator ==(Vector3 a, Vector3 b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Vector3 a, Vector3 b)
		{
			if (!(a.X != b.X) && !(a.Y != b.Y))
			{
				return (a.Z != b.Z);
			}

			return true;
		}

		#region Transform
		public static Vector3 TransformNormal(Vector3 normal, Matrix matrix)
        {
            Vector3 vector;
            float num3 = ((normal.X * matrix.M11) + (normal.Y * matrix.M21)) + (normal.Z * matrix.M31);
            float num2 = ((normal.X * matrix.M12) + (normal.Y * matrix.M22)) + (normal.Z * matrix.M32);
            float num = ((normal.X * matrix.M13) + (normal.Y * matrix.M23)) + (normal.Z * matrix.M33);
            vector.X = num3;
            vector.Y = num2;
            vector.Z = num;
            return vector;
        }

        public static void TransformNormal(ref Vector3 normal, ref Matrix matrix, out Vector3 newVector)
        {
            float num3 = ((normal.X * matrix.M11) + (normal.Y * matrix.M21)) + (normal.Z * matrix.M31);
            float num2 = ((normal.X * matrix.M12) + (normal.Y * matrix.M22)) + (normal.Z * matrix.M32);
            float num = ((normal.X * matrix.M13) + (normal.Y * matrix.M23)) + (normal.Z * matrix.M33);
            newVector.X = num3;
            newVector.Y = num2;
            newVector.Z = num;
        }

        public static Vector3 Transform(Vector3 position, Matrix matrix)
        {
            Vector3 vector;
            float num3 = (((position.X * matrix.M11) + (position.Y * matrix.M21)) + (position.Z * matrix.M31)) + matrix.M41;
            float num2 = (((position.X * matrix.M12) + (position.Y * matrix.M22)) + (position.Z * matrix.M32)) + matrix.M42;
            float num = (((position.X * matrix.M13) + (position.Y * matrix.M23)) + (position.Z * matrix.M33)) + matrix.M43;
            vector.X = num3;
            vector.Y = num2;
            vector.Z = num;
            return vector;
        }

        public static Vector3 Transform(Vector3 value, Quaternion rotation)
        {
            Vector3 vector;
            float num12 = rotation.X + rotation.X;
            float num2 = rotation.Y + rotation.Y;
            float num = rotation.Z + rotation.Z;
            float num11 = rotation.W * num12;
            float num10 = rotation.W * num2;
            float num9 = rotation.W * num;
            float num8 = rotation.X * num12;
            float num7 = rotation.X * num2;
            float num6 = rotation.X * num;
            float num5 = rotation.Y * num2;
            float num4 = rotation.Y * num;
            float num3 = rotation.Z * num;
            float num15 = ((value.X * ((1f - num5) - num3)) + (value.Y * (num7 - num9))) + (value.Z * (num6 + num10));
            float num14 = ((value.X * (num7 + num9)) + (value.Y * ((1f - num8) - num3))) + (value.Z * (num4 - num11));
            float num13 = ((value.X * (num6 - num10)) + (value.Y * (num4 + num11))) + (value.Z * ((1f - num8) - num5));
            vector.X = num15;
            vector.Y = num14;
            vector.Z = num13;
            return vector;
		}

        public static void Transform(ref Vector3 position, ref Matrix matrix, out Vector3 result)
        {
            float num3 = (((position.X * matrix.M11) + (position.Y * matrix.M21)) + (position.Z * matrix.M31)) + matrix.M41;
            float num2 = (((position.X * matrix.M12) + (position.Y * matrix.M22)) + (position.Z * matrix.M32)) + matrix.M42;
            float num = (((position.X * matrix.M13) + (position.Y * matrix.M23)) + (position.Z * matrix.M33)) + matrix.M43;
            result.X = num3;
            result.Y = num2;
            result.Z = num;
        }


		#endregion

		#region Min/Max
		/// <summary>
		/// Clamps the values of the vector to be within a specified range.
		/// </summary>
		/// <param name="min">the minimum value</param>
		/// <param name="max">the maximum value</param>
		/// <returns>a new <see cref="Vector3" /> that has been clamped within the specified range</returns>
		public Vector3 Clamp(ref Vector3 min, ref Vector3 max)
		{
			float x = X;
			x = (x > max.X) ? max.X : x;
			x = (x < min.X) ? min.X : x;
			float y = Y;
			y = (y > max.Y) ? max.Y : y;
			y = (y < min.Y) ? min.Y : y;
			float z = Z;
			z = (z > max.Z) ? max.Z : z;
			z = (z < min.Z) ? min.Z : z;

			return new Vector3(x, y, z);
		}

		public static Vector3 Min(Vector3 a, Vector3 b)
		{
			return new Vector3(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));
		}

		public static Vector3 Max(Vector3 a, Vector3 b)
		{
			return new Vector3(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));
		}
		#endregion

		#region Overrides
		/// <summary>
		/// Checks equality of two vectors.
		/// </summary>
		/// <param name="other">the other vector to compare with</param>
		/// <returns>true if both vectors are equal; false otherwise</returns>
		public bool Equals(Vector3 other)
		{
			return ((X == other.X) && (Y == other.Y) && (Z == other.Z));
		}

		/// <summary>
		/// Checks equality with another object.
		/// </summary>
		/// <param name="obj">the object to compare</param>
		/// <returns>true if the object is <see cref="Vector3" /> and is equal; false otherwise</returns>
		public override bool Equals(object obj)
		{
			return obj is Vector3 && Equals((Vector3)obj);
		}

		public override int GetHashCode()
		{
			return (X.GetHashCode() + Y.GetHashCode() + Y.GetHashCode());
		}

		public override string ToString()
		{
			return string.Format("(X:{0}, Y:{1}, Z:{2})", X, Y, Z);
		}
		#endregion
	}
}