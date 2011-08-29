using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace QSlim.MxKit
{
    internal class MxEdge
    {
        internal static int EDGE_NULL = int.MinValue;
        internal int V1, V2;

        internal MxEdge()
        {
            V1 = EDGE_NULL;
            V2 = EDGE_NULL;
        }

        internal MxEdge(int vertAId, int vertBId)
        {
            V1 = vertAId;
            V2 = vertBId;
        }

        internal MxEdge(MxEdge edge)
        {
            V1 = edge.V1;
            V2 = edge.V2;
        }

        internal int OppositeVertex(int vertId)
        {
            if (V1 == vertId) return V2;
            Debug.Assert(V2 == vertId);
            return V1;
        }
    }
}
