using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSlim.MxKit
{
    internal class MxPairContraction
    {
        internal int VertId1;
        internal int VertId2;
        internal double[] DeltaV1;
        internal double[] DeltaV2;
        internal int DeltaPivot;
        internal List<int> DeltaFaces;
        internal List<int> DeadFaces; 

        internal MxPairContraction()
        {
            DeltaV1 = new double[3];
            DeltaV2 = new double[3];
            DeltaFaces = new List<int>(6);
            DeadFaces = new List<int>(6);
        }

        internal MxPairContraction(MxPairContraction contraction)
        {
            VertId1 = contraction.VertId1;
            VertId2 = contraction.VertId2;
            DeltaV1 = (double[])contraction.DeltaV1.Clone();
            DeltaV2 = (double[])contraction.DeltaV2.Clone();
            DeltaFaces = new List<int>(contraction.DeltaFaces);
            DeadFaces = new List<int>(contraction.DeadFaces);

            DeltaPivot = contraction.DeltaPivot;
        }
    }
}
