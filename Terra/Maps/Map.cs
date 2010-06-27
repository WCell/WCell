using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Terra.Maps
{
    public abstract class Map<T>
    {
        public int Width;
        public int Height;
        
        public float Min;
        public float Max;

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

        public abstract float Eval(int i, int j);
        
        public abstract void RawRead(BinaryReader reader);
        
        public abstract void TextRead(TextReader reader);
        
        public virtual T[] GetBlock()
        {
            return null;
        }

        public virtual void FindLimits()
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
    }
}
