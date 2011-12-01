using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSlim.MxKit
{
    internal static class Extensions
    {
        private const double EPSILON = 1e-6;
        private const double EPSILON2 = 1e-12;

        internal static void MxRemove<T>(this List<T> list, int id)
        {
            var end = list.Count - 1;
            var obj = list[end];
            list[id] = obj;
            list.RemoveAt(end);
        }

        internal static bool IsCloseEnoughTo(this double[] vec1, double[] vec2)
        {
            var len = Math.Min(vec1.Length, vec2.Length);
            for (var i = 0; i < len; i++)
            {
                if (!vec1[i].IsCloseEnoughTo(vec2[i])) return false;
            }
            return true;
        }

        internal static bool IsCloseEnoughTo(this double d, double val)
        {
            return Math.Abs(d - val) < EPSILON;
        }

        internal static bool IsSqCloseEnoughTo(this double d, double val)
        {
            return Math.Abs(d - val) < EPSILON2;
        }
    }
}
