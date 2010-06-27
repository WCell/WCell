using WCell.Util.Graphics;

namespace Terra.Geometry
{
    public static class GeometryHelpers
    {
        public const float Epsilon = 1.0e-6f;
        public const float EpsilonSquared = 1.0e-12f;

        /// <summary>
        /// Return 2*area of the oriented triangle given by vertices A, B, C
        /// Area is positive if the winding order is CCW
        /// </summary>
        public static float TriArea(ref Vector2 a, ref Vector2 b, ref Vector2 c)
        {
            var area = (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
            return area;
        }

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
        public static bool IsCounterClockwise(ref Vector2 a, ref Vector2 b, ref Vector2 c)
        {
            return (TriArea(ref a, ref b, ref c) > 0.0);
        }

        /// <summary>
        /// Returns true if the given point is on the right hand side of line segment (origin, destination)
        /// </summary>
        public static bool IsRightOf(ref Vector2 point, ref Vector2 origin, ref Vector2 destination)
        {
            return IsCounterClockwise(ref point, ref destination, ref origin);
        }

        /// <summary>
        /// Returns true if the given point is on the left hand side of line segment (origin, destination)
        /// </summary>
        public static bool IsLeftOf(ref Vector2 point, ref Vector2 origin, ref Vector2 destination)
        {
            return IsCounterClockwise(ref point, ref origin, ref destination);
        }

        /// <summary>
        /// Returns true if the given point is inside the circle defined by points a, b, c
        /// </summary>
        public static bool IsInCircle(ref Vector2 point, ref Vector2 a, ref Vector2 b, ref Vector2 c)
        {
            var aProd = (a.X * a.X + a.Y * a.Y) * TriArea(ref b, ref c, ref point);
            var bProd = (b.X * b.X + b.Y * b.Y) * TriArea(ref a, ref c, ref point);
            var cProd = (c.X * c.X + c.Y * c.Y) * TriArea(ref a, ref b, ref point);
            var dProd = (point.X * point.X + point.Y * point.Y) * TriArea(ref a, ref b, ref c);

            return ((aProd - bProd + cProd - dProd) > Epsilon);
        }
    }
}