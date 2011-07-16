using System;
using System.Collections.Generic;
using System.Diagnostics;
using C5;
using WCell.Core.Paths;
using WCell.Terrain.Legacy;
using WCell.Terrain.Recast.NavMesh;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.Terrain.Pathfinding
{
	/// <summary>
	/// TODO: Destination of local shortest path is the vertex before the next turn, not necessarily the final vertex
	/// TODO: Consider jumping
	/// TODO: Non-land actors (water, lava, air etc)
	/// TODO: Recycle Path and HashMap objects
	/// </summary>
	public class Pathfinder
	{
		public static float MinWaypointThreshold = 2.0f;
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

		public Path FindPath(Vector3 start, Vector3 destination)
		{
			var path = new Path();
			FindPath(start, destination, path);
			return path;
		}

		public void FindPath(Vector3 start, Vector3 destination, Path path)
		{
			// find the corridor
			var corridor = FindCorridor(start, destination);
			if (corridor.IsNull)
			{
				// could not find path
				return;
			}

			// steer through corridor
			FindPath(destination, corridor, path);
		}

		public void FindPath(Vector3 destination, SearchItem corridor, Path path)
		{
			var mesh = Tile.NavMesh;

			path.Reset(corridor.NodeCount);
			path.Add(destination); // the destination is the final vertex of the path

			if (corridor.NodeCount < 3)
			{
				if (corridor.NodeCount == 2)
				{
					// Two neighboring nodes:

					Vector3 newPoint;
					Vector3 right, left;

					// TODO: Project into 2d, so we can intersect two lines instead of a line and a plane

					// greedily span a vertical plane over the line between start and destination
					var start = corridor.Previous.PointOfReference;
					var plane = new Plane(start, destination, new Vector3(start.X, start.Y, start.Z + 10));

					// and then intersect the plane with the edge that we want to traverse
					mesh.GetEdgePoints(corridor.Previous.Triangle, corridor.Edge, out right, out left);
					if (!Intersection.ClosestPointToPlane(right, left, plane, out newPoint))
					{
						// this is geometrically impossible but might happen due to numerical inaccuracy
						//Debugger.Break();
						newPoint = right;
					}
					path.Add(newPoint);
				}
			}
			else
			{
				// More than two nodes:
				

							//// if distance to new point is smaller than the threshold, replace, rather than add
							//var distToLast = lastLeft
							//if (distToLast < MinWaypointThreshold)
							//{

							//}
				// steer backwards through the corridor
				//Vector3 leftOrigin = destination, rightOrigin = destination;
				var origin = destination;
				Vector3 lastRight, lastLeft;
				int lastLeftIdx = -1, lastRightIdx = -1;
				var current = corridor;
				mesh.GetEdgePoints(current.Previous.Triangle, current.Edge, out lastRight, out lastLeft);
				for (var i = corridor.NodeCount - 2; i > 0; i--)
				{
					current = current.Previous;

					// get the two points that span the traversed edge
					int leftIndex, rightIndex;
					mesh.GetEdgePointIndices(current.Previous.Triangle, current.Edge, out rightIndex, out leftIndex);

					var addedLeft = false;
					if (leftIndex != lastLeftIdx)
					{
						// cast new ray and see if the last point is still inside the corridor
						var left = mesh.Vertices[leftIndex];
						var lastLeft2D = lastRight.XY;
						var dir = left.XY - origin.XY;
						var normal = dir.RightNormal();

						var leftOrig2D = origin.XY;
						var toLastLeft = lastLeft2D - leftOrig2D;

						var dist = Vector2.Dot(toLastLeft, normal);

						if (dist - MathUtil.Epsilonf > 0)
						{
							// right of the left ray -> put obstructing vertex into path
							origin = lastLeft;
							path.Add(lastLeft);
							addedLeft = true;
						}

						lastLeft = left;
						lastLeftIdx = leftIndex;
					}

					if (rightIndex != lastRightIdx)
					{
						var right = mesh.Vertices[rightIndex];
						if (!addedLeft)
						{
							// cast new ray and see if the last point is still inside the corridor
							var lastRight2D = lastLeft.XY;
							var dir = right.XY - origin.XY;
							var normal = dir.RightNormal();

							var rightOrig2D = origin.XY;
							var toLastRight = lastRight2D - rightOrig2D;

							var dist = Vector2.Dot(toLastRight, normal);

							if (dist + MathUtil.Epsilonf < 0)
							{
								// left of the right ray -> put obstructing vertex into path
								origin = lastRight;
								path.Add(lastRight);
							}
						}
						lastRight = right;
						lastRightIdx = rightIndex;
					}


				}
			}
		}

		public SearchItem FindCorridor(Vector3 start, Vector3 destination)
		{
			System.Collections.Generic.HashSet<int> visited;
			return FindCorridor(start, destination, out visited);
		}

		/// <summary>
		/// Basic A* implementation that searches through the navmesh.
		/// </summary>
		public SearchItem FindCorridor(Vector3 start, Vector3 destination,
			out System.Collections.Generic.HashSet<int> visited)
		{
			var mesh = NavMesh;
			visited = null;
			if (mesh == null) return SearchItem.Null;

			var triStart = mesh.FindFirstTriangleUnderneath(start);
			var triEnd = mesh.FindFirstTriangleUnderneath(destination);

			if (triStart == -1 || triEnd == -1)
			{
				visited = null;
				return SearchItem.Null;
			}

			visited = new System.Collections.Generic.HashSet<int> { triStart };

			// the start location is the first node
			var current = new SearchItem(triStart, -1, Vector3.Distance(start, destination), ref start);
			if (triStart == triEnd)
			{
				return current;
			}

			// create fringe
			var fringe = new IntervalHeap<SearchItem>(comparer) { current };

			// start search
			var corridor = SearchItem.Null;
			do
			{
				// next round
				current = fringe.DeleteMin();

				// get the vertices and neighbors of the current triangle
				var triangle = mesh.GetTriangle(current.Triangle);
				var neighbors = mesh.GetNeighborsOf(current.Triangle);

				//var poly = ((NavMesh)Mesh).Polygons[current.Triangle / 3];

				// iterate over all neighbors
				for (var edge = 0; edge < TerrainUtil.NeighborsPerTriangle; edge++)
				{
					var neighbor = neighbors[edge] * 3;

					if (neighbor < 0 || visited.Contains(neighbor)) continue;

					//var npoly = ((NavMesh)Mesh).Polygons[neighbor / 3];
					//var ntri = Mesh.GetTriangle(neighbor);

					visited.Add(neighbor);


					// determine the edge that we want to traverse

					Vector3 left, right;
					switch (edge)
					{
						case TerrainUtil.ABEdgeIndex:
							left = triangle.Point1;
							right = triangle.Point2;
							break;
						case TerrainUtil.ACEdgeIndex:
							left = triangle.Point3;
							right = triangle.Point1;
							break;
						case TerrainUtil.BCEdgeIndex:
							left = triangle.Point2;
							right = triangle.Point3;
							break;
						default:
							throw new Exception("Impossible");
					}

					var refPoint = (left + right) / 2;		// the middle of the edge to be traversed
					var dist = Vector3.Distance(refPoint, destination);
					var newItem = new SearchItem(neighbor, edge, dist, ref refPoint, current);

					if (triEnd == neighbor)
					{
						// we are done
						corridor = newItem;
						goto Done;
					}
					else
					{
						fringe.Add(newItem);
					}
				}
			} while (!fringe.IsEmpty);

		Done:
			return corridor;
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
