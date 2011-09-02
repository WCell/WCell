using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSlim.MxKit
{
    internal class MxVector4
    {
        private double[] elt = new double[4];

        public MxVector4(double x, double y, double z, double w)
        {
            elt[0] = x;
            elt[1] = y;
            elt[2] = z;
            elt[3] = w;
        }

        public MxVector4(double[] vec4) 
            : this(vec4[0], vec4[1], vec4[2], vec4[3])
        {
        }

        public MxVector4(MxVector4 vec4)
            : this(vec4[0], vec4[1], vec4[2], vec4[3])
        {
        }

        public MxVector4(MxVector3 vec3, double w)
            : this(vec3[0], vec3[1], vec3[2], w)
        {
        }

        internal int Dim { get { return 4; } }
        
        internal double this[int index]
        {
            get { return elt[index]; }
            set { elt[index] = value; }
        }

        public static MxVector4 operator +(MxVector4 vec1, MxVector4 vec2)
        {
            var newVec = new MxVector4(0.0, 0.0, 0.0, 0.0);
            newVec[0] = vec1[0] + vec2[0];
            newVec[1] = vec1[1] + vec2[1];
            newVec[2] = vec1[2] + vec2[2];
            newVec[3] = vec1[3] + vec2[3];
            return newVec;
        }

        public static MxVector4 operator -(MxVector4 vec1, MxVector4 vec2)
        {
            var newVec = new MxVector4(0.0, 0.0, 0.0, 0.0);
            newVec[0] = vec1[0] - vec2[0];
            newVec[1] = vec1[1] - vec2[1];
            newVec[2] = vec1[2] - vec2[2];
            newVec[3] = vec1[3] - vec2[3];
            return newVec;
        }

        public static MxVector4 operator -(MxVector4 vec)
        {
            return new MxVector4(-vec[0], -vec[1], -vec[2], -vec[3]);
        }

        public static double operator *(MxVector4 vec1, MxVector4 vec2)
        {
            return (vec1[0]*vec2[0] + vec1[1]*vec2[1] + vec1[2]*vec2[2] + vec1[3]*vec2[3]);
        }

        public static MxVector4 operator *(MxVector4 vec, double scalar)
        {
            var newVec = new MxVector4(0.0, 0.0, 0.0, 0.0);
            newVec[0] = vec[0]*scalar;
            newVec[1] = vec[1]*scalar;
            newVec[2] = vec[2]*scalar;
            newVec[3] = vec[3]*scalar;
            return newVec;
        }

        public static MxVector4 operator /(MxVector4 vec, double scalar)
        {
            if (scalar.IsCloseEnoughTo(0.0)) return vec;
            var invScalar = 1.0/scalar;
            var newVec = new MxVector4(0.0, 0.0, 0.0, 0.0);
            newVec[0] = vec[0]*invScalar;
            newVec[1] = vec[1]*invScalar;
            newVec[2] = vec[2]*invScalar;
            newVec[3] = vec[3]*invScalar;
            return newVec;
        }

        internal static void Unitize(ref MxVector4 vec)
        {
            var denom = vec[0]*vec[0] + vec[1]*vec[1] + vec[2]*vec[2] + vec[3]*vec[3];
            if (denom.IsSqCloseEnoughTo(0.0)) return;

            denom = Math.Sqrt(denom);
            var invDenom = 1.0 / denom;
            vec[0] *= invDenom;
            vec[1] *= invDenom;
            vec[2] *= invDenom;
            vec[3] *= invDenom;
        }

        internal static MxVector4 Cross(MxVector4 a, MxVector4 b, MxVector4 c)
        {
            var ret = new MxVector4(0.0, 0.0, 0.0, 0.0);

            var d1 = (b[2] * c[3]) - (b[3] * c[2]);
            var d2 = (b[1] * c[3]) - (b[3] * c[1]);
            var d3 = (b[1] * c[2]) - (b[2] * c[1]);
            var d4 = (b[0] * c[3]) - (b[3] * c[0]);
            var d5 = (b[0] * c[2]) - (b[2] * c[0]);
            var d6 = (b[0] * c[1]) - (b[1] * c[0]);

            ret[0] = -a[1]*d1 + a[2]*d2 - a[3]*d3;
            ret[1] =  a[0]*d1 - a[2]*d4 + a[3]*d5;
            ret[2] = -a[0]*d2 + a[1]*d4 - a[3]*d6;
            ret[3] =  a[0]*d3 - a[1]*d5 + a[2]*d6;
            return ret;
        }
    }
}
