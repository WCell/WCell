using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terra
{
    internal static class Extensions
    {
        internal static bool IsNullOrEmpty(this Array array)
        {
            if (array == null) return true;
            return (array.Length == 0);
        }

        internal static void Fill(this int[,] array, int fillVal)
        {
            for (var i = 0; i < array.GetLength(0); i++)
            {
                for (var j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] = fillVal;
                }
            }
        }
    }
}
