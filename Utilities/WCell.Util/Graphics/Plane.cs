using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace WCell.Util.Graphics
{
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public struct Plane : IEquatable<Plane>
    {
        public Vector3 Normal;
        public float D;
        public Plane(float a, float b, float c, float d)
        {
            this.Normal.X = a;
            this.Normal.Y = b;
            this.Normal.Z = c;
            this.D = d;
        }

        public Plane(Vector3 normal, float d)
        {
            this.Normal = normal;
            this.D = d;
        }

        public Plane(Vector4 value)
        {
            this.Normal.X = value.X;
            this.Normal.Y = value.Y;
            this.Normal.Z = value.Z;
            this.D = value.W;
        }

        public Plane(Vector3 point1, Vector3 point2, Vector3 point3)
        {
            var oneToTwo = point2 - point1;
            var oneToThree = point3 - point1;
        	Normal = Vector3.Cross(oneToTwo, oneToThree);
        	Normal.Normalize();

            D = -Vector3.Dot(Normal, point1);
        }

        public bool Equals(Plane other)
        {
            return ((((this.Normal.X == other.Normal.X) && (this.Normal.Y == other.Normal.Y)) && (this.Normal.Z == other.Normal.Z)) && (this.D == other.D));
        }

        public override bool Equals(object obj)
        {
            bool flag = false;
            if (obj is Plane)
            {
                flag = this.Equals((Plane)obj);
            }
            return flag;
        }

        public override int GetHashCode()
        {
            return (this.Normal.GetHashCode() + this.D.GetHashCode());
        }

        public override string ToString()
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            return string.Format(currentCulture, "{{Normal:{0} D:{1}}}", new object[] { this.Normal.ToString(), this.D.ToString(currentCulture) });
        }

        public void Normalize()
        {
            float num2 = ((this.Normal.X * this.Normal.X) + (this.Normal.Y * this.Normal.Y)) + (this.Normal.Z * this.Normal.Z);
            if (Math.Abs((float)(num2 - 1f)) >= 1.192093E-07f)
            {
                float num = 1f / ((float)Math.Sqrt((double)num2));
                this.Normal.X *= num;
                this.Normal.Y *= num;
                this.Normal.Z *= num;
                this.D *= num;
            }
        }

        public static Plane Normalize(Plane value)
        {
            Plane plane;
            float num2 = ((value.Normal.X * value.Normal.X) + (value.Normal.Y * value.Normal.Y)) + (value.Normal.Z * value.Normal.Z);
            if (Math.Abs((float)(num2 - 1f)) < 1.192093E-07f)
            {
                plane.Normal = value.Normal;
                plane.D = value.D;
                return plane;
            }
            float num = 1f / ((float)Math.Sqrt((double)num2));
            plane.Normal.X = value.Normal.X * num;
            plane.Normal.Y = value.Normal.Y * num;
            plane.Normal.Z = value.Normal.Z * num;
            plane.D = value.D * num;
            return plane;
        }

        public static void Normalize(ref Plane value, out Plane result)
        {
            float num2 = ((value.Normal.X * value.Normal.X) + (value.Normal.Y * value.Normal.Y)) + (value.Normal.Z * value.Normal.Z);
            if (Math.Abs((float)(num2 - 1f)) < 1.192093E-07f)
            {
                result.Normal = value.Normal;
                result.D = value.D;
            }
            else
            {
                float num = 1f / ((float)Math.Sqrt((double)num2));
                result.Normal.X = value.Normal.X * num;
                result.Normal.Y = value.Normal.Y * num;
                result.Normal.Z = value.Normal.Z * num;
                result.D = value.D * num;
            }
        }

        public static Plane Transform(Plane plane, Matrix matrix)
        {
            Plane plane2;
            Matrix matrix2;
            Matrix.Invert(ref matrix, out matrix2);
            float x = plane.Normal.X;
            float y = plane.Normal.Y;
            float z = plane.Normal.Z;
            float d = plane.D;
            plane2.Normal.X = (((x * matrix2.M11) + (y * matrix2.M12)) + (z * matrix2.M13)) + (d * matrix2.M14);
            plane2.Normal.Y = (((x * matrix2.M21) + (y * matrix2.M22)) + (z * matrix2.M23)) + (d * matrix2.M24);
            plane2.Normal.Z = (((x * matrix2.M31) + (y * matrix2.M32)) + (z * matrix2.M33)) + (d * matrix2.M34);
            plane2.D = (((x * matrix2.M41) + (y * matrix2.M42)) + (z * matrix2.M43)) + (d * matrix2.M44);
            return plane2;
        }

        public static void Transform(ref Plane plane, ref Matrix matrix, out Plane result)
        {
            Matrix matrix2;
            Matrix.Invert(ref matrix, out matrix2);
            float x = plane.Normal.X;
            float y = plane.Normal.Y;
            float z = plane.Normal.Z;
            float d = plane.D;
            result.Normal.X = (((x * matrix2.M11) + (y * matrix2.M12)) + (z * matrix2.M13)) + (d * matrix2.M14);
            result.Normal.Y = (((x * matrix2.M21) + (y * matrix2.M22)) + (z * matrix2.M23)) + (d * matrix2.M24);
            result.Normal.Z = (((x * matrix2.M31) + (y * matrix2.M32)) + (z * matrix2.M33)) + (d * matrix2.M34);
            result.D = (((x * matrix2.M41) + (y * matrix2.M42)) + (z * matrix2.M43)) + (d * matrix2.M44);
        }

        public static Plane Transform(Plane plane, Quaternion rotation)
        {
            Plane plane2;
            float num15 = rotation.X + rotation.X;
            float num5 = rotation.Y + rotation.Y;
            float num = rotation.Z + rotation.Z;
            float num14 = rotation.W * num15;
            float num13 = rotation.W * num5;
            float num12 = rotation.W * num;
            float num11 = rotation.X * num15;
            float num10 = rotation.X * num5;
            float num9 = rotation.X * num;
            float num8 = rotation.Y * num5;
            float num7 = rotation.Y * num;
            float num6 = rotation.Z * num;
            float num24 = (1f - num8) - num6;
            float num23 = num10 - num12;
            float num22 = num9 + num13;
            float num21 = num10 + num12;
            float num20 = (1f - num11) - num6;
            float num19 = num7 - num14;
            float num18 = num9 - num13;
            float num17 = num7 + num14;
            float num16 = (1f - num11) - num8;
            float x = plane.Normal.X;
            float y = plane.Normal.Y;
            float z = plane.Normal.Z;
            plane2.Normal.X = ((x * num24) + (y * num23)) + (z * num22);
            plane2.Normal.Y = ((x * num21) + (y * num20)) + (z * num19);
            plane2.Normal.Z = ((x * num18) + (y * num17)) + (z * num16);
            plane2.D = plane.D;
            return plane2;
        }

        public static void Transform(ref Plane plane, ref Quaternion rotation, out Plane result)
        {
            float num15 = rotation.X + rotation.X;
            float num5 = rotation.Y + rotation.Y;
            float num = rotation.Z + rotation.Z;
            float num14 = rotation.W * num15;
            float num13 = rotation.W * num5;
            float num12 = rotation.W * num;
            float num11 = rotation.X * num15;
            float num10 = rotation.X * num5;
            float num9 = rotation.X * num;
            float num8 = rotation.Y * num5;
            float num7 = rotation.Y * num;
            float num6 = rotation.Z * num;
            float num24 = (1f - num8) - num6;
            float num23 = num10 - num12;
            float num22 = num9 + num13;
            float num21 = num10 + num12;
            float num20 = (1f - num11) - num6;
            float num19 = num7 - num14;
            float num18 = num9 - num13;
            float num17 = num7 + num14;
            float num16 = (1f - num11) - num8;
            float x = plane.Normal.X;
            float y = plane.Normal.Y;
            float z = plane.Normal.Z;
            result.Normal.X = ((x * num24) + (y * num23)) + (z * num22);
            result.Normal.Y = ((x * num21) + (y * num20)) + (z * num19);
            result.Normal.Z = ((x * num18) + (y * num17)) + (z * num16);
            result.D = plane.D;
        }

        public float Dot(Vector4 value)
        {
            return ((((this.Normal.X * value.X) + (this.Normal.Y * value.Y)) + (this.Normal.Z * value.Z)) + (this.D * value.W));
        }

        public void Dot(ref Vector4 value, out float result)
        {
            result = (((this.Normal.X * value.X) + (this.Normal.Y * value.Y)) + (this.Normal.Z * value.Z)) + (this.D * value.W);
        }

        public float DotCoordinate(Vector3 value)
        {
            return ((((this.Normal.X * value.X) + (this.Normal.Y * value.Y)) + (this.Normal.Z * value.Z)) + this.D);
        }

        public void DotCoordinate(ref Vector3 value, out float result)
        {
            result = (((this.Normal.X * value.X) + (this.Normal.Y * value.Y)) + (this.Normal.Z * value.Z)) + this.D;
        }

        public float DotNormal(Vector3 value)
        {
            return (((this.Normal.X * value.X) + (this.Normal.Y * value.Y)) + (this.Normal.Z * value.Z));
        }

        public void DotNormal(ref Vector3 value, out float result)
        {
            result = ((this.Normal.X * value.X) + (this.Normal.Y * value.Y)) + (this.Normal.Z * value.Z);
        }

        public PlaneIntersectionType Intersects(BoundingBox box)
        {
            Vector3 vector;
            Vector3 vector2;
            vector2.X = (this.Normal.X >= 0f) ? box.Min.X : box.Max.X;
            vector2.Y = (this.Normal.Y >= 0f) ? box.Min.Y : box.Max.Y;
            vector2.Z = (this.Normal.Z >= 0f) ? box.Min.Z : box.Max.Z;
            vector.X = (this.Normal.X >= 0f) ? box.Max.X : box.Min.X;
            vector.Y = (this.Normal.Y >= 0f) ? box.Max.Y : box.Min.Y;
            vector.Z = (this.Normal.Z >= 0f) ? box.Max.Z : box.Min.Z;
            float num = ((this.Normal.X * vector2.X) + (this.Normal.Y * vector2.Y)) + (this.Normal.Z * vector2.Z);
            if ((num + this.D) > 0f)
            {
                return PlaneIntersectionType.Front;
            }
            num = ((this.Normal.X * vector.X) + (this.Normal.Y * vector.Y)) + (this.Normal.Z * vector.Z);
            if ((num + this.D) < 0f)
            {
                return PlaneIntersectionType.Back;
            }
            return PlaneIntersectionType.Intersecting;
        }

        public void Intersects(ref BoundingBox box, out PlaneIntersectionType result)
        {
            Vector3 vector;
            Vector3 vector2;
            vector2.X = (this.Normal.X >= 0f) ? box.Min.X : box.Max.X;
            vector2.Y = (this.Normal.Y >= 0f) ? box.Min.Y : box.Max.Y;
            vector2.Z = (this.Normal.Z >= 0f) ? box.Min.Z : box.Max.Z;
            vector.X = (this.Normal.X >= 0f) ? box.Max.X : box.Min.X;
            vector.Y = (this.Normal.Y >= 0f) ? box.Max.Y : box.Min.Y;
            vector.Z = (this.Normal.Z >= 0f) ? box.Max.Z : box.Min.Z;
            float num = ((this.Normal.X * vector2.X) + (this.Normal.Y * vector2.Y)) + (this.Normal.Z * vector2.Z);
            if ((num + this.D) > 0f)
            {
                result = PlaneIntersectionType.Front;
            }
            else
            {
                num = ((this.Normal.X * vector.X) + (this.Normal.Y * vector.Y)) + (this.Normal.Z * vector.Z);
                if ((num + this.D) < 0f)
                {
                    result = PlaneIntersectionType.Back;
                }
                else
                {
                    result = PlaneIntersectionType.Intersecting;
                }
            }
        }

        public PlaneIntersectionType Intersects(BoundingSphere sphere)
        {
            float num2 = ((sphere.Center.X * this.Normal.X) + (sphere.Center.Y * this.Normal.Y)) + (sphere.Center.Z * this.Normal.Z);
            float num = num2 + this.D;
            if (num > sphere.Radius)
            {
                return PlaneIntersectionType.Front;
            }
            if (num < -sphere.Radius)
            {
                return PlaneIntersectionType.Back;
            }
            return PlaneIntersectionType.Intersecting;
        }

        public void Intersects(ref BoundingSphere sphere, out PlaneIntersectionType result)
        {
            float num2 = ((sphere.Center.X * this.Normal.X) + (sphere.Center.Y * this.Normal.Y)) + (sphere.Center.Z * this.Normal.Z);
            float num = num2 + this.D;
            if (num > sphere.Radius)
            {
                result = PlaneIntersectionType.Front;
            }
            else if (num < -sphere.Radius)
            {
                result = PlaneIntersectionType.Back;
            }
            else
            {
                result = PlaneIntersectionType.Intersecting;
            }
        }

        public static bool operator ==(Plane lhs, Plane rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Plane lhs, Plane rhs)
        {
            if (((lhs.Normal.X == rhs.Normal.X) && (lhs.Normal.Y == rhs.Normal.Y)) && (lhs.Normal.Z == rhs.Normal.Z))
            {
                return (lhs.D != rhs.D);
            }
            return true;
        }
    }

    public enum PlaneIntersectionType
    {
        Front,
        Back,
        Intersecting
    }
}