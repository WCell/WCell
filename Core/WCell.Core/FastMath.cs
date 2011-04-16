/*************************************************************************
 *
 *   file		: FastMath.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-03-10 00:28:05 +0100 (ma, 10 mar 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 183 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WCell.Core
{
    public static class FastMath
    {
        /// <summary>
        /// Clamps a number by an upper and lower limit.
        /// </summary>
        /// <remarks>This method effectively lets you bound a value between two values, as in
        /// if you have bounds of 0 - 100, and a number is 130, it'll clamp it to 100, and vise versa.</remarks>
        /// <param name="value">the value to clamp</param>
        /// <param name="min">the minimum bound</param>
        /// <param name="max">the maximum bound</param>
        /// <returns>either the original number, or a clamped number based on the upper/lower bounds</returns>
        public static short ClampMinMax(short value, short min, short max)
        {
            if (value > max)
            {
                return max;
            }
            if (value < min)
            {
                return min;
            }

            return value;
        }

        /// <summary>
        /// Clamps a number by an upper and lower limit.
        /// </summary>
        /// <remarks>This method effectively lets you bound a value between two values, as in
        /// if you have bounds of 0 - 100, and a number is 130, it'll clamp it to 100, and vise versa.</remarks>
        /// <param name="value">the value to clamp</param>
        /// <param name="min">the minimum bound</param>
        /// <param name="max">the maximum bound</param>
        /// <returns>either the original number, or a clamped number based on the upper/lower bounds</returns>
        public static ushort ClampMinMax(ushort value, ushort min, ushort max)
        {
            if (value > max)
            {
                return max;
            }
            if (value < min)
            {
                return min;
            }

            return value;
        }

        /// <summary>
        /// Clamps a number by an upper and lower limit.
        /// </summary>
        /// <remarks>This method effectively lets you bound a value between two values, as in
        /// if you have bounds of 0 - 100, and a number is 130, it'll clamp it to 100, and vise versa.</remarks>
        /// <param name="value">the value to clamp</param>
        /// <param name="min">the minimum bound</param>
        /// <param name="max">the maximum bound</param>
        /// <returns>either the original number, or a clamped number based on the upper/lower bounds</returns>
        public static int ClampMinMax(int value, int min, int max)
        {
            if (value > max)
            {
                return max;
            }
            if (value < min)
            {
                return min;
            }

            return value;
        }

        /// <summary>
        /// Clamps a number by an upper and lower limit.
        /// </summary>
        /// <remarks>This method effectively lets you bound a value between two values, as in
        /// if you have bounds of 0 - 100, and a number is 130, it'll clamp it to 100, and vise versa.</remarks>
        /// <param name="value">the value to clamp</param>
        /// <param name="min">the minimum bound</param>
        /// <param name="max">the maximum bound</param>
        /// <returns>either the original number, or a clamped number based on the upper/lower bounds</returns>
        public static uint ClampMinMax(uint value, uint min, uint max)
        {
            if (value > max)
            {
                return max;
            }
            if (value < min)
            {
                return min;
            }

            return value;
        }

        /// <summary>
        /// Clamps a number by an upper and lower limit.
        /// </summary>
        /// <remarks>This method effectively lets you bound a value between two values, as in
        /// if you have bounds of 0 - 100, and a number is 130, it'll clamp it to 100, and vise versa.</remarks>
        /// <param name="value">the value to clamp</param>
        /// <param name="min">the minimum bound</param>
        /// <param name="max">the maximum bound</param>
        /// <returns>either the original number, or a clamped number based on the upper/lower bounds</returns>
        public static long ClampMinMax(long value, long min, long max)
        {
            if (value > max)
            {
                return max;
            }
            if (value < min)
            {
                return min;
            }

            return value;
        }

        /// <summary>
        /// Clamps a number by an upper and lower limit.
        /// </summary>
        /// <remarks>This method effectively lets you bound a value between two values, as in
        /// if you have bounds of 0 - 100, and a number is 130, it'll clamp it to 100, and vise versa.</remarks>
        /// <param name="value">the value to clamp</param>
        /// <param name="min">the minimum bound</param>
        /// <param name="max">the maximum bound</param>
        /// <returns>either the original number, or a clamped number based on the upper/lower bounds</returns>
        public static ulong ClampMinMax(ulong value, ulong min, ulong max)
        {
            if (value > max)
            {
                return max;
            }
            if (value < min)
            {
                return min;
            }

            return value;
        }

        /// <summary>
        /// Clamps a number by an upper and lower limit.
        /// </summary>
        /// <remarks>This method effectively lets you bound a value between two values, as in
        /// if you have bounds of 0 - 100, and a number is 130, it'll clamp it to 100, and vise versa.</remarks>
        /// <param name="value">the value to clamp</param>
        /// <param name="min">the minimum bound</param>
        /// <param name="max">the maximum bound</param>
        /// <returns>either the original number, or a clamped number based on the upper/lower bounds</returns>
        public static float ClampMinMax(float value, float min, float max)
        {
            if (value > max)
            {
                return max;
            }
            if (value < min)
            {
                return min;
            }

            return value;
        }

        /// <summary>
        /// Clamps a number by an upper and lower limit.
        /// </summary>
        /// <remarks>This method effectively lets you bound a value between two values, as in
        /// if you have bounds of 0 - 100, and a number is 130, it'll clamp it to 100, and vise versa.</remarks>
        /// <param name="value">the value to clamp</param>
        /// <param name="min">the minimum bound</param>
        /// <param name="max">the maximum bound</param>
        /// <returns>either the original number, or a clamped number based on the upper/lower bounds</returns>
        public static double ClampMinMax(double value, double min, double max)
        {
            if (value > max)
            {
                return max;
            }
            if (value < min)
            {
                return min;
            }

            return value;
        }

        /// <summary>
        /// Clamps a number by an upper and lower limit.
        /// </summary>
        /// <remarks>This method effectively lets you bound a value between two values, as in
        /// if you have bounds of 0 - 100, and a number is 130, it'll clamp it to 100, and vise versa.</remarks>
        /// <param name="value">the value to clamp</param>
        /// <param name="min">the minimum bound</param>
        /// <param name="max">the maximum bound</param>
        /// <returns>either the original number, or a clamped number based on the upper/lower bounds</returns>
        public static decimal ClampMinMax(decimal value, decimal min, decimal max)
        {
            if (value > max)
            {
                return max;
            }
            if (value < min)
            {
                return min;
            }

            return value;
        }

        public static int FastInterlockedAdd(ref int location, int value)
        {
            if (value == 1)
            {
                return Interlocked.Increment(ref location);
            }
            if (value == -1)
            {
                return Interlocked.Decrement(ref location);
            }
            return Interlocked.Add(ref location, value);
        }

        public static long FastInterlockedAdd(ref long location, long value)
        {
            if (value == 1)
            {
                return Interlocked.Increment(ref location);
            }
            if (value == -1)
            {
                return Interlocked.Decrement(ref location);
            }
            return Interlocked.Add(ref location, value);
        }
    }
}