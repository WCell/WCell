using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace WCell.Util.Graphics
{
    [StructLayout(LayoutKind.Sequential, Size = 16*4)]
    public struct Matrix : IEquatable<Matrix>
    {
        public float M11;
        public float M12;
        public float M13;
        public float M14;
        public float M21;
        public float M22;
        public float M23;
        public float M24;
        public float M31;
        public float M32;
        public float M33;
        public float M34;
        public float M41;
        public float M42;
        public float M43;
        public float M44;
        private static Matrix _identity;

        public static Matrix Identity
        {
            get
            {
                return _identity;
            }
        }

        public Vector3 Up
        {
            get
            {
                Vector3 vector;
                vector.X = this.M21;
                vector.Y = this.M22;
                vector.Z = this.M23;
                return vector;
            }
            set
            {
                this.M21 = value.X;
                this.M22 = value.Y;
                this.M23 = value.Z;
            }
        }

        public Vector3 Down
        {
            get
            {
                Vector3 vector;
                vector.X = -this.M21;
                vector.Y = -this.M22;
                vector.Z = -this.M23;
                return vector;
            }
            set
            {
                this.M21 = -value.X;
                this.M22 = -value.Y;
                this.M23 = -value.Z;
            }
        }

        public Vector3 Right
        {
            get
            {
                Vector3 vector;
                vector.X = this.M11;
                vector.Y = this.M12;
                vector.Z = this.M13;
                return vector;
            }
            set
            {
                this.M11 = value.X;
                this.M12 = value.Y;
                this.M13 = value.Z;
            }
        }

        public Vector3 Left
        {
            get
            {
                Vector3 vector;
                vector.X = -this.M11;
                vector.Y = -this.M12;
                vector.Z = -this.M13;
                return vector;
            }
            set
            {
                this.M11 = -value.X;
                this.M12 = -value.Y;
                this.M13 = -value.Z;
            }
        }

        public Vector3 Forward
        {
            get
            {
                Vector3 vector;
                vector.X = -this.M31;
                vector.Y = -this.M32;
                vector.Z = -this.M33;
                return vector;
            }
            set
            {
                this.M31 = -value.X;
                this.M32 = -value.Y;
                this.M33 = -value.Z;
            }

        }

        public Vector3 Backward
        {
            get
            {
                Vector3 vector;
                vector.X = this.M31;
                vector.Y = this.M32;
                vector.Z = this.M33;
                return vector;
            }
            set
            {
                this.M31 = value.X;
                this.M32 = value.Y;
                this.M33 = value.Z;
            }
        }

        public Vector3 Translation
        {
            get
            {
                Vector3 vector;
                vector.X = this.M41;
                vector.Y = this.M42;
                vector.Z = this.M43;
                return vector;
            }
            set
            {
                this.M41 = value.X;
                this.M42 = value.Y;
                this.M43 = value.Z;
            }
        }

        public Matrix(float m11, float m12, float m13, float m14, float m21, float m22, float m23, float m24, float m31, float m32, float m33, float m34, float m41, float m42, float m43, float m44)
        {
            this.M11 = m11;
            this.M12 = m12;
            this.M13 = m13;
            this.M14 = m14;
            this.M21 = m21;
            this.M22 = m22;
            this.M23 = m23;
            this.M24 = m24;
            this.M31 = m31;
            this.M32 = m32;
            this.M33 = m33;
            this.M34 = m34;
            this.M41 = m41;
            this.M42 = m42;
            this.M43 = m43;
            this.M44 = m44;
        }

        public static Matrix CreateTranslation(Vector3 position)
        {
            Matrix matrix;
            matrix.M11 = 1f;
            matrix.M12 = 0f;
            matrix.M13 = 0f;
            matrix.M14 = 0f;
            matrix.M21 = 0f;
            matrix.M22 = 1f;
            matrix.M23 = 0f;
            matrix.M24 = 0f;
            matrix.M31 = 0f;
            matrix.M32 = 0f;
            matrix.M33 = 1f;
            matrix.M34 = 0f;
            matrix.M41 = position.X;
            matrix.M42 = position.Y;
            matrix.M43 = position.Z;
            matrix.M44 = 1f;
            return matrix;
        }

        public static void CreateTranslation(ref Vector3 position, out Matrix result)
        {
            result.M11 = 1f;
            result.M12 = 0f;
            result.M13 = 0f;
            result.M14 = 0f;
            result.M21 = 0f;
            result.M22 = 1f;
            result.M23 = 0f;
            result.M24 = 0f;
            result.M31 = 0f;
            result.M32 = 0f;
            result.M33 = 1f;
            result.M34 = 0f;
            result.M41 = position.X;
            result.M42 = position.Y;
            result.M43 = position.Z;
            result.M44 = 1f;
        }

        public static Matrix CreateTranslation(float xPosition, float yPosition, float zPosition)
        {
            Matrix matrix;
            matrix.M11 = 1f;
            matrix.M12 = 0f;
            matrix.M13 = 0f;
            matrix.M14 = 0f;
            matrix.M21 = 0f;
            matrix.M22 = 1f;
            matrix.M23 = 0f;
            matrix.M24 = 0f;
            matrix.M31 = 0f;
            matrix.M32 = 0f;
            matrix.M33 = 1f;
            matrix.M34 = 0f;
            matrix.M41 = xPosition;
            matrix.M42 = yPosition;
            matrix.M43 = zPosition;
            matrix.M44 = 1f;
            return matrix;
        }

        public static void CreateTranslation(float xPosition, float yPosition, float zPosition, out Matrix result)
        {
            result.M11 = 1f;
            result.M12 = 0f;
            result.M13 = 0f;
            result.M14 = 0f;
            result.M21 = 0f;
            result.M22 = 1f;
            result.M23 = 0f;
            result.M24 = 0f;
            result.M31 = 0f;
            result.M32 = 0f;
            result.M33 = 1f;
            result.M34 = 0f;
            result.M41 = xPosition;
            result.M42 = yPosition;
            result.M43 = zPosition;
            result.M44 = 1f;
        }

        public static Matrix CreateScale(float xScale, float yScale, float zScale)
        {
            Matrix matrix;
            float num3 = xScale;
            float num2 = yScale;
            float num = zScale;
            matrix.M11 = num3;
            matrix.M12 = 0f;
            matrix.M13 = 0f;
            matrix.M14 = 0f;
            matrix.M21 = 0f;
            matrix.M22 = num2;
            matrix.M23 = 0f;
            matrix.M24 = 0f;
            matrix.M31 = 0f;
            matrix.M32 = 0f;
            matrix.M33 = num;
            matrix.M34 = 0f;
            matrix.M41 = 0f;
            matrix.M42 = 0f;
            matrix.M43 = 0f;
            matrix.M44 = 1f;
            return matrix;
        }

        public static void CreateScale(float xScale, float yScale, float zScale, out Matrix result)
        {
            float num3 = xScale;
            float num2 = yScale;
            float num = zScale;
            result.M11 = num3;
            result.M12 = 0f;
            result.M13 = 0f;
            result.M14 = 0f;
            result.M21 = 0f;
            result.M22 = num2;
            result.M23 = 0f;
            result.M24 = 0f;
            result.M31 = 0f;
            result.M32 = 0f;
            result.M33 = num;
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
        }

        public static Matrix CreateScale(Vector3 scales)
        {
            Matrix matrix;
            float x = scales.X;
            float y = scales.Y;
            float z = scales.Z;
            matrix.M11 = x;
            matrix.M12 = 0f;
            matrix.M13 = 0f;
            matrix.M14 = 0f;
            matrix.M21 = 0f;
            matrix.M22 = y;
            matrix.M23 = 0f;
            matrix.M24 = 0f;
            matrix.M31 = 0f;
            matrix.M32 = 0f;
            matrix.M33 = z;
            matrix.M34 = 0f;
            matrix.M41 = 0f;
            matrix.M42 = 0f;
            matrix.M43 = 0f;
            matrix.M44 = 1f;
            return matrix;
        }

        public static void CreateScale(ref Vector3 scales, out Matrix result)
        {
            float x = scales.X;
            float y = scales.Y;
            float z = scales.Z;
            result.M11 = x;
            result.M12 = 0f;
            result.M13 = 0f;
            result.M14 = 0f;
            result.M21 = 0f;
            result.M22 = y;
            result.M23 = 0f;
            result.M24 = 0f;
            result.M31 = 0f;
            result.M32 = 0f;
            result.M33 = z;
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
        }

        public static Matrix CreateScale(float scale)
        {
            Matrix matrix;
            float num = scale;
            matrix.M11 = num;
            matrix.M12 = 0f;
            matrix.M13 = 0f;
            matrix.M14 = 0f;
            matrix.M21 = 0f;
            matrix.M22 = num;
            matrix.M23 = 0f;
            matrix.M24 = 0f;
            matrix.M31 = 0f;
            matrix.M32 = 0f;
            matrix.M33 = num;
            matrix.M34 = 0f;
            matrix.M41 = 0f;
            matrix.M42 = 0f;
            matrix.M43 = 0f;
            matrix.M44 = 1f;
            return matrix;
        }

        public static void CreateScale(float scale, out Matrix result)
        {
            float num = scale;
            result.M11 = num;
            result.M12 = 0f;
            result.M13 = 0f;
            result.M14 = 0f;
            result.M21 = 0f;
            result.M22 = num;
            result.M23 = 0f;
            result.M24 = 0f;
            result.M31 = 0f;
            result.M32 = 0f;
            result.M33 = num;
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
        }

        public static Matrix CreateRotationX(float radians)
        {
            Matrix matrix;
            float num2 = (float)Math.Cos((double)radians);
            float num = (float)Math.Sin((double)radians);
            matrix.M11 = 1f;
            matrix.M12 = 0f;
            matrix.M13 = 0f;
            matrix.M14 = 0f;
            matrix.M21 = 0f;
            matrix.M22 = num2;
            matrix.M23 = num;
            matrix.M24 = 0f;
            matrix.M31 = 0f;
            matrix.M32 = -num;
            matrix.M33 = num2;
            matrix.M34 = 0f;
            matrix.M41 = 0f;
            matrix.M42 = 0f;
            matrix.M43 = 0f;
            matrix.M44 = 1f;
            return matrix;
        }

        public static void CreateRotationX(float radians, out Matrix result)
        {
            float num2 = (float)Math.Cos((double)radians);
            float num = (float)Math.Sin((double)radians);
            result.M11 = 1f;
            result.M12 = 0f;
            result.M13 = 0f;
            result.M14 = 0f;
            result.M21 = 0f;
            result.M22 = num2;
            result.M23 = num;
            result.M24 = 0f;
            result.M31 = 0f;
            result.M32 = -num;
            result.M33 = num2;
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
        }

        public static Matrix CreateRotationY(float radians)
        {
            Matrix matrix;
            var num2 = (float)Math.Cos(radians);
            var num = (float)Math.Sin(radians);

            matrix.M11 = num2;
            matrix.M12 = 0f;
            matrix.M13 = -num;
            matrix.M14 = 0f;
            matrix.M21 = 0f;
            matrix.M22 = 1f;
            matrix.M23 = 0f;
            matrix.M24 = 0f;
            matrix.M31 = num;
            matrix.M32 = 0f;
            matrix.M33 = num2;
            matrix.M34 = 0f;
            matrix.M41 = 0f;
            matrix.M42 = 0f;
            matrix.M43 = 0f;
            matrix.M44 = 1f;
            return matrix;
        }

        public static void CreateRotationY(float radians, out Matrix result)
        {
            var num2 = (float)Math.Cos(radians);
            var num = (float)Math.Sin(radians);

            result.M11 = num2;
            result.M12 = 0f;
            result.M13 = -num;
            result.M14 = 0f;
            result.M21 = 0f;
            result.M22 = 1f;
            result.M23 = 0f;
            result.M24 = 0f;
            result.M31 = num;
            result.M32 = 0f;
            result.M33 = num2;
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
        }

        public static Matrix CreateRotationZ(float radians)
        {
            Matrix matrix;
            float num2 = (float)Math.Cos((double)radians);
            float num = (float)Math.Sin((double)radians);
            matrix.M11 = num2;
            matrix.M12 = num;
            matrix.M13 = 0f;
            matrix.M14 = 0f;
            matrix.M21 = -num;
            matrix.M22 = num2;
            matrix.M23 = 0f;
            matrix.M24 = 0f;
            matrix.M31 = 0f;
            matrix.M32 = 0f;
            matrix.M33 = 1f;
            matrix.M34 = 0f;
            matrix.M41 = 0f;
            matrix.M42 = 0f;
            matrix.M43 = 0f;
            matrix.M44 = 1f;
            return matrix;
        }

        public static void CreateRotationZ(float radians, out Matrix result)
        {
            float num2 = (float)Math.Cos((double)radians);
            float num = (float)Math.Sin((double)radians);
            result.M11 = num2;
            result.M12 = num;
            result.M13 = 0f;
            result.M14 = 0f;
            result.M21 = -num;
            result.M22 = num2;
            result.M23 = 0f;
            result.M24 = 0f;
            result.M31 = 0f;
            result.M32 = 0f;
            result.M33 = 1f;
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
        }

        public static Matrix CreateFromAxisAngle(Vector3 axis, float angle)
        {
            Matrix matrix;
            float x = axis.X;
            float y = axis.Y;
            float z = axis.Z;
            float num2 = (float)Math.Sin((double)angle);
            float num = (float)Math.Cos((double)angle);
            float num11 = x * x;
            float num10 = y * y;
            float num9 = z * z;
            float num8 = x * y;
            float num7 = x * z;
            float num6 = y * z;
            matrix.M11 = num11 + (num * (1f - num11));
            matrix.M12 = (num8 - (num * num8)) + (num2 * z);
            matrix.M13 = (num7 - (num * num7)) - (num2 * y);
            matrix.M14 = 0f;
            matrix.M21 = (num8 - (num * num8)) - (num2 * z);
            matrix.M22 = num10 + (num * (1f - num10));
            matrix.M23 = (num6 - (num * num6)) + (num2 * x);
            matrix.M24 = 0f;
            matrix.M31 = (num7 - (num * num7)) + (num2 * y);
            matrix.M32 = (num6 - (num * num6)) - (num2 * x);
            matrix.M33 = num9 + (num * (1f - num9));
            matrix.M34 = 0f;
            matrix.M41 = 0f;
            matrix.M42 = 0f;
            matrix.M43 = 0f;
            matrix.M44 = 1f;
            return matrix;
        }

        public static void CreateFromAxisAngle(ref Vector3 axis, float angle, out Matrix result)
        {
            float x = axis.X;
            float y = axis.Y;
            float z = axis.Z;
            float num2 = (float)Math.Sin((double)angle);
            float num = (float)Math.Cos((double)angle);
            float num11 = x * x;
            float num10 = y * y;
            float num9 = z * z;
            float num8 = x * y;
            float num7 = x * z;
            float num6 = y * z;
            result.M11 = num11 + (num * (1f - num11));
            result.M12 = (num8 - (num * num8)) + (num2 * z);
            result.M13 = (num7 - (num * num7)) - (num2 * y);
            result.M14 = 0f;
            result.M21 = (num8 - (num * num8)) - (num2 * z);
            result.M22 = num10 + (num * (1f - num10));
            result.M23 = (num6 - (num * num6)) + (num2 * x);
            result.M24 = 0f;
            result.M31 = (num7 - (num * num7)) + (num2 * y);
            result.M32 = (num6 - (num * num6)) - (num2 * x);
            result.M33 = num9 + (num * (1f - num9));
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
        }

        public static Matrix CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane)
        {
            Matrix matrix;
            matrix.M11 = 2f / width;
            matrix.M12 = matrix.M13 = matrix.M14 = 0f;
            matrix.M22 = 2f / height;
            matrix.M21 = matrix.M23 = matrix.M24 = 0f;
            matrix.M33 = 1f / (zNearPlane - zFarPlane);
            matrix.M31 = matrix.M32 = matrix.M34 = 0f;
            matrix.M41 = matrix.M42 = 0f;
            matrix.M43 = zNearPlane / (zNearPlane - zFarPlane);
            matrix.M44 = 1f;
            return matrix;
        }

        public static void CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane, out Matrix result)
        {
            result.M11 = 2f / width;
            result.M12 = result.M13 = result.M14 = 0f;
            result.M22 = 2f / height;
            result.M21 = result.M23 = result.M24 = 0f;
            result.M33 = 1f / (zNearPlane - zFarPlane);
            result.M31 = result.M32 = result.M34 = 0f;
            result.M41 = result.M42 = 0f;
            result.M43 = zNearPlane / (zNearPlane - zFarPlane);
            result.M44 = 1f;
        }

        public static Matrix CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane)
        {
            Matrix matrix;
            matrix.M11 = 2f / (right - left);
            matrix.M12 = matrix.M13 = matrix.M14 = 0f;
            matrix.M22 = 2f / (top - bottom);
            matrix.M21 = matrix.M23 = matrix.M24 = 0f;
            matrix.M33 = 1f / (zNearPlane - zFarPlane);
            matrix.M31 = matrix.M32 = matrix.M34 = 0f;
            matrix.M41 = (left + right) / (left - right);
            matrix.M42 = (top + bottom) / (bottom - top);
            matrix.M43 = zNearPlane / (zNearPlane - zFarPlane);
            matrix.M44 = 1f;
            return matrix;
        }

        public static void CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane, out Matrix result)
        {
            result.M11 = 2f / (right - left);
            result.M12 = result.M13 = result.M14 = 0f;
            result.M22 = 2f / (top - bottom);
            result.M21 = result.M23 = result.M24 = 0f;
            result.M33 = 1f / (zNearPlane - zFarPlane);
            result.M31 = result.M32 = result.M34 = 0f;
            result.M41 = (left + right) / (left - right);
            result.M42 = (top + bottom) / (bottom - top);
            result.M43 = zNearPlane / (zNearPlane - zFarPlane);
            result.M44 = 1f;
        }

        public static Matrix CreateLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
        {
            Matrix matrix;
            Vector3 vector = Vector3.Normalize(cameraPosition - cameraTarget);
            Vector3 vector2 = Vector3.Normalize(Vector3.Cross(cameraUpVector, vector));
            Vector3 vector3 = Vector3.Cross(vector, vector2);
            matrix.M11 = vector2.X;
            matrix.M12 = vector3.X;
            matrix.M13 = vector.X;
            matrix.M14 = 0f;
            matrix.M21 = vector2.Y;
            matrix.M22 = vector3.Y;
            matrix.M23 = vector.Y;
            matrix.M24 = 0f;
            matrix.M31 = vector2.Z;
            matrix.M32 = vector3.Z;
            matrix.M33 = vector.Z;
            matrix.M34 = 0f;
            matrix.M41 = -Vector3.Dot(vector2, cameraPosition);
            matrix.M42 = -Vector3.Dot(vector3, cameraPosition);
            matrix.M43 = -Vector3.Dot(vector, cameraPosition);
            matrix.M44 = 1f;
            return matrix;
        }

        public static void CreateLookAt(ref Vector3 cameraPosition, ref Vector3 cameraTarget, ref Vector3 cameraUpVector, out Matrix result)
        {
            Vector3 vector = Vector3.Normalize(cameraPosition - cameraTarget);
            Vector3 vector2 = Vector3.Normalize(Vector3.Cross(cameraUpVector, vector));
            Vector3 vector3 = Vector3.Cross(vector, vector2);
            result.M11 = vector2.X;
            result.M12 = vector3.X;
            result.M13 = vector.X;
            result.M14 = 0f;
            result.M21 = vector2.Y;
            result.M22 = vector3.Y;
            result.M23 = vector.Y;
            result.M24 = 0f;
            result.M31 = vector2.Z;
            result.M32 = vector3.Z;
            result.M33 = vector.Z;
            result.M34 = 0f;
            result.M41 = -Vector3.Dot(vector2, cameraPosition);
            result.M42 = -Vector3.Dot(vector3, cameraPosition);
            result.M43 = -Vector3.Dot(vector, cameraPosition);
            result.M44 = 1f;
        }

        public static Matrix CreateWorld(Vector3 position, Vector3 forward, Vector3 up)
        {
            Matrix matrix;
            Vector3 vector = Vector3.Normalize(-forward);
            Vector3 vector2 = Vector3.Normalize(Vector3.Cross(up, vector));
            Vector3 vector3 = Vector3.Cross(vector, vector2);
            matrix.M11 = vector2.X;
            matrix.M12 = vector2.Y;
            matrix.M13 = vector2.Z;
            matrix.M14 = 0f;
            matrix.M21 = vector3.X;
            matrix.M22 = vector3.Y;
            matrix.M23 = vector3.Z;
            matrix.M24 = 0f;
            matrix.M31 = vector.X;
            matrix.M32 = vector.Y;
            matrix.M33 = vector.Z;
            matrix.M34 = 0f;
            matrix.M41 = position.X;
            matrix.M42 = position.Y;
            matrix.M43 = position.Z;
            matrix.M44 = 1f;
            return matrix;
        }

        public static void CreateWorld(ref Vector3 position, ref Vector3 forward, ref Vector3 up, out Matrix result)
        {
            Vector3 vector = Vector3.Normalize(-forward);
            Vector3 vector2 = Vector3.Normalize(Vector3.Cross(up, vector));
            Vector3 vector3 = Vector3.Cross(vector, vector2);
            result.M11 = vector2.X;
            result.M12 = vector2.Y;
            result.M13 = vector2.Z;
            result.M14 = 0f;
            result.M21 = vector3.X;
            result.M22 = vector3.Y;
            result.M23 = vector3.Z;
            result.M24 = 0f;
            result.M31 = vector.X;
            result.M32 = vector.Y;
            result.M33 = vector.Z;
            result.M34 = 0f;
            result.M41 = position.X;
            result.M42 = position.Y;
            result.M43 = position.Z;
            result.M44 = 1f;
        }

        public static Matrix CreateFromQuaternion(Quaternion quaternion)
        {
            Matrix matrix;
            float num9 = quaternion.X * quaternion.X;
            float num8 = quaternion.Y * quaternion.Y;
            float num7 = quaternion.Z * quaternion.Z;
            float num6 = quaternion.X * quaternion.Y;
            float num5 = quaternion.Z * quaternion.W;
            float num4 = quaternion.Z * quaternion.X;
            float num3 = quaternion.Y * quaternion.W;
            float num2 = quaternion.Y * quaternion.Z;
            float num = quaternion.X * quaternion.W;
            matrix.M11 = 1f - (2f * (num8 + num7));
            matrix.M12 = 2f * (num6 + num5);
            matrix.M13 = 2f * (num4 - num3);
            matrix.M14 = 0f;
            matrix.M21 = 2f * (num6 - num5);
            matrix.M22 = 1f - (2f * (num7 + num9));
            matrix.M23 = 2f * (num2 + num);
            matrix.M24 = 0f;
            matrix.M31 = 2f * (num4 + num3);
            matrix.M32 = 2f * (num2 - num);
            matrix.M33 = 1f - (2f * (num8 + num9));
            matrix.M34 = 0f;
            matrix.M41 = 0f;
            matrix.M42 = 0f;
            matrix.M43 = 0f;
            matrix.M44 = 1f;
            return matrix;
        }

        public static void CreateFromQuaternion(ref Quaternion quaternion, out Matrix result)
        {
            float num9 = quaternion.X * quaternion.X;
            float num8 = quaternion.Y * quaternion.Y;
            float num7 = quaternion.Z * quaternion.Z;
            float num6 = quaternion.X * quaternion.Y;
            float num5 = quaternion.Z * quaternion.W;
            float num4 = quaternion.Z * quaternion.X;
            float num3 = quaternion.Y * quaternion.W;
            float num2 = quaternion.Y * quaternion.Z;
            float num = quaternion.X * quaternion.W;
            result.M11 = 1f - (2f * (num8 + num7));
            result.M12 = 2f * (num6 + num5);
            result.M13 = 2f * (num4 - num3);
            result.M14 = 0f;
            result.M21 = 2f * (num6 - num5);
            result.M22 = 1f - (2f * (num7 + num9));
            result.M23 = 2f * (num2 + num);
            result.M24 = 0f;
            result.M31 = 2f * (num4 + num3);
            result.M32 = 2f * (num2 - num);
            result.M33 = 1f - (2f * (num8 + num9));
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
        }

        public static Matrix CreateFromYawPitchRoll(float yaw, float pitch, float roll)
        {
            Matrix matrix;
            Quaternion quaternion;
            Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll, out quaternion);
            CreateFromQuaternion(ref quaternion, out matrix);
            return matrix;
        }

        public static void CreateFromYawPitchRoll(float yaw, float pitch, float roll, out Matrix result)
        {
            Quaternion quaternion;
            Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll, out quaternion);
            CreateFromQuaternion(ref quaternion, out result);
        }
        
        public static Matrix Transform(Matrix value, Quaternion rotation)
        {
            Matrix matrix;
            float num21 = rotation.X + rotation.X;
            float num11 = rotation.Y + rotation.Y;
            float num10 = rotation.Z + rotation.Z;
            float num20 = rotation.W * num21;
            float num19 = rotation.W * num11;
            float num18 = rotation.W * num10;
            float num17 = rotation.X * num21;
            float num16 = rotation.X * num11;
            float num15 = rotation.X * num10;
            float num14 = rotation.Y * num11;
            float num13 = rotation.Y * num10;
            float num12 = rotation.Z * num10;
            float num9 = (1f - num14) - num12;
            float num8 = num16 - num18;
            float num7 = num15 + num19;
            float num6 = num16 + num18;
            float num5 = (1f - num17) - num12;
            float num4 = num13 - num20;
            float num3 = num15 - num19;
            float num2 = num13 + num20;
            float num = (1f - num17) - num14;
            matrix.M11 = ((value.M11 * num9) + (value.M12 * num8)) + (value.M13 * num7);
            matrix.M12 = ((value.M11 * num6) + (value.M12 * num5)) + (value.M13 * num4);
            matrix.M13 = ((value.M11 * num3) + (value.M12 * num2)) + (value.M13 * num);
            matrix.M14 = value.M14;
            matrix.M21 = ((value.M21 * num9) + (value.M22 * num8)) + (value.M23 * num7);
            matrix.M22 = ((value.M21 * num6) + (value.M22 * num5)) + (value.M23 * num4);
            matrix.M23 = ((value.M21 * num3) + (value.M22 * num2)) + (value.M23 * num);
            matrix.M24 = value.M24;
            matrix.M31 = ((value.M31 * num9) + (value.M32 * num8)) + (value.M33 * num7);
            matrix.M32 = ((value.M31 * num6) + (value.M32 * num5)) + (value.M33 * num4);
            matrix.M33 = ((value.M31 * num3) + (value.M32 * num2)) + (value.M33 * num);
            matrix.M34 = value.M34;
            matrix.M41 = ((value.M41 * num9) + (value.M42 * num8)) + (value.M43 * num7);
            matrix.M42 = ((value.M41 * num6) + (value.M42 * num5)) + (value.M43 * num4);
            matrix.M43 = ((value.M41 * num3) + (value.M42 * num2)) + (value.M43 * num);
            matrix.M44 = value.M44;
            return matrix;
        }

        public static void Transform(ref Matrix value, ref Quaternion rotation, out Matrix result)
        {
            float num21 = rotation.X + rotation.X;
            float num11 = rotation.Y + rotation.Y;
            float num10 = rotation.Z + rotation.Z;
            float num20 = rotation.W * num21;
            float num19 = rotation.W * num11;
            float num18 = rotation.W * num10;
            float num17 = rotation.X * num21;
            float num16 = rotation.X * num11;
            float num15 = rotation.X * num10;
            float num14 = rotation.Y * num11;
            float num13 = rotation.Y * num10;
            float num12 = rotation.Z * num10;
            float num9 = (1f - num14) - num12;
            float num8 = num16 - num18;
            float num7 = num15 + num19;
            float num6 = num16 + num18;
            float num5 = (1f - num17) - num12;
            float num4 = num13 - num20;
            float num3 = num15 - num19;
            float num2 = num13 + num20;
            float num = (1f - num17) - num14;
            float num37 = ((value.M11 * num9) + (value.M12 * num8)) + (value.M13 * num7);
            float num36 = ((value.M11 * num6) + (value.M12 * num5)) + (value.M13 * num4);
            float num35 = ((value.M11 * num3) + (value.M12 * num2)) + (value.M13 * num);
            float num34 = value.M14;
            float num33 = ((value.M21 * num9) + (value.M22 * num8)) + (value.M23 * num7);
            float num32 = ((value.M21 * num6) + (value.M22 * num5)) + (value.M23 * num4);
            float num31 = ((value.M21 * num3) + (value.M22 * num2)) + (value.M23 * num);
            float num30 = value.M24;
            float num29 = ((value.M31 * num9) + (value.M32 * num8)) + (value.M33 * num7);
            float num28 = ((value.M31 * num6) + (value.M32 * num5)) + (value.M33 * num4);
            float num27 = ((value.M31 * num3) + (value.M32 * num2)) + (value.M33 * num);
            float num26 = value.M34;
            float num25 = ((value.M41 * num9) + (value.M42 * num8)) + (value.M43 * num7);
            float num24 = ((value.M41 * num6) + (value.M42 * num5)) + (value.M43 * num4);
            float num23 = ((value.M41 * num3) + (value.M42 * num2)) + (value.M43 * num);
            float num22 = value.M44;
            result.M11 = num37;
            result.M12 = num36;
            result.M13 = num35;
            result.M14 = num34;
            result.M21 = num33;
            result.M22 = num32;
            result.M23 = num31;
            result.M24 = num30;
            result.M31 = num29;
            result.M32 = num28;
            result.M33 = num27;
            result.M34 = num26;
            result.M41 = num25;
            result.M42 = num24;
            result.M43 = num23;
            result.M44 = num22;
        }

        public override string ToString()
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            return ("{ " + string.Format(currentCulture, "{{M11:{0} M12:{1} M13:{2} M14:{3}}} ", new object[] { this.M11.ToString(currentCulture), this.M12.ToString(currentCulture), this.M13.ToString(currentCulture), this.M14.ToString(currentCulture) }) + string.Format(currentCulture, "{{M21:{0} M22:{1} M23:{2} M24:{3}}} ", new object[] { this.M21.ToString(currentCulture), this.M22.ToString(currentCulture), this.M23.ToString(currentCulture), this.M24.ToString(currentCulture) }) + string.Format(currentCulture, "{{M31:{0} M32:{1} M33:{2} M34:{3}}} ", new object[] { this.M31.ToString(currentCulture), this.M32.ToString(currentCulture), this.M33.ToString(currentCulture), this.M34.ToString(currentCulture) }) + string.Format(currentCulture, "{{M41:{0} M42:{1} M43:{2} M44:{3}}} ", new object[] { this.M41.ToString(currentCulture), this.M42.ToString(currentCulture), this.M43.ToString(currentCulture), this.M44.ToString(currentCulture) }) + "}");
        }

        public bool Equals(Matrix other)
        {
            return ((((((this.M11 == other.M11) && (this.M22 == other.M22)) && ((this.M33 == other.M33) && (this.M44 == other.M44))) && (((this.M12 == other.M12) && (this.M13 == other.M13)) && ((this.M14 == other.M14) && (this.M21 == other.M21)))) && ((((this.M23 == other.M23) && (this.M24 == other.M24)) && ((this.M31 == other.M31) && (this.M32 == other.M32))) && (((this.M34 == other.M34) && (this.M41 == other.M41)) && (this.M42 == other.M42)))) && (this.M43 == other.M43));
        }

        public override bool Equals(object obj)
        {
            bool flag = false;
            if (obj is Matrix)
            {
                flag = this.Equals((Matrix)obj);
            }
            return flag;
        }

        public override int GetHashCode()
        {
            return (((((((((((((((this.M11.GetHashCode() + this.M12.GetHashCode()) + this.M13.GetHashCode()) + this.M14.GetHashCode()) + this.M21.GetHashCode()) + this.M22.GetHashCode()) + this.M23.GetHashCode()) + this.M24.GetHashCode()) + this.M31.GetHashCode()) + this.M32.GetHashCode()) + this.M33.GetHashCode()) + this.M34.GetHashCode()) + this.M41.GetHashCode()) + this.M42.GetHashCode()) + this.M43.GetHashCode()) + this.M44.GetHashCode());
        }

        public static Matrix Transpose(Matrix matrix)
        {
            Matrix matrix2;
            matrix2.M11 = matrix.M11;
            matrix2.M12 = matrix.M21;
            matrix2.M13 = matrix.M31;
            matrix2.M14 = matrix.M41;
            matrix2.M21 = matrix.M12;
            matrix2.M22 = matrix.M22;
            matrix2.M23 = matrix.M32;
            matrix2.M24 = matrix.M42;
            matrix2.M31 = matrix.M13;
            matrix2.M32 = matrix.M23;
            matrix2.M33 = matrix.M33;
            matrix2.M34 = matrix.M43;
            matrix2.M41 = matrix.M14;
            matrix2.M42 = matrix.M24;
            matrix2.M43 = matrix.M34;
            matrix2.M44 = matrix.M44;
            return matrix2;
        }

        public static void Transpose(ref Matrix matrix, out Matrix result)
        {
            float num16 = matrix.M11;
            float num15 = matrix.M12;
            float num14 = matrix.M13;
            float num13 = matrix.M14;
            float num12 = matrix.M21;
            float num11 = matrix.M22;
            float num10 = matrix.M23;
            float num9 = matrix.M24;
            float num8 = matrix.M31;
            float num7 = matrix.M32;
            float num6 = matrix.M33;
            float num5 = matrix.M34;
            float num4 = matrix.M41;
            float num3 = matrix.M42;
            float num2 = matrix.M43;
            float num = matrix.M44;
            result.M11 = num16;
            result.M12 = num12;
            result.M13 = num8;
            result.M14 = num4;
            result.M21 = num15;
            result.M22 = num11;
            result.M23 = num7;
            result.M24 = num3;
            result.M31 = num14;
            result.M32 = num10;
            result.M33 = num6;
            result.M34 = num2;
            result.M41 = num13;
            result.M42 = num9;
            result.M43 = num5;
            result.M44 = num;
        }

        public float Determinant()
        {
            float num22 = this.M11;
            float num21 = this.M12;
            float num20 = this.M13;
            float num19 = this.M14;
            float num12 = this.M21;
            float num11 = this.M22;
            float num10 = this.M23;
            float num9 = this.M24;
            float num8 = this.M31;
            float num7 = this.M32;
            float num6 = this.M33;
            float num5 = this.M34;
            float num4 = this.M41;
            float num3 = this.M42;
            float num2 = this.M43;
            float num = this.M44;
            float num18 = (num6 * num) - (num5 * num2);
            float num17 = (num7 * num) - (num5 * num3);
            float num16 = (num7 * num2) - (num6 * num3);
            float num15 = (num8 * num) - (num5 * num4);
            float num14 = (num8 * num2) - (num6 * num4);
            float num13 = (num8 * num3) - (num7 * num4);
            return ((((num22 * (((num11 * num18) - (num10 * num17)) + (num9 * num16))) - (num21 * (((num12 * num18) - (num10 * num15)) + (num9 * num14)))) + (num20 * (((num12 * num17) - (num11 * num15)) + (num9 * num13)))) - (num19 * (((num12 * num16) - (num11 * num14)) + (num10 * num13))));
        }

        public static Matrix Invert(Matrix matrix)
        {
            Matrix matrix2;
            float num5 = matrix.M11;
            float num4 = matrix.M12;
            float num3 = matrix.M13;
            float num2 = matrix.M14;
            float num9 = matrix.M21;
            float num8 = matrix.M22;
            float num7 = matrix.M23;
            float num6 = matrix.M24;
            float num17 = matrix.M31;
            float num16 = matrix.M32;
            float num15 = matrix.M33;
            float num14 = matrix.M34;
            float num13 = matrix.M41;
            float num12 = matrix.M42;
            float num11 = matrix.M43;
            float num10 = matrix.M44;
            float num23 = (num15 * num10) - (num14 * num11);
            float num22 = (num16 * num10) - (num14 * num12);
            float num21 = (num16 * num11) - (num15 * num12);
            float num20 = (num17 * num10) - (num14 * num13);
            float num19 = (num17 * num11) - (num15 * num13);
            float num18 = (num17 * num12) - (num16 * num13);
            float num39 = ((num8 * num23) - (num7 * num22)) + (num6 * num21);
            float num38 = -(((num9 * num23) - (num7 * num20)) + (num6 * num19));
            float num37 = ((num9 * num22) - (num8 * num20)) + (num6 * num18);
            float num36 = -(((num9 * num21) - (num8 * num19)) + (num7 * num18));
            float num = 1f / ((((num5 * num39) + (num4 * num38)) + (num3 * num37)) + (num2 * num36));
            matrix2.M11 = num39 * num;
            matrix2.M21 = num38 * num;
            matrix2.M31 = num37 * num;
            matrix2.M41 = num36 * num;
            matrix2.M12 = -(((num4 * num23) - (num3 * num22)) + (num2 * num21)) * num;
            matrix2.M22 = (((num5 * num23) - (num3 * num20)) + (num2 * num19)) * num;
            matrix2.M32 = -(((num5 * num22) - (num4 * num20)) + (num2 * num18)) * num;
            matrix2.M42 = (((num5 * num21) - (num4 * num19)) + (num3 * num18)) * num;
            float num35 = (num7 * num10) - (num6 * num11);
            float num34 = (num8 * num10) - (num6 * num12);
            float num33 = (num8 * num11) - (num7 * num12);
            float num32 = (num9 * num10) - (num6 * num13);
            float num31 = (num9 * num11) - (num7 * num13);
            float num30 = (num9 * num12) - (num8 * num13);
            matrix2.M13 = (((num4 * num35) - (num3 * num34)) + (num2 * num33)) * num;
            matrix2.M23 = -(((num5 * num35) - (num3 * num32)) + (num2 * num31)) * num;
            matrix2.M33 = (((num5 * num34) - (num4 * num32)) + (num2 * num30)) * num;
            matrix2.M43 = -(((num5 * num33) - (num4 * num31)) + (num3 * num30)) * num;
            float num29 = (num7 * num14) - (num6 * num15);
            float num28 = (num8 * num14) - (num6 * num16);
            float num27 = (num8 * num15) - (num7 * num16);
            float num26 = (num9 * num14) - (num6 * num17);
            float num25 = (num9 * num15) - (num7 * num17);
            float num24 = (num9 * num16) - (num8 * num17);
            matrix2.M14 = -(((num4 * num29) - (num3 * num28)) + (num2 * num27)) * num;
            matrix2.M24 = (((num5 * num29) - (num3 * num26)) + (num2 * num25)) * num;
            matrix2.M34 = -(((num5 * num28) - (num4 * num26)) + (num2 * num24)) * num;
            matrix2.M44 = (((num5 * num27) - (num4 * num25)) + (num3 * num24)) * num;
            return matrix2;
        }

        public static void Invert(ref Matrix matrix, out Matrix result)
        {
            float num5 = matrix.M11;
            float num4 = matrix.M12;
            float num3 = matrix.M13;
            float num2 = matrix.M14;
            float num9 = matrix.M21;
            float num8 = matrix.M22;
            float num7 = matrix.M23;
            float num6 = matrix.M24;
            float num17 = matrix.M31;
            float num16 = matrix.M32;
            float num15 = matrix.M33;
            float num14 = matrix.M34;
            float num13 = matrix.M41;
            float num12 = matrix.M42;
            float num11 = matrix.M43;
            float num10 = matrix.M44;
            float num23 = (num15 * num10) - (num14 * num11);
            float num22 = (num16 * num10) - (num14 * num12);
            float num21 = (num16 * num11) - (num15 * num12);
            float num20 = (num17 * num10) - (num14 * num13);
            float num19 = (num17 * num11) - (num15 * num13);
            float num18 = (num17 * num12) - (num16 * num13);
            float num39 = ((num8 * num23) - (num7 * num22)) + (num6 * num21);
            float num38 = -(((num9 * num23) - (num7 * num20)) + (num6 * num19));
            float num37 = ((num9 * num22) - (num8 * num20)) + (num6 * num18);
            float num36 = -(((num9 * num21) - (num8 * num19)) + (num7 * num18));
            float num = 1f / ((((num5 * num39) + (num4 * num38)) + (num3 * num37)) + (num2 * num36));
            result.M11 = num39 * num;
            result.M21 = num38 * num;
            result.M31 = num37 * num;
            result.M41 = num36 * num;
            result.M12 = -(((num4 * num23) - (num3 * num22)) + (num2 * num21)) * num;
            result.M22 = (((num5 * num23) - (num3 * num20)) + (num2 * num19)) * num;
            result.M32 = -(((num5 * num22) - (num4 * num20)) + (num2 * num18)) * num;
            result.M42 = (((num5 * num21) - (num4 * num19)) + (num3 * num18)) * num;
            float num35 = (num7 * num10) - (num6 * num11);
            float num34 = (num8 * num10) - (num6 * num12);
            float num33 = (num8 * num11) - (num7 * num12);
            float num32 = (num9 * num10) - (num6 * num13);
            float num31 = (num9 * num11) - (num7 * num13);
            float num30 = (num9 * num12) - (num8 * num13);
            result.M13 = (((num4 * num35) - (num3 * num34)) + (num2 * num33)) * num;
            result.M23 = -(((num5 * num35) - (num3 * num32)) + (num2 * num31)) * num;
            result.M33 = (((num5 * num34) - (num4 * num32)) + (num2 * num30)) * num;
            result.M43 = -(((num5 * num33) - (num4 * num31)) + (num3 * num30)) * num;
            float num29 = (num7 * num14) - (num6 * num15);
            float num28 = (num8 * num14) - (num6 * num16);
            float num27 = (num8 * num15) - (num7 * num16);
            float num26 = (num9 * num14) - (num6 * num17);
            float num25 = (num9 * num15) - (num7 * num17);
            float num24 = (num9 * num16) - (num8 * num17);
            result.M14 = -(((num4 * num29) - (num3 * num28)) + (num2 * num27)) * num;
            result.M24 = (((num5 * num29) - (num3 * num26)) + (num2 * num25)) * num;
            result.M34 = -(((num5 * num28) - (num4 * num26)) + (num2 * num24)) * num;
            result.M44 = (((num5 * num27) - (num4 * num25)) + (num3 * num24)) * num;
        }

        public static Matrix Lerp(Matrix matrix1, Matrix matrix2, float amount)
        {
            Matrix matrix;
            matrix.M11 = matrix1.M11 + ((matrix2.M11 - matrix1.M11) * amount);
            matrix.M12 = matrix1.M12 + ((matrix2.M12 - matrix1.M12) * amount);
            matrix.M13 = matrix1.M13 + ((matrix2.M13 - matrix1.M13) * amount);
            matrix.M14 = matrix1.M14 + ((matrix2.M14 - matrix1.M14) * amount);
            matrix.M21 = matrix1.M21 + ((matrix2.M21 - matrix1.M21) * amount);
            matrix.M22 = matrix1.M22 + ((matrix2.M22 - matrix1.M22) * amount);
            matrix.M23 = matrix1.M23 + ((matrix2.M23 - matrix1.M23) * amount);
            matrix.M24 = matrix1.M24 + ((matrix2.M24 - matrix1.M24) * amount);
            matrix.M31 = matrix1.M31 + ((matrix2.M31 - matrix1.M31) * amount);
            matrix.M32 = matrix1.M32 + ((matrix2.M32 - matrix1.M32) * amount);
            matrix.M33 = matrix1.M33 + ((matrix2.M33 - matrix1.M33) * amount);
            matrix.M34 = matrix1.M34 + ((matrix2.M34 - matrix1.M34) * amount);
            matrix.M41 = matrix1.M41 + ((matrix2.M41 - matrix1.M41) * amount);
            matrix.M42 = matrix1.M42 + ((matrix2.M42 - matrix1.M42) * amount);
            matrix.M43 = matrix1.M43 + ((matrix2.M43 - matrix1.M43) * amount);
            matrix.M44 = matrix1.M44 + ((matrix2.M44 - matrix1.M44) * amount);
            return matrix;
        }

        public static void Lerp(ref Matrix matrix1, ref Matrix matrix2, float amount, out Matrix result)
        {
            result.M11 = matrix1.M11 + ((matrix2.M11 - matrix1.M11) * amount);
            result.M12 = matrix1.M12 + ((matrix2.M12 - matrix1.M12) * amount);
            result.M13 = matrix1.M13 + ((matrix2.M13 - matrix1.M13) * amount);
            result.M14 = matrix1.M14 + ((matrix2.M14 - matrix1.M14) * amount);
            result.M21 = matrix1.M21 + ((matrix2.M21 - matrix1.M21) * amount);
            result.M22 = matrix1.M22 + ((matrix2.M22 - matrix1.M22) * amount);
            result.M23 = matrix1.M23 + ((matrix2.M23 - matrix1.M23) * amount);
            result.M24 = matrix1.M24 + ((matrix2.M24 - matrix1.M24) * amount);
            result.M31 = matrix1.M31 + ((matrix2.M31 - matrix1.M31) * amount);
            result.M32 = matrix1.M32 + ((matrix2.M32 - matrix1.M32) * amount);
            result.M33 = matrix1.M33 + ((matrix2.M33 - matrix1.M33) * amount);
            result.M34 = matrix1.M34 + ((matrix2.M34 - matrix1.M34) * amount);
            result.M41 = matrix1.M41 + ((matrix2.M41 - matrix1.M41) * amount);
            result.M42 = matrix1.M42 + ((matrix2.M42 - matrix1.M42) * amount);
            result.M43 = matrix1.M43 + ((matrix2.M43 - matrix1.M43) * amount);
            result.M44 = matrix1.M44 + ((matrix2.M44 - matrix1.M44) * amount);
        }

        public static Matrix Negate(Matrix matrix)
        {
            Matrix matrix2;
            matrix2.M11 = -matrix.M11;
            matrix2.M12 = -matrix.M12;
            matrix2.M13 = -matrix.M13;
            matrix2.M14 = -matrix.M14;
            matrix2.M21 = -matrix.M21;
            matrix2.M22 = -matrix.M22;
            matrix2.M23 = -matrix.M23;
            matrix2.M24 = -matrix.M24;
            matrix2.M31 = -matrix.M31;
            matrix2.M32 = -matrix.M32;
            matrix2.M33 = -matrix.M33;
            matrix2.M34 = -matrix.M34;
            matrix2.M41 = -matrix.M41;
            matrix2.M42 = -matrix.M42;
            matrix2.M43 = -matrix.M43;
            matrix2.M44 = -matrix.M44;
            return matrix2;
        }

        public static void Negate(ref Matrix matrix, out Matrix result)
        {
            result.M11 = -matrix.M11;
            result.M12 = -matrix.M12;
            result.M13 = -matrix.M13;
            result.M14 = -matrix.M14;
            result.M21 = -matrix.M21;
            result.M22 = -matrix.M22;
            result.M23 = -matrix.M23;
            result.M24 = -matrix.M24;
            result.M31 = -matrix.M31;
            result.M32 = -matrix.M32;
            result.M33 = -matrix.M33;
            result.M34 = -matrix.M34;
            result.M41 = -matrix.M41;
            result.M42 = -matrix.M42;
            result.M43 = -matrix.M43;
            result.M44 = -matrix.M44;
        }

        public static Matrix Add(Matrix matrix1, Matrix matrix2)
        {
            Matrix matrix;
            matrix.M11 = matrix1.M11 + matrix2.M11;
            matrix.M12 = matrix1.M12 + matrix2.M12;
            matrix.M13 = matrix1.M13 + matrix2.M13;
            matrix.M14 = matrix1.M14 + matrix2.M14;
            matrix.M21 = matrix1.M21 + matrix2.M21;
            matrix.M22 = matrix1.M22 + matrix2.M22;
            matrix.M23 = matrix1.M23 + matrix2.M23;
            matrix.M24 = matrix1.M24 + matrix2.M24;
            matrix.M31 = matrix1.M31 + matrix2.M31;
            matrix.M32 = matrix1.M32 + matrix2.M32;
            matrix.M33 = matrix1.M33 + matrix2.M33;
            matrix.M34 = matrix1.M34 + matrix2.M34;
            matrix.M41 = matrix1.M41 + matrix2.M41;
            matrix.M42 = matrix1.M42 + matrix2.M42;
            matrix.M43 = matrix1.M43 + matrix2.M43;
            matrix.M44 = matrix1.M44 + matrix2.M44;
            return matrix;
        }

        public static void Add(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
        {
            result.M11 = matrix1.M11 + matrix2.M11;
            result.M12 = matrix1.M12 + matrix2.M12;
            result.M13 = matrix1.M13 + matrix2.M13;
            result.M14 = matrix1.M14 + matrix2.M14;
            result.M21 = matrix1.M21 + matrix2.M21;
            result.M22 = matrix1.M22 + matrix2.M22;
            result.M23 = matrix1.M23 + matrix2.M23;
            result.M24 = matrix1.M24 + matrix2.M24;
            result.M31 = matrix1.M31 + matrix2.M31;
            result.M32 = matrix1.M32 + matrix2.M32;
            result.M33 = matrix1.M33 + matrix2.M33;
            result.M34 = matrix1.M34 + matrix2.M34;
            result.M41 = matrix1.M41 + matrix2.M41;
            result.M42 = matrix1.M42 + matrix2.M42;
            result.M43 = matrix1.M43 + matrix2.M43;
            result.M44 = matrix1.M44 + matrix2.M44;
        }

        public static Matrix Subtract(Matrix matrix1, Matrix matrix2)
        {
            Matrix matrix;
            matrix.M11 = matrix1.M11 - matrix2.M11;
            matrix.M12 = matrix1.M12 - matrix2.M12;
            matrix.M13 = matrix1.M13 - matrix2.M13;
            matrix.M14 = matrix1.M14 - matrix2.M14;
            matrix.M21 = matrix1.M21 - matrix2.M21;
            matrix.M22 = matrix1.M22 - matrix2.M22;
            matrix.M23 = matrix1.M23 - matrix2.M23;
            matrix.M24 = matrix1.M24 - matrix2.M24;
            matrix.M31 = matrix1.M31 - matrix2.M31;
            matrix.M32 = matrix1.M32 - matrix2.M32;
            matrix.M33 = matrix1.M33 - matrix2.M33;
            matrix.M34 = matrix1.M34 - matrix2.M34;
            matrix.M41 = matrix1.M41 - matrix2.M41;
            matrix.M42 = matrix1.M42 - matrix2.M42;
            matrix.M43 = matrix1.M43 - matrix2.M43;
            matrix.M44 = matrix1.M44 - matrix2.M44;
            return matrix;
        }

        public static void Subtract(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
        {
            result.M11 = matrix1.M11 - matrix2.M11;
            result.M12 = matrix1.M12 - matrix2.M12;
            result.M13 = matrix1.M13 - matrix2.M13;
            result.M14 = matrix1.M14 - matrix2.M14;
            result.M21 = matrix1.M21 - matrix2.M21;
            result.M22 = matrix1.M22 - matrix2.M22;
            result.M23 = matrix1.M23 - matrix2.M23;
            result.M24 = matrix1.M24 - matrix2.M24;
            result.M31 = matrix1.M31 - matrix2.M31;
            result.M32 = matrix1.M32 - matrix2.M32;
            result.M33 = matrix1.M33 - matrix2.M33;
            result.M34 = matrix1.M34 - matrix2.M34;
            result.M41 = matrix1.M41 - matrix2.M41;
            result.M42 = matrix1.M42 - matrix2.M42;
            result.M43 = matrix1.M43 - matrix2.M43;
            result.M44 = matrix1.M44 - matrix2.M44;
        }

        public static Matrix Multiply(Matrix matrix1, Matrix matrix2)
        {
            Matrix matrix;
            matrix.M11 = (((matrix1.M11 * matrix2.M11) + (matrix1.M12 * matrix2.M21)) + (matrix1.M13 * matrix2.M31)) + (matrix1.M14 * matrix2.M41);
            matrix.M12 = (((matrix1.M11 * matrix2.M12) + (matrix1.M12 * matrix2.M22)) + (matrix1.M13 * matrix2.M32)) + (matrix1.M14 * matrix2.M42);
            matrix.M13 = (((matrix1.M11 * matrix2.M13) + (matrix1.M12 * matrix2.M23)) + (matrix1.M13 * matrix2.M33)) + (matrix1.M14 * matrix2.M43);
            matrix.M14 = (((matrix1.M11 * matrix2.M14) + (matrix1.M12 * matrix2.M24)) + (matrix1.M13 * matrix2.M34)) + (matrix1.M14 * matrix2.M44);
            matrix.M21 = (((matrix1.M21 * matrix2.M11) + (matrix1.M22 * matrix2.M21)) + (matrix1.M23 * matrix2.M31)) + (matrix1.M24 * matrix2.M41);
            matrix.M22 = (((matrix1.M21 * matrix2.M12) + (matrix1.M22 * matrix2.M22)) + (matrix1.M23 * matrix2.M32)) + (matrix1.M24 * matrix2.M42);
            matrix.M23 = (((matrix1.M21 * matrix2.M13) + (matrix1.M22 * matrix2.M23)) + (matrix1.M23 * matrix2.M33)) + (matrix1.M24 * matrix2.M43);
            matrix.M24 = (((matrix1.M21 * matrix2.M14) + (matrix1.M22 * matrix2.M24)) + (matrix1.M23 * matrix2.M34)) + (matrix1.M24 * matrix2.M44);
            matrix.M31 = (((matrix1.M31 * matrix2.M11) + (matrix1.M32 * matrix2.M21)) + (matrix1.M33 * matrix2.M31)) + (matrix1.M34 * matrix2.M41);
            matrix.M32 = (((matrix1.M31 * matrix2.M12) + (matrix1.M32 * matrix2.M22)) + (matrix1.M33 * matrix2.M32)) + (matrix1.M34 * matrix2.M42);
            matrix.M33 = (((matrix1.M31 * matrix2.M13) + (matrix1.M32 * matrix2.M23)) + (matrix1.M33 * matrix2.M33)) + (matrix1.M34 * matrix2.M43);
            matrix.M34 = (((matrix1.M31 * matrix2.M14) + (matrix1.M32 * matrix2.M24)) + (matrix1.M33 * matrix2.M34)) + (matrix1.M34 * matrix2.M44);
            matrix.M41 = (((matrix1.M41 * matrix2.M11) + (matrix1.M42 * matrix2.M21)) + (matrix1.M43 * matrix2.M31)) + (matrix1.M44 * matrix2.M41);
            matrix.M42 = (((matrix1.M41 * matrix2.M12) + (matrix1.M42 * matrix2.M22)) + (matrix1.M43 * matrix2.M32)) + (matrix1.M44 * matrix2.M42);
            matrix.M43 = (((matrix1.M41 * matrix2.M13) + (matrix1.M42 * matrix2.M23)) + (matrix1.M43 * matrix2.M33)) + (matrix1.M44 * matrix2.M43);
            matrix.M44 = (((matrix1.M41 * matrix2.M14) + (matrix1.M42 * matrix2.M24)) + (matrix1.M43 * matrix2.M34)) + (matrix1.M44 * matrix2.M44);
            return matrix;
        }

        public static void Multiply(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
        {
            float num16 = (((matrix1.M11 * matrix2.M11) + (matrix1.M12 * matrix2.M21)) + (matrix1.M13 * matrix2.M31)) + (matrix1.M14 * matrix2.M41);
            float num15 = (((matrix1.M11 * matrix2.M12) + (matrix1.M12 * matrix2.M22)) + (matrix1.M13 * matrix2.M32)) + (matrix1.M14 * matrix2.M42);
            float num14 = (((matrix1.M11 * matrix2.M13) + (matrix1.M12 * matrix2.M23)) + (matrix1.M13 * matrix2.M33)) + (matrix1.M14 * matrix2.M43);
            float num13 = (((matrix1.M11 * matrix2.M14) + (matrix1.M12 * matrix2.M24)) + (matrix1.M13 * matrix2.M34)) + (matrix1.M14 * matrix2.M44);
            float num12 = (((matrix1.M21 * matrix2.M11) + (matrix1.M22 * matrix2.M21)) + (matrix1.M23 * matrix2.M31)) + (matrix1.M24 * matrix2.M41);
            float num11 = (((matrix1.M21 * matrix2.M12) + (matrix1.M22 * matrix2.M22)) + (matrix1.M23 * matrix2.M32)) + (matrix1.M24 * matrix2.M42);
            float num10 = (((matrix1.M21 * matrix2.M13) + (matrix1.M22 * matrix2.M23)) + (matrix1.M23 * matrix2.M33)) + (matrix1.M24 * matrix2.M43);
            float num9 = (((matrix1.M21 * matrix2.M14) + (matrix1.M22 * matrix2.M24)) + (matrix1.M23 * matrix2.M34)) + (matrix1.M24 * matrix2.M44);
            float num8 = (((matrix1.M31 * matrix2.M11) + (matrix1.M32 * matrix2.M21)) + (matrix1.M33 * matrix2.M31)) + (matrix1.M34 * matrix2.M41);
            float num7 = (((matrix1.M31 * matrix2.M12) + (matrix1.M32 * matrix2.M22)) + (matrix1.M33 * matrix2.M32)) + (matrix1.M34 * matrix2.M42);
            float num6 = (((matrix1.M31 * matrix2.M13) + (matrix1.M32 * matrix2.M23)) + (matrix1.M33 * matrix2.M33)) + (matrix1.M34 * matrix2.M43);
            float num5 = (((matrix1.M31 * matrix2.M14) + (matrix1.M32 * matrix2.M24)) + (matrix1.M33 * matrix2.M34)) + (matrix1.M34 * matrix2.M44);
            float num4 = (((matrix1.M41 * matrix2.M11) + (matrix1.M42 * matrix2.M21)) + (matrix1.M43 * matrix2.M31)) + (matrix1.M44 * matrix2.M41);
            float num3 = (((matrix1.M41 * matrix2.M12) + (matrix1.M42 * matrix2.M22)) + (matrix1.M43 * matrix2.M32)) + (matrix1.M44 * matrix2.M42);
            float num2 = (((matrix1.M41 * matrix2.M13) + (matrix1.M42 * matrix2.M23)) + (matrix1.M43 * matrix2.M33)) + (matrix1.M44 * matrix2.M43);
            float num = (((matrix1.M41 * matrix2.M14) + (matrix1.M42 * matrix2.M24)) + (matrix1.M43 * matrix2.M34)) + (matrix1.M44 * matrix2.M44);
            result.M11 = num16;
            result.M12 = num15;
            result.M13 = num14;
            result.M14 = num13;
            result.M21 = num12;
            result.M22 = num11;
            result.M23 = num10;
            result.M24 = num9;
            result.M31 = num8;
            result.M32 = num7;
            result.M33 = num6;
            result.M34 = num5;
            result.M41 = num4;
            result.M42 = num3;
            result.M43 = num2;
            result.M44 = num;
        }

        public static Matrix Multiply(Matrix matrix1, float scaleFactor)
        {
            Matrix matrix;
            float num = scaleFactor;
            matrix.M11 = matrix1.M11 * num;
            matrix.M12 = matrix1.M12 * num;
            matrix.M13 = matrix1.M13 * num;
            matrix.M14 = matrix1.M14 * num;
            matrix.M21 = matrix1.M21 * num;
            matrix.M22 = matrix1.M22 * num;
            matrix.M23 = matrix1.M23 * num;
            matrix.M24 = matrix1.M24 * num;
            matrix.M31 = matrix1.M31 * num;
            matrix.M32 = matrix1.M32 * num;
            matrix.M33 = matrix1.M33 * num;
            matrix.M34 = matrix1.M34 * num;
            matrix.M41 = matrix1.M41 * num;
            matrix.M42 = matrix1.M42 * num;
            matrix.M43 = matrix1.M43 * num;
            matrix.M44 = matrix1.M44 * num;
            return matrix;
        }

        public static void Multiply(ref Matrix matrix1, float scaleFactor, out Matrix result)
        {
            float num = scaleFactor;
            result.M11 = matrix1.M11 * num;
            result.M12 = matrix1.M12 * num;
            result.M13 = matrix1.M13 * num;
            result.M14 = matrix1.M14 * num;
            result.M21 = matrix1.M21 * num;
            result.M22 = matrix1.M22 * num;
            result.M23 = matrix1.M23 * num;
            result.M24 = matrix1.M24 * num;
            result.M31 = matrix1.M31 * num;
            result.M32 = matrix1.M32 * num;
            result.M33 = matrix1.M33 * num;
            result.M34 = matrix1.M34 * num;
            result.M41 = matrix1.M41 * num;
            result.M42 = matrix1.M42 * num;
            result.M43 = matrix1.M43 * num;
            result.M44 = matrix1.M44 * num;
        }

        public static Matrix Divide(Matrix matrix1, Matrix matrix2)
        {
            Matrix matrix;
            matrix.M11 = matrix1.M11 / matrix2.M11;
            matrix.M12 = matrix1.M12 / matrix2.M12;
            matrix.M13 = matrix1.M13 / matrix2.M13;
            matrix.M14 = matrix1.M14 / matrix2.M14;
            matrix.M21 = matrix1.M21 / matrix2.M21;
            matrix.M22 = matrix1.M22 / matrix2.M22;
            matrix.M23 = matrix1.M23 / matrix2.M23;
            matrix.M24 = matrix1.M24 / matrix2.M24;
            matrix.M31 = matrix1.M31 / matrix2.M31;
            matrix.M32 = matrix1.M32 / matrix2.M32;
            matrix.M33 = matrix1.M33 / matrix2.M33;
            matrix.M34 = matrix1.M34 / matrix2.M34;
            matrix.M41 = matrix1.M41 / matrix2.M41;
            matrix.M42 = matrix1.M42 / matrix2.M42;
            matrix.M43 = matrix1.M43 / matrix2.M43;
            matrix.M44 = matrix1.M44 / matrix2.M44;
            return matrix;
        }

        public static void Divide(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
        {
            result.M11 = matrix1.M11 / matrix2.M11;
            result.M12 = matrix1.M12 / matrix2.M12;
            result.M13 = matrix1.M13 / matrix2.M13;
            result.M14 = matrix1.M14 / matrix2.M14;
            result.M21 = matrix1.M21 / matrix2.M21;
            result.M22 = matrix1.M22 / matrix2.M22;
            result.M23 = matrix1.M23 / matrix2.M23;
            result.M24 = matrix1.M24 / matrix2.M24;
            result.M31 = matrix1.M31 / matrix2.M31;
            result.M32 = matrix1.M32 / matrix2.M32;
            result.M33 = matrix1.M33 / matrix2.M33;
            result.M34 = matrix1.M34 / matrix2.M34;
            result.M41 = matrix1.M41 / matrix2.M41;
            result.M42 = matrix1.M42 / matrix2.M42;
            result.M43 = matrix1.M43 / matrix2.M43;
            result.M44 = matrix1.M44 / matrix2.M44;
        }

        public static Matrix Divide(Matrix matrix1, float divider)
        {
            Matrix matrix;
            float num = 1f / divider;
            matrix.M11 = matrix1.M11 * num;
            matrix.M12 = matrix1.M12 * num;
            matrix.M13 = matrix1.M13 * num;
            matrix.M14 = matrix1.M14 * num;
            matrix.M21 = matrix1.M21 * num;
            matrix.M22 = matrix1.M22 * num;
            matrix.M23 = matrix1.M23 * num;
            matrix.M24 = matrix1.M24 * num;
            matrix.M31 = matrix1.M31 * num;
            matrix.M32 = matrix1.M32 * num;
            matrix.M33 = matrix1.M33 * num;
            matrix.M34 = matrix1.M34 * num;
            matrix.M41 = matrix1.M41 * num;
            matrix.M42 = matrix1.M42 * num;
            matrix.M43 = matrix1.M43 * num;
            matrix.M44 = matrix1.M44 * num;
            return matrix;
        }

        public static void Divide(ref Matrix matrix1, float divider, out Matrix result)
        {
            float num = 1f / divider;
            result.M11 = matrix1.M11 * num;
            result.M12 = matrix1.M12 * num;
            result.M13 = matrix1.M13 * num;
            result.M14 = matrix1.M14 * num;
            result.M21 = matrix1.M21 * num;
            result.M22 = matrix1.M22 * num;
            result.M23 = matrix1.M23 * num;
            result.M24 = matrix1.M24 * num;
            result.M31 = matrix1.M31 * num;
            result.M32 = matrix1.M32 * num;
            result.M33 = matrix1.M33 * num;
            result.M34 = matrix1.M34 * num;
            result.M41 = matrix1.M41 * num;
            result.M42 = matrix1.M42 * num;
            result.M43 = matrix1.M43 * num;
            result.M44 = matrix1.M44 * num;
        }

        public static Matrix operator -(Matrix matrix1)
        {
            Matrix matrix;
            matrix.M11 = -matrix1.M11;
            matrix.M12 = -matrix1.M12;
            matrix.M13 = -matrix1.M13;
            matrix.M14 = -matrix1.M14;
            matrix.M21 = -matrix1.M21;
            matrix.M22 = -matrix1.M22;
            matrix.M23 = -matrix1.M23;
            matrix.M24 = -matrix1.M24;
            matrix.M31 = -matrix1.M31;
            matrix.M32 = -matrix1.M32;
            matrix.M33 = -matrix1.M33;
            matrix.M34 = -matrix1.M34;
            matrix.M41 = -matrix1.M41;
            matrix.M42 = -matrix1.M42;
            matrix.M43 = -matrix1.M43;
            matrix.M44 = -matrix1.M44;
            return matrix;
        }

        public static bool operator ==(Matrix matrix1, Matrix matrix2)
        {
            return ((((((matrix1.M11 == matrix2.M11) && (matrix1.M22 == matrix2.M22)) && ((matrix1.M33 == matrix2.M33) && (matrix1.M44 == matrix2.M44))) && (((matrix1.M12 == matrix2.M12) && (matrix1.M13 == matrix2.M13)) && ((matrix1.M14 == matrix2.M14) && (matrix1.M21 == matrix2.M21)))) && ((((matrix1.M23 == matrix2.M23) && (matrix1.M24 == matrix2.M24)) && ((matrix1.M31 == matrix2.M31) && (matrix1.M32 == matrix2.M32))) && (((matrix1.M34 == matrix2.M34) && (matrix1.M41 == matrix2.M41)) && (matrix1.M42 == matrix2.M42)))) && (matrix1.M43 == matrix2.M43));
        }

        public static bool operator !=(Matrix matrix1, Matrix matrix2)
        {
            if (((((matrix1.M11 == matrix2.M11) && (matrix1.M12 == matrix2.M12)) && ((matrix1.M13 == matrix2.M13) && (matrix1.M14 == matrix2.M14))) && (((matrix1.M21 == matrix2.M21) && (matrix1.M22 == matrix2.M22)) && ((matrix1.M23 == matrix2.M23) && (matrix1.M24 == matrix2.M24)))) && ((((matrix1.M31 == matrix2.M31) && (matrix1.M32 == matrix2.M32)) && ((matrix1.M33 == matrix2.M33) && (matrix1.M34 == matrix2.M34))) && (((matrix1.M41 == matrix2.M41) && (matrix1.M42 == matrix2.M42)) && (matrix1.M43 == matrix2.M43))))
            {
                return (matrix1.M44 != matrix2.M44);
            }
            return true;
        }

        public static Matrix operator +(Matrix matrix1, Matrix matrix2)
        {
            Matrix matrix;
            matrix.M11 = matrix1.M11 + matrix2.M11;
            matrix.M12 = matrix1.M12 + matrix2.M12;
            matrix.M13 = matrix1.M13 + matrix2.M13;
            matrix.M14 = matrix1.M14 + matrix2.M14;
            matrix.M21 = matrix1.M21 + matrix2.M21;
            matrix.M22 = matrix1.M22 + matrix2.M22;
            matrix.M23 = matrix1.M23 + matrix2.M23;
            matrix.M24 = matrix1.M24 + matrix2.M24;
            matrix.M31 = matrix1.M31 + matrix2.M31;
            matrix.M32 = matrix1.M32 + matrix2.M32;
            matrix.M33 = matrix1.M33 + matrix2.M33;
            matrix.M34 = matrix1.M34 + matrix2.M34;
            matrix.M41 = matrix1.M41 + matrix2.M41;
            matrix.M42 = matrix1.M42 + matrix2.M42;
            matrix.M43 = matrix1.M43 + matrix2.M43;
            matrix.M44 = matrix1.M44 + matrix2.M44;
            return matrix;
        }

        public static Matrix operator -(Matrix matrix1, Matrix matrix2)
        {
            Matrix matrix;
            matrix.M11 = matrix1.M11 - matrix2.M11;
            matrix.M12 = matrix1.M12 - matrix2.M12;
            matrix.M13 = matrix1.M13 - matrix2.M13;
            matrix.M14 = matrix1.M14 - matrix2.M14;
            matrix.M21 = matrix1.M21 - matrix2.M21;
            matrix.M22 = matrix1.M22 - matrix2.M22;
            matrix.M23 = matrix1.M23 - matrix2.M23;
            matrix.M24 = matrix1.M24 - matrix2.M24;
            matrix.M31 = matrix1.M31 - matrix2.M31;
            matrix.M32 = matrix1.M32 - matrix2.M32;
            matrix.M33 = matrix1.M33 - matrix2.M33;
            matrix.M34 = matrix1.M34 - matrix2.M34;
            matrix.M41 = matrix1.M41 - matrix2.M41;
            matrix.M42 = matrix1.M42 - matrix2.M42;
            matrix.M43 = matrix1.M43 - matrix2.M43;
            matrix.M44 = matrix1.M44 - matrix2.M44;
            return matrix;
        }

        public static Matrix operator *(Matrix matrix1, Matrix matrix2)
        {
            Matrix matrix;
            matrix.M11 = (((matrix1.M11 * matrix2.M11) + (matrix1.M12 * matrix2.M21)) + (matrix1.M13 * matrix2.M31)) + (matrix1.M14 * matrix2.M41);
            matrix.M12 = (((matrix1.M11 * matrix2.M12) + (matrix1.M12 * matrix2.M22)) + (matrix1.M13 * matrix2.M32)) + (matrix1.M14 * matrix2.M42);
            matrix.M13 = (((matrix1.M11 * matrix2.M13) + (matrix1.M12 * matrix2.M23)) + (matrix1.M13 * matrix2.M33)) + (matrix1.M14 * matrix2.M43);
            matrix.M14 = (((matrix1.M11 * matrix2.M14) + (matrix1.M12 * matrix2.M24)) + (matrix1.M13 * matrix2.M34)) + (matrix1.M14 * matrix2.M44);
            matrix.M21 = (((matrix1.M21 * matrix2.M11) + (matrix1.M22 * matrix2.M21)) + (matrix1.M23 * matrix2.M31)) + (matrix1.M24 * matrix2.M41);
            matrix.M22 = (((matrix1.M21 * matrix2.M12) + (matrix1.M22 * matrix2.M22)) + (matrix1.M23 * matrix2.M32)) + (matrix1.M24 * matrix2.M42);
            matrix.M23 = (((matrix1.M21 * matrix2.M13) + (matrix1.M22 * matrix2.M23)) + (matrix1.M23 * matrix2.M33)) + (matrix1.M24 * matrix2.M43);
            matrix.M24 = (((matrix1.M21 * matrix2.M14) + (matrix1.M22 * matrix2.M24)) + (matrix1.M23 * matrix2.M34)) + (matrix1.M24 * matrix2.M44);
            matrix.M31 = (((matrix1.M31 * matrix2.M11) + (matrix1.M32 * matrix2.M21)) + (matrix1.M33 * matrix2.M31)) + (matrix1.M34 * matrix2.M41);
            matrix.M32 = (((matrix1.M31 * matrix2.M12) + (matrix1.M32 * matrix2.M22)) + (matrix1.M33 * matrix2.M32)) + (matrix1.M34 * matrix2.M42);
            matrix.M33 = (((matrix1.M31 * matrix2.M13) + (matrix1.M32 * matrix2.M23)) + (matrix1.M33 * matrix2.M33)) + (matrix1.M34 * matrix2.M43);
            matrix.M34 = (((matrix1.M31 * matrix2.M14) + (matrix1.M32 * matrix2.M24)) + (matrix1.M33 * matrix2.M34)) + (matrix1.M34 * matrix2.M44);
            matrix.M41 = (((matrix1.M41 * matrix2.M11) + (matrix1.M42 * matrix2.M21)) + (matrix1.M43 * matrix2.M31)) + (matrix1.M44 * matrix2.M41);
            matrix.M42 = (((matrix1.M41 * matrix2.M12) + (matrix1.M42 * matrix2.M22)) + (matrix1.M43 * matrix2.M32)) + (matrix1.M44 * matrix2.M42);
            matrix.M43 = (((matrix1.M41 * matrix2.M13) + (matrix1.M42 * matrix2.M23)) + (matrix1.M43 * matrix2.M33)) + (matrix1.M44 * matrix2.M43);
            matrix.M44 = (((matrix1.M41 * matrix2.M14) + (matrix1.M42 * matrix2.M24)) + (matrix1.M43 * matrix2.M34)) + (matrix1.M44 * matrix2.M44);
            return matrix;
        }

        public static Matrix operator *(Matrix matrix, float scaleFactor)
        {
            Matrix matrix2;
            float num = scaleFactor;
            matrix2.M11 = matrix.M11 * num;
            matrix2.M12 = matrix.M12 * num;
            matrix2.M13 = matrix.M13 * num;
            matrix2.M14 = matrix.M14 * num;
            matrix2.M21 = matrix.M21 * num;
            matrix2.M22 = matrix.M22 * num;
            matrix2.M23 = matrix.M23 * num;
            matrix2.M24 = matrix.M24 * num;
            matrix2.M31 = matrix.M31 * num;
            matrix2.M32 = matrix.M32 * num;
            matrix2.M33 = matrix.M33 * num;
            matrix2.M34 = matrix.M34 * num;
            matrix2.M41 = matrix.M41 * num;
            matrix2.M42 = matrix.M42 * num;
            matrix2.M43 = matrix.M43 * num;
            matrix2.M44 = matrix.M44 * num;
            return matrix2;
        }

        public static Matrix operator *(float scaleFactor, Matrix matrix)
        {
            Matrix matrix2;
            float num = scaleFactor;
            matrix2.M11 = matrix.M11 * num;
            matrix2.M12 = matrix.M12 * num;
            matrix2.M13 = matrix.M13 * num;
            matrix2.M14 = matrix.M14 * num;
            matrix2.M21 = matrix.M21 * num;
            matrix2.M22 = matrix.M22 * num;
            matrix2.M23 = matrix.M23 * num;
            matrix2.M24 = matrix.M24 * num;
            matrix2.M31 = matrix.M31 * num;
            matrix2.M32 = matrix.M32 * num;
            matrix2.M33 = matrix.M33 * num;
            matrix2.M34 = matrix.M34 * num;
            matrix2.M41 = matrix.M41 * num;
            matrix2.M42 = matrix.M42 * num;
            matrix2.M43 = matrix.M43 * num;
            matrix2.M44 = matrix.M44 * num;
            return matrix2;
        }

        public static Matrix operator /(Matrix matrix1, Matrix matrix2)
        {
            Matrix matrix;
            matrix.M11 = matrix1.M11 / matrix2.M11;
            matrix.M12 = matrix1.M12 / matrix2.M12;
            matrix.M13 = matrix1.M13 / matrix2.M13;
            matrix.M14 = matrix1.M14 / matrix2.M14;
            matrix.M21 = matrix1.M21 / matrix2.M21;
            matrix.M22 = matrix1.M22 / matrix2.M22;
            matrix.M23 = matrix1.M23 / matrix2.M23;
            matrix.M24 = matrix1.M24 / matrix2.M24;
            matrix.M31 = matrix1.M31 / matrix2.M31;
            matrix.M32 = matrix1.M32 / matrix2.M32;
            matrix.M33 = matrix1.M33 / matrix2.M33;
            matrix.M34 = matrix1.M34 / matrix2.M34;
            matrix.M41 = matrix1.M41 / matrix2.M41;
            matrix.M42 = matrix1.M42 / matrix2.M42;
            matrix.M43 = matrix1.M43 / matrix2.M43;
            matrix.M44 = matrix1.M44 / matrix2.M44;
            return matrix;
        }

        public static Matrix operator /(Matrix matrix1, float divider)
        {
            Matrix matrix;
            float num = 1f / divider;
            matrix.M11 = matrix1.M11 * num;
            matrix.M12 = matrix1.M12 * num;
            matrix.M13 = matrix1.M13 * num;
            matrix.M14 = matrix1.M14 * num;
            matrix.M21 = matrix1.M21 * num;
            matrix.M22 = matrix1.M22 * num;
            matrix.M23 = matrix1.M23 * num;
            matrix.M24 = matrix1.M24 * num;
            matrix.M31 = matrix1.M31 * num;
            matrix.M32 = matrix1.M32 * num;
            matrix.M33 = matrix1.M33 * num;
            matrix.M34 = matrix1.M34 * num;
            matrix.M41 = matrix1.M41 * num;
            matrix.M42 = matrix1.M42 * num;
            matrix.M43 = matrix1.M43 * num;
            matrix.M44 = matrix1.M44 * num;
            return matrix;
        }

        static Matrix()
        {
            _identity = new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f);
        }

        // Nested Types
        [StructLayout(LayoutKind.Sequential)]
        private struct CanonicalBasis
        {
            public Vector3 Row0;
            public Vector3 Row1;
            public Vector3 Row2;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct VectorBasis
        {
            public unsafe Vector3* Element0;
            public unsafe Vector3* Element1;
            public unsafe Vector3* Element2;
        }
    }

}
