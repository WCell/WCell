using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using WCell.Terrain.Recast.NavMesh;
using WCell.Util.Graphics;
using WCell.Util.Variables;

namespace WCell.Terrain.Recast
{
	public static class RecastAPI
	{
		#region Misc & Init
		public const string RecastDllName = "Recast.dll";

		public const string RecastFolder = "../../Recast/Bin/";
		public const string InputMeshExtension = ".obj";
		public const string NavMeshExtension = ".nav";

		public static string MeshesFolder
		{
			get { return RecastFolder + "Meshes/"; }
		}

		public static string GetMeshFilename(string name)
		{
			Directory.CreateDirectory(MeshesFolder);
			return MeshesFolder + name + InputMeshExtension;
		}

		public static readonly string[] Dlls = new[] { RecastDllName, "SDL.dll" };

		private static bool inited;

		public static void InitAPI()
		{
			if (inited)
			{
				return;
				//throw new InvalidOperationException("Tried to initialize API twice.");
			}
			inited = true;
			// must do this, so it finds the dlls and other files (since those paths arent configurable)
			//Environment.CurrentDirectory = RecastFolder;
		}
		#endregion



		//public delegate void BuildMeshCallback(
		//    int userId,
		//    int vertComponentCount,
		//    int polyCount,
		//    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]float[] vertComponents,

		//    int totalPolyIndexCount,				// count of all vertex indices in all polys
		//    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]byte[] pIndexCounts,
		//    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)]uint[] pIndices,
		//    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)]uint[] pNeighbors,
		//    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]ushort[] pFlags,
		//    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]byte[] polyAreasAndTypes);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public delegate void BuildMeshCallback(
			int userId,
			int vertComponentCount,
			int polyCount,
			IntPtr vertComponents,

			int totalPolyIndexCount,				// count of all vertex indices in all polys
			IntPtr pIndexCounts,
			IntPtr pIndices,
			IntPtr pNeighbors,
			IntPtr pFlags,
			IntPtr polyAreasAndTypes
			);

		[DllImport(RecastDllName,
					EntryPoint = "buildMeshFromFile",
					CallingConvention = CallingConvention.Cdecl,
					CharSet = CharSet.Ansi)]
		public static extern int BuildMeshFromFile(int userId,
			byte[] inputFilename,
			byte[] navMeshFilename,
			[MarshalAs(UnmanagedType.FunctionPtr)] BuildMeshCallback callback
			);


		internal static int BuildMesh(int id, string inputFilename, string navMeshFilename, BuildMeshCallback callback)
		{
			inputFilename += "\0";
			navMeshFilename += "\0";
			var inputFilenameBytes = Encoding.ASCII.GetBytes(inputFilename);
			var navMeshFilenameBytes = Encoding.ASCII.GetBytes(navMeshFilename);
			return BuildMeshFromFile(id, inputFilenameBytes, navMeshFilenameBytes, callback);
		}
	}
}
