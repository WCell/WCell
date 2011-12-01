using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSlim.MxKit
{
    internal class MxVertex
    {
        private static int X = 0;
        private static int Y = 1;
        private static int Z = 2;

        internal VertexInfo Info;

        internal MxVertex()
        {
            Info = new VertexInfo
            {
                Pos = new double[3],
                Proxy = new ProxyStruct(),
            };
        }

        internal MxVertex(double x, double y, double z)
        {
            Info = new VertexInfo
            {
                Pos = new double[3],
                Proxy = new ProxyStruct(),
            };
            Info.Pos[X] = x;
            Info.Pos[Y] = y;
            Info.Pos[Z] = z;
        }
        
        internal MxVertex(MxVertex vert)
        {
            Info = vert.Info;
        }

        internal double[] Pos
        {
            get { return Info.Pos; } 
            set { Info.Pos = value; }
        }

        internal double this[int index]
        {
            get { return Info.Pos[index]; }
            set { Info.Pos[index] = value; }
        }

        internal struct VertexInfo
        {
            internal double[] Pos;
            internal ProxyStruct Proxy;
        }

        internal struct ProxyStruct
        {
            internal int Parent;
            internal int Next;
            internal int Prev;
        }
    }
}
