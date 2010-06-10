using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MPQNav;
using MPQNav.MPQ;

namespace TerrainDisplay.Recast
{
	/// <summary>
	/// Run the Recast GUI with our data
	/// </summary>
	public class RecastRunner
	{
		private readonly ITerrainManager _manager;
		Vector3[] vectors;
		int[] indices;
		private PtrBoolCallback GenerateMeshCallback;

		private VertexPositionNormalColored[] _cachedVertices;


		public RecastRunner(ITerrainManager manager)
		{
			_manager = manager;
			GenerateMeshCallback = GenerateMesh;
		}

		private void BuildVerticiesAndIndicies()
		{
			Vector3[] vectors;
			int[] indices;
			_manager.GetRecastTriangleMesh(out vectors, out indices);



			_cachedVertices = new VertexPositionNormalColored[vectors.Length];
			//_cachedIndices = indices;
			for (var i = 0; i < vectors.Length; i++)
			{
				_cachedVertices[i] = new VertexPositionNormalColored(vectors[i], Color.White, Vector3.Up);
			}

		}

		public bool GenerateMesh(IntPtr geom)
		{
			//Console.WriteLine("GenMesh");

			_manager.GetRecastTriangleMesh(out vectors, out indices);
			RecastAPI.GenerateMesh(geom, vectors, indices, "WoW");
			return true;
		}

		public void Start()
		{
			Environment.CurrentDirectory = RecastAPI.RecastFolder;
			//RecastAPI.RemoveMeshGenerator("xxx");

			RecastAPI.AddMeshGenerator("[Test]", GenerateMeshCallback);

			RecastAPI.SetNavSpeed(100);
			RecastAPI.StartRecast();
		}
	}
}
