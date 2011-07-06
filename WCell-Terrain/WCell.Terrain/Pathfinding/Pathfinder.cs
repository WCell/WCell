using System;
using System.Collections.Generic;
using C5;
using WCell.Terrain.Collision;
using WCell.Terrain.Recast.NavMesh;
using WCell.Util.Graphics;

namespace WCell.Terrain.Pathfinding
{
	/// <summary>
	/// TODO: Destination of local shortest path is the vertex before the next turn, not necessarily the final vertex
	/// TODO: Consider jumping
	/// TODO: Actor requirements on liquid (can actor only walk on ground or also in certain types of liquid?)
	/// </summary>
	public class Pathfinder
	{
		static readonly IComparer<SearchItem> comparer = new SearchItemComparer();

		public readonly TerrainTile Tile;

		public Pathfinder(TerrainTile tile)
		{
			Tile = tile;
		}

		public NavMesh NavMesh
		{
			get { return Tile.NavMesh; }
		}

		public Vector3[] FindPathUnflavored(//float actorHeight, 
			Vector3 start, Vector3 destination)
		{
			var winner = FindPath(start, destination);

			if (winner.IsNull)
			{
				// could not find path
				return null;
			}

			var current = winner;
			var path = new Vector3[winner.NodeCount];
			// the destination is the final vertex
			path[winner.NodeCount - 1] = destination;

			// add all intermediate waypoints 
			for (var i = winner.NodeCount - 1; i > 0; i--)
			{
				path[i - 1] = current.EnterPos;
				current = current.Previous;
			}

			return path;
		}

		/// <summary>
		/// 
		/// </summary>
		public SearchItem FindPath(Vector3 start, Vector3 destination)
		{
			System.Collections.Generic.HashSet<int> visited;
			return FindPath(start, destination, out visited);
		}

		public SearchItem FindPath(Vector3 start, Vector3 destination,
			out System.Collections.Generic.HashSet<int> visited)
		{
			var mesh = NavMesh;

			var triStart = mesh.FindFirstTriangleUnderneath(start);
			var triEnd = mesh.FindFirstTriangleUnderneath(destination);

			if (triStart == -1 || triEnd == -1)
			{
				visited = null;
				return SearchItem.Null;
			}

			visited = new System.Collections.Generic.HashSet<int> { triStart };

			if (triStart == triEnd)
			{
				return new SearchItem(triEnd, 0, destination, SearchItem.Null);
			}

			var fringe = new IntervalHeap<SearchItem>(comparer)
				{
					new SearchItem(triStart, Vector3.Distance(start, destination), start, SearchItem.Null)
				};

			SearchItem current;
			var winner = SearchItem.Null;

			do
			{
				// new round
				current = fringe.DeleteMin();
				
				// get the vertices and neighbors of the current triangle
				var triangle = mesh.GetTriangle(current.Triangle);
				var neighbors = mesh.GetNeighborsOf(current.Triangle);
				//var poly = ((NavMesh)Mesh).Polygons[current.Triangle / 3];

				// iterate over all neighbors
				for (var i = 0; i < TerrainUtil.NeighborsPerTriangle; i++)
				{
					var neighbor = neighbors[i]*3;

					if (neighbor < 0 || visited.Contains(neighbor)) continue;

					//var npoly = ((NavMesh)Mesh).Polygons[neighbor / 3];
					//var ntri = Mesh.GetTriangle(neighbor);

					visited.Add(neighbor);


					// determine the edge that we want to traverse
					Vector3 edgeP1, edgeP2;
					switch (i)
					{
						case TerrainUtil.ABEdgeIndex:
							edgeP1 = triangle.Point1;
							edgeP2 = triangle.Point2;
							break;
						case TerrainUtil.ACEdgeIndex:
							edgeP1 = triangle.Point1;
							edgeP2 = triangle.Point3;
							break;
						case TerrainUtil.BCEdgeIndex:
							edgeP1 = triangle.Point2;
							edgeP2 = triangle.Point3;
							break;
						default:
							throw new Exception("Impossible");
					}

					
					//var newPoint = Intersection.FindClosestPointInSegment(edgeP1, edgeP2, destination);

					// TODO: Fix local path decision by employing iterative method

					// To find the next point, we greedily (and inaccurately) span a vertical plane through the line between the last and the next point
					// This is far from optimal, will have to employ a better method for non-linear paths
					var s = current.EnterPos;
					var plane = new Plane(s, destination, new Vector3(s.X, s.Y, s.Z + 10));

					// and then intersect the plane with the edge that we want to traverse
					Vector3 newPoint;
					if (!Intersection.LineSegmentIntersectsPlane(edgeP1, edgeP2, plane, out newPoint))
					{
						newPoint = edgeP1;
					}

					var newItem = new SearchItem(neighbor, Vector3.Distance(newPoint, destination), newPoint, current);
					if (triEnd == neighbor)
					{
						// we are done
						winner = newItem;
						goto Done;
					}
					else
					{
						fringe.Add(newItem);
					}
				}
			} while (!fringe.IsEmpty);

			Done:

			// prepend the destination
			winner = new SearchItem(triEnd, 0, destination, winner);
			return winner;
		}

		internal class SearchItemComparer : IComparer<SearchItem>
		{
			/// <summary>
			/// Negative value means, x is less than y.
			/// Positive value means, y is less than x.
			/// </summary>
			public int Compare(SearchItem x, SearchItem y)
			{
				return (int)(x.GetSearchValue() - y.GetSearchValue() + 0.5f);
			}
		}
	}
}
