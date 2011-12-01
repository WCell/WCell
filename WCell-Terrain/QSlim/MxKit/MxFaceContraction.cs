using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSlim.MxKit
{
    internal class MxFaceContraction
    {
        internal int FaceId;
        internal double[] DeltaV1;
        internal double[] DeltaV2;
        internal double[] DeltaV3;
        internal List<int> DeltaFaces;
        internal List<int> DeadFaces;

        internal MxFaceContraction()
        {
            DeltaV1 = new double[3];
            DeltaV2 = new double[3];
            DeltaV3 = new double[3];
            DeltaFaces = new List<int>(6);
            DeadFaces = new List<int>(6);
        }
    }
}
