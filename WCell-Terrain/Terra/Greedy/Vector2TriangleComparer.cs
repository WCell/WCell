using System.Collections.Generic;
using WCell.Util.Graphics;

namespace Terra.Greedy
{
    internal class Vector2TriangleComparer : IComparer<Vector2>
    {
        public int Compare(Vector2 vec1, Vector2 vec2)
        {
            if (vec1.Y == vec2.Y) return 0;
            return (vec1.Y < vec2.Y) ? -1 : 1;
        }
    }
}