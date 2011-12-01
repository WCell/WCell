using System;
using System.Linq;
using System.Text;

namespace QSlim.MxKit
{
    internal interface MxIHeapable
    {
        bool IsInHeap { get; }
        int HeapPosition { get; set; }
        float HeapKey { get; set; }

        void SetAsNotInHeap();
    }
}
