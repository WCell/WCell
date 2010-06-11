using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerrainDisplay.Recast
{
	/// <summary>
	/// Managed version of dtPoly
	/// </summary>
	public class NavMeshPoly
	{
		public const int DT_VERTS_PER_POLYGON = 6;

		//unsigned int firstLink;											// Index to first link in linked list. 
		public ushort[] Vertices = new ushort[DT_VERTS_PER_POLYGON];		// Indices to vertices of the poly.
		public ushort[] Neighbors = new ushort[DT_VERTS_PER_POLYGON];		// Refs to neighbours of the poly.
		public ushort Flags;												// Flags (see dtPolyFlags).
		public byte Area = 6;												// Area ID of the polygon.
		public byte Type = 2;												// Polygon type, see dtPolyTypes.
	}
}
