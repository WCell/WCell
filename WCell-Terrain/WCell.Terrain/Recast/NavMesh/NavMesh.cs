using System;
using System.Collections.Generic;
using WCell.Terrain.Legacy;
using WCell.Util.Graphics;

namespace WCell.Terrain.Recast.NavMesh
{
	public class NavMesh : IShape
	{
		public const ushort ExternalLinkId = 0x8000;

		public readonly TerrainTile Tile;

		public NavMesh(TerrainTile tile, NavMeshPolygon[] polys, Vector3[] vertices, int[] indices)
		{
			Tile = tile;
			Polygons = polys;
			Vertices = vertices;
			Indices = indices;
		}

		public NavMeshPolygon[] Polygons
		{
			get;
			private set;
		}

		public Vector3[] Vertices
		{
			get;
			private set;
		}

		public int[] Indices
		{
			get;
			private set;
		}

		public IEnumerable<int> GetPotentialColliders(Ray ray)
		{
			for (var i = 0; i < Indices.Length; i += 3)
			{
				yield return i;
			}
		}

		public int[] GetNeighborsOf(int triIndex)
		{
			return Polygons[triIndex/3].Neighbors;
		}

		#region Not Implemented Yet
		/// <summary>
		/// Some people might refer to these as "portals": They allow agents to jump between disconnected tiles.
		/// See: http://digestingduck.blogspot.com/2010/01/off-mesh-connection-progress-pt-3.html
		/// </summary>
		public NavOffMeshConnection[] OffMeshConnections;

		/// <summary>
		/// Indexes the DetailedVertices and DetailedTriangles by the Tile's Polygons
		/// </summary>
		public NavMeshPolyDetail[] DetailPolygons;

		///// <summary>
		///// Eye candy (colored version of the original polygons?)
		///// </summary>
		public NavMeshDetailTriIndex[] DetailedTriangles;
		#endregion

		/// <summary>
		/// Build triangles for rendering purposes
		/// 
		/// Imagine a hexagon with vertices 0 through 5
		/// Then this method adds 4 (n - 2) triangles for it:
		///  0, 1, 2
		///  0, 2, 3
		///  0, 3, 4
		///  0, 4, 5
		/// </summary>
		public void GetTriangles(out List<int> indices)
		{
			indices = new List<int>();

			var i = 0;
			foreach (var poly in Polygons)
			{
				// build indices
				for (var j = 2; j < poly.Indices.Length; j++)
				{
					indices.Add(poly.Indices[0]);
					indices.Add(poly.Indices[j - 1]);
					indices.Add(poly.Indices[j]);
				}
			}

			Indices = indices.ToArray();
		}
	}
}
