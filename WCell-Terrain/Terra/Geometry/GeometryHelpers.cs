using WCell.Util.Graphics;

namespace Terra.Geometry
{
    internal static class GeometryHelpers
    {
        public const float Epsilon = 1.0e-6f;
        public const float EpsilonSquared = 1.0e-12f;

        /// <summary>
        /// Return 2*area of the oriented triangle given by vertices A, B, C
        /// Area is positive if the winding order is CCW
        /// </summary>
        public static float TriArea(Vector2 a, Vector2 b, Vector2 c)
        {
            var area = (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
            return area;
        }

        /// <summary>
        /// Returns true if the winding order of Triangle ABC is CCW
        /// </summary>
        public static bool IsCounterClockwise(Vector2 a, Vector2 b, Vector2 c)
        {
            return (TriArea(a, b, c) > 0.0);
        }

        /// <summary>
        /// Returns true if the given point is on the right hand side of line segment (origin, destination)
        /// </summary>
        public static bool IsRightOf(Vector2 point, Vector2 origin, Vector2 destination)
        {
            return IsCounterClockwise(point, destination, origin);
        }

        /// <summary>
        /// Returns true if the given point is on the left hand side of line segment (origin, destination)
        /// </summary>
        public static bool IsLeftOf(Vector2 point, Vector2 origin, Vector2 destination)
        {
            return IsCounterClockwise(point, origin, destination);
        }

        /// <summary>
        /// Returns true if the given point is inside the circle defined by points a, b, c
        /// </summary>
        public static bool IsInCircle(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            var aProd = (a.X*a.X + a.Y*a.Y)*TriArea(b, c, d);
            var bProd = (b.X*b.X + b.Y*b.Y)*TriArea(a, c, d);
            var cProd = (c.X*c.X + c.Y*c.Y)*TriArea(a, b, d);
            var dProd = (d.X*d.X + d.Y*d.Y)*TriArea(a, b, c); 
            return (aProd - bProd + cProd - dProd) > Epsilon;
        }
    }
}