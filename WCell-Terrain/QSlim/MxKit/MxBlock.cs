using System;
using System.Collections;
using System.Collections.Generic;

namespace QSlim.MxKit
{
    internal class MxBlock<T> : IEnumerable<T> where T : new()

    {
        private T[] block;

        protected MxBlock()
        {
        }

        internal MxBlock(int length)
        {
            Init(length);
        }

        internal int Length { get; private set; }

        internal T this[int index]
        {
            get { return block[index]; }
            set { block[index] = value; }
        }

        protected void Init(int length)
        {
            Length = length;
            block = new T[Length];
            for (var i = 0; i < Length; i++)
            {
                block[i] = new T();
            }
        }

        internal void Resize(int length)
        {
            Array.Resize(ref block, length);
            for (var i = Length; i < length; i++)
            {
                block[i] = new T();
            }
            Length = length;
        }

        internal void Copy(ref T[] newArray)
        {
            Copy(ref newArray, newArray.Length);
        }

        internal void Copy(ref T[] newArray, int length)
        {
            for (var i = 0; i < Math.Min(length, Length); i++)
            {
                newArray[i] = block[i];
            }
        }

        internal void Copy(MxBlock<T> newBlock)
        {
            Copy(newBlock, newBlock.Length);
        }

        internal void Copy(MxBlock<T> newBlock, int length)
        {
            for (var i = 0; i < Math.Min(length, Length); i++)
            {
                newBlock[i] = block[i];
            }
        }

        #region Implementation of IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var t in block)
            {
                yield return t;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
