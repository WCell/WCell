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
		public static readonly int VERTS_PER_POLYGON = 6;
	    

		public uint FirstLink;												// Index to first link in linked list.
	    public ushort VertCount;
        public ushort[] Vertices;											// Indices to vertices of the poly.
		public ushort[] Neighbors;											// Refs to neighbours of the poly.
		public ushort Flags;												// Flags (see dtPolyFlags).
		public byte Area;   												// Area ID of the polygon.
		public NavMeshPolyTypes Type;   									// Polygon type, see dtPolyTypes
	}

    public enum NavMeshPolyTypes : byte
    {
        Ground = 0,
        OffMeshConnection = 1,
    }
}
