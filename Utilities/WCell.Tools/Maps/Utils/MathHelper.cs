using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Tools.Maps.Utils
{
    public static class MathHelper
    {
        public static float ToRadians(float degrees)
        {
            return (degrees * 0.01745329f);
        }
    }
}