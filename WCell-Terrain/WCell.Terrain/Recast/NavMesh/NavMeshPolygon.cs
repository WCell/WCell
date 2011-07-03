using System;
using System.Runtime.InteropServices;

namespace WCell.Terrain.Recast.NavMesh
{

	public class NavMeshPolygon
	{
		public const int VERTS_PER_POLYGON = 6;

		public int[] Indices;								// Indices to vertices of the poly
		public NavMeshPolygon[] Neighbors;					// neighbors of this poly

		public ushort Flags;								// Flags (see dtPolyFlags)
		public byte Area;
		public NavMeshPolygonTypes Type;
	}

	public enum NavMeshPolygonTypes : byte
	{
		Ground = 0,
		OffMeshConnection = 1,
	}
}
