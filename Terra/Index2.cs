using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terra
{
    public struct Index2 : IEquatable<Index2>
    {
        public static Index2 One = new Index2
        {
            X = 1,
            Y = 1
        };

        public int X;
        public int Y;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof (Index2)) return false;
            return Equals((Index2) obj);
        }

        public bool Equals(Index2 other)
        {
            return (other.X == X && other.Y == Y);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X*397) ^ Y;
            }
        }

        public static bool operator ==(Index2 left, Index2 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Index2 left, Index2 right)
        {
            return !left.Equals(right);
        }

        public static Index2 operator -(Index2 left, Index2 right)
        {
            return new Index2
            {
                X = left.X - right.X,
                Y = left.Y - right.Y
            };
        }

        public static Index2 Abs(Index2 idx)
        {
            return new Index2
            {
                X = Math.Abs(idx.X),
                Y = Math.Abs(idx.Y)
            };
        }

        public static bool operator <=(Index2 left, Index2 right)
        {
            if (left.X > right.X) return false;
            return !(left.Y > right.Y);
        }

        public static bool operator >=(Index2 left, Index2 right)
        {
            if (left.X < right.X) return false;
            return !(left.Y < right.Y);
        }
    }
}
