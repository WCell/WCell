using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSlim.MxKit
{
    public enum MxPlacement
    {
        EndPoints = 0,
        EndOrMid = 1,
        Line = 2,
        Optimal = 3
    }

    public enum MxWeighting
    {
        Uniform = 0,
        Area = 1,
        Angle = 2,
        Average = 3,
        AreaAverage = 4,
        RawNormals = 5
    }

    public enum MxBinding
    {
        UnBound = 0x0,
        PerFace = 0x1,
        PerVertex = 0x2
    }

    [Flags]
    public enum MxFlags : byte
    {
        None = 0x00,
        Valid = 0x01,
        Proxy = 0x02,
        Touched = 0x04
    }
}
