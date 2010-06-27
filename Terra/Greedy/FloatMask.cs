using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Terra.Greedy
{
    public class FloatMask : ImportMask<float>
    {
        private float[,] data;

        public FloatMask(int width, int height)
        {
            Width = width;
            Height = height;
            data = new float[width,height];
        }

        public float this[int x, int y]
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

        public override float Apply(int x, int y, float val)
        {
            return this[x, y]*val;
        }

        public static FloatMask ReadMask(Stream stream)
        {
            var reader = new BinaryReader(stream);
            return null;
        }

    }
}
