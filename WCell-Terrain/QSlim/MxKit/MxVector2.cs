using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSlim.MxKit
{
    internal class MxVector2
    {
        private double[] elt = new double[2];

        public MxVector2(double x, double y)
        {
            elt[0] = x;
            elt[1] = y;
        }

        public MxVector2(double[] vec2)
        {
            elt[0] = vec2[0];
            elt[1] = vec2[1];
        }

        internal int Dim { get { return 2; } }
        internal double this[int index]
        {
            get { return elt[index]; }
            set { elt[index] = value; }
        }

        public static MxVector2 operator +(MxVector2 vec1, MxVector2 vec2)
        {
            var newVec = new MxVector2(0.0, 0.0);
            newVec[0] = vec1[0] + vec2[0];
            newVec[1] = vec1[1] + vec2[1];
            return newVec;
        }

        public static MxVector2 operator -(MxVector2 vec1, MxVector2 vec2)
        {
            var newVec = new MxVector2(0.0, 0.0);
            newVec[0] = vec1[0] - vec2[0];
            newVec[1] = vec1[1] - vec2[1];
            return newVec;
        }

        public static MxVector2 operator -(MxVector2 vec1)
        {
            var newVec = new MxVector2(0.0, 0.0);
            newVec[0] = -vec1[0];
            newVec[1] = -vec1[1];
            return newVec;
        }

        public static MxVector2 operator *(MxVector2 vec, double scalar)
        {
            var newVec = new MxVector2(0.0, 0.0);
            newVec[0] = vec[0]*scalar;
            newVec[1] = vec[1]*scalar;
            return newVec;
        }

        public static MxVector2 operator /(MxVector2 vec, double scalar)
        {
            if (scalar.IsCloseEnoughTo(0.0)) return vec;
            var invScalar = 1.0/scalar;
            var newVec = new MxVector2(0.0, 0.0);
            newVec[0] = vec[0]*invScalar;
            newVec[1] = vec[1]*invScalar;
            return newVec;
        }

        internal static void Unitize3(ref MxVector2 vec)
        {
            var denom = vec[0]*vec[0] + vec[1]*vec[1];
            if (denom.IsSqCloseEnoughTo(0.0)) return;

            denom = Math.Sqrt(denom);
            var invDenom = 1.0 / denom;
            vec[0] *= invDenom;
            vec[1] *= invDenom;
        }
    }
}