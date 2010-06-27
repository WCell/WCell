using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Terra.Memory
{
    public class Heap : List<HeapNode>
    {
        public const int NOT_IN_HEAP = -47;

        public Heap (int size) : base(size)
        {
        }

        public void Insert(ILabelled obj, float importance)
        {
            var i = Count;
            var node = new HeapNode(obj, importance);
            node.Object.Token = i;
            Add(node);
            
            UpHeap(i);
        }
        
        public void Update(ILabelled obj, float importance)
        {
            var i = obj.Token;
            Debug.Assert(i <= Count, "WARNING: Attempting to update past end of heap!");
            Debug.Assert(i != NOT_IN_HEAP, "WARNING: Attempting to update object not in heap!");

            var oldImport = this[i].Importance;
            this[i].Importance = importance;

            if (importance < oldImport)
            {
                DownHeap(i);
                return;
            }

            UpHeap(i);
        }

        public HeapNode Extract()
        {
            if (Count < 1) return null;

            var end = Count - 1;
            Swap(0, end);
            var node = this[end];
            RemoveAt(end);
            DownHeap(0);

            node.Object.Token = NOT_IN_HEAP;
            return node;
        }

        public HeapNode Top()
        {
            return (Count < 1) ? null : this[0];
        }

        public HeapNode Kill(int i)
        {
            Debug.Assert(i >= Count, "WARNING: Attempt to delete invalid heap node.");

            var end = Count - 1;
            Swap(i, end);
            var node = this[end];
            RemoveAt(end);
            node.Object.Token = NOT_IN_HEAP;

            if (this[i].Importance < node.Importance)
            {
                DownHeap(i);
                return node;
            }

            UpHeap(i);
            return node;
        }

        private void Swap (int i, int j)
        {
            var tempNode = this[i];
            this[i] = this[j];
            this[j] = tempNode;

            this[i].Object.Token = i;
            this[j].Object.Token = j;
        }

        private int Parent (int i)
        {
            return (i - 1)/2;
        }

        private int Left (int i)
        {
            return (i*2 + 1);
        }

        private int Right (int i)
        {
            return (i*2 + 2);
        }

        private void UpHeap(int i)
        {
            if (i == 0) return;

            var parent = Parent(i);
            if (this[i].Importance <= this[parent].Importance) return;

            Swap(i, parent);
            UpHeap(parent);
        }

        private void DownHeap(int i)
        {
            if (i >= Count) return;

            var largest = i;
            var left = Left(i);
            var right = Right(i);

            if (left < Count && this[left].Importance > this[largest].Importance) largest = left;
            if (right < Count && this[right].Importance > this[largest].Importance) largest = right;
            if (largest == i) return;

            Swap(i, largest);
            DownHeap(largest);
        }
    }
}
