using System;
using WCell.Terrain.GUI.Recast;
using WCell.Terrain.GUI.Util;
using WCell.Terrain.Recast.NavMesh;
using WCell.Util.Graphics;

namespace WCell.Terrain.GUI
{
	/// <summary>
	/// Run the Recast GUI with our data.
	/// TODO: Use the new Recast generation code to move data, render and run it
	/// </summary>
	public class RecastRunner
	{
		private PtrBoolCallback InputMeshGenerator;

	    Vector3[] vertices;
		int[] indices;

		private VertexPositionNormalColored[] _cachedVertices;


		public RecastRunner()
		{
            //InputMeshGenerator = GenerateInputMesh;
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

		///// <summary>
		///// TODO: Add Identifier for mesh to be generated
		///// </summary>
		///// <param name="geom"></param>
		///// <returns></returns>
		//bool GenerateInputMesh(IntPtr geom)
		//{
		//    //Console.WriteLine("GenMesh");
		//    _manager.GetRecastTriangleMesh(out vertices, out indices);
		//    for (var i = 0; i < vertices.Length; i++)
		//    {
		//        XNAUtil.TransformWoWCoordsToXNACoords(ref vertices[i]);
		//    }
		//    RecastAPI.GenerateInputMesh(geom, vertices, indices, "Default");
		//    return true;
		//}

		//public void Start()
		//{
		//    //RecastAPI.RemoveMeshGenerator("xxx");
		//    RecastAPI.InitAPI();
		//    RecastAPI.AddInputMeshGenerator("[Default]", InputMeshGenerator);		// Input
		//    RecastAPI.NavMeshGenerated += OnNewNavMesh; // Ouptut
		//    RecastAPI.SetNavSpeed(100.0f);
		//    RecastAPI.RunRecast();
		//}

		//private void OnNewNavMesh(NavMesh mesh)
		//{
		//    // toy around with mesh
		//    _manager.MeshLoader.SetNavMesh(mesh);
		//}
	}
}
