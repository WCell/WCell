using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace WCell.Util.Graphics
{
	/// <summary>
	/// Defines an axis-aligned bounding box.
	/// </summary>
	[StructLayout(LayoutKind.Explicit, Size = 24)]
	public struct BoundingBox : IEquatable<BoundingBox>
	{
	    public static BoundingBox INVALID = new BoundingBox(new Vector3(float.MaxValue, float.MaxValue, float.MaxValue),
	                                                        new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));
		/// <summary>
		/// The lower-left bound of the box.
		/// </summary>
		[FieldOffset(0)]
		public Vector3 Min;

		/// <summary>
		/// The upper-right bound of the box.
		/// </summary>
		[FieldOffset(12)]
		public Vector3 Max;

		/// <summary>
		/// Creates a new <see cref="BoundingBox" /> with the given coordinates.
		/// </summary>
		/// <param name="minX">lower-bound X</param>
		/// <param name="minY">lower-bound Y</param>
		/// <param name="minZ">lower-bound Z</param>
		/// <param name="maxX">upper-bound X</param>
		/// <param name="maxY">upper-bound Y</param>
		/// <param name="maxZ">upper-bound Z</param>
		public BoundingBox(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
		{
			Min = new Vector3(minX, minY, minZ);
			Max = new Vector3(maxX, maxY, maxZ);
		}

		/// <summary>
		/// Creates a new <see cref="BoundingBox" /> with the given coordinates.
		/// </summary>
		/// <param name="minX">lower-bound X</param>
		/// <param name="minY">lower-bound Y</param>
		/// <param name="minZ">lower-bound Z</param>
		/// <param name="max">upper-bound vector</param>
		public BoundingBox(float minX, float minY, float minZ, Vector3 max)
		{
			Min = new Vector3(minX, minY, minZ);
			Max = max;
		}

		/// <summary>
		/// Creates a new <see cref="BoundingBox" /> with the given coordinates.
		/// </summary>
		/// <param name="min">lower-bound vector</param>
		/// <param name="maxX">upper-bound X</param>
		/// <param name="maxY">upper-bound Y</param>
		/// <param name="maxZ">upper-bound Z</param>
		public BoundingBox(Vector3 min, float maxX, float maxY, float maxZ)
		{
			Min = min;
			Max = new Vector3(maxX, maxY, maxZ);
		}

		/// <summary>
		/// Creates a new <see cref="BoundingBox" /> with the given coordinates.
		/// </summary>
		/// <param name="min">lower-bound vector</param>
		/// <param name="max">upper-bound vector</param>
		public BoundingBox(Vector3 min, Vector3 max)
		{
			Min = min;
			Max = max;
		}

        /// <summary>
        /// Creates a new <see cref="BoundingBox" /> containing the given vectors.
        /// </summary>
        /// <param name="vectors">The array of vectors to use.</param>
        public BoundingBox(Vector3[] vectors)
        {
            var minX = float.MaxValue;
            var minY = float.MaxValue;
            var minZ = float.MaxValue;

            var maxX = float.MinValue;
            var maxY = float.MinValue;
            var maxZ = float.MinValue;

            for (var i = 0; i < vectors.Length; i++)
            {
                var vector = vectors[i];
                minX = Math.Min(vector.X, minX);
                maxX = Math.Max(vector.X, maxX);

                minY = Math.Min(vector.Y, minY);
                maxY = Math.Max(vector.Y, maxY);

                minZ = Math.Min(vector.Z, minZ);
                maxZ = Math.Max(vector.Z, maxZ);
            }

            Min = new Vector3(minX, minY, minZ);
            Max = new Vector3(maxX, maxY, maxZ);
        }

		public float Width
		{
			get { return Max.X - Min.X; }
		}

		public float Height
		{
			get { return Max.Y - Min.Y; }
		}

        /// <summary>
		/// Checks whether the current <see cref="BoundingBox" /> intersects with the given <see cref="BoundingBox" />.
		/// </summary>
		/// <param name="box">the <see cref="BoundingBox" /> to check for intersection</param>
		/// <returns>an enumeration value describing the type of intersection between the two boxes</returns>
		public IntersectionType Intersects(ref BoundingBox box)
		{
			if ((Max.X < box.Min.X) || (Min.X > box.Max.X))
			{
				return IntersectionType.NoIntersection;
			}
			if ((Max.Y < box.Min.Y) || (Min.Y > box.Max.Y))
			{
				return IntersectionType.NoIntersection;
			}
			if ((Max.Z < box.Min.Z) || (Min.Z > box.Max.Z))
			{
				return IntersectionType.NoIntersection;
			}
			if ((((Min.X <= box.Min.X) && (box.Max.X <= Max.X)) 
			     && ((Min.Y <= box.Min.Y) && (box.Max.Y <= Max.Y))
			     && ((Min.Z <= box.Min.Z) && (box.Max.Z <= Max.Z))))
			{
				return IntersectionType.Contained;
			}

			return IntersectionType.Intersects;
		}

		/// <summary>
		/// Checks whether the current <see cref="BoundingBox" /> intersects with the given <see cref="BoundingSphere" />.
		/// </summary>
		/// <param name="sphere">the <see cref="BoundingSphere" /> to check for intersection</param>
		/// <returns>an enumeration value describing the type of intersection between the box and sphere</returns>
		public IntersectionType Intersects(ref BoundingSphere sphere)
		{
			Vector3 clampedCenter = sphere.Center.Clamp(ref Min, ref Max);
			float dist = sphere.Center.GetDistanceSquared(ref clampedCenter);
			float radius = sphere.Radius;

			if (dist > (radius * radius))
			{
				return IntersectionType.NoIntersection;
			}

			if (((((Min.X + radius) <= sphere.Center.X) && 
			      (sphere.Center.X <= (Max.X - radius))) && 
			     (((Max.X - Min.X) > radius) && ((Min.Y + radius) <= sphere.Center.Y))) && 
			    (((sphere.Center.Y <= (Max.Y - radius)) && ((Max.Y - Min.Y) > radius)) && 
			     ((((Min.Z + radius) <= sphere.Center.Z) && (sphere.Center.Z <= (Max.Z - radius))) && 
			      (Max.X - Min.X) > radius)))
			{
				return IntersectionType.Contained;
			}

			return IntersectionType.Intersects;
		}

		/// <summary>
		/// Checks whether the <see cref="BoundingBox" /> contains the given <see cref="BoundingBox" />.
		/// </summary>
		/// <param name="box">the <see cref="BoundingBox" /> to check for containment.</param>
		/// <returns>true if the <see cref="BoundingBox" /> is contained; false otherwise</returns>
		public bool Contains(ref BoundingBox box)
		{
			return (box.Min.X > Min.X && box.Min.Y > Min.Y && box.Min.Z > Min.Z)
			       && (box.Max.X < Max.X && box.Max.Y < Max.Y && box.Max.Z < Max.Z);
		}

		/// <summary>
		/// Checks whether the <see cref="BoundingBox" /> contains the given point.
		/// </summary>
		/// <param name="point">the point to check for containment.</param>
		/// <returns>true if the point is contained; false otherwise</returns>
		public bool Contains(ref Vector3 point)
		{
			return (Min.X <= point.X && point.X <= Max.X)
			       && (Min.Y <= point.Y && point.Y <= Max.Y) 
			       && (Min.Z <= point.Z && point.Z <= Max.Z);
		}

		/// <summary>
		/// Checks whether the <see cref="BoundingBox" /> contains the given point.
		/// </summary>
		/// <param name="point">the point to check for containment.</param>
		/// <returns>true if the point is contained; false otherwise</returns>
		public bool Contains(ref Vector4 point)
		{
			return (Min.X <= point.X && point.X <= Max.X)
			       && (Min.Y <= point.Y && point.Y <= Max.Y)
			       && (Min.Z <= point.Z && point.Z <= Max.Z);
		}

		/// <summary>
		/// Checks equality of two boxes.
		/// </summary>
		/// <param name="other">the other box to compare with</param>
		/// <returns>true if both boxes are equal; false otherwise</returns>
		public bool Equals(BoundingBox other)
		{
			return ((Min == other.Min) && (Max == other.Max));
		}

		/// <summary>
		/// Checks equality with another object.
		/// </summary>
		/// <param name="obj">the object to compare</param>
		/// <returns>true if the object is <see cref="BoundingBox" /> and is equal; false otherwise</returns>
		public override bool Equals(object obj)
		{
			return obj is BoundingBox && Equals((BoundingBox) obj);
		}

		public override int GetHashCode()
		{
			return (Min.GetHashCode() + Max.GetHashCode());
		}

		public static bool operator ==(BoundingBox a, BoundingBox b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(BoundingBox a, BoundingBox b)
		{
			if (!(a.Min != b.Min))
			{
				return (a.Max != b.Max);
			}

			return true;
		}

		public override string ToString()
		{
			return string.Format("(Min: {0}, Max: {1})", Min, Max);
		}

        public float? Intersects(Ray ray)
        {
            float num = 0f;
            float maxValue = float.MaxValue;
            if (Math.Abs(ray.Direction.X) < 1E-06f)
            {
                if ((ray.Position.X < this.Min.X) || (ray.Position.X > this.Max.X))
                {
                    return null;
                }
            }
            else
            {
                float num11 = 1f / ray.Direction.X;
                float num8 = (this.Min.X - ray.Position.X) * num11;
                float num7 = (this.Max.X - ray.Position.X) * num11;
                if (num8 > num7)
                {
                    float num14 = num8;
                    num8 = num7;
                    num7 = num14;
                }
                num = Math.Max(num8, num);
                maxValue = Math.Min(num7, maxValue);
                if (num > maxValue)
                {
                    return null;
                }
            }
            if (Math.Abs(ray.Direction.Y) < 1E-06f)
            {
                if ((ray.Position.Y < this.Min.Y) || (ray.Position.Y > this.Max.Y))
                {
                    return null;
                }
            }
            else
            {
                float num10 = 1f / ray.Direction.Y;
                float num6 = (this.Min.Y - ray.Position.Y) * num10;
                float num5 = (this.Max.Y - ray.Position.Y) * num10;
                if (num6 > num5)
                {
                    float num13 = num6;
                    num6 = num5;
                    num5 = num13;
                }
                num = Math.Max(num6, num);
                maxValue = Math.Min(num5, maxValue);
                if (num > maxValue)
                {
                    return null;
                }
            }
            if (Math.Abs(ray.Direction.Z) < 1E-06f)
            {
                if ((ray.Position.Z < this.Min.Z) || (ray.Position.Z > this.Max.Z))
                {
                    return null;
                }
            }
            else
            {
                float num9 = 1f / ray.Direction.Z;
                float num4 = (this.Min.Z - ray.Position.Z) * num9;
                float num3 = (this.Max.Z - ray.Position.Z) * num9;
                if (num4 > num3)
                {
                    float num12 = num4;
                    num4 = num3;
                    num3 = num12;
                }
                num = Math.Max(num4, num);
                maxValue = Math.Min(num3, maxValue);
                if (num > maxValue)
                {
                    return null;
                }
            }
            return new float?(num);
        }

        public void Intersects(ref Ray ray, out float? result)
        {
            result = 0;
            float num = 0f;
            float maxValue = float.MaxValue;
            if (Math.Abs(ray.Direction.X) < 1E-06f)
            {
                if ((ray.Position.X < this.Min.X) || (ray.Position.X > this.Max.X))
                {
                    return;
                }
            }
            else
            {
                float num11 = 1f / ray.Direction.X;
                float num8 = (this.Min.X - ray.Position.X) * num11;
                float num7 = (this.Max.X - ray.Position.X) * num11;
                if (num8 > num7)
                {
                    float num14 = num8;
                    num8 = num7;
                    num7 = num14;
                }
                num = Math.Max(num8, num);
                maxValue = Math.Min(num7, maxValue);
                if (num > maxValue)
                {
                    return;
                }
            }
            if (Math.Abs(ray.Direction.Y) < 1E-06f)
            {
                if ((ray.Position.Y < this.Min.Y) || (ray.Position.Y > this.Max.Y))
                {
                    return;
                }
            }
            else
            {
                float num10 = 1f / ray.Direction.Y;
                float num6 = (this.Min.Y - ray.Position.Y) * num10;
                float num5 = (this.Max.Y - ray.Position.Y) * num10;
                if (num6 > num5)
                {
                    float num13 = num6;
                    num6 = num5;
                    num5 = num13;
                }
                num = Math.Max(num6, num);
                maxValue = Math.Min(num5, maxValue);
                if (num > maxValue)
                {
                    return;
                }
            }
            if (Math.Abs(ray.Direction.Z) < 1E-06f)
            {
                if ((ray.Position.Z < this.Min.Z) || (ray.Position.Z > this.Max.Z))
                {
                    return;
                }
            }
            else
            {
                float num9 = 1f / ray.Direction.Z;
                float num4 = (this.Min.Z - ray.Position.Z) * num9;
                float num3 = (this.Max.Z - ray.Position.Z) * num9;
                if (num4 > num3)
                {
                    float num12 = num4;
                    num4 = num3;
                    num3 = num12;
                }
                num = Math.Max(num4, num);
                maxValue = Math.Min(num3, maxValue);
                if (num > maxValue)
                {
                    return;
                }
            }
            result = new float?(num);
        }

        public static BoundingBox Join(ref BoundingBox a, ref BoundingBox b)
        {
            var newMin = Vector3.Min(a.Min, b.Min);
            var newMax = Vector3.Max(a.Max, b.Max);

            return new BoundingBox(newMin, newMax);
        }
	}
}