using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.Graphics
{
    public static class MathHelper
    {
        public static float ToRadians(float degrees)
        {
            return (degrees * 0.01745329f);

        }

        public static float ToDegrees(float radians)
        {
            return (radians * 57.29578f);
        }



        internal static float Max(float f1, float f2)
        {
            return (f1 > f2) ? f1 : f2;
        }

        internal static float Min(float f1, float f2)
        {
            return (f1 < f2) ? f1 : f2; 
        }
    }
}
