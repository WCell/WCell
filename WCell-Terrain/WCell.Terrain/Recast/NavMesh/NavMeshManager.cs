using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WCell.Terrain.MPQ;
using TerrainDisplay.Util;
using WCell.Util.Graphics;

namespace TerrainDisplay.Recast
{
    public class NavMeshManager
    {
    	private NavMesh _mesh;

		public NavMeshManager(ITerrainManager manager)
		{
			Manager = manager;

			// configure Recast
			RecastAPI.InitAPI();
			RecastAPI.AddInputMeshGenerator("[Default]", GenerateInputMesh);		// Input
			RecastAPI.NavMeshGenerated += SetNavMesh;								// Ouptut
		}

		bool GenerateInputMesh(IntPtr geom)
		{
			//Console.WriteLine("GenMesh");
			Vector3[] vectors;
			int[] indices;

			// TODO: Call on all vertices: PositionUtil.TransformWoWCoordsToXNACoords(ref vertex);
			Manager.GetRecastTriangleMesh(out vectors, out indices);
			RecastAPI.GenerateInputMesh(geom, vectors, indices, "[Default]");
			return true;
		}

    	public ITerrainManager Manager
    	{
    		get; 
			private set;
    	}

        public void SetNavMesh(NavMesh mesh)
        {
            _mesh = mesh;
			lock (this)
			{
				Monitor.PulseAll(this);
			}
        }

        public void GetMeshVerticesAndIndices(out List<Vector3> vertices, out List<int> indices)
		{
			vertices = new List<Vector3>();
			indices = new List<int>();

			if (_mesh == null)
			{
				//lock (this)
				//{
				//    if (_mesh == null)			// check again, after obtaining lock
				//    {
						
				//        lock (this)
				//        {
				//            Monitor.Wait(this);
				//        }
				//    }
				//}
				return;
			}

            var tiles = _mesh.Tiles;
            var debugCount = 0;
            for (var x = 0; x < _mesh.Width; x++)
            {
                for (var y = 0; y < _mesh.Height; y++)
                {
                    var tile = tiles[x, y];
                    if (tile == null) continue;
                    debugCount++;
                    //if (debugCount != 5) continue;
                    

                    var offset = vertices.Count();
                    for (var i = 0; i < tile.Vertices.Length; i++)
                    {
                        var vertex = tile.Vertices[i];
                        vertices.Add(vertex);
                    }

                    var detailOffset = vertices.Count;
                    for (var i = 0; i < tile.DetailedVertices.Length; i++)
                    {
                        var vertex = tile.DetailedVertices[i];
                        vertices.Add(vertex);
                    }

                    for (int i = 0; i < tile.Polygons.Length; i++)
                    {
                        var polygon = tile.Polygons[i];
                        if (polygon == null) continue;
                        if (polygon.Type == NavMeshPolyTypes.OffMeshConnection) continue;

                        var detail = tile.DetailPolygons[i];
                        for (var j = 0; j < detail.TriCount; j++)
                        {
                            var detailTri = tile.DetailedTriangles[detail.TriBase + j];
                            for (var k = 0; k < 3; k++)
                            {
                                var curVertIdx = detailTri[k];
                                if (curVertIdx < polygon.VertCount)
                                {
                                    var polyVert = polygon.Vertices[curVertIdx];
                                    indices.Add(offset + polyVert);
                                }
                                else
                                {
                                    var detailIdx = (detail.VertBase + curVertIdx - polygon.VertCount);
                                    indices.Add(detailOffset + detailIdx);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
