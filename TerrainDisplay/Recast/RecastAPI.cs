using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using WCell.Util.Graphics;
using WCell.Util.Variables;

namespace TerrainDisplay.Recast
{
	public delegate void PtrVoidCallback(IntPtr ptr);
	public delegate bool PtrBoolCallback(IntPtr ptr);

	public delegate void NavMeshGeneratedHandler(NavMesh mesh);
	public delegate void NavMeshTileGeneratedHandler(NavMeshTile tile);

	public static class RecastAPI
	{
		#region Native Delegates

		delegate void _NavMeshGeneratedHandler(
			long navMeshId, int w, int h,
			[MarshalAs(UnmanagedType.LPArray, SizeConst = 3)]float[] origin,
			float tileW, float tileH, int maxTiles
			);

		delegate void _NavMeshDoneHandler(long navMeshId);

		delegate void _NavMeshTileGeneratedHandler(
			long navMeshId, int x, int y, int nextX, int nextY, uint flags,
			int vertCount, 
                [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 6)]float[] vertices,		// vertices
			
            int pCount, int pVCount,																	// polygons
				[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 8)]uint[] pFlinks,
				[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 9)]ushort[] pVerts,
				[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 9)]ushort[] pNeighbors,
				[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 8)]ushort[] pFlags,
				[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 8)]ushort[] pvCounts,
				[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 8)]byte[] pAreas,
				[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 8)]byte[] pTypes,

			int lCount,
				[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 17)]uint[] lRefs,
				[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 17)]uint[] lNextLinks,
				[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 17)]byte[] lEdges,
				[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 17)]byte[] lSides,
				[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 17)]byte[] lBMins,
				[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 17)]byte[] lBMaxs,

            int dCount,
                [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 24)]ushort[] dVBases,
                [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 24)]ushort[] dVCounts,
                [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 24)]ushort[] dTBases,
                [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 24)]ushort[] dTCounts,
                
            int dVCount,
                [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 29)]float[] dVerts, 
                
            int dTCount,
                [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 31)]byte[] dTris,

			int omCount,
				[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 33)]float[] omPos,
				[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 33)]float[] omRads,
				[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 33)]ushort[] omPolys,
				[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 33)]byte[] omFlags,
				[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 33)]byte[] omSides
			);

		#endregion

		#region Misc & Init
		public const string RecastDllName = "Recast.dll";
		public const string RecastFolder = "../../../../Recast/Bin/";

		public static readonly string[] Dlls = new[] { RecastDllName, "SDL.dll" };

		public const int MaxVertsPerPolygon = 6;

        private static bool _addHooks = true;

		/// <summary>
		/// Whether to register hooks to recast, to be notified upon new meshes and tiles
		/// </summary>
		[NotVariable]
        public static bool AddHooks
		{
			get { return _addHooks; }
			set
			{
				_addHooks = value;
				if (inited)
				{
					if (value)
					{
						DoAddHooks();
					}
					else
					{
						SetNavMeshGeneratedCallback(null);
						SetNavMeshTileAddedCallback(null);
						SetNavMeshDoneCallback(null);
					}
				}
			}
		}

		static void DoAddHooks()
		{
			SetNavMeshGeneratedCallback(_NavMeshGeneratedCallback);
			SetNavMeshTileAddedCallback(_NavMeshTileGeneratedCallback);
			SetNavMeshDoneCallback(_NavMeshDoneDoneCallback);
		}

		private static bool inited;

		public static void InitAPI()
		{
			if (inited)
			{
				throw new InvalidOperationException("Tried to initialize API twice.");
			}
			inited = true;
			// must do this, so it finds the dlls and other files (since those paths arent configurable)
			Environment.CurrentDirectory = RecastFolder;

			// ensure data correctness
			if (MaxVertsPerPolygon != GetMaxVertsPerPolygon())
			{
				throw new Exception("MaxVertsPerPolygon does not match with the settings of the underlying " + RecastDllName);
			}

			if (AddHooks)
			{
				DoAddHooks();
			}
		}

		private static readonly _NavMeshGeneratedHandler _NavMeshGeneratedCallback = OnNavMeshGenerated;
		private static readonly _NavMeshTileGeneratedHandler _NavMeshTileGeneratedCallback = OnNavMeshTileGenerated;
		private static readonly _NavMeshDoneHandler _NavMeshDoneDoneCallback = OnNavMeshDone;

		#endregion

		/// <summary>
		/// Runs the Recast GUI
		/// </summary>
		[DllImport(RecastDllName, EntryPoint = "startrecast", CallingConvention = CallingConvention.Cdecl)]
		public static extern void RunRecast();

		/// <summary>
		/// Set navigation speed for the GUI
		/// </summary>
		[DllImport(RecastDllName, EntryPoint = "navSetSpeed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SetNavSpeed(float speed);

		#region Input Mesh
		/// <summary>
		/// Add callback with the given name into the Mesh selection list within the GUI
		/// </summary>
		[DllImport(RecastDllName, EntryPoint = "meshGenAdd", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern void AddInputMeshGenerator(
			[MarshalAs(UnmanagedType.LPStr)] string name,
			[MarshalAs(UnmanagedType.FunctionPtr)] PtrBoolCallback callback);

		/// <summary>
		/// Remove the callback with the given name from the Mesh selection list within the GUI
		/// </summary>
		/// <param name="name"></param>
		[DllImport(RecastDllName, EntryPoint = "meshGenRemove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern void RemoveMeshGenerator(
			[MarshalAs(UnmanagedType.LPStr)] string name
		);

		[DllImport(RecastDllName, EntryPoint = "meshtest", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		static extern void Test(
			[MarshalAs(UnmanagedType.FunctionPtr)] Action cb
		);

		/// <summary>
		/// Generate a mesh using the input mesh parameters, within the given InputGeometry object "geom"
		/// </summary>
		public static void GenerateInputMesh(IntPtr geom, Vector3[] vertices, int[] _triangles, string name)
		{
			var verts = new float[vertices.Length * 3];
			var triangles = _triangles;
			//var triangles = _triangles.Reverse().ToArray();
			int idx = 0;
			for (var i = 0; i < vertices.Length; i++)
			{
				var vector = vertices[i];
				verts[idx++] = vector.X;
				verts[idx++] = vector.Y;
				verts[idx++] = vector.Z;
			}

			//GenerateMesh(geom, verts, 2*max, triangles, max, name);
			GenerateInputMesh(geom, verts, vertices.Length, triangles.Reverse().ToArray(), triangles.Length / 3, name);
		}

		/// <summary>
		/// Generate a mesh using the input mesh parameters, within the given InputGeometry object "geom"
		/// </summary>
		[DllImport(RecastDllName, EntryPoint = "genMesh", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		static extern void GenerateInputMesh(IntPtr geom, float[] vertices, int vcount, int[] triangles, int tcount,
											   [MarshalAs(UnmanagedType.LPStr)] string name);
		#endregion

		#region NavMesh
		/// <summary>
		/// Invoked when a new NavMesh has been generated
		/// </summary>
		public static event NavMeshGeneratedHandler NavMeshGenerated;

		/// <summary>
		/// Invoked when a new Tile within a NavMesh has been generated
		/// </summary>
		public static event NavMeshTileGeneratedHandler NavMeshTileAdded;

		/// <summary>
		/// Gets the max verts per polygon constant
		/// </summary>
		[DllImport(RecastDllName, EntryPoint = "navmeshGetMaxVertsPerPolygon", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		static extern int GetMaxVertsPerPolygon();

		/// <summary>
		/// Set a callback to be called for every generated Nav mesh
		/// </summary>
		[DllImport(RecastDllName, EntryPoint = "navmeshSetNavMeshCreatedCallback", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		static extern void SetNavMeshGeneratedCallback(_NavMeshGeneratedHandler cb);

		/// <summary>
		/// Set a callback to be called for every tile of a generated Nav mesh
		/// </summary>
		[DllImport(RecastDllName, EntryPoint = "navmeshSetTileCreatedCallback", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		static extern void SetNavMeshTileAddedCallback(_NavMeshTileGeneratedHandler cb);

		/// <summary>
		/// Set a callback to be called for every tile of a generated Nav mesh
		/// </summary>
		[DllImport(RecastDllName, EntryPoint = "navmeshSetNavMeshDoneCallback", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		static extern void SetNavMeshDoneCallback(_NavMeshDoneHandler cb);


		/// <summary>
		/// TODO: Use self-generated Id, so we can use an array for lookup
		/// </summary>
		private static readonly Dictionary<long, NavMesh> UnfinishedNavMeshes = new Dictionary<long,NavMesh>();
		static void OnNavMeshGenerated(long navMeshId, int w, int h, float[] origin, float tileW, float tileH, int maxTiles)
		{
			var mesh = new NavMesh(navMeshId, w, h, new Vector3(origin[0], origin[1], origin[2]), tileW, tileH, maxTiles);
			UnfinishedNavMeshes.Add(navMeshId, mesh);
		}

		static void OnNavMeshTileGenerated(
			long navMeshId, int x, int y, int nextX, int nextY, uint flags,

			int vertCount, float[] vertices,

			int pCount, int pVCount,																	// polygons
				uint[] pFlinks,
				ushort[] pVerts,
				ushort[] pNeighbors,
				ushort[] pFlags,
				ushort[] pvCounts,
				byte[] pAreas,
				byte[] pTypes,

			int lCount,																						// links
				uint[] lRefs,
				uint[] lNextLinks,
				byte[] lEdges,
				byte[] lSides,
				byte[] lBMins,
				byte[] lBMaxs,

            int dCount,
                ushort[] dVBases,
                ushort[] dVCounts,
                ushort[] dTBases,
                ushort[] dTCounts,

            int dVCount,
                float[] dVerts,

            int dTCount,
                byte[] dTris,

			int omCount,																					// off mesh connections
				float[] omPos,
				float[] omRads,
				ushort[] omPolys,
				byte[] omFlags,
				byte[] omSides
				)
		{
			NavMesh mesh;
			if (!UnfinishedNavMeshes.TryGetValue(navMeshId, out mesh))
			{
				throw new Exception("Tile sent for non-existing mesh: " + navMeshId);
			}

			// vertices
			var realVertCount = vertCount / 3;
			var realVerts = new Vector3[realVertCount];
			var idx = 0;
			for (var i = 0; i < realVertCount; i++)
			{
				realVerts[i] = new Vector3(vertices[idx++], vertices[idx++], vertices[idx++]);
			}

			// polygons
			var polys = new NavMeshPolygon[pCount];
			var vidx = 0;
			for (var i = 0; i < pCount; i++)
			{
				var verts = new ushort[MaxVertsPerPolygon];
				var neighbors = new ushort[MaxVertsPerPolygon];
				Array.Copy(pVerts, vidx, verts, 0, MaxVertsPerPolygon);
				Array.Copy(pNeighbors, vidx, neighbors, 0, MaxVertsPerPolygon);
				polys[i] = new NavMeshPolygon
				{
					FirstLink = pFlinks[i],
					Flags = pFlags[i],
					Neighbors = neighbors,
					Vertices = verts,
					Area = pAreas[i],
					Type = (NavMeshPolyTypes)pTypes[i],
                    VertCount = pvCounts[i]
				};

                //var count = 1;
                //for (var j = 1; j < MaxVertsPerPolygon; j++)
                //{
                //    if (polys[i].Vertices[j] <= 0) continue;
                //    count++;
                //}
                //polys[i].VertCount = (byte)count;

				vidx += MaxVertsPerPolygon;
			}

			// links
			var links = new NavMeshPolyLink[lCount];
			for (var i = 0; i < lCount; i++)
			{
				links[i] = new NavMeshPolyLink
				{
					Bmax = lBMaxs[i],
					Bmin = lBMins[i],
					Edge = lEdges[i],
					Next = lNextLinks[i],
					Reference = lRefs[i],
					Side = lSides[i]
				};
			}

		    var details = new NavMeshPolyDetail[dCount];
            for (var i = 0; i < dCount; i++)
            {
                details[i] = new NavMeshPolyDetail
                {
                    TriBase = dTBases[i],
                    TriCount = dTCounts[i],
                    VertBase = dVBases[i],
                    VertCount = dVCounts[i]
                };
            }

            if (dVCount == 0) Console.WriteLine("Managed dVertCount: {0}", dVCount);
		    var detailVerts = new Vector3[dVCount/3];
            var dVIdx = 0;
            for (var i = 0; i < (dVCount/3); i++)
            {
                detailVerts[i] = new Vector3
                {
                    X = dVerts[dVIdx++],
                    Y = dVerts[dVIdx++],
                    Z = dVerts[dVIdx++]
                };
            }

            if (dTCount == 0) Console.WriteLine("Managed dTriCount: {0}", dTCount);
		    var detailTris = new NavMeshDetailTriIndex[dTCount/4];
		    var dTIdx = 0;
            for (var i = 0; i < (dTCount/4); i++)
            {
                detailTris[i] = new NavMeshDetailTriIndex
                {
                    Index0 = dTris[dTIdx++],
                    Index1 = dTris[dTIdx++],
                    Index2 = dTris[dTIdx++]
                };
                dTIdx++;
            }

			// Off-mesh connections
			var omCons = new NavOffMeshConnection[omCount];
			var pidx = 0;
			for (var i = 0; i < omCount; i++)
			{
				var pos = new[]{
			        new Vector3(omPos[pidx++], omPos[pidx++], omPos[pidx++]),
			        new Vector3(omPos[pidx++], omPos[pidx++], omPos[pidx++])
			    };

				omCons[i] = new NavOffMeshConnection
				{
					Flags = omFlags[i],
					Polygon = omPolys[i],
					Positions = pos,
					Radius = omRads[i],
					Side = omSides[i]
				};
			}

			var tile = new NavMeshTile
			{
				X = x,
				Y = y,
				NextX = nextX,
				NextY = nextY,
				Flags = flags,
				Vertices = realVerts,
				Polygons = polys,
				Links = links,
				OffMeshConnections = omCons,
                DetailPolygons = details,
                DetailedVertices = detailVerts,
                DetailedTriangles = detailTris
			};

			mesh.Tiles[x, y] = tile;

			if (mesh.Initialized)
			{
				var evt = NavMeshTileAdded;
				if (evt != null)
				{
					evt(tile);
				}
			}
		}

		private static void OnNavMeshDone(long navMeshId)
		{
			NavMesh mesh;
			if (!UnfinishedNavMeshes.TryGetValue(navMeshId, out mesh))
			{
				throw new Exception("Non-existing mesh received done signal: " + navMeshId);
			}

			UnfinishedNavMeshes.Remove(navMeshId);
			mesh.Initialized = true;

			var evt = NavMeshGenerated;
			if (evt != null)
			{
				evt(mesh);
			}
		}
		#endregion
	}
}
