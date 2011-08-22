using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using WCell.Terrain.Legacy;
using WCell.Util.Graphics;

namespace WCell.Terrain
{
	public static class TerrainUtil
	{
		public const int TrianglePointA = 1 << 0;
		public const int TrianglePointB = 1 << 1;
		public const int TrianglePointC = 1 << 2;

		public const int ABEdgeMask = TrianglePointA | TrianglePointB;
		public const int ACEdgeMask = TrianglePointA | TrianglePointC;
		public const int BCEdgeMask = TrianglePointB | TrianglePointC;

		public const int ABEdgeIndex = 0;
		public const int BCEdgeIndex = 1;
        public const int CAEdgeIndex = 2;

		public const int NeighborsPerTriangle = 3;


		public static int GetSharedEdgeMask(this Triangle tri1, Triangle tri2)
		{
			var mask = 0;
			if (tri1.Point1.Equals(tri2.Point1) || tri1.Point1.Equals(tri2.Point2) || tri1.Point1.Equals(tri2.Point3))
			{
				mask = TrianglePointA;
			}
			if (tri1.Point2.Equals(tri2.Point1) || tri1.Point2.Equals(tri2.Point2) || tri1.Point2.Equals(tri2.Point3))
			{
				mask |= TrianglePointB;
			}
			if (tri1.Point3.Equals(tri2.Point1) || tri1.Point3.Equals(tri2.Point2) || tri1.Point3.Equals(tri2.Point3))
			{
				mask |= TrianglePointC;
			}
			return mask;
		}

        /// <summary>
        /// Returns the ordered set of edge points for the given edge of Triangle ABC (clockwise winding)
        ///   as if from the inside looking out
        /// </summary>
        /// <param name="shape">The shape which contains the triangle</param>
        /// <param name="tri">The index of the triangle within the shape</param>
        /// <param name="edge">The index of the edge</param>
        /// <param name="right">The point on the edge oriented to the right of the viewer</param>
        /// <param name="left">The point on the edge oriented to the left of the viewer</param>
        public static void GetInsideOrderedEdgePoints(this IShape shape, int tri, int edge, out Vector3 right, out Vector3 left)
		{
            /*              C(3)
             *              /  \
             *         ----/--+ \
             *            /      \
             *         B(2)------A(1)
             *           curTriangle
             */
			var triangle = shape.GetTriangle(tri);
			switch (edge)
			{
				case ABEdgeIndex:
					left = triangle.Point1;
					right = triangle.Point2;
					break;
				case CAEdgeIndex:
					left = triangle.Point3;
					right = triangle.Point1;
					break;
				case BCEdgeIndex:
					left = triangle.Point2;
					right = triangle.Point3;
					break;
				default:
					throw new Exception("Impossible");
			}
		}

        /// <summary>
        /// Returns the ordered set of edge points for the given edge of Triangle ABC (clockwise winding)
        ///   as if from the outside looking in
        /// </summary>
        /// <param name="shape">The shape which contains the triangle</param>
        /// <param name="tri">The index of the triangle within the shape</param>
        /// <param name="edge">The index of the edge</param>
        /// <param name="right">The point on the edge oriented to the right of the viewer</param>
        /// <param name="left">The point on the edge oriented to the left of the viewer</param>
        /// <param name="apex">The point on the triangle opposite the edge</param>
        public static void GetOutsideOrderedEdgePointsPlusApex(this IShape shape, int tri, int edge, out Vector3 right, out Vector3 left, out Vector3 apex)
        {
            var triangle = shape.GetTriangle(tri);
            triangle.GetOutsideOrderedEdgePointsPlusApex(edge, out right, out left, out apex);
        }

        public static void GetOutsideOrderedEdgePoints(this IShape shape, int tri, int edge, out Vector3 right, out Vector3 left)
        {
            var triangle = shape.GetTriangle(tri);
            triangle.GetOutsideOrderedEdgePoints(edge, out right, out left);
        }

		public static void GetEdgePointIndices(this IShape shape, int tri, int edge, out int right, out int left)
		{
			switch (edge)
			{
				case TerrainUtil.ABEdgeIndex:
					left = 0;
					right = 1;
					break;
				case TerrainUtil.CAEdgeIndex:
					left = 2;
					right = 0;
					break;
				case TerrainUtil.BCEdgeIndex:
					left = 1;
					right = 2;
					break;
				default:
					throw new Exception("Impossible");
			}

			left = shape.Indices[tri + left];
			right = shape.Indices[tri + right];

			int third;
			switch (edge)
			{
				case TerrainUtil.ABEdgeIndex:
					third = 2;
					break;
				case TerrainUtil.CAEdgeIndex:
					third = 1;
					break;
				case TerrainUtil.BCEdgeIndex:
					third = 0;
					break;
				default:
					throw new Exception("Impossible");
			}

			// if we cast a ray from the third point to the middle of the other two, 
			// left must be left of that ray
			// and right must be to the right
			var p1 = shape.Vertices[shape.Indices[tri + third]].XY;
			var rightP = shape.Vertices[right].XY;
			var leftP = shape.Vertices[left].XY;

			var p2 = (rightP + leftP) / 2;

			var dir = p2 - p1;

			var normal = dir.RightNormal();
			var dist = Intersection.DirectionToPoint(rightP, p1, p1 + normal);
			Debug.Assert(dist > 0);
			dist = Intersection.DirectionToPoint(leftP, p1, p1 + normal);
			Debug.Assert(dist < 0);
		}

		public static int GetEdge(int mask)
		{
			int edge;
			switch (mask)
			{
				case ABEdgeMask:
					// neighbor shares a and b
					edge = ABEdgeIndex;
					break;
				case ACEdgeMask:
					// second shares a and c
					edge = CAEdgeIndex;
					break;
				case BCEdgeMask:
					// neighbor shares b and c
					edge = BCEdgeIndex;
					break;
				default:
					edge = -1;
					break;
			}
			return edge;
		}

        public static void GetOutsideOrderedEdgePoints(this Triangle triangle, int edge, out Vector3 right, out Vector3 left)
        {
            /*              C(3)
             *              /  \
             *        +----/--  \
             *            /      \
             *         B(2)------A(1)
             *           curTriangle
             */
            switch (edge)
            {
                case ABEdgeIndex:
                    right = triangle.Point1;
                    left = triangle.Point2;
                    break;
                case BCEdgeIndex:
                    right = triangle.Point2;
                    left = triangle.Point3;
                    break;
                case CAEdgeIndex:
                    right = triangle.Point3;
                    left = triangle.Point1;
                    break;

                default:
                    throw new Exception("Impossible");
            }
        }

        /// <summary>
        /// Returns the ordered set of edge points for the given edge of Triangle ABC (clockwise winding)
        ///   as if from the outside looking in
        /// </summary>
        /// <param name="triangle">The index of the triangle within the shape</param>
        /// <param name="edge">The index of the edge</param>
        /// <param name="right">The point on the edge oriented to the right of the viewer</param>
        /// <param name="left">The point on the edge oriented to the left of the viewer</param>
        /// <param name="apex">The point on the triangle opposite the edge</param>
        public static void GetOutsideOrderedEdgePointsPlusApex(this Triangle triangle, int edge, out Vector3 right, out Vector3 left, out Vector3 apex)
        {
            /*              C(3)
             *              /  \
             *        +----/--  \
             *            /      \
             *         B(2)------A(1)
             *           curTriangle
             */
            switch (edge)
            {
                case ABEdgeIndex:
                    right = triangle.Point1;
                     left = triangle.Point2;
                     apex = triangle.Point3;
                    break;
                case BCEdgeIndex:
                    right = triangle.Point2;
                     left = triangle.Point3;
                     apex = triangle.Point1;
                    break;
                case CAEdgeIndex:
                    right = triangle.Point3;
                     left = triangle.Point1;
                     apex = triangle.Point2;
                    break;
                
                default:
                    throw new Exception("Impossible");
            }
        }

        public static bool TriangleContainsPointXY(this IShape shape, int tri, Vector2 point)
        {
            var triangle = shape.GetTriangle(tri);
            return triangle.ContainsPointXY(point);
        }

        public static bool ContainsPointXY(this Triangle triangle, Vector2 point)
        {
            var a = triangle.Point1.XY;
            var b = triangle.Point2.XY;
            var c = triangle.Point3.XY;

            if (!GeometryHelpers.IsRightOfOrOn(point, a, b)) return false;
            if (!GeometryHelpers.IsRightOfOrOn(point, b, c)) return false;
            if (!GeometryHelpers.IsRightOfOrOn(point, c, a)) return false;

            return true;
        }
	}
}
