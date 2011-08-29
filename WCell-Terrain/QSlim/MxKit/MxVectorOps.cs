using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSlim.MxKit
{
    internal static class MxVectorOps
    {
        internal static void Cross3(ref double[] ret, double[] u, double[] v)
        {
            ret[0] = u[1]*v[2] - v[1]*u[2];
            ret[1] = v[0]*u[2] - u[0]*v[2];
            ret[2] = u[0]*v[1] - v[0]*u[1];
        }

        internal static void Unitize3(ref double[] ret)
        {
            var denom = ret[0]*ret[0] + ret[1]*ret[1] + ret[2]*ret[2];
            
            if (denom.IsSqCloseEnoughTo(0.0)) return;
            
            denom = Math.Sqrt(denom);
            var invDenom = 1.0/denom;
            ret[0] *= invDenom;
            ret[1] *= invDenom;
            ret[2] *= invDenom;
        }

        internal static double Dot3(double[] u, double[] v)
        {
            return u[0] * v[0] + u[1] * v[1] + u[2] * v[2];
        }

        internal static double Length3(double[] u)
        {
            var len2 = u[0]*u[0] + u[1]*u[1] + u[2]*u[2];
            return Math.Sqrt(len2);
        }

        internal static void Sub3(ref double[] diff, double[] u, double[] v)
        {
            diff[0] = u[0] - v[0];
            diff[1] = u[1] - v[1];
            diff[2] = u[2] - v[2];
        }

        internal static void SubFrom3(ref double[] from, double[] subtractor)
        {
            from[0] -= subtractor[0];
            from[1] -= subtractor[1];
            from[2] -= subtractor[2];
        }

        internal static void AddInto3(ref double[] collector, double[] addend)
        {
            collector[0] += addend[0];
            collector[1] += addend[1];
            collector[2] += addend[2];
        }
    }
}
