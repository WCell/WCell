using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Graphics;

namespace WCell.Terrain.Collision
{
	public static class CollisionUtil
	{
		public static Triangle GetTriangle(this IShape shape, int index)
		{
			var vertices = shape.Vertices;
			var indices = shape.Indices;
			return new Triangle(vertices[indices[index]], vertices[indices[index + 1]], vertices[indices[index + 2]]);
		}

		/// <summary>
		/// Returns the first triangle that is directly underneath the given position.
		/// The value is -1, if none was found.
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>
		public static int FindFirstTriangleUnderneath(this IShape shape, Vector3 pos)
		{
			// shoot a ray straight down
			pos.Z += 0.001f;						// make sure, it is slightly above the surface
			var ray = new Ray(pos, Vector3.Down);
			return shape.FindFirstHitTriangle(ray);
		}

		public static int IntersectFirstTriangle(this IShape shape, Ray selectRay, out Vector3 v)
		{
			float distance;
			var hit = shape.FindFirstHitTriangle(selectRay, out distance);
			if (hit == -1)
			{
				v = Vector3.Zero;
			}
			else
			{
				v = selectRay.Position + selectRay.Direction*distance;
			}
			return hit;
		}

		/// <summary>
		/// Returns the first triangle that is directly underneath the given position.
		/// The value is -1, if none was found.
		/// </summary>
		/// <returns></returns>
		public static int FindFirstHitTriangle(this IShape shape, Ray selectRay)
		{
			// shoot a ray straight down
			float distance;
			return shape.FindFirstHitTriangle(selectRay, out distance);
		}

		/// <summary>
		/// Intersects the given ray with the tile and sets triangle to the first one that got hit
		/// <param name="distance">The distance from Ray origin to, where it hits the returned triangle</param>
		/// </summary>
		public static int FindFirstHitTriangle(this IShape shape, Ray selectRay, out float distance)
		{
			distance = float.MaxValue;

			var triangle = -1;

			var verts = shape.Vertices;
			var indices = shape.Indices;
			foreach (var tri in shape.GetPotentialColliders(selectRay))
			{
				// we are assuming that each shape only stores triangles which are 3 consecutive indices
				var vec0 = verts[indices[tri]];
				var vec1 = verts[indices[tri + 1]];
				var vec2 = verts[indices[tri + 2]];

				float time;
				if (!Intersection.RayTriangleIntersect(selectRay, vec0, vec1, vec2, out time)) continue;
				if (time > distance) continue;

				distance = time;
				triangle = tri;
			}

			return triangle;
		}


		/// <summary>
		/// Any triangle that shares 2 vertices with the given triangle is a neighbor
		/// </summary>
		public static int[] GetEdgeNeighborsOf(this IShape shape, int triIndex)
		{
			var indices = shape.Indices;
			var a = indices[triIndex++];
			var b = indices[triIndex++];
			var c = indices[triIndex];

			var neighbors = new int[WCellTerrainConstants.NeighborsPerTriangle];

			// fill with "invalid index"
			for (var i = 0; i < WCellTerrainConstants.NeighborsPerTriangle; i++)
			{
				neighbors[i] = -1;
			}

			// TODO: Use a datastructure to cut down this search
			for (var i = 0; i < indices.Length; i += 3)
			{
				if (i != triIndex)	// don't return itself
				{
					var a2 = indices[i];
					var b2 = indices[i + 1];
					var c2 = indices[i + 2];

					var nCount = 0;
					var mask = 0;
					if (a == a2 || a == b2 || a == c2)
					{
						// some vertex matches the first vertex of the triangle
						nCount++;
						mask |= WCellTerrainConstants.TrianglePointA;
					}
					if (b == a2 || b == b2 || b == c2)
					{
						// some vertex matches the second vertex of the triangle
						nCount++;
						mask |= WCellTerrainConstants.TrianglePointB;
					}
					if (c == a2 || c == b2 || c == c2)
					{
						// some vertex matches the third vertex of the triangle
						nCount++;
						mask |= WCellTerrainConstants.TrianglePointC;
					}

					if (nCount == 2)
					{
						// we have a neighbor
						switch (mask)
						{
							case WCellTerrainConstants.ABEdgeMask:
								// neighbor shares a and b
								neighbors[WCellTerrainConstants.ABEdgeIndex] = i;
								break;
							case WCellTerrainConstants.ACEdgeMask:
								// second shares a and c
								neighbors[WCellTerrainConstants.ACEdgeIndex] = i;
								break;
							case WCellTerrainConstants.BCEdgeMask:
								// neighbor shares b and c
								neighbors[WCellTerrainConstants.BCEdgeIndex] = i;
								break;

						}
					}
				}
			}

			return neighbors;
		}
	}

}
