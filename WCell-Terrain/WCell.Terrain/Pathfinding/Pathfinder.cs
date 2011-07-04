using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using C5;
using WCell.Terrain.Collision;
using WCell.Util.Graphics;

namespace WCell.Terrain.Pathfinding
{
	/// <summary>
	/// TODO: Cull unwalkable nodes (ceiling too low; too close to wall etc)
	/// </summary>
	public class Pathfinder
	{
		static readonly IComparer<SearchItem> comparer = new SearchItemComparer();

		public IShape Shape;

		public Pathfinder()
		{
		}

		public Vector3[] FindPathUnflavored(float actorHeight, Vector3 start, Vector3 destination)
		{
			var winner = FindPath(actorHeight, start, destination);

			if (winner.IsNull)
			{
				// could not find path
				return null;
			}

			var current = winner;
			var path = new Vector3[winner.NodeCount + 1];
			// the destination is the final vertex
			path[winner.NodeCount] = destination;

			// add all intermediate waypoints 
			for (var i = winner.NodeCount - 1; i >= 0; i--)
			{
				path[i] = current.EnterPos;
				current = current.Previous;
			}

			return path;
		}

		/// <summary>
		/// 
		/// </summary>
		public SearchItem FindPath(float actorHeight, Vector3 start, Vector3 destination)
		{
			System.Collections.Generic.HashSet<int> visited;
			return FindPath(actorHeight, start, destination, out visited);
		}

		/// <summary>
		/// TODO: Destination of local shortest path is the vertex before the next turn, not necessarily the final vertex
		/// TODO: Consider jumping
		/// TODO: Actor requirements on liquid (can actor only walk on ground or also in certain types of liquid?)
		/// </summary>
		public SearchItem FindPath(float actorHeight, Vector3 start, Vector3 destination,
			out System.Collections.Generic.HashSet<int> visited)
		{
			var triStart = Shape.FindFirstTriangleUnderneath(start);
			var triEnd = Shape.FindFirstTriangleUnderneath(destination);

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

			// next waypoint is the intersection of:
			// The direct line to the destination and The traversed edge between last and current triangle
			do
			{
				// new round
				current = fringe.DeleteMin();
				
				// get the vertices and neighbors of the current triangle
				Triangle triangle = Shape.GetTriangle(current.Triangle);
				var neighbors = Shape.GetEdgeNeighborsOf(current.Triangle);

				// iterate over all neighbors
				for (var i = 0; i < WCellTerrainConstants.NeighborsPerTriangle; i++)
				{
					var neighbor = neighbors[i];

					if (neighbor == -1 || visited.Contains(neighbor)) continue;

					visited.Add(neighbor);

					// TODO: If the highest vertex is the one that is not shared between the two neighbors, we cannot walk on it from this direction

					// determine the edge that we want to traverse
					Vector3 edgeP1, edgeP2;
					switch (i)
					{
						case WCellTerrainConstants.ABEdgeIndex:
							edgeP1 = triangle.Point1;
							edgeP2 = triangle.Point2;
							break;
						case WCellTerrainConstants.ACEdgeIndex:
							edgeP1 = triangle.Point1;
							edgeP2 = triangle.Point3;
							break;
						case WCellTerrainConstants.BCEdgeIndex:
							edgeP1 = triangle.Point2;
							edgeP2 = triangle.Point3;
							break;
						default:
							throw new Exception("Impossible");
					}

					
					//var newPoint = Intersection.FindClosestPointInSegment(edgeP1, edgeP2, destination);
					// To find the next point, we span a vertical plane through the line between the last and the next point
					var s = current.EnterPos;
					var plane = new Plane(s, destination, new Vector3(s.X, s.Y, s.Z + 10));

					// and then intersect the plane with the edge that we want to traverse
					Vector3 newPoint;
					if (!Intersection.LineSegmentIntersectsPlane(edgeP1, edgeP2, plane, out newPoint))
					{
						// this is not right
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
