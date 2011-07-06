using System;
using System.Runtime.InteropServices;
using WCell.Util.Graphics;

namespace WCell.Terrain.Recast.NavMesh
{

	public class NavMeshPolygon
	{
		public const int VERTS_PER_POLYGON = 6;

		public int[] Indices;								// Indices to vertices of the poly
		public int[] Neighbors;								// neighbors of this poly

		public ushort Flags;								// Flags (see dtPolyFlags)
		public byte Area;
		public NavMeshPolygonTypes Type;

		public Triangle GetTriangle(Vector3[] verts)
		{
			return new Triangle(verts[Indices[0]], verts[Indices[1]], verts[Indices[2]]);
		}
	}

	public enum NavMeshPolygonTypes : byte
	{
		Ground = 0,
		OffMeshConnection = 1,
	}
}
