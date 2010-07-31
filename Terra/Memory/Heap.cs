using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Terra.Memory
{
    internal class Heap
    {
        private HeapNode[] nodes;
        private int m_Size;
        private int m_Capacity;
        public const int NOT_IN_HEAP = -47;

        public Heap()
        {
            m_Size = 0;
            m_Capacity = 0;
        }

        public Heap (int size)
        {
            m_Size = 0;
            m_Capacity = size;
            nodes = new HeapNode[size];
        }

        public void Insert(ILabelled obj, float importance)
        {
            if (m_Size == m_Capacity)
            {
                Resize(2*m_Size);
            }

            var i = m_Size++;
            var node = new HeapNode(obj, importance);
            node.Object.Token = i;
            nodes[i] = node;
            
            UpHeap(i);
        }
        
        private void Resize(int newSize)
        {
            var newArray = new HeapNode[newSize];
            for (var i = 0; i < m_Size; i++)
            {
                newArray[i] = nodes[i];
            }
            nodes = newArray;
            m_Capacity = newSize;
        }

        public void Update(ILabelled obj, float importance)
        {
            var i = obj.Token;
            Debug.Assert(i <= m_Size, "WARNING: Attempting to update past end of heap!");
            Debug.Assert(i != NOT_IN_HEAP, "WARNING: Attempting to update object not in heap!");

            var oldImport = nodes[i].Importance;
            nodes[i].Importance = importance;

            if (importance < oldImport)
            {
                DownHeap(i);
                return;
            }

            UpHeap(i);
        }

        public HeapNode Extract()
        {
            if (m_Size < 1) return null;

            Swap(0, m_Size - 1);
            m_Size--;
            
            DownHeap(0);

            nodes[m_Size].Object.Token = NOT_IN_HEAP;
            return nodes[m_Size];
        }

        public HeapNode Top()
        {
            return (m_Size < 1) ? null : nodes[0];
        }

        public HeapNode Kill(int i)
        {
            if (i >= m_Size)
                Debugger.Break();

            Swap(i, m_Size - 1);
            m_Size--;
            nodes[m_Size].Object.Token = NOT_IN_HEAP;

            if (nodes[i].Importance < nodes[m_Size].Importance)
            {
                DownHeap(i);
                return nodes[m_Size];
            }
            
            UpHeap(i);
            return nodes[m_Size];
        }

        private void Swap (int i, int j)
        {
            var tempNode = nodes[i];
            nodes[i] = nodes[j];
            nodes[j] = tempNode;

            nodes[i].Object.Token = i;
            nodes[j].Object.Token = j;
        }

        private static int Parent (int i)
        {
            return (i - 1)/2;
        }

        private static int Left (int i)
        {
            return (i*2 + 1);
        }

        private static int Right (int i)
        {
            return (i*2 + 2);
        }

        private void UpHeap(int i)
        {
            if (i == 0) return;

            var parent = Parent(i);
            if (nodes[i].Importance <= nodes[parent].Importance) return;

            Swap(i, parent);
            UpHeap(parent);
        }

        private void DownHeap(int i)
        {
            if (i >= m_Size) return;

            var largest = i;
            var left = Left(i);
            var right = Right(i);

            if (left < m_Size && nodes[left].Importance > nodes[largest].Importance) largest = left;
            if (right < m_Size && nodes[right].Importance > nodes[largest].Importance) largest = right;
            if (largest == i) return;

            Swap(i, largest);
            DownHeap(largest);
        }
    }
}
