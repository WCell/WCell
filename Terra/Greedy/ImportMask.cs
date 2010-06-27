using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terra.Greedy
{
    public class ImportMask<T>
    {
        public int Width;
        public int Height;

        public ImportMask()
        {
            Width = 0;
            Height = 0;
        }

        public virtual T Apply(int x, int y, T val)
        {
            return val;
        }
    }
}
