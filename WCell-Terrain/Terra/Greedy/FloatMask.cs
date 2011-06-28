using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Terra.Greedy
{
    internal class FloatMask
    {
        private float[,] data;
        public int Width;
        public int Height;


        protected FloatMask(int width, int height)
        {
            Width = width;
            Height = height;
            data = new float[width,height];
        }

        public virtual float this[int x, int y]
        {
            get
            {
                Debug.Assert(x >= 0);
                Debug.Assert(y >= 0);
                Debug.Assert(x < Width);
                Debug.Assert(y < Height);

                return data[x, y];
            }
        }

        public virtual float Apply(int x, int y, float val)
        {
            return this[x, y]*val;
        }

        public static FloatMask LoadFromArray(float[,] array)
        {
            var width = array.GetLength(0);
            var height = array.GetLength(1);

            return new FloatMask(width, height)
            {
                data = array
            };
        }
    }

    internal class DefaultMask : FloatMask
    {
        public DefaultMask(int width, int height) : base(width, height)
        {
        }

        public override float this[int x, int y]
        {
            get
            {
                return 1.0f;
            }
        }

        public override float Apply(int x, int y, float val)
        {
            return val;
        }
    }
}
