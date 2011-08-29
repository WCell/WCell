using System.Collections.Generic;
using System.Diagnostics;

namespace QSlim.MxKit
{
    internal class MxHeap : List<MxIHeapable>
    {
        internal const int NOT_IN_HEAP = -47;

        internal MxHeap() : base(8)
        {
        }

        internal MxHeap(int length) : base(length)
        {
        }

        internal int Size { get { return Count; } }

        internal MxIHeapable Top { get { return ((Count < 1) ? null : this[0]); } }

        internal void Insert(MxIHeapable item)
        {
            Insert(item, item.HeapKey);
        }

        internal void Insert(MxIHeapable item, float key)
        {
            item.HeapKey = key;
            var id = Count;
            Add(item);
            item.HeapPosition = id;
            UpHeap(id);
        }

        internal void Update(MxIHeapable item)
        {
            Update(item, item.HeapKey);
        }

        internal void Update(MxIHeapable item, float key)
        {
            Debug.Assert(item.IsInHeap);
            item.HeapKey = key;

            var index = item.HeapPosition;
            if (index > 0 && key > this[Parent(index)].HeapKey)
            {
                UpHeap(index);
            }
            else
            {
                DownHeap(index);
            }
        }

        internal void UpHeap(int i)
        {
            var moving = this[i];
            var index = i;
            var parent = Parent(i);

            while (index > 0 && moving.HeapKey > this[parent].HeapKey)
            {
                Place(this[parent], index);
                index = parent;
                parent = Parent(index);
            }

            if (index != i)
            {
                Place(moving, index);
            }
        }

        internal void DownHeap(int i)
        {
            var moving = this[i];
            var index = i;
            var left = Left(index);
            var right = Right(index);

            int largest;
            while(left < Count)
            {
                if (right < Count && this[left].HeapKey < this[right].HeapKey)
                {
                    largest = right;
                }
                else
                {
                    largest = left;
                }

                if (moving.HeapKey >= this[largest].HeapKey) break;

                Place(this[largest], index);
                index = largest;
                left = Left(index);
                right = Right(index);
            }

            if (index != i)
            {
                Place(moving, index);
            }
        }

        internal MxIHeapable Extract()
        {
            if (Count < 1) return null;
            Swap(0, (Count - 1));
            var dead = Drop();

            if (Count > 0) DownHeap(0);
            dead.SetAsNotInHeap();

            return dead;
        }

        public new MxIHeapable Remove(MxIHeapable item)
        {
            return RemoveItem(item);
        }

        internal MxIHeapable RemoveItem(MxIHeapable item)
        {
            if (!item.IsInHeap) return null;
            var index = item.HeapPosition;
            Swap(index, (Count - 1));
            Drop();
            item.SetAsNotInHeap();

            if (index == Count) return item;

            if (this[index].HeapKey < item.HeapKey)
            {
                DownHeap(index);
            }
            else
            {
                UpHeap(index);
            }

            return item;
        }

        private void Place(MxIHeapable x, int position)
        {
            this[position] = x;
            x.HeapPosition = position;
        }

        private void Swap(int pos1, int pos2)
        {
            var temp = this[pos1];
            Place(this[pos2], pos1);
            Place(temp, pos2);
        }

        private static int Parent(int i)
        {
            var val = (i - 1);
            if (val < 0) val += int.MaxValue;
            return val/2;
        }

        private int Left(int i)
        {
            return (2*i + 1);
        }

        private int Right(int i)
        {
            return (2*i + 2);
        }

        private MxIHeapable Drop()
        {
            var toDrop = this[Count - 1];
            RemoveAt(Count - 1);
            return toDrop;
        }
    }
}