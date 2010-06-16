using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerrainDisplay.Recast
{
	/// <summary>
	/// Managed version of dtPoly
	/// </summary>
	public class NavMeshPolygon
	{
		public static int VERTS_PER_POLYGON = 6;

		public int FirstLink;												// Index to first link in linked list.
		public ushort[] Vertices;											// Indices to vertices of the poly.
		public ushort[] Neighbors;											// Refs to neighbours of the poly.
		public ushort Flags;												// Flags (see dtPolyFlags).
		public byte Area = 6;												// Area ID of the polygon.
		public byte Type = 2;												// Polygon type, see dtPolyTypes.
	}
}
