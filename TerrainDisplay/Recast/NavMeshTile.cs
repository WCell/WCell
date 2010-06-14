using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;


namespace TerrainDisplay.Recast
{
	/// <summary>
	/// Managed version of dtMeshTile.
	/// Usage of members is not quite self-explanatory due to a high level of compression.
	/// </summary>
	public class NavMeshTile
	{
		/// <summary>
		/// The coordinates of the Tile within the mesh
		/// </summary>
		public int X, Y;

		/// <summary>
		/// The coordinates of the next Tile within the mesh
		/// </summary>
		public int NextX, NextY;

		/// <summary>
		/// The vertices of which this tile consists
		/// </summary>
		public Vector3[] Vertices;

		/// <summary>
		/// Visible polygons, spanned between sets of vertices
		/// </summary>
		public NavMeshPolygon[] Polygons;

		/// <summary>
		/// Some people might refer to these as "portals": They allow agents to jump between disconnected tiles.
		/// See: http://digestingduck.blogspot.com/2010/01/off-mesh-connection-progress-pt-3.html
		/// </summary>
		public NavOffMeshConnection[] OffMeshConnections;

		/// <summary>
		/// Links to other tiles
		/// </summary>
		public NavMeshPolyLink[] Links;

		/// <summary>
		/// Tile flags, see dtTileFlags
		/// </summary>
		public uint Flags;

		///// <summary>
		///// The entire tile in byte form (used for serialization/deserialization)
		///// </summary>
		//public byte[] Data;

		///// <summary>
		///// The tree is needed for fast execution of the query* functions in big tiles, which find polygons without a local context.
		///// Those functions are needed for different things (eg findNearestPoly), but not for pathfinding.
		///// </summary>
		//public BVTreeNode TreeNode;

		///// <summary>
		///// Eye candy
		///// </summary>
		//public Vector3[] DetailedVertices;

		///// <summary>
		///// Eye candy (colored version of the original polygons?)
		///// </summary>
		//public byte[] DetailedTriangles;
	}
}
