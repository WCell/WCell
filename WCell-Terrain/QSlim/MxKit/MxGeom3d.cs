using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSlim.MxKit
{
    internal class MxGeom3d
    {
        private const double FourRootThree = 6.928203230275509;

        internal static MxVector3 TriangleRawNormal(MxVector3 v1, MxVector3 v2, MxVector3 v3)
        {
            return MxVector3.Cross(v2 - v1, v3 - v1);
        }

        internal static double TriangleArea(MxVector3 v1, MxVector3 v2, MxVector3 v3)
        {
            var vec = TriangleRawNormal(v1, v2, v3);
            return 0.5*(MxVector3.Norm(vec));
        }

        internal static MxVector3 TriangleNormal(MxVector3 v1, MxVector3 v2, MxVector3 v3)
        {
            var n = TriangleRawNormal(v1, v2, v3);
            MxVector3.Unitize(ref n);
            return n;
        }

        internal static MxVector4 TrianglePlane(MxVector3 v1, MxVector3 v2, MxVector3 v3)
        {
            var n = TriangleNormal(v1, v2, v3);
            return new MxVector4(n, -(n*v1));
        }

        internal static MxVector4 TriangleRawPlane(MxVector3 v1, MxVector3 v2, MxVector3 v3)
        {
            var n = TriangleRawNormal(v1, v2, v3);
            return new MxVector4(n, -(n*v1));
        }

        internal static double TriangleCompactness(MxVector3 v1, MxVector3 v2, MxVector3 v3)
        {
            var area = TriangleArea(v1, v2, v3);
            var denom = MxVector3.NormSquared(v2 - v1) + MxVector3.NormSquared(v3 - v2) + MxVector3.NormSquared(v1 - v3);

            return (FourRootThree*area/denom);
        }
    }
}
