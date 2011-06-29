using System;
using WCell.Util.Graphics;
using TerrainDisplay.MPQ;

namespace TerrainDisplay.Recast
{
	/// <summary>
	/// Run the Recast GUI with our data
	/// </summary>
	public class RecastRunner
	{
		private PtrBoolCallback InputMeshGenerator;

		private readonly ITerrainManager _manager;
	    Vector3[] vectors;
		int[] indices;

		private VertexPositionNormalColored[] _cachedVertices;



		public RecastRunner(ITerrainManager manager)
		{
			_manager = manager;
            InputMeshGenerator = GenerateInputMesh;
		}

        //private void BuildVerticiesAndIndicies()
        //{
        //    Vector3[] vertices;
        //    _manager.GetRecastTriangleMesh(out vertices, out indices);



        //    _cachedVertices = new VertexPositionNormalColored[vectors.Length];
        //    //_cachedIndices = indices;
        //    for (var i = 0; i < vectors.Length; i++)
        //    {
        //        _cachedVertices[i] = new VertexPositionNormalColored(vectors[i], Color.White, Vector3.Up);
        //    }

        //}

		/// <summary>
		/// TODO: Add Identifier for mesh to be generated
		/// </summary>
		/// <param name="geom"></param>
		/// <returns></returns>
		bool GenerateInputMesh(IntPtr geom)
		{
			//Console.WriteLine("GenMesh");
			_manager.GetRecastTriangleMesh(out vectors, out indices);
			RecastAPI.GenerateInputMesh(geom, vectors, indices, "Default");
			return true;
		}

		public void Start()
		{
			//RecastAPI.RemoveMeshGenerator("xxx");
			RecastAPI.InitAPI();
			RecastAPI.AddInputMeshGenerator("[Default]", InputMeshGenerator);		// Input
			RecastAPI.NavMeshGenerated += OnNewNavMesh; // Ouptut
			RecastAPI.SetNavSpeed(100.0f);
			RecastAPI.RunRecast();
		}

		private void OnNewNavMesh(NavMesh mesh)
		{
			// toy around with mesh
		    _manager.MeshManager.SetNavMesh(mesh);
		}
	}
}
