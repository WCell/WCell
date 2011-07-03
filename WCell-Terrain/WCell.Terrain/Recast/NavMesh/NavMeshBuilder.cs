using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using WCell.Constants;
using WCell.Util.Graphics;

namespace WCell.Terrain.Recast.NavMesh
{
	public class NavMeshBuilder
	{
		public static string GenerateTileName(TerrainTile tile)
		{
			return string.Format("tile_{0}_{1}", tile.TileX, tile.TileY);
		}

		public static int GenerateTileId(TerrainTile tile)
		{
			return tile.TileX + tile.TileY * TerrainConstants.TilesPerMapSide;
		}

		public static Point2D GetTileCoords(int tileId)
		{
			return new Point2D(tileId % TerrainConstants.TilesPerMapSide, tileId / TerrainConstants.TilesPerMapSide);
		}

		public readonly Terrain Terrain;
		private readonly RecastAPI.BuildMeshCallback SmashMeshDlgt;

		public NavMeshBuilder(Terrain terrain)
		{
			Terrain = terrain;
			SmashMeshDlgt = SmashMesh;
		}

		public string InputMeshPrefix
		{
			get
			{
				Directory.CreateDirectory(WCellTerrainSettings.RecastInputMeshFolder);
				return WCellTerrainSettings.RecastInputMeshFolder + Terrain.MapId;
			}
		}

		public string NavMeshPrefix
		{
			get
			{
				Directory.CreateDirectory(WCellTerrainSettings.RecastNavMeshFolder);
				return WCellTerrainSettings.RecastNavMeshFolder + Terrain.MapId;
			}
		}

		/// <summary>
		/// Builds the mesh for a single tile.
		/// NOTE: This overrides the Terrain's NavMesh property.
		/// In order to move between tiles, we need to generate one mesh for the entire map
		/// </summary>
		public void BuildMesh(TerrainTile tile)
		{
			// export to file
			var tileName = GenerateTileName(tile);
			var inputMeshFile = InputMeshPrefix + "_" + tileName + RecastAPI.InputMeshExtension;
			var navMeshFile = NavMeshPrefix + "_" + tileName + RecastAPI.NavMeshExtension;
			ExportRecastInputMesh(tile, inputMeshFile);

			// let recast to build the navmesh and then call our callback
			if (!File.Exists(navMeshFile))
			{
				Console.Write("Building new NavMesh - This will take a while...");
			}
			var start = DateTime.Now;
			var result = RecastAPI.BuildMesh(
				GenerateTileId(tile),
				inputMeshFile,
				navMeshFile,
				SmashMeshDlgt);

			if (result == 0)
			{
				throw new Exception("Could not build mesh for tile " + tileName + " in map " + Terrain.MapId);
			}
			
			if (!File.Exists(navMeshFile))
			{
				Console.WriteLine("Done in {0:0.000}s", (DateTime.Now - start).TotalSeconds);
			}
		}

		public void ExportRecastInputMesh(TerrainTile tile, string filename)
		{
			if (File.Exists(filename)) return;			// skip existing files
			
			var verts = tile.TerrainVertices;
			var indices = tile.TerrainIndices;

			var start = DateTime.Now;
			Console.Write("Writing file {0}...", filename);

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
			var polyAreasAndTypes = (byte*) polyAreasAndTypesPtr;


			// create vertices array
			var vertCount = vertComponentCount / 3;
			var vertices = new Vector3[vertCount];
			for (int i = 0, v = 0; i < vertCount; i++, v += 3)
			{
			    vertices[i] = new Vector3(vertComponents[v], vertComponents[v + 1], vertComponents[v + 2]);
			    RecastUtil.TransformRecastCoordsToWoWCoords(ref vertices[i]);
			}

			// polygon first pass -> Create polygons
			var polys = new NavMeshPolygon[polyCount];
			var polyEdgeIndex = 0;
			for (var i = 0; i < polyCount; i++)
			{
			    var poly = polys[i] = new NavMeshPolygon();
			    var polyIndexCount = pIndexCounts[i];
			    poly.Indices = new int[polyIndexCount];
			    for (var j = 0; j < polyIndexCount; j++)
			    {
			        poly.Indices[j] = (int) pIndices[polyEdgeIndex + j];
			        Debug.Assert(poly.Indices[j] >= 0 && poly.Indices[j] < vertCount);
			    }

			    polyEdgeIndex += polyIndexCount;
			}

			// polygon second pass -> Initialize neighbors
			polyEdgeIndex = 0;
			for (var i = 0; i < polyCount; i++)
			{
			    var poly = polys[i];
			    var polyIndexCount = pIndexCounts[i];
			    poly.Neighbors = new NavMeshPolygon[polyIndexCount];
			    for (var j = 0; j < polyIndexCount; j++)
			    {
			        var neighbor = (int)pNeighbors[polyEdgeIndex + j];
			        if (neighbor == -1) continue;

			        poly.Neighbors[j] = polys[neighbor];
			    }

			    polyEdgeIndex += polyIndexCount;
			}

			// create new NavMesh object
			Terrain.NavMesh = new NavMesh(Terrain, polys, vertices);
		}
	}
}
