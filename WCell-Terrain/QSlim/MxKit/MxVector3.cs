using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSlim.MxKit
{
    internal class MxVector3
    {
        private double[] elt = new double[3];

        internal MxVector3(double x, double y, double z)
        {
            elt[0] = x;
            elt[1] = y;
            elt[2] = z;
        }

        internal MxVector3(double[] vec3)
        {
            elt[0] = vec3[0];
            elt[1] = vec3[1];
            elt[2] = vec3[2];
        }

        internal MxVector3(MxVertex vert)
        {
            var pos = vert.Pos;
            elt[0] = pos[0];
            elt[1] = pos[1];
            elt[2] = pos[2];
        }

        internal int Dim { get { return 3; } }
        
        internal double this[int index]
        {
            get { return elt[index]; }
            set { elt[index] = value; }
        }

        public static MxVector3 operator +(MxVector3 vec1, MxVector3 vec2)
        {
            var newVec = new MxVector3(0.0, 0.0, 0.0);
            newVec[0] = vec1[0] + vec2[0];
            newVec[1] = vec1[1] + vec2[1];
            newVec[2] = vec1[2] + vec2[2];
            return newVec;
        }

        public static MxVector3 operator -(MxVector3 vec1, MxVector3 vec2)
        {
            var newVec = new MxVector3(0.0, 0.0, 0.0);
            newVec[0] = vec1[0] - vec2[0];
            newVec[1] = vec1[1] - vec2[1];
            newVec[2] = vec1[2] - vec2[2];
            return newVec;
        }

        public static MxVector3 operator -(MxVector3 vec1)
        {
            var newVec = new MxVector3(0.0, 0.0, 0.0);
            newVec[0] = -vec1[0];
            newVec[1] = -vec1[1];
            newVec[2] = -vec1[2];
            return newVec;
        }

        public static MxVector3 operator *(MxVector3 vec, double scalar)
        {
            var newVec = new MxVector3(0.0, 0.0, 0.0);
            newVec[0] = vec[0]*scalar;
            newVec[1] = vec[1]*scalar;
            newVec[2] = vec[2]*scalar;
            return newVec;
        }

        public static MxVector3 operator *(double scalar, MxVector3 vec)
        {
            return (vec*scalar);
        }

        public static double operator *(MxVector3 vec1, MxVector3 vec2)
        {
            return (vec1[0]*vec2[0] + vec1[1]*vec2[1] + vec1[2]*vec2[2]);
        }

        public static MxVector3 operator /(MxVector3 vec, double scalar)
        {
            if (scalar.IsCloseEnoughTo(0.0)) return vec;
            var invScalar = 1.0/scalar;
            var newVec = new MxVector3(0.0, 0.0, 0.0);
            newVec[0] = vec[0]*invScalar;
            newVec[1] = vec[1]*invScalar;
            newVec[2] = vec[2]*invScalar;
            return newVec;
        }

        public static MxVector3 operator ^(MxVector3 vec1, MxVector3 vec2)
        {
            return Cross(vec1, vec2);
        }

        internal static void Unitize(ref MxVector3 vec)
        {
            var denom = vec[0]*vec[0] + vec[1]*vec[1] + vec[2]*vec[2];
            if (denom.IsSqCloseEnoughTo(0.0)) return;

            denom = Math.Sqrt(denom);
            var invDenom = 1.0 / denom;
            vec[0] *= invDenom;
            vec[1] *= invDenom;
            vec[2] *= invDenom;
        }

        internal static MxVector3 Cross(MxVector3 u, MxVector3 v)
        {
            var ret = new MxVector3(0.0, 0.0, 0.0);
            ret[0] = u[1] * v[2] - v[1] * u[2];
            ret[1] = v[0] * u[2] - u[0] * v[2];
            ret[2] = u[0] * v[1] - v[0] * u[1];
            return ret;
        }

        internal static double NormSquared(MxVector3 u)
        {
            return (u*u);
        }
        internal static double Norm(MxVector3 u)
        {
            return Math.Sqrt(NormSquared(u));
        }
    }
}
