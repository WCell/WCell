using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSlim.MxKit
{
    internal class MxBlock2<T> : MxBlock<T> where T : new()
    {
        internal int Columns { get; private set; }
        internal int Rows { get; private set; }

        protected MxBlock2()
        {
        }

        internal MxBlock2(int cols, int rows)
            : base(cols*rows)
        {
            Columns = cols;
            Rows = rows;
        }

        internal T this[int col, int row]
        {
            get { return base[row*Columns + col]; }
            set { base[row*Columns + col] = value; }
        }

        internal void Resize(int cols, int rows)
        {
            Columns = cols;
            Rows = rows;
            Resize(cols*rows);
        }
    }
}
