using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace TerrainDisplay.Recast
{
	public delegate void PtrVoidCallback(IntPtr ptr);
	public delegate bool PtrBoolCallback(IntPtr ptr);

	public static class RecastAPI
	{
		public const string RecastDllName = "Recast.dll";
		public const string RecastFolder = "../../../../Recast/Bin/";

		public static readonly string[] Dlls = new[] { RecastDllName, "SDL.dll" };

		public static void InitAPI()
		{	
			// copy over necessary dll files
			foreach (var dll in Dlls)
			{
				EnsureDll(dll);
			}
		}

		private static void EnsureDll(string dllName)
		{
			var targetFile = new FileInfo(Path.Combine(Environment.CurrentDirectory, dllName));
			if (!targetFile.Exists)
			{
				var file = new FileInfo(Path.Combine(RecastFolder, dllName));
				file.CopyTo(targetFile.FullName);
			}
		}

		[DllImport(RecastDllName, EntryPoint = "startrecast", CallingConvention = CallingConvention.Cdecl)]
		public static extern void StartRecast();

		[DllImport(RecastDllName, EntryPoint = "meshGenAdd", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern void AddMeshGenerator(
			[MarshalAs(UnmanagedType.LPStr)] string name,
			[MarshalAs(UnmanagedType.FunctionPtr)] PtrBoolCallback callback);

		[DllImport(RecastDllName, EntryPoint = "meshGenRemove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern void RemoveMeshGenerator(
			[MarshalAs(UnmanagedType.LPStr)] string name
		);

		[DllImport(RecastDllName, EntryPoint = "meshtest", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Test(
			[MarshalAs(UnmanagedType.FunctionPtr)] Action cb
		);

		public static void GenerateMesh(IntPtr geom, Vector3[] vertices, int[] _triangles, string name)
		{
			int max = 1024;

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
			GenerateMesh(geom, verts, vertices.Length, triangles.Reverse().ToArray(), triangles.Length / 3, name);
		}

		[DllImport(RecastDllName, EntryPoint = "genMesh", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern void GenerateMesh(IntPtr geom, float[] vertices, int vcount, int[] triangles, int tcount,
		                                       [MarshalAs(UnmanagedType.LPStr)] string name);

		[DllImport(RecastDllName, EntryPoint = "navSetSpeed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SetNavSpeed(float speed);

											   
	}
}
