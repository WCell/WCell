using System;
using System.Collections.Generic;
using Terra;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Terrain.MPQ.ADTs;
using WCell.Util;
using WCell.Util.Graphics;
using QSlim;

namespace WCell.Terrain.MPQ
{
	/// <summary>
	/// Collection of heightmap-, liquid-, WMO- and M2- data of a single map tile
	/// </summary>
    public class ADT : TerrainTile
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

        public bool IsWMOOnly;


		/// <summary>
		/// Array of MH20 chunks which give the ADT FLUID vertex information for this ADT
		/// </summary>
		public readonly MH2O[,] LiquidInfo = new MH2O[TerrainConstants.ChunksPerTileSide, TerrainConstants.ChunksPerTileSide];

        #region Constructors
		public ADT(int x, int y, Terrain terrain) :
			base(x, y, terrain)
		{
			LiquidChunks = new TerrainLiquidChunk[TerrainConstants.ChunksPerTileSide, TerrainConstants.ChunksPerTileSide];
		}

    	#endregion

		public ADTChunk GetADTChunk(int x, int y)
		{
			return (ADTChunk) Chunks[x, y];
		}


		#region Vertex & Index generation

		private static readonly bool[,] EmptyHolesArray = new bool[4, 4];

        public void GenerateMapWithQSlimSimplification()
        {
            List<Vector3> tileVerts;
            List<int> tileIndices;
            GetTerrainMesh(out tileVerts, out tileIndices);

            var slim = new QSlim.QSlim(tileVerts, tileIndices);
            var numTris = (tileIndices.Count / 3);
            slim.Simplify(numTris / 2);

            List<Vector3> newVerts;
            List<int> newIdx;
            slim.GenerateOutput(out newVerts, out newIdx);

            AppendWMOandM2Info(ref newVerts, ref newIdx);
            TerrainIndices = newIdx.ToArray();
            TerrainVertices = newVerts.ToArray();
        }

	    public void GenerateMapWithNoSimplification()
        {
            List<Vector3> tileVerts;
            List<int> tileIndices;
            GetTerrainMesh(out tileVerts, out tileIndices);
            
            AppendWMOandM2Info(ref tileVerts, ref tileIndices);
            TerrainIndices = tileIndices.ToArray();
            TerrainVertices = tileVerts.ToArray();
        }

	    public void GenerateMapWithTerraSimplification()
		{
			const int heightsPerTileSide = TerrainConstants.UnitsPerChunkSide * TerrainConstants.ChunksPerTileSide;
			var tileHeights = new float[heightsPerTileSide + 1, heightsPerTileSide + 1];
			var tileHoles = new List<Index2>();


			for (var x = 0; x < TerrainConstants.ChunksPerTileSide; x++)
			{
				for (var y = 0; y < TerrainConstants.ChunksPerTileSide; y++)
				{
					var chunk = Chunks[x, y];
					var heights = chunk.Heights.GetLowResMapMatrix();
                    var holes = (chunk.HolesMask > 0) ? chunk.HolesMap : EmptyHolesArray;

					// Add the height map values, inserting them into their correct positions
					for (var unitRow = 0; unitRow <= TerrainConstants.UnitsPerChunkSide; unitRow++)
					{
						for (var unitCol = 0; unitCol <= TerrainConstants.UnitsPerChunkSide; unitCol++)
						{
							var tileX = x * TerrainConstants.UnitsPerChunkSide + unitRow;
							var tileY = y * TerrainConstants.UnitsPerChunkSide + unitCol;

							tileHeights[tileX, tileY] = heights[unitRow, unitCol] + chunk.MedianHeight;

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

			List<int> indices;
			terra.GenerateOutput(allHoleIndices, out newVertices, out indices);

			// convert
			for (var i = 0; i < newVertices.Count; i++)
			{
				var vertex = newVertices[i];
				var xPos = TerrainConstants.CenterPoint - (TileX * TerrainConstants.TileSize) - (vertex.X * TerrainConstants.UnitSize);
				var yPos = TerrainConstants.CenterPoint - (TileY * TerrainConstants.TileSize) - (vertex.Y * TerrainConstants.UnitSize);
				newVertices[i] = new Vector3(xPos, yPos, vertex.Z);
			}

	        AppendWMOandM2Info(ref newVertices, ref indices);

			TerrainIndices = indices.ToArray();
			TerrainVertices = newVertices.ToArray();
		}

		/// <summary>
		/// Adds the rendering liquid vertices to the provided list for the MapChunk given by:
		/// </summary>
		/// <param name="indexY">The y index of the map chunk.</param>
		/// <param name="indexX">The x index of the map chunk</param>
		/// <param name="vertices">The Collection to add the vertices to.</param>
		/// <returns>The number of vertices added.</returns>
		public override int GenerateLiquidVertices(int indexX, int indexY, ICollection<Vector3> vertices)
		{
			var count = 0;
			var chunk = Chunks[indexX, indexY];

			if (chunk == null) return count;
			if (!chunk.HasLiquid) return count;

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
			var header = chunk.WaterInfo.Header;
			var bounds = new RectInt32(header.XOffset, header.YOffset, header.Width, header.Height);
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
		public override void GenerateLiquidIndices(int indexX, int indexY, int offset, List<int> indices)
		{
			var chunk = Chunks[indexX, indexY];
			if (chunk == null) return;
			if (!chunk.HasLiquid) return;

			var renderMap = chunk.LiquidMap;
			var header = chunk.WaterInfo.Header;
			var bounds = new RectInt32(header.XOffset, header.YOffset, header.Width, header.Height);
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

			var zPos = mcnk.Heights.GetLowResMapMatrix()[unitX, unitY] + mcnk.MedianHeight;

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

        private void GetTerrainMesh(out List<Vector3> tileVerts, out List<int> tileIndices)
        {
            const int heightsPerTileSide = TerrainConstants.UnitsPerChunkSide * TerrainConstants.ChunksPerTileSide;
            var tileVertLocations = new int[heightsPerTileSide + 1, heightsPerTileSide + 1];
            var tileHolesMap = new bool[heightsPerTileSide + 1, heightsPerTileSide + 1];
            tileVerts = new List<Vector3>();

            for (var i = 0; i < (heightsPerTileSide + 1); i++)
            {
                for (var j = 0; j < (heightsPerTileSide + 1); j++)
                {
                    tileVertLocations[i, j] = -1;
                }
            }

            for (var x = 0; x < TerrainConstants.ChunksPerTileSide; x++)
            {
                for (var y = 0; y < TerrainConstants.ChunksPerTileSide; y++)
                {
                    var chunk = Chunks[x, y];
                    var heights = chunk.Heights.GetLowResMapMatrix();
                    var holes = (chunk.HolesMask > 0) ? chunk.HolesMap : EmptyHolesArray;

                    // Add the height map values, inserting them into their correct positions
                    for (var unitX = 0; unitX <= TerrainConstants.UnitsPerChunkSide; unitX++)
                    {
                        for (var unitY = 0; unitY <= TerrainConstants.UnitsPerChunkSide; unitY++)
                        {
                            var tileX = (x*TerrainConstants.UnitsPerChunkSide) + unitX;
                            var tileY = (y*TerrainConstants.UnitsPerChunkSide) + unitY;

                            var vertIndex = tileVertLocations[tileX, tileY];
                            if (vertIndex == -1)
                            {
                                var xPos = TerrainConstants.CenterPoint
                                           - (TileX * TerrainConstants.TileSize)
                                           - (tileX * TerrainConstants.UnitSize);
                                var yPos = TerrainConstants.CenterPoint
                                           - (TileY * TerrainConstants.TileSize)
                                           - (tileY * TerrainConstants.UnitSize);
                                var zPos = (heights[unitX, unitY] + chunk.MedianHeight);
                                tileVertLocations[tileX, tileY] = tileVerts.Count;
                                tileVerts.Add(new Vector3(xPos, yPos, zPos));
                            }


                            if (unitY == TerrainConstants.UnitsPerChunkSide) continue;
                            if (unitX == TerrainConstants.UnitsPerChunkSide) continue;

                            tileHolesMap[tileX, tileY] = holes[unitX / 2, unitY / 2];
                        }
                    }
                }
            }

            tileIndices = new List<int>();
            for (var tileX = 0; tileX < heightsPerTileSide; tileX++)
            {
                for (var tileY = 0; tileY < heightsPerTileSide; tileY++)
                {
                    if (tileHolesMap[tileX, tileY]) continue;
                    /*This 3 index makes the top triangle
                                *
                                *0--1--2
                                *| /| /
                                *|/ |/ 
                                *9  10 11
                                */
                    var vertId0 = tileVertLocations[tileX, tileY];
                    var vertId1 = tileVertLocations[tileX, tileY + 1];
                    var vertId9 = tileVertLocations[tileX + 1, tileY];
                    tileIndices.Add(vertId0);
                    tileIndices.Add(vertId1);
                    tileIndices.Add(vertId9);
                    
                    /*This 3 index makes the bottom triangle
                                 *
                                 *0  1   2
                                 *  /|  /|
                                 * / | / |
                                 *9--10--11
                                 */
                    var vertId10 = tileVertLocations[tileX + 1, tileY + 1];
                    tileIndices.Add(vertId1);
                    tileIndices.Add(vertId10);
                    tileIndices.Add(vertId9);
                }
            }
        }

		private void AppendWMOandM2Info(ref List<Vector3> verts, ref List<int> indices)
		{
            if (verts == null) verts = new List<Vector3>();
            if (indices == null) indices = new List<int>();

            var clipper = new MeshClipper(Bounds);
            List<Vector3> newVertices;
            List<int> newIndices;
		    
            // Add WMO & M2 vertices
		    int offset;
		    foreach (var wmo in WMOs)
		    {
		        clipper.ClipMesh(wmo.WmoVertices, wmo.WmoIndices, out newVertices, out newIndices);
                
                offset = verts.Count;
                if (newVertices.Count > 0 && newIndices.Count > 0)
                {
                    verts.AddRange(newVertices);
                    foreach (var index in newIndices)
                    {
                        indices.Add(offset + index);
                    }
                }

                clipper.ClipMesh(wmo.WmoM2Vertices, wmo.WmoM2Indices, out newVertices, out newIndices);

                offset = verts.Count;
                if (newVertices.Count <= 0 || newIndices.Count <= 0) continue;
                verts.AddRange(newVertices);
                foreach (var index in newIndices)
                {
                    indices.Add(offset + index);
                }
            }

		    foreach (var m2 in M2s)
		    {
		        clipper.ClipMesh(m2.Vertices, m2.Indices, out newVertices, out newIndices);

		        offset = verts.Count;
		        if (newVertices.Count == 0 || newIndices.Count == 0) continue;

		        verts.AddRange(newVertices);
		        foreach (var index in newIndices)
		        {
		            indices.Add(offset + index);
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

		public override float GetLiquidHeight(Point2D chunkCoord, Point2D unitCoord)
		{
			var chunk = Chunks[chunkCoord.X, chunkCoord.Y];
			if (!chunk.HasLiquid)
				return float.MinValue;


			var liquidHeights = chunk.LiquidHeights;

			return (liquidHeights == null) ? float.MinValue
											: liquidHeights[unitCoord.X, unitCoord.Y];
		}

		public override LiquidType GetLiquidType(Point2D chunkCoord)
		{
            var chunk = Chunks[chunkCoord.X, chunkCoord.Y];

		    return (chunk.HasLiquid)
		               ? chunk.LiquidType
		               : LiquidType.None;
		}

        private static void EnsureClockwiseWinding(List<int> indices, List<Vector3> vertices)
        {
            if (indices == null || indices.Count < 3) return;
            if (vertices == null || vertices.Count < 1) return;

            for (var i = 0; i < indices.Count; i += 3)
            {

                var vert0 = vertices[indices[i + 0]];
                var vert1 = vertices[indices[i + 1]];
                var vert2 = vertices[indices[i + 2]];

                if (!GeometryHelpers.IsCounterClockwiseXY(vert0, vert1, vert2)) continue;

                var temp = indices[i + 1];
                indices[i + 1] = indices[i + 2];
                indices[i + 2] = temp;
            }
        }
    }
}
