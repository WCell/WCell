using System;
using System.Collections.Generic;
using Terra;
using WCell.Constants;
using WCell.Terrain.Collision;
using WCell.Terrain.MPQ;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.Terrain
{
	/// <summary>
	/// Represents a tile within one map
	/// TODO: In the future, one Tile will only be represented by single indices into the map's array of vertices, indices and neighbors (plus the same for liquids and some other things)
	/// </summary>
	public class TerrainTile : IShape
	{
		// TODO: Change from list to Array
		public List<int> LiquidIndices;
		public List<Vector3> LiquidVertices;
		public int[] TerrainIndices;
		public Vector3[] TerrainVertices;

		/// <summary>
		/// 
		/// </summary>
		private readonly Point2D coordinates;

		public TerrainTile(Point2D coords, Terrain terrain)
		{
			// MapChunks = new TerrainChunk[TerrainConstants.ChunksPerTileSide, TerrainConstants.ChunksPerTileSide];
			coordinates = coords;
			Terrain = terrain;
		}

		public Terrain Terrain
		{
			get;
			private set;
		}

		public Vector3[] Vertices
		{
			get { return TerrainVertices; }
		}

		public int[] Indices
		{
			get { return TerrainIndices; }
		}

		/// <summary>
		/// Filename of the ADT
		/// </summary>
		/// <example>Azeroth_32_32.adt</example>
		public string FileName
		{
			get { return TerrainConstants.GetMapFilename(TileX, TileY); }
		}

		/// <summary>
		/// The X offset of the map in the 64 x 64 grid
		/// </summary>
		public int TileX
		{
			get { return coordinates.X; }
		}

		/// <summary>
		/// The Y offset of the map in the 64 x 64 grid
		/// </summary>
		public int TileY
		{
			get { return coordinates.Y; }
		}

		public Rect Bounds
		{
			get
			{
				var topLeftX = TerrainConstants.CenterPoint - ((TileX) * TerrainConstants.TileSize);
				var topLeftY = TerrainConstants.CenterPoint - ((TileY) * TerrainConstants.TileSize);
				var botRightX = topLeftX - TerrainConstants.TileSize;
				var botRightY = topLeftY - TerrainConstants.TileSize;
				return new Rect(new Point(topLeftX, topLeftY), new Point(botRightX, botRightY));
			}
		}

		public IEnumerable<int> GetPotentialColliders(Ray ray)
		{
			// TODO: Get colliding triangles (more efficiently)

			for (var i = 0; i < TerrainIndices.Length; i += 3)
			{
				yield return i;
			}
		}

		public void GetTriangle(int triIndex, out Triangle triangle)
		{
			triangle = new Triangle
			{
				Point1 = TerrainVertices[TerrainIndices[triIndex++]],
				Point2 = TerrainVertices[TerrainIndices[triIndex++]],
				Point3 = TerrainVertices[TerrainIndices[triIndex]]
			};
		}

		#region Terrain Queries

		/*
				private static float InterpolateTriangle(ref Vector3 point1, ref Vector3 point2, ref Vector3 point3, HeightMapFraction heightMapFraction)
				{
					var Ax = point2.X - point1.X;
					var Ay = point2.Y - point1.Y;
					var Az = point2.Z - point1.Z;

					var Bx = point3.X - point1.X;
					var By = point3.Y - point1.Y;
					var Bz = point3.Z - point1.Z;

					var Nnx = (Ay * Bz) - (Az * By);
					var Nny = (Az * Bx) - (Ax * Bz);
					var Nnz = (Ax * By) - (Ay * Bx);
					var Nnsquared = ((Nnx * Nnx) + (Nny * Nny)) + (Nnz * Nnz);
					var length = 1f / ((float)Math.Sqrt(Nnsquared));

					var Nx = Nnx * length;
					var Ny = Nny * length;
					var Nz = Nnz * length;
					var D = (Nx * point1.X) + (Ny * point1.Y) + (Nz * point1.Z);

					var temp = D - (Nx * heightMapFraction.FractionX) - (Ny * heightMapFraction.FractionY);
					return (temp / Nz);
				}
		*/

		private static float BilinearInterpolate(ref float topLeft, ref float topRight, ref float bottomLeft, ref float bottomRight, ref float xf, ref float yf)
		{
			// +--------------> X
			// | TL--------TR    Coordinates are:
			// | |         |      TL 0,0
			// | |         |      TR 1/2,0
			// | |         |      BL 0,1/2
			// | |         |      BR 1/2,1/2
			// | |         |      
			// | BL--------BR
			// V Y
			// top edge:
			//    (hf0 - topLeft) = ((topRight - topLeft)/(0.5 - 0.0))*(xf - 0.0);
			//                    = 2.0*(topRight - topLeft)*xf;
			//     hf0 = 2.0*topRight*xf - 2.0*topLeft*xf + topLeft;
			//         = (2.0*xf)*topRight + (1 - 2.0*xf)*topLeft;
			// bottom edge:
			//     hf1 = (2.0*xf)*bottomright + (1 - 2.0*xf)*bottomLeft;
			// 
			// final height:
			//     hf = (2.0*yf)*hf1 + (1 - 2.0*yf)*hf0
			var xxf = xf * 2.0f;
			var yyf = yf * 2.0f;

			var hf0 = xxf * topRight + ((1 - xxf) * topLeft);
			var hf1 = xxf * bottomRight + ((1 - xxf) * bottomLeft);

			return ((yyf * hf1) + ((1 - yyf) * hf0));
		}

		private static bool Intersect(Rect chunkRect, ref Vector3 vertex0, ref Vector3 vertex1, ref Vector3 vertex2)
		{
			if (chunkRect.Contains(vertex0.X, vertex0.Y)) return true;
			if (chunkRect.Contains(vertex1.X, vertex1.Y)) return true;
			if (chunkRect.Contains(vertex2.X, vertex2.Y)) return true;

			// Check if any of the Chunk's corners are contained in the triangle
			if (Intersection.PointInTriangle2DXY(chunkRect.TopLeft, vertex0, vertex1, vertex2)) return true;
			if (Intersection.PointInTriangle2DXY(chunkRect.TopRight, vertex0, vertex1, vertex2)) return true;
			if (Intersection.PointInTriangle2DXY(chunkRect.BottomLeft, vertex0, vertex1, vertex2)) return true;
			if (Intersection.PointInTriangle2DXY(chunkRect.BottomRight, vertex0, vertex1, vertex2)) return true;

			// Check if any of the triangle's line segments intersect the chunk's bounds
			if (Intersection.IntersectSegmentRectangle2DXY(chunkRect, vertex0, vertex1)) return true;
			if (Intersection.IntersectSegmentRectangle2DXY(chunkRect, vertex0, vertex2)) return true;
			if (Intersection.IntersectSegmentRectangle2DXY(chunkRect, vertex1, vertex2)) return true;

			return false;
		}
		#endregion


		#region Legacy Code
		//private void SortTrisIntoChunks()
		//{
		//    //Triangulate the indices
		//    for (var i = 0; i < TerrainIndices.Length; )
		//    {
		//        var triangle = new Index3
		//            {
		//                Index0 = (short) TerrainIndices[i++],
		//                Index1 = (short) TerrainIndices[i++],
		//                Index2 = (short) TerrainIndices[i++]
		//            };

		//        var vertex0 = TerrainVertices[triangle.Index0];
		//        var vertex1 = TerrainVertices[triangle.Index1];
		//        var vertex2 = TerrainVertices[triangle.Index2];

		//        var min = Vector3.Min(Vector3.Min(vertex0, vertex1), vertex2);
		//        var max = Vector3.Max(Vector3.Max(vertex0, vertex1), vertex2);
		//        var triRect = new Rect(new Point(min.X, min.Y), new Point(max.X, max.Y));

		//        int startX, startY;
		//        PositionUtil.GetXYForPos(min, out startX, out startY);

		//        int endX, endY;
		//        PositionUtil.GetXYForPos(max, out endX, out endY);

		//        if (startX > endX) MathHelpers.Swap(ref startX, ref endX);
		//        if (startY > endY) MathHelpers.Swap(ref startY, ref endY);

		//        var basePoint = Bounds.BottomRight;
		//        for (var chunkX = startX; chunkX <= endX; chunkX++)
		//        {
		//            for (var chunkY = startY; chunkY <= endY; chunkY++)
		//            {
		//                var chunk = Chunks[chunkX, chunkY];
		//                var chunkBaseX = basePoint.X - chunk.X * TerrainConstants.ChunkSize;
		//                var chunkBaseY = basePoint.Y - chunk.Y * TerrainConstants.ChunkSize;
		//                var chunkBottomX = chunkBaseX - TerrainConstants.ChunkSize;
		//                var chunkBottomY = chunkBaseY - TerrainConstants.ChunkSize;
		//                var chunkRect = new Rect(new Point(chunkBaseX, chunkBaseY),
		//                                         new Point(chunkBottomX, chunkBottomY));

		//                if (!chunkRect.IntersectsWith(triRect)) continue;

		//                if (Intersect(chunkRect, ref vertex0, ref vertex1, ref vertex2))
		//                {
		//                    chunk.TerrainTris.Add(triangle);
		//                }
		//            }
		//        }
		//    }
		//}
		#endregion
	}
}