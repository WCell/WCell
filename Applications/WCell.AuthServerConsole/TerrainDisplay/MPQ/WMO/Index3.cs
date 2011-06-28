using System;

namespace TerrainDisplay.MPQ.WMO
{
    public struct Index3 : IEquatable<Index3>
    {
        public short Index0;
        public short Index1;
        public short Index2;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof (Index3)) return false;
            return Equals((Index3) obj);
        }

        public bool Equals(Index3 other)
        {
            return other.Index0 == Index0 && other.Index1 == Index1 && other.Index2 == Index2;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = Index0.GetHashCode();
                result = (result*397) ^ Index1.GetHashCode();
                result = (result*397) ^ Index2.GetHashCode();
                return result;
            }
        }

        public static bool operator ==(Index3 left, Index3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Index3 left, Index3 right)
        {
            return !left.Equals(right);
        }
    }
}