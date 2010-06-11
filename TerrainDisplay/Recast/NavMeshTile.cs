using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;


public class OffMeshConnection
{
	public Vector3[] pos;							// Both end point locations.
	public float rad;								// Link connection radius.
	public byte poly;								// Poly Id
	public byte flags;								// Link flags
	public byte side;								// End point side.

	//public dtOffMeshConnection()
	//{
	//    pos = new Vector3[2];
	//}
}

public struct PolyLink
{
	public int Reference;					// Neighbour reference.
	public int next;						// Index to next link.
	public byte edge;						// Index to polygon edge which owns this link. 
	public byte side;						// If boundary link, defines on which side the link is.
	public byte bmin, bmax;					// If boundary link, defines the sub edge area.
}

namespace TerrainDisplay.Recast
{
	/// <summary>
	/// Managed version of dtMeshTile.
	/// Usage of members is not quite self-explanatory due to a high level of compression.
	/// </summary>
	public class NavMeshTile
	{
		public NavMeshPoly[] Polys;
		public Vector3[] Vertices;

		/// <summary>
		/// Not sure what these are
		/// </summary>
		public OffMeshConnection[] OffMeshConnections;

		/// <summary>
		/// The tree is needed for fast execution of the query* functions in big tiles, which find polygons without a local context.
		/// Those functions are needed for different things (eg findNearestPoly), but not for pathfinding.
		/// </summary>
		//public BVTreeNode TreeNode;

		/// <summary>
		/// Not sure what these are
		/// </summary>
		public PolyLink[] Links;

		/// <summary>
		/// No idea what these are
		/// </summary>
		public Vector3[] DetailedVertices;

		/// <summary>
		/// Not sure what these are
		/// </summary>
		public byte[] DetailedTriangles;

		/// <summary>
		/// Tile flags, see dtTileFlags
		/// </summary>
		public int Flags;

		/// <summary>
		/// The entire tile in byte form (used for serialization/deserialization)
		/// </summary>
		public byte[] Data;
	}
}
