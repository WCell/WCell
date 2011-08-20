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
			//FindPath(destination, corridor, path);
            FindPathStringPull(start, destination, corridor, path);
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
					//var plane = new Plane(start, destination, new Vector3(start.X, start.Y, start.Z + 10));

                    var startXY = corridor.Previous.PointOfReference.XY;
				    var endXY = destination.XY;

					// and then intersect the plane with the edge that we want to traverse
					mesh.GetInsideOrderedEdgePoints(corridor.Previous.Triangle, corridor.Edge, out right, out left);
				    var rightXY = right.XY;
				    var leftXY = left.XY;

					//if (!Intersection.ClosestPointToPlane(right, left, plane, out newPoint))
					//{
						// this is geometrically impossible but might happen due to numerical inaccuracy
						//Debugger.Break();
					//	newPoint = right;
					//}

				    Vector2 newPointXY;
				    if (Intersection.IntersectionOfLineSegments2D(startXY, endXY, rightXY, leftXY, out newPointXY))
				    {
				        float newZ;
				        if (newPointXY.LerpZ(start, destination, out newZ))
				        {
				            newPoint = new Vector3(newPointXY, newZ);
				        }
				        else
				        {
				            // this is geometrically impossible but might happen due to numerical inaccuracy
				            Debugger.Break();
				            newPoint = right;
				        }
				    }
				    else
				    {
				        // this is geometrically impossible but might happen due to numerical inaccuracy
				        Debugger.Break();
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
				mesh.GetInsideOrderedEdgePoints(current.Previous.Triangle, current.Edge, out lastRight, out lastLeft);
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

        public void FindPathStringPull(Vector3 origin3d, Vector3 destination3d, SearchItem corridor, Path path)
        {
            var mesh = NavMesh;
            var corridorStart = corridor;
            var pathNodes = new List<Vector3>(corridor.NodeCount);
            pathNodes.Add(destination3d);
            //path.Reset(corridor.NodeCount);
            //path.Add(destination3d);

            Vector3 curTriApex3d, curTriLeft3d, curTriRight3d;
            Vector3 prevTriApex3d, prevTriLeft3d, prevTriRight3d;
            Vector2 curTriApex, destination, curTriLeft, curTriRight;
            int curTriangle, curEdge;

            bool apexInFunnelLeft;
            bool apexInFunnelRight;
            while (corridor.Previous.Previous != SearchItem.Null) // The next triangle contains the starting point
            {
                curTriangle = corridor.Previous.Triangle;
                curEdge = corridor.Edge;
                mesh.GetOutsideOrderedEdgePointsPlusApex(curTriangle, curEdge, out curTriRight3d, out curTriLeft3d,
                                                         out curTriApex3d);

                destination = destination3d.XY;
                curTriApex = curTriApex3d.XY;
                curTriLeft = curTriLeft3d.XY;
                curTriRight = curTriRight3d.XY;

                var foundNewDestination = false;
                while (!foundNewDestination) // Found new point for the path
                {
                    if (corridor.Previous.Previous == SearchItem.Null) break;

                    apexInFunnelLeft = GeometryHelpers.IsRightOfOrOn(curTriApex, destination, curTriLeft);
                    apexInFunnelRight = GeometryHelpers.IsLeftOfOrOn(curTriApex, destination, curTriRight);

                    if (apexInFunnelLeft && apexInFunnelRight)
                    {
                        // the apex of the next triangle in the corridor is within the funnel. Narrow the funnel.
                        var prevEdge = corridor.Previous.Edge;
                        var prevTriangle = corridor.Previous.Previous.Triangle;
                        mesh.GetOutsideOrderedEdgePointsPlusApex(prevTriangle, prevEdge, out prevTriRight3d,
                                                                 out prevTriLeft3d, out prevTriApex3d);

                        var prevTriApex = prevTriApex3d.XY;
                        var prevTriLeft = prevTriLeft3d.XY;
                        var prevTriRight = prevTriRight3d.XY;

                        if (curTriLeft == prevTriLeft)
                        {
                            curTriRight3d = curTriApex3d;
                            curTriRight = curTriApex;
                        }
                        else if (curTriRight == prevTriRight)
                        {
                            curTriLeft3d = curTriApex3d;
                            curTriLeft = curTriApex;
                        }
                        else
                        {
                            throw new Exception("WTF! You screwed the pooch here pal, find the error.");
                        }

                        curTriApex3d = prevTriApex3d;
                        curTriApex = prevTriApex;
                    }
                    else if (!apexInFunnelLeft && apexInFunnelRight)
                    {
                        destination3d = curTriLeft3d;
                        pathNodes.Add(destination3d);
                        //path.Add(destination3d);
                        foundNewDestination = true;
                    }
                    else if (apexInFunnelLeft)
                    {
                        destination3d = curTriRight3d;
                        pathNodes.Add(destination3d);
                        //path.Add(destination3d);
                        foundNewDestination = true;
                    }
                    else
                    {
                        throw new Exception("This should never happen.");
                    }

                    corridor = corridor.Previous;
                }
            }

            // The next triangle in the corridor contains the starting point
            curTriangle = corridor.Previous.Triangle;
            curEdge = corridor.Edge;
            mesh.GetOutsideOrderedEdgePointsPlusApex(curTriangle, curEdge, out curTriRight3d, out curTriLeft3d,
                                                     out curTriApex3d);
            destination = destination3d.XY;
            curTriApex = origin3d.XY;
            curTriLeft = curTriLeft3d.XY;
            curTriRight = curTriRight3d.XY;
            apexInFunnelLeft = GeometryHelpers.IsRightOfOrOn(curTriApex, destination, curTriLeft);
            apexInFunnelRight = GeometryHelpers.IsLeftOfOrOn(curTriApex, destination, curTriRight);

            if (apexInFunnelLeft && apexInFunnelRight)
            {
                // the starting point of the corridor is within the funnel. We're done.
                pathNodes.Add(origin3d);
                //path.Add(origin3d);
            }
            else if (!apexInFunnelLeft && apexInFunnelRight)
            {
                // the starting point of the corridor is outside the funnel on the left
                //add the left point and then the starting point
                pathNodes.Add(curTriLeft3d);
                pathNodes.Add(origin3d);
                //path.Add(curTriLeft3d);
                //path.Add(origin3d);
            }
            else if (apexInFunnelLeft)
            {
                // the starting point of the corridor is outside the funnel on the right
                //add the right point and then the starting point
                pathNodes.Add(curTriRight3d);
                pathNodes.Add(origin3d);
                //path.Add(curTriRight3d);
                //path.Add(origin3d);
            }
            else
            {
                throw new Exception("This should never happen.");
            }

            StringPull(corridorStart, pathNodes);
            path.Reset(pathNodes.Count);
            path.Add(pathNodes);
        }

        private void StringPull(SearchItem corridor, List<Vector3> points)
        {
            if (points.Count < 3) return;
            for (var curIndex = 0; curIndex < (points.Count - 2); curIndex++)
            {
                var curPoint = points[curIndex];
                for (var nextIndex = (curIndex + 2); nextIndex < points.Count; nextIndex++)
                {
                    var nextPoint = points[nextIndex];
                    
                    var corridorStart = corridor;
                    if (!HasLOSXY(curPoint.XY, nextPoint.XY, corridorStart)) continue;

                    var removeAt = curIndex + 1;
                    while(removeAt < nextIndex)
                    {
                        points.RemoveAt(removeAt);
                        nextIndex--;
                    }
                }
            }
        }

        private bool HasLOSXY(Vector2 pointA, Vector2 pointB, SearchItem corridor)
        {
            // Find the triangle containing pointA
            while(corridor != SearchItem.Null)
            {
                if (NavMesh.TriangleContainsPointXY(corridor.Triangle, pointA))
                {
                    break;
                }
                corridor = corridor.Previous;
            }
            
            // If no triangle contains pointA, then fail
            if (corridor == SearchItem.Null)
            {
                Console.WriteLine("No triangle contained pointA.");
                return false;
            }

            Console.WriteLine("Made it past.");
            // Are the points within the same triangle? Notice that triangles are guaranteed convex...
            if (NavMesh.TriangleContainsPointXY(corridor.Triangle, pointB)) return true;

            while (corridor.Previous != SearchItem.Null)
            {
                // We want to check that the line segment [pointA, pointB] intersects the edge we crossed
                var curTriangle = corridor.Previous.Triangle;
                var edge = corridor.Edge;

                Vector3 right3d, left3d;
                NavMesh.GetOutsideOrderedEdgePoints(curTriangle, edge, out right3d, out left3d);

                var right = right3d.XY;
                var left = left3d.XY;
                if (!Intersection.LineSegmentsIntersect2D(pointA, pointB, right, left)) return false;

                // Ok, the move into this triangle was legal, if PointB is in the tri, then we're done
                if (NavMesh.TriangleContainsPointXY(curTriangle, pointB)) return true;

                // Move to the next edge check ...
                corridor = corridor.Previous;
            }
            
            // Default
            return false;
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
				//var triangle = mesh.GetTriangle(current.Triangle);
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
                    mesh.GetInsideOrderedEdgePoints(current.Triangle, edge, out right, out left);
                    //switch (edge)
                    //{
                    //    case TerrainUtil.ABEdgeIndex:
                    //        left = triangle.Point1;
                    //        right = triangle.Point2;
                    //        break;
                    //    case TerrainUtil.CAEdgeIndex:
                    //        left = triangle.Point3;
                    //        right = triangle.Point1;
                    //        break;
                    //    case TerrainUtil.BCEdgeIndex:
                    //        left = triangle.Point2;
                    //        right = triangle.Point3;
                    //        break;
                    //    default:
                    //        throw new Exception("Impossible");
                    //}

					var refPoint = (left + right) / 2.0f;		// the middle of the edge to be traversed
					var dist = Vector3.Distance(refPoint, destination);
					var newItem = new SearchItem(neighbor, edge, dist, ref refPoint, current);

					if (triEnd == neighbor)
					{
						// we are done
						corridor = newItem;
						goto Done;
					}

				    fringe.Add(newItem);
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
