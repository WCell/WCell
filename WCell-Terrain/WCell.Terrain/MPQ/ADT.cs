using System;
using System.Collections.Generic;
using Terra;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Terrain.Collision.QuadTree;
using WCell.Terrain.MPQ.ADTs;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.Terrain.MPQ
{
	/// <summary>
	/// Collection of heightmap-, liquid-, WMO- and M2- data of a single map tile
	/// </summary>
    public class ADT : TerrainTile, IQuadObject
    {
    	public MapId Map { get; set; }
    	private const float MAX_FLAT_LAND_DELTA = 0.005f;
        private const float MAX_FLAT_WATER_DELTA = 0.001f;

        #region Parsing

        /// <summary>
        /// Version of the ADT
        /// </summary>
        /// <example></example>
        public int Version;

        public readonly MapHeader Header = new MapHeader();

        public MapChunkInfo[] MapChunkInfo;

        /// <summary>
        /// List of MDDF Chunks which are placement information for M2s
        /// </summary>
        public readonly List<MapDoodadDefinition> DoodadDefinitions = new List<MapDoodadDefinition>();
        /// <summary>
        /// List of MODF Chunks which are placement information for WMOs
        /// </summary>
        public readonly List<MapObjectDefinition> ObjectDefinitions = new List<MapObjectDefinition>();

        public readonly List<string> ModelFiles = new List<string>();
        public readonly List<int> ModelNameOffsets = new List<int>();
        public readonly List<string> ObjectFiles = new List<string>();
        public readonly List<int> ObjectFileOffsets = new List<int>();


		public M2[] M2s;
		public WMORoot[] WMOs;

		/// <summary>
		/// Array of MCNK chunks which give the ADT vertex information for this ADT
		/// </summary>
		public readonly ADTChunk[,] Chunks = new ADTChunk[TerrainConstants.ChunksPerTileSide, TerrainConstants.ChunksPerTileSide];
        #endregion


        private int _nodeId = -1;
        public int NodeId
        {
            get { return _nodeId; }
            set { _nodeId = value; }
        }

        public QuadTree<ADTChunk> QuadTree;
        public bool IsWMOOnly;


		/// <summary>
		/// Array of MH20 chunks which give the ADT FLUID vertex information for this ADT
		/// </summary>
		public readonly MH2O[,] LiquidInfo = new MH2O[TerrainConstants.ChunksPerTileSide, TerrainConstants.ChunksPerTileSide];

        #region Constructors
		public ADT(Point2D coordinates, Terrain terrain) :
			base(coordinates, terrain)
        {
        }

    	#endregion

		public ADTChunk GetADTChunk(int x, int y)
		{
			return (ADTChunk) Chunks[x, y];
		}

        public void BuildQuadTree()
        {
            var basePoint = Bounds.BottomRight;
            QuadTree = new QuadTree<ADTChunk>(basePoint, TerrainConstants.ChunksPerTileSide, TerrainConstants.ChunkSize);
            foreach (ADTChunk chunk in Chunks)
            {
                var topLeftX = basePoint.X - chunk.X * TerrainConstants.ChunkSize;
                var topLeftY = basePoint.Y - chunk.Y * TerrainConstants.ChunkSize;
                var botRightX = topLeftX - TerrainConstants.ChunkSize;
                var botRightY = topLeftY - TerrainConstants.ChunkSize;
                
                chunk.Bounds = new Rect(new Point(topLeftX - 1f, topLeftY - 1f),
                                        new Point(botRightX + 1f, botRightY + 1f));
                if (!QuadTree.Insert(chunk))
                {
                    Console.WriteLine((string) "Failed to insert ADTChunk into the QuadTree: {0}", (object) chunk);
                }

                chunk.Bounds = new Rect(new Point(topLeftX, topLeftY),
                                        new Point(botRightX, botRightY));
            }
		}



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


			// TODO: Pool huge lists & arrays
			List<Vector3> newVertices;

			var indices = new List<int>();
			terra.GenerateOutput(allHoleIndices, out newVertices, out indices);

			// convert
			for (int i = 0; i < newVertices.Count; i++)
			{
				var vertex = newVertices[i];
				var xPos = TerrainConstants.CenterPoint - (TileX * TerrainConstants.TileSize) - (vertex.X * TerrainConstants.UnitSize);
				var yPos = TerrainConstants.CenterPoint - (TileY * TerrainConstants.TileSize) - (vertex.Y * TerrainConstants.UnitSize);
				newVertices[i] = new Vector3(xPos, yPos, vertex.Z);
			}

			// Add WMO & M2 vertices
			foreach (var wmo in WMOs)
			{
				var offset = newVertices.Count;
				newVertices.AddRange(wmo.WmoVertices);
				foreach (var index in wmo.WmoIndices)
				{
					indices.Add(offset + index);
				}
			}

			foreach (var m2 in M2s)
			{
				var offset = newVertices.Count;
				newVertices.AddRange(m2.Vertices);
				foreach (var index in m2.Indices)
				{
					indices.Add(offset + index);
				}
			}

			TerrainIndices = indices.ToArray();
			TerrainVertices = newVertices.ToArray();
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

		#endregion


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
    }
}
