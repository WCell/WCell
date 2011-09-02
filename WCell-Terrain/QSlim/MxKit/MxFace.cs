using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace QSlim.MxKit
{
    internal class MxFace
    {
        internal int[] VertexIds;

        internal MxFace()
        {
            VertexIds = new[] {int.MinValue, int.MinValue, int.MinValue};
        }

        internal MxFace(int vId0, int vId1, int vId2)
        {
            VertexIds = new[] {vId0, vId1, vId2};
        }

        internal MxFace(MxFace face)
        {
            VertexIds = new[] {face.VertexIds[0], face.VertexIds[1], face.VertexIds[2]};
        }

        internal int this[int index]
        {
            get { return VertexIds[index]; }
            set { VertexIds[index] = value; }
        }

        internal int RemapVertex(int fromId, int toId)
        {
            var nMapped = 0;
            for (var i = 0; i < 3; i++)
            {
                if (VertexIds[i] != fromId) continue;
                VertexIds[i] = toId;
                nMapped++;
            }

            return nMapped;
        }

        internal int FindVertex(int vertIndex)
        {
            if (VertexIds[0] == vertIndex) return 0;
            if (VertexIds[1] == vertIndex) return 1;
            Debug.Assert(VertexIds[2] == vertIndex);
            return 2;
        }

        internal int OppositeVertex(int vId0, int vId1)
        {
            if (VertexIds[0] != vId0 && VertexIds[0] != vId1) return VertexIds[0];
            if (VertexIds[1] != vId0 && VertexIds[1] != vId1) return VertexIds[1];
            Debug.Assert(VertexIds[2] != vId0 && VertexIds[2] != vId1);
            return VertexIds[2];
        }

        internal bool IsInOrder(int vId0, int vId1)
        {
            if (VertexIds[0] == vId0) return (VertexIds[1] == vId1);
            if (VertexIds[1] == vId0) return (VertexIds[2] == vId1);
            Debug.Assert(VertexIds[2] == vId0);
            return VertexIds[0] == vId1;
        }
    }
}