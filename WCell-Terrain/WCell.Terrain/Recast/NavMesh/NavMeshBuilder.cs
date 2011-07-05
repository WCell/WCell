using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Util;
using WCell.Util.Graphics;
using WCell.Terrain.Collision;

namespace WCell.Terrain.Recast.NavMesh
{
	public class NavMeshBuilder
	{
		public static int GenerateTileId(TerrainTile tile)
		{
			return tile.TileX + tile.TileY * TerrainConstants.TilesPerMapSide;
		}

		public static Point2D GetTileCoords(int tileId)
		{
			return new Point2D(tileId % TerrainConstants.TilesPerMapSide, tileId / TerrainConstants.TilesPerMapSide);
		}

		public static bool DoesNavMeshExist(MapId map, int tileX, int tileY)
		{
			return File.Exists(GetNavMeshFile(map, tileX, tileY));
		}

		public static string GetNavMeshFile(MapId map, int tileX, int tileY)
		{
			var tileName = TerrainConstants.GetTileName(tileX, tileY);
			return WCellTerrainSettings.RecastNavMeshFolder + (int)map + "/" + tileName + RecastAPI.NavMeshExtension;
		}

		public static string GetInputMeshFile(MapId map, int tileX, int tileY)
		{
			var tileName = TerrainConstants.GetTileName(tileX, tileY);
			return WCellTerrainSettings.RecastInputMeshFolder + (int)map + "/" + tileName + RecastAPI.InputMeshExtension;
		}

		public readonly TerrainTile Tile;
		private readonly RecastAPI.BuildMeshCallback SmashMeshDlgt;

		public NavMeshBuilder(TerrainTile tile)
		{
			SmashMeshDlgt = SmashMesh;
			Tile = tile;
		}

		public Terrain Terrain
		{
			get { return Tile.Terrain; }
		}

		public Vector3 Min
		{
			get;
			private set;
		}

		public Vector3 Max
		{
			get;
			private set;
		}

		public void ExportRecastInputMesh(TerrainTile tile, string filename)
		{
			if (File.Exists(filename)) return;			// skip existing files

			var verts = tile.TerrainVertices;
			var indices = tile.TerrainIndices;

			var start = DateTime.Now;
			Console.Write("Writing input file {0}...", filename);

			using (var file = new StreamWriter(filename))
			{
				foreach (var vertex in verts)
				{
					var v = vertex;
					RecastUtil.TransformWoWCoordsToRecastCoords(ref v);
					file.WriteLine("v {0} {1} {2}", v.X, v.Y, v.Z);
				}

				// write faces
				for (var i = 0; i < indices.Length; i += 3)
				{
					//file.WriteLine("f {0} {1} {2}", indices[i] + 1, indices[i + 1] + 1, indices[i + 2] + 1);
					file.WriteLine("f {0} {1} {2}", indices[i + 2] + 1, indices[i + 1] + 1, indices[i] + 1);
				}
			}
			Console.WriteLine("Done. - Exported {0} triangles in: {1:0.000}s",
									indices.Length / 3, (DateTime.Now - start).TotalSeconds);
		}

		/// <summary>
		/// Builds the mesh for a single tile.
		/// NOTE: This sets the Terrain's NavMesh property.
		/// In order to move between tiles, we need to generate one mesh for the entire map
		/// </summary>
		public void BuildMesh(TerrainTile tile)
		{
			// let recast build the navmesh and then call our callback
			var inputFile = GetInputMeshFile(tile.Terrain.MapId, tile.TileX, tile.TileY);
			var navMeshFile = GetNavMeshFile(tile.Terrain.MapId, tile.TileX, tile.TileY);
			var exists = DoesNavMeshExist(tile.Terrain.MapId, tile.TileX, tile.TileY);

			if (!exists)
			{
				Directory.CreateDirectory(new FileInfo(inputFile).Directory.FullName);
				Directory.CreateDirectory(new FileInfo(navMeshFile).Directory.FullName);
				// export input mesh to file
				ExportRecastInputMesh(tile, inputFile);

				Console.WriteLine("Building new NavMesh...");
			}

			var start = DateTime.Now;
			var result = RecastAPI.BuildMesh(
				GenerateTileId(tile),
				inputFile,
				navMeshFile,
				SmashMeshDlgt);

			if (result == 0)
			{
				throw new Exception("Could not build mesh for tile " + TerrainConstants.GetTileName(tile.TileX, tile.TileY) + " in map " + Terrain.MapId);
			}

			if (!exists)
			{
				Console.WriteLine("Done in {0:0.000}s", (DateTime.Now - start).TotalSeconds);
			}

			//// move all vertices above the surface
			//var mesh = tile.Terrain.NavMesh;
			//for (var i = 0; i < mesh.Vertices.Length; i++)
			//{
			//    var vert = mesh.Vertices[i];
			//    vert.Z += 0.001f;
			//    var ray = new Ray(vert, Vector3.Down);	// see if vertex is above surface
			//    var hit = tile.FindFirstHitTriangle(ray);
			//    if (hit == -1)
			//    {
			//        vert.Z -= 0.001f;

			//        // vertex is below surface
			//        ray = new Ray(vert, Vector3.Up);	// find surface right above mesh
			//        hit = tile.FindFirstHitTriangle(ray);
			//        if (hit != -1)
			//        {
			//            // set vertex height equal to terrain
			//            var tri = tile.GetTriangle(hit);
			//            var plane = new Plane(tri.Point1, tri.Point2, tri.Point3);
			//            Intersection.LineSegmentIntersectsPlane(ray.Position, ray.Direction, plane, out mesh.Vertices[i]);
			//        }
			//    }
			//}
		}

		/// <summary>
		/// Put it all together
		/// </summary>
		private unsafe void SmashMesh(
			int userId,
			int vertComponentCount,
			int polyCount,
			IntPtr vertComponentsPtr,

			int totalPolyIndexCount,				// count of all vertex indices in all polys
			IntPtr pIndexCountsPtr,
			IntPtr pIndicesPtr,
			IntPtr pNeighborsPtr,
			IntPtr pFlagsPtr,
			IntPtr polyAreasAndTypesPtr
			)
		{
			//var coords = GetTileCoords(userId);
			//var tile = Terrain.GetTile(coords);

			//if (tile == null)
			//{
			//    throw new Exception("Invalid tile: " + id);
			//}

			// read native data
			var vertComponents = (float*)vertComponentsPtr;
			var pIndexCounts = (byte*)pIndexCountsPtr;
			var pIndices = (uint*)pIndicesPtr;
			var pNeighbors = (uint*)pNeighborsPtr;
			var pFlags = (ushort*)pFlagsPtr;
			var polyAreasAndTypes = (byte*)polyAreasAndTypesPtr;


			// create vertices array
			var indices = new int[totalPolyIndexCount];
			var vertCount = vertComponentCount / 3;

			List<Vector3> vertexList;
			using (LargeObjectPools.Vector3ListPool.Borrow(out vertexList))
			{
				var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
				var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

				for (int i = 0, v = 0; i < vertCount; i++, v += 3)
				{
					var vert = new Vector3(vertComponents[v], vertComponents[v + 1], vertComponents[v + 2]);
					RecastUtil.TransformRecastCoordsToWoWCoords(ref vert);
					vertexList.Add(vert);

					// expand bounding box
					min.X = Math.Min(min.X, vert.X);
					min.Y = Math.Min(min.Y, vert.Y);
					min.Z = Math.Min(min.Z, vert.Z);

					max.X = Math.Max(max.X, vert.X);
					max.Y = Math.Max(max.Y, vert.Y);
					max.Z = Math.Max(max.Z, vert.Z);
				}

				Min = min;
				Max = max;

				var vertices = EliminateDoubleVertices(vertexList, pIndices, totalPolyIndexCount);

				// polygon first pass -> Create polygons
				var polys = new NavMeshPolygon[polyCount];
				var polyEdgeIndex = 0;
				var p = 0;
				for (var i = 0; i < polyCount; i++)
				{
					var polyIndexCount = pIndexCounts[i];
					var poly = polys[i] = new NavMeshPolygon();
					poly.Indices = new int[polyIndexCount];
					poly.Neighbors = new int[polyIndexCount];

					Debug.Assert(3 == polyIndexCount);

					for (var j = 0; j < polyIndexCount; j++)
					{
						var idx = (int)pIndices[polyEdgeIndex + j];
						indices[p++] = idx;
						poly.Indices[j] = idx;
						poly.Neighbors[j] = -1;

						Debug.Assert(poly.Indices[j] >= 0 && poly.Indices[j] < vertCount);
					}

					// switch first and third index because our collision test needs the triangles to go in the other direction
					var tmp = poly.Indices[0];
					indices[p - 3] = poly.Indices[0] = poly.Indices[2];
					indices[p - 1] = poly.Indices[2] = tmp;

					polyEdgeIndex += polyIndexCount;
				}

				// polygon second pass -> Initialize neighbors
				polyEdgeIndex = 0;

				var undecidedNeighborRelations = new List<Tuple<int, int>>();
				for (var i = 0; i < polyCount; i++)
				{
					var poly = polys[i];
					var polyIndexCount = pIndexCounts[i];
					var a = poly.Indices[0];
					var b = poly.Indices[1];
					var c = poly.Indices[2];

					for (var j = 0; j < polyIndexCount; j++)
					{
						var neighbor = (int)pNeighbors[polyEdgeIndex + j];
						if (neighbor == -1) continue;

						var neighborPoly = polys[neighbor];

						// sort the neighbor poly into the array of neighbors, correctly
						var a2 = neighborPoly.Indices[0];
						var b2 = neighborPoly.Indices[1];
						var c2 = neighborPoly.Indices[2];


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


						//var va = vertices[a];
						//var vb = vertices[b];
						//var vc = vertices[c];
						//var ua = vertices[a2];
						//var ub = vertices[b2];
						//var uc = vertices[c2];

						//var vs = new List<Vector3>() { va, vb, vc };
						//var us = new List<Vector3>() { ua, ub, uc };
						//var mc = 0;
						//for (var ii = 0; ii < vs.Count; ii++)
						//{
						//    var vv = vs[ii];
						//    for (var jj = 0; jj < us.Count; jj++)
						//    {
						//        var uu = us[jj];
						//        if (vv.Equals(uu))
						//        {
						//            mc++;
						//            break;
						//        }
						//    }
						//}
						//Debug.Assert(mc == 2);
						//Debug.Assert(nCount == 2);

						if (nCount < 2)
						{
							undecidedNeighborRelations.Add(Tuple.Create(i, neighbor));
						}

						var edge = 0;
						switch (mask)
						{
							case WCellTerrainConstants.ABEdgeMask:
								// neighbor shares a and b
								edge = WCellTerrainConstants.ABEdgeIndex;
								break;
							case WCellTerrainConstants.ACEdgeMask:
								// second shares a and c
								edge = WCellTerrainConstants.ACEdgeIndex;
								break;
							case WCellTerrainConstants.BCEdgeMask:
								// neighbor shares b and c
								edge = WCellTerrainConstants.BCEdgeIndex;
								break;
							default:
								continue;
						}

						poly.Neighbors[edge] = neighbor;
					}

					polyEdgeIndex += polyIndexCount;
				}

				//// just sort the not quite connected neighbor into a free slot (and hope its correct)
				//foreach (var rel in undecidedNeighborRelations)
				//{
				//    var poly = polys[rel.Item1];

				//    for (var i = 0; i < poly.Neighbors.Length; i++)
				//    {
				//        var neigh = poly.Neighbors[i];
				//        if (neigh == -1)
				//        {
				//            poly.Neighbors[i] = rel.Item2;
				//            break;
				//        }
				//    }
				//}

				// create new NavMesh object
				Tile.NavMesh = new NavMesh(Tile, polys, vertices, indices);
			}
		}

		/// <summary>
		/// Make sure that all vertices that are shared are referenced, using the same index.
		/// Uses locality hashing to achieve the elimination process in amortized O(n)
		/// </summary>
		private unsafe Vector3[] EliminateDoubleVertices(List<Vector3> vertices, uint* indices, int indexCount)
		{
			var vertCount = vertices.Count;
			List<Vector3> newVertices;
			using (LargeObjectPools.Vector3ListPool.Borrow(out newVertices))
			{
				// TODO: Also recycle these
				var hashtable = new List<Tuple<int, Vector3>>[vertCount];

				var maxLength = Vector3.Distance(Min, Max);
				var actualIndices = new int[vertCount];
				for (var i = 0; i < vertices.Count; i++)
				{
					var vert = vertices[i];

					// use relative length as hash
					var len = Vector3.Distance(vert, Min) + MathUtil.Epsilonf; // prevent division by zero
					var index = (int)((len / maxLength) * vertCount - 1);

					// search through up to 3 candidate lists for vertices that are equal to this one
					var match = false;
					for (int j = Math.Max(0, index - 1), maxj = Math.Min(vertCount - 1, index + 1); j < maxj; j++)
					{
						var neighborList = hashtable[j];
						if (neighborList != null)
						{
							foreach (var neighbor in neighborList)
							{
								if (neighbor.Item2.Equals(vert))
								{
									Debug.Assert(vert.Equals(neighbor.Item2));
									// we got a match -> Redirect index
									match = true;
									actualIndices[i] = neighbor.Item1;
								}
							}
						}
					}

					if (!match)
					{
						// no match -> add unique vertex
						var list = hashtable[index];
						if (list == null)
						{
							hashtable[index] = list = new List<Tuple<int, Vector3>>(5);
						}
						var actualIndex = newVertices.Count;
						list.Add(Tuple.Create(actualIndex, vert));
						actualIndices[i] = actualIndex;
						newVertices.Add(vert);
					}
				}

				// set vertices to the new list
				for (var i = 0; i < indexCount; i++)
				{
					var idx = indices[i];
					var origVert = vertices[(int) idx];
					var newVert = newVertices[actualIndices[idx]];
					Debug.Assert(origVert.Equals(newVert));
					indices[i] = (uint)actualIndices[idx];
				}

				return newVertices.ToArray();
			}
		}
	}
}
