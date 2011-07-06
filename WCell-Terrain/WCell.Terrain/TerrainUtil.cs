using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
