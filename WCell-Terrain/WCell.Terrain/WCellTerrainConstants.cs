using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Terrain
{
	public static class WCellTerrainConstants
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
	}
}
