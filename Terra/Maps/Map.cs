using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Terra.Maps
{
    internal class Map
    {
        // [width, height]
        private float[,] data;

        public int Width;
        public int Height;
        
        public float Min;
        public float Max;


        public Map(int w, int h)
        {
            Width = w;
            Height = h;

            data = new float[w,h];
        }

        public float this[int i, int j]
        {
            get
            {
                return Eval(i, j);
            }
        }

        public float this[float i, float j]
        {
            get
            {
                return Eval((int) i, (int) j);
            }
        }

        public float Eval (float i, float j)
        {
            return Eval((int) i, (int) j);
        }

        public float Eval(int i, int j)
        {
            Debug.Assert(i >= 0);
            Debug.Assert(j >= 0);
            Debug.Assert(i < Width);
            Debug.Assert(j < Height);

            // [width, height]
            return data[i, j];
        }
        
        public float[] GetBlock()
        {
            var retArray = new float[data.Length];
            for (var j = 0; j < Width; j++)
            {
                for (var i = 0; i < Height; i++)
                {
                    retArray[j * Width + i] = data[i, j];
                }
            }
            return retArray;
        }

        public void FindLimits()
        {
            Min = float.MaxValue;
            Max = float.MinValue;

            for (var i = 0; i < Width; i++)
            {
                for (var j = 0; j < Height; j++)
                {
                    var val = Eval(i, j);

                    if (val < Min) Min = val;
                    if (val > Max) Max = val;
                }
            }
        }

        public static Map LoadFromArray(float[,] array)
        {
            var width = array.GetLength(0);
            var height = array.GetLength(1);

            return new Map(width, height)
            {
                Width = width,
                Height = height,
                data = array
            };
        }
    }
}
