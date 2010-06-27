using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Terra.Maps
{
    public abstract class DirectMap<T> : Map<T>
    {
        // [width, height]
        private T[,] data;


        protected DirectMap(int w, int h)
        {
            Width = w;
            Height = h;

            data = new T[w,h];
        }


        protected T Evaluate(int i, int j)
        {
            Debug.Assert(i >= 0);
            Debug.Assert(j >= 0);
            Debug.Assert(i < Width);
            Debug.Assert(j < Height);

            // [width, height]
            return data[i, j];
        }

        public override T[] GetBlock()
        {
            var retArray = new T[data.Length];
            for (var j = 0; j < Width; j++)
            {
                for (var i = 0; i < Height; i++)
                {
                    retArray[j*Width + i] = data[i, j];
                }
            }
            return retArray;
        }
    }
}
