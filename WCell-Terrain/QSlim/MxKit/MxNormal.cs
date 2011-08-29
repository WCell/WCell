using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace QSlim.MxKit
{
    internal class MxNormal
    {
        private static int X = 0;
        private static int Y = 1;
        private static int Z = 2;

        private short[] Dir;

        internal MxNormal()
        {
            Dir = new short[3];
        }

        internal MxNormal(double x, double y, double z)
        {
            Dir = new short[3];
            Set(x, y, z);
        }

        internal MxNormal(double[] nml)
        {
            Dir = new short[3];
            Set(nml);
        }

        internal double this[int index]
        {
            get
            {
                Debug.Assert(index < 3);
                return ShortToDouble(Dir[index]);
            }
            set
            {
                Debug.Assert(index < 3);
                Dir[index] = DoubleToShort(value);
            }
        }

        internal short Raw(int index)
        {
            return Dir[index];
        }

        internal void Set(double x, double y, double z)
        {
            Dir[X] = DoubleToShort(x);
            Dir[Y] = DoubleToShort(y);
            Dir[Z] = DoubleToShort(z);
        }

        internal void Set(double[] nml)
        {
            Dir[X] = DoubleToShort(nml[X]);
            Dir[Y] = DoubleToShort(nml[Y]);
            Dir[Z] = DoubleToShort(nml[Z]);
        }

        private static short DoubleToShort(double dbl)
        {
            return (short) IntRound((dbl > 1.0 ? 1.0 : dbl)*(short.MaxValue));
        }

        private static double ShortToDouble(short sht)
        {
            return ((double)sht)/((double)short.MaxValue);
        }

        private static int IntRound(double dbl)
        {
            return (int) (dbl + 0.5);
        }
    }
}
