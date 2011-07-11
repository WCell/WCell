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
		public const int ACEdgeIndex = 1;
		public const int BCEdgeIndex = 2;

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

		public static void GetEdgePoints(this IShape shape, int tri, int edge, out Vector3 right, out Vector3 left)
		{
			var triangle = shape.GetTriangle(tri);
			switch (edge)
			{
				case TerrainUtil.ABEdgeIndex:
					left = triangle.Point1;
					right = triangle.Point2;
					break;
				case TerrainUtil.ACEdgeIndex:
					left = triangle.Point3;
					right = triangle.Point1;
					break;
				case TerrainUtil.BCEdgeIndex:
					left = triangle.Point2;
					right = triangle.Point3;
					break;
				default:
					throw new Exception("Impossible");
			}
		}

		public static void GetEdgePointIndices(this IShape shape, int tri, int edge, out int right, out int left)
		{
			switch (edge)
			{
				case TerrainUtil.ABEdgeIndex:
					left = 0;
					right = 1;
					break;
				case TerrainUtil.ACEdgeIndex:
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
				case TerrainUtil.ACEdgeIndex:
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
					edge = ACEdgeIndex;
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
	}
}
