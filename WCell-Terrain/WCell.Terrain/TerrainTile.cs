using System;
using System.Collections.Generic;
using Terra;
using WCell.Constants;
using WCell.Terrain.Collision;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.Terrain
{
	/// <summary>
	/// Represents a tile within one map
	/// TODO: In the future, one Tile will only be represented by single indices into the map's array of vertices, indices and neighbors (plus the same for liquids and some other things)
	/// </summary>
	public class TerrainTile
	{
		// TODO: Change from list to Array
		public List<int> LiquidIndices;
		public List<Vector3> LiquidVertices;
		public int[] TerrainIndices;
		public Vector3[] TerrainVertices;

		/// <summary>
		/// Each triangle has 3 neighbors
		/// </summary>
		public int[,] TerrainTriangleNeighbors;

		/// <summary>
		/// 
		/// </summary>
		private readonly Point2D coordinates;

		/// <summary>
		/// Array of MCNK chunks which give the ADT vertex information for this ADT
		/// </summary>
		public readonly TerrainChunk[,] Chunks = new TerrainChunk[TerrainConstants.ChunksPerTileSide, TerrainConstants.ChunksPerTileSide];

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

		/// <summary>
		/// Any triangle that shares 2 vertices with the given triangle is a neighbor
		/// </summary>
		public int[] GetNeighborsOf(int triIndex)
		{
			var a = TerrainIndices[triIndex++];
			var b = TerrainIndices[triIndex++];
			var c = TerrainIndices[triIndex];

			var neighbors = new int[WCellTerrainConstants.NeighborsPerTriangle];

			// fill with "invalid index"
			for (var i = 0; i < WCellTerrainConstants.NeighborsPerTriangle; i++)
			{
				neighbors[i] = -1;
			}

			// TODO: Use a datastructure to cut down this search
			for (var i = 0; i < TerrainIndices.Length; i += 3)
			{
				if (i != triIndex)	// don't return itself
				{
					var a2 = TerrainIndices[i];
					var b2 = TerrainIndices[i + 1];
					var c2 = TerrainIndices[i + 2];

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

					if (nCount == 2)
					{
						// we have a neighbor
						switch (mask)
						{
							case WCellTerrainConstants.ABEdgeMask:
								// neighbor shares a and b
								neighbors[WCellTerrainConstants.ABEdgeIndex] = i;
								break;
							case WCellTerrainConstants.ACEdgeMask:
								// second shares a and c
								neighbors[WCellTerrainConstants.ACEdgeIndex] = i;
								break;
							case WCellTerrainConstants.BCEdgeMask:
								// neighbor shares b and c
								neighbors[WCellTerrainConstants.BCEdgeIndex] = i;
								break;

						}
					}
				}
			}

			return neighbors;
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
		/// <summary>
		/// Returns the first triangle that is directly underneath the given position.
		/// The value is -1, if none was found.
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>
		public int FindFirstTriangleUnderneath(Vector3 pos)
		{
			// shoot a ray straight down
			pos.Z += 0.001f;						// make sure, it is slightly above the surface
			var ray = new Ray(pos, Vector3.Down);
			return FindFirstHitTriangle(ray);
		}

		public Vector3 IntersectFirstTriangle(Ray selectRay)
		{
			float distance;
			FindFirstHitTriangle(selectRay, out distance);
			
			return selectRay.Position + selectRay.Direction * distance;
		}

		/// <summary>
		/// Returns the first triangle that is directly underneath the given position.
		/// The value is -1, if none was found.
		/// </summary>
		/// <returns></returns>
		public int FindFirstHitTriangle(Ray selectRay)
		{
			// shoot a ray straight down
			float distance;
			return FindFirstHitTriangle(selectRay, out distance);
		}

		/// <summary>
		/// Intersects the given ray with the tile and sets triangle to the first one that got hit
		/// <param name="distance">The distance from Ray origin to, where it hits the returned triangle</param>
		/// </summary>
		public int FindFirstHitTriangle(Ray selectRay, out float distance)
		{
			var pos3D = selectRay.Position;
			var dir3D = selectRay.Direction;

			var ray3D = new Ray(pos3D, dir3D);
			var pos2D = new Vector2(pos3D.X, pos3D.Y);
			var dir2D = new Vector2(dir3D.X, dir3D.Y).NormalizedCopy();
			var ray2D = new Ray2D(pos2D, dir2D);

			distance = float.MaxValue;

			var triangle = -1;

			foreach (var tri in GetPotentialColliders(ray2D))
			{
				var vec0 = TerrainVertices[TerrainIndices[tri]];
				var vec1 = TerrainVertices[TerrainIndices[tri+1]];
				var vec2 = TerrainVertices[TerrainIndices[tri+2]];

				float time;
				if (!Intersection.RayTriangleIntersect(ray3D, vec0, vec1, vec2, out time)) continue;
				if (time > distance) continue;

				distance = time;
				triangle = tri;
			}

			return triangle;
		}

		public float GetInterpolatedHeight(Point2D chunkCoord, Point2D unitCoord, HeightMapFraction heightMapFraction)
		{
			// Height stored as: h5 - its v8 grid, h1-h4 - its v9 grid
			// +--------------> X
			// | h1--------h2   Coordinates are:
			// | |    |    |     h1 0,0
			// | |  1 |  2 |     h2 0,1
			// | |----h5---|     h3 1,0
			// | |  3 |  4 |     h4 1,1
			// | |    |    |     h5 1/2,1/2
			// | h3--------h4
			// V Y
			if (Chunks == null) return float.NaN;

			var chunk = Chunks[chunkCoord.X, chunkCoord.Y];
			if (chunk == null) return float.NaN;

			var medianHeight = chunk.MedianHeight;
			if (chunk.IsFlat) return medianHeight;


			// TODO: Fix interpolated height
			return medianHeight;

			//// Fixme:  Indexing is backwards.
			//var heightX = (unitCoord.Y == TerrainConstants.UnitsPerChunkSide)
			//                  ? (TerrainConstants.UnitsPerChunkSide - 1)
			//                  : unitCoord.Y;

			//var heightY = (unitCoord.X == TerrainConstants.UnitsPerChunkSide)
			//                  ? (TerrainConstants.UnitsPerChunkSide - 1)
			//                  : unitCoord.X;


			//// Determine what quad we're in
			//var xf = heightMapFraction.FractionX;
			//var yf = heightMapFraction.FractionY;

			//float topLeft, topRight, bottomLeft, bottomRight;
			//if (xf < 0.5f)
			//{
			//    if (yf < 0.5)
			//    {
			//        // Calc the #1 quad
			//        // +--------------> X
			//        // | h1--------h2    Coordinates are:
			//        // | |    |    |      h1 0,0
			//        // | |  1 |    |      h2 0,1
			//        // | |----h5---|      h3 1,0
			//        // | |    |    |      h4 1,1
			//        // | |    |    |      h5 1/2,1/2
			//        // | h3--------h4
			//        // V Y
			//        var h1 = chunk.OuterHeightDiff[heightX, heightY];
			//        var h2 = chunk.OuterHeightDiff[heightX, heightY + 1];
			//        var h3 = chunk.OuterHeightDiff[heightX + 1, heightY];
			//        var h5 = chunk.InnerHeightDiff[heightX, heightY];

			//        topLeft = h1;
			//        topRight = (h1 + h2) / 2.0f;
			//        bottomLeft = (h1 + h3) / 2.0f;
			//        bottomRight = h5;
			//    }
			//    else
			//    {
			//        // Calc the #3 quad
			//        // +--------------> X
			//        // | h1--------h2    Coordinates are:
			//        // | |    |    |      h1 0,0
			//        // | |    |    |      h2 0,1
			//        // | |----h5---|      h3 1,0
			//        // | |  3 |    |      h4 1,1
			//        // | |    |    |      h5 1/2,1/2
			//        // | h3--------h4
			//        // V Y
			//        var h1 = chunk.OuterHeightDiff[heightX, heightY];
			//        var h3 = chunk.OuterHeightDiff[heightX + 1, heightY];
			//        var h4 = chunk.OuterHeightDiff[heightX + 1, heightY + 1];
			//        var h5 = chunk.InnerHeightDiff[heightX, heightY];

			//        topLeft = (h1 + h3) / 2.0f;
			//        topRight = h5;
			//        bottomLeft = h3;
			//        bottomRight = (h3 + h4) / 2.0f;

			//        yf -= 0.5f;
			//    }
			//}
			//else
			//{
			//    if (yf < 0.5)
			//    {
			//        // Calc the #2 quad
			//        // +--------------> X
			//        // | h1--------h2    Coordinates are:
			//        // | |    |    |      h1 0,0
			//        // | |    |  2 |      h2 0,1
			//        // | |----h5---|      h3 1,0
			//        // | |    |    |      h4 1,1
			//        // | |    |    |      h5 1/2,1/2
			//        // | h3--------h4
			//        // V Y
			//        var h1 = chunk.OuterHeightDiff[heightX, heightY];
			//        var h2 = chunk.OuterHeightDiff[heightX, heightY + 1];
			//        var h4 = chunk.OuterHeightDiff[heightX + 1, heightY + 1];
			//        var h5 = chunk.InnerHeightDiff[heightX, heightY];

			//        topLeft = (h1 + h2) / 2.0f;
			//        topRight = h2;
			//        bottomLeft = h5;
			//        bottomRight = (h2 + h4) / 2.0f;

			//        xf -= 0.5f;
			//    }
			//    else
			//    {
			//        // Calc the #4 quad
			//        // +--------------> X
			//        // | h1--------h2    Coordinates are:
			//        // | |    |    |      h1 0,0
			//        // | |    |    |      h2 0,1
			//        // | |----h5---|      h3 1,0
			//        // | |    |  4 |      h4 1,1
			//        // | |    |    |      h5 1/2,1/2
			//        // | h3--------h4
			//        // V Y
			//        var h2 = chunk.OuterHeightDiff[heightX, heightY + 1];
			//        var h3 = chunk.OuterHeightDiff[heightX + 1, heightY];
			//        var h4 = chunk.OuterHeightDiff[heightX + 1, heightY + 1];
			//        var h5 = chunk.InnerHeightDiff[heightX, heightY];

			//        topLeft = h5;
			//        topRight = (h2 + h4) / 2.0f;
			//        bottomLeft = (h3 + h4) / 2.0f;
			//        bottomRight = h4;

			//        xf -= 0.5f;
			//        yf -= 0.5f;
			//    }
			//}

			////var heightDiff = InterpolateTriangle(ref vec1, ref vec2, ref vec3, heightMapFraction);
			//var heightDiff = BilinearInterpolate(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight, ref xf,
			//                                     ref yf);
			//return medianHeight + heightDiff;
		}

		public float GetLiquidHeight(Point2D chunkCoord, Point2D unitCoord)
		{
			var chunk = Chunks[chunkCoord.X, chunkCoord.Y];
			if (!chunk.IsLiquid)
				return float.MinValue;


			var liquidHeights = chunk.LiquidHeights;

			return (liquidHeights == null) ? float.MinValue
											: liquidHeights[unitCoord.X, unitCoord.Y];
		}

		public FluidType GetFluidType(Point2D chunkCoord)
		{
			var chunk = Chunks[chunkCoord.X, chunkCoord.Y];

			if (!chunk.IsLiquid)
				return chunk.LiquidType;

			return chunk.LiquidType;
		}

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

		#region Vertex & Index generation

		private static readonly bool[,] EmptyHolesArray = new bool[4, 4];

		public void GenerateHeightVertexAndIndices()
		{
			const int heightsPerTileSide = TerrainConstants.UnitsPerChunkSide * TerrainConstants.ChunksPerTileSide;
			var tileHeights = new float[heightsPerTileSide + 1, heightsPerTileSide + 1];
			var tileHoles = new List<Index2>();


			for (var x = 0; x < TerrainConstants.ChunksPerTileSide; x++)
			{
				for (var y = 0; y < TerrainConstants.ChunksPerTileSide; y++)
				{
					var chunk = Chunks[y, x];
					var heights = chunk.Heights.GetLowResMapMatrix();
					var holes = (chunk.HolesMask > 0) ? chunk.HolesMap : EmptyHolesArray;

					// Add the height map values, inserting them into their correct positions
					for (var unitRow = 0; unitRow <= TerrainConstants.UnitsPerChunkSide; unitRow++)
					{
						for (var unitCol = 0; unitCol <= TerrainConstants.UnitsPerChunkSide; unitCol++)
						{
							var tileX = x * TerrainConstants.UnitsPerChunkSide + unitRow;
							var tileY = y * TerrainConstants.UnitsPerChunkSide + unitCol;

							tileHeights[tileX, tileY] = heights[unitCol, unitRow] + chunk.MedianHeight;

							if (unitCol == TerrainConstants.UnitsPerChunkSide) continue;
							if (unitRow == TerrainConstants.UnitsPerChunkSide) continue;

							// Add the hole vertices to the pre-insertion 'script'
							if (!holes[unitRow / 2, unitCol / 2]) continue;

							tileHoles.AddUnique(new Index2
							{
								Y = tileY,
								X = tileX
							});

							tileHoles.AddUnique(new Index2
							{
								Y = Math.Min(tileY + 1, heightsPerTileSide),
								X = tileX
							});

							tileHoles.AddUnique(new Index2
							{
								Y = tileY,
								X = Math.Min(tileX + 1, heightsPerTileSide)
							});

							tileHoles.AddUnique(new Index2
							{
								Y = Math.Min(tileY + 1, heightsPerTileSide),
								X = Math.Min(tileX + 1, heightsPerTileSide)
							});
						}
					}
				}
			}

			var allHoleIndices = new List<Index2>();
			foreach (var index in tileHoles)
			{
				var count = 0;
				for (var i = -1; i < 2; i++)
				{
					for (var j = -1; j < 2; j++)
					{
						if (!tileHoles.Contains(new Index2
						{
							X = index.X + i,
							Y = index.Y + j
						})) continue;
						count++;
					}
				}
				if (count != 9) continue;
				allHoleIndices.AddUnique(index);
			}

			var terra = new Terra.Terra(1.0f, tileHeights);
			terra.ScriptedPreInsertion(tileHoles);
			terra.Triangulate();

			List<Vector3> newVertices;

			var indices = new List<int>();
			terra.GenerateOutput(allHoleIndices, out newVertices, out indices);
			TerrainIndices = indices.ToArray();

			TerrainVertices = new Vector3[newVertices.Count];
			for (int i = 0; i < newVertices.Count; i++)
			{
				var vertex = newVertices[i];
				var xPos = TerrainConstants.CenterPoint - (TileX*TerrainConstants.TileSize) - (vertex.X*TerrainConstants.UnitSize);
				var yPos = TerrainConstants.CenterPoint - (TileY*TerrainConstants.TileSize) - (vertex.Y*TerrainConstants.UnitSize);
				TerrainVertices[i] = new Vector3(xPos, yPos, vertex.Z);
			}

			// TODO: Add WMO & M2 vertices
		}

		public void GenerateLiquidVertexAndIndices()
		{
			LiquidVertices = new List<Vector3>();
			LiquidIndices = new List<int>();

			var vertexCounter = 0;
			for (var indexX = 0; indexX < 16; indexX++)
			{
				for (var indexY = 0; indexY < 16; indexY++)
				{
					var tempVertexCounter = GenerateLiquidVertices(indexY, indexX, LiquidVertices);
					GenerateLiquidIndices(indexY, indexX, vertexCounter, LiquidIndices);
					vertexCounter += tempVertexCounter;
				}
			}
		}

		/// <summary>
		/// Adds the rendering liquid vertices to the provided list for the MapChunk given by:
		/// </summary>
		/// <param name="indexY">The y index of the map chunk.</param>
		/// <param name="indexX">The x index of the map chunk</param>
		/// <param name="vertices">The Collection to add the vertices to.</param>
		/// <returns>The number of vertices added.</returns>
		public int GenerateLiquidVertices(int indexY, int indexX, ICollection<Vector3> vertices)
		{
			var count = 0;
			var chunk = Chunks[indexX, indexY];

			if (chunk == null) return count;
			if (!chunk.IsLiquid) return count;

			//var clr = Color.Green;

			//switch (mh2O.Header.Type)
			//{
			//    case FluidType.Water:
			//        clr = Color.Blue;
			//        break;
			//    case FluidType.Lava:
			//        clr = Color.Red;
			//        break;
			//    case FluidType.OceanWater:
			//        clr = Color.Coral;
			//        break;
			//}

			var mh2OHeightMap = chunk.LiquidHeights;
			var bounds = chunk.LiquidBounds;
			for (var xStep = 0; xStep <= TerrainConstants.UnitsPerChunkSide; xStep++)
			{
				for (var yStep = 0; yStep < 9; yStep++)
				{
					var xPos = TerrainConstants.CenterPoint - (TileX * TerrainConstants.TileSize) -
							   (indexX * TerrainConstants.ChunkSize) - (xStep * TerrainConstants.UnitSize);
					var yPos = TerrainConstants.CenterPoint - (TileY * TerrainConstants.TileSize) -
							   (indexY * TerrainConstants.ChunkSize) - (yStep * TerrainConstants.UnitSize);

					if (((xStep < bounds.X) || ((xStep - bounds.X) > bounds.Height)) ||
						((yStep < bounds.Y) || ((yStep - bounds.Y) > bounds.Width)))
					{
						continue;
					}

					var zPos = mh2OHeightMap[yStep - bounds.Y, xStep - bounds.X];

					var position = new Vector3(xPos, yPos, zPos);

					vertices.Add(position);
					count++;
				}
			}
			return count;
		}

		/// <summary>
		/// Adds the rendering liquid indices to the provided list for the MapChunk given by:
		/// </summary>
		/// <param name="indexY">The y index of the map chunk.</param>
		/// <param name="indexX">The x index of the map chunk</param>
		/// <param name="offset">The number to add to the indices so as to match the end of the Vertices list.</param>
		/// <param name="indices">The Collection to add the indices to.</param>
		public void GenerateLiquidIndices(int indexY, int indexX, int offset, List<int> indices)
		{
			var chunk = Chunks[indexX, indexY];
			if (chunk == null) return;
			if (!chunk.IsLiquid) return;

			var renderMap = chunk.LiquidMap;
			var bounds = chunk.LiquidBounds;
			for (int r = bounds.X; r < (bounds.X + bounds.Height); r++)
			{
				for (int c = bounds.Y; c < (bounds.Y + bounds.Width); c++)
				{
					var row = r - bounds.X;
					var col = c - bounds.Y;

					if (!renderMap[col, row] && ((bounds.Height != 8) || (bounds.Width != 8))) continue;
					indices.Add(offset + ((row + 1) * (bounds.Width + 1) + col));
					indices.Add(offset + (row * (bounds.Width + 1) + col));
					indices.Add(offset + (row * (bounds.Width + 1) + col + 1));
					indices.Add(offset + ((row + 1) * (bounds.Width + 1) + col + 1));
					indices.Add(offset + ((row + 1) * (bounds.Width + 1) + col));
					indices.Add(offset + (row * (bounds.Width + 1) + col + 1));
				}
			}
		}


		/// <summary>
		/// Adds the rendering liquid vertices to the provided list for the MapChunk given by:
		/// </summary>
		/// <param name="chunkY">The y index of the map chunk.</param>
		/// <param name="chunkX">The x index of the map chunk</param>
		/// <param name="vertices">The Collection to add the vertices to.</param>
		/// <returns>The number of vertices added.</returns>
		public int GenerateHeightVertices(int chunkY, int chunkX, ICollection<Vector3> vertices)
		{
			var lowResNormal = Chunks[chunkX, chunkY].Normals.GetLowResNormalMatrix();

			var counter = 0;
			for (var xStep = 0; xStep < 9; xStep++)
			{
				for (var yStep = 0; yStep < 9; yStep++)
				{
					var position = GetVertex(chunkX, chunkY, xStep, yStep);

					var theNormal = lowResNormal[yStep, xStep];

					//var cosAngle = Vector3.Dot(Vector3.Up, theNormal);
					//var angle = MathHelper.ToDegrees((float)Math.Acos(cosAngle));

					//if (angle > 50.0)
					//{
					//    color = Color.Brown;
					//}
					vertices.Add(position);
					counter++;
				}
			}
			return counter;
		}

		private Vector3 GetVertex(int chunkX, int chunkY, int unitX, int unitY)
		{
			var mcnk = Chunks[chunkX, chunkY];

			var xPos = TerrainConstants.CenterPoint - (TileX * TerrainConstants.TileSize) - (chunkX * TerrainConstants.ChunkSize) -
								  (unitX * TerrainConstants.UnitSize);
			var yPos = TerrainConstants.CenterPoint - (TileY * TerrainConstants.TileSize) - (chunkY * TerrainConstants.ChunkSize) -
					   (unitY * TerrainConstants.UnitSize);

			var zPos = mcnk.Heights.GetLowResMapMatrix()[unitY, unitX] + mcnk.MedianHeight;

			return new Vector3(xPos, yPos, zPos);
		}


		/// <summary>
		/// Adds the rendering indices to the provided list for the MapChunk given by:
		/// </summary>
		/// <param name="indexY">The y index of the map chunk.</param>
		/// <param name="indexX">The x index of the map chunk</param>
		/// <param name="offset">The number to add to the indices so as to match the end of the Vertices list.</param>
		/// <param name="indices">The Collection to add the indices to.</param>
		public void GenerateHeightIndices(int indexY, int indexX, int offset, List<int> indices)
		{
			var chunk = Chunks[indexX, indexY];

			var holesMap = new bool[4, 4];
			if (chunk.HolesMask > 0)
			{
				holesMap = chunk.HolesMap;
			}

			for (var row = 0; row < 8; row++)
			{
				for (var col = 0; col < 8; col++)
				{
					if (holesMap[row / 2, col / 2]) continue;
					//{
					//    indices.Add(offset + ((row + 1) * (8 + 1) + col));
					//    indices.Add(offset + (row * (8 + 1) + col));
					//    indices.Add(offset + (row * (8 + 1) + col + 1));

					//    indices.Add(offset + ((row + 1) * (8 + 1) + col + 1));
					//    indices.Add(offset + ((row + 1) * (8 + 1) + col));
					//    indices.Add(offset + (row * (8 + 1) + col + 1));
					//    continue;
					//}
					//* The order metter*/
					/*This 3 index add the up triangle
								*
								*0--1--2
								*| /| /
								*|/ |/ 
								*9  10 11
								*/

					indices.Add(offset + ((row + 1) * (8 + 1) + col)); //9 ... 10
					indices.Add(offset + (row * (8 + 1) + col)); //0 ... 1
					indices.Add(offset + (row * (8 + 1) + col + 1)); //1 ... 2

					/*This 3 index add the low triangle
								 *
								 *0  1   2
								 *  /|  /|
								 * / | / |
								 *9--10--11
								 */

					indices.Add(offset + ((row + 1) * (8 + 1) + col + 1));
					indices.Add(offset + ((row + 1) * (8 + 1) + col));
					indices.Add(offset + (row * (8 + 1) + col + 1));
				}
			}
		}


		public IEnumerable<int> GetPotentialColliders(Ray2D ray2D)
		{
			// TODO: Get colliding triangles (more efficiently)

			for (var i = 0; i < TerrainIndices.Length; i += 3)
			{
				yield return i;
			}
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