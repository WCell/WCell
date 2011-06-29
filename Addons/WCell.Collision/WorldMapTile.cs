using System;
using System.IO;
using System.Text;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Terrain.MPQ.ADT;
using WCell.Util.Graphics;

namespace WCell.Collision
{
    internal class WorldMapTile : ADT
    {
        private const string fileType = "ter";

        private readonly bool[,] LiquidProfile = 
            new bool[TerrainConstants.ChunksPerTileSide,TerrainConstants.ChunksPerTileSide];

        private readonly FluidType[,] LiquidTypes =
            new FluidType[TerrainConstants.ChunksPerTileSide,TerrainConstants.ChunksPerTileSide];

        private readonly HeightMap[,] HeightMaps =
            new HeightMap[TerrainConstants.ChunksPerTileSide,TerrainConstants.ChunksPerTileSide];

        
        
        /// <summary>
        /// Reads a new Tile from the given file.
		/// The writer is defined in <see cref="TerrainExtractor.Extractor.TileExtractor">the TileExtractor class</see>
        /// </summary>
        /// <param name="file">The full path to the TileInfo file to open.</param>
		public WorldMapTile(string file, Point2D coord, MapId map) : 
			base(coord, map)
        {
            if (!File.Exists(file))
                return;

			Load(file);
        }

		#region Loading
    	private void Load(string file)
    	{
			using (var fs = new FileStream(file, FileMode.Open))
			using (var reader = new BinaryReader(fs))
			{
				var key = reader.ReadString();
				if (key != fileType)
				{
					throw new ArgumentException("Invalid tile file: " + file);
				}

				/*
				writer.Write(adt.IsWMOOnly);
				WriteWMODefs(writer, adt.ObjectDefinitions);

				if (adt.IsWMOOnly) return;
				WriteM2Defs(writer, adt.DoodadDefinitions);

				WriteQuadTree(writer, adt.QuadTree);

				writer.Write(adt.TerrainVertices);

				for (var x = 0; x < TerrainConstants.ChunksPerTileSide; x++)
				{
					for (var y = 0; y < TerrainConstants.ChunksPerTileSide; y++)
					{
						var chunk = adt.MapChunks[y, x];
						// Whether this chunk has a height map
						WriteChunkInfo(writer, chunk);
					}
				}
				 */
				IsWMOOnly = reader.ReadBoolean();
				ReadWMODefs(reader);

				if (IsWMOOnly) return;
				ReadM2Defs(reader);
				ReadQuadTree(reader);
				


				//ReadLiquidProfile(br, LiquidProfile);
				//ReadLiquidTypes(br, LiquidTypes);
				//ReadHeightMaps(br, LiquidProfile, HeightMaps);
			}
    	}

		private void ReadWMODefs(BinaryReader reader)
		{

		}

		private void ReadM2Defs(BinaryReader reader)
		{

		}

		private void ReadQuadTree(BinaryReader reader)
		{

		}


        private static void ReadLiquidProfile(BinaryReader reader, bool[,] profile)
        {
            for (var y = 0; y < TerrainConstants.ChunksPerTileSide; y++)
            {
                for (var x = 0; x < TerrainConstants.ChunksPerTileSide; x++)
                {
                    profile[x, y] = reader.ReadBoolean();
                }
            }
        }

        private static void ReadLiquidTypes(BinaryReader reader, FluidType[,] types)
        {
            for (var y = 0; y < TerrainConstants.ChunksPerTileSide; y++)
            {
                for (var x = 0; x < TerrainConstants.ChunksPerTileSide; x++)
                {
                    types[x, y] = (FluidType)reader.ReadByte();
                }
            }
        }

        private static void ReadHeightMaps(BinaryReader reader, bool[,] profile, HeightMap[,] heightMaps)
        {
            for (var y = 0; y < TerrainConstants.ChunksPerTileSide; y++)
            {
                for (var x = 0; x < TerrainConstants.ChunksPerTileSide; x++)
                {
                    heightMaps[x, y] = ReadHeightMap(reader, profile[x, y]);
                }
            }
        }

        private static HeightMap ReadHeightMap(BinaryReader reader, bool readLiquid)
        {
            /* This method should now read this structure
             * TileExtractor WriteChunkInfo(BinaryWriter writer, ADTChunk chunk)
            writer.Write(chunk.NodeId);
            writer.Write(chunk.IsFlat);
            // The base height for this chunk
            writer.Write(chunk.Header.Z);
            // The wmos and m2s (UniqueIds) that overlap this chunk
            WriteChunkModelRefs(writer, chunk.DoodadRefs);
            WriteChunkObjRefs(writer, chunk.ObjectRefs);

            writer.Write(chunk.TerrainTris);
            //writer.Write(chunk.Header.Holes > 0);
            //if (chunk.Header.Holes > 0)
            //{
            //    WriteChunkHolesMap(writer, chunk.Header.GetHolesMap());
            //}

            //// The height map
            //if (!chunk.IsFlat)
            //{
            //    WriteChunkHeightMap(writer, chunk);
            //}

            // The liquid information);
            if (chunk.WaterInfo == null)
            {
                writer.Write(false);
                return;
            }

            writer.Write(chunk.WaterInfo.Header.Used);
            if (!chunk.WaterInfo.Header.Used) return;

            writer.Write((ushort)chunk.WaterInfo.Header.Flags);
            writer.Write((ushort)chunk.WaterInfo.Header.Type);
            writer.Write(chunk.WaterInfo.IsFlat);
            writer.Write(chunk.WaterInfo.Header.HeightLevel1);
            writer.Write(chunk.WaterInfo.Header.HeightLevel2);

            if (chunk.WaterInfo.Header.Flags.HasFlag(MH2OFlags.Ocean)) return;
            WriteWaterRenderBits(writer, chunk.WaterInfo.GetRenderBitMapMatrix());

            if (chunk.WaterInfo.IsFlat) return;
            WriteWaterHeights(writer, chunk.WaterInfo.GetMapHeightsMatrix());*/

            var map = new HeightMap
            {
                IsFlat = reader.ReadBoolean(),
                MedianHeight = reader.ReadUInt16()
            };

            if (map.IsFlat)
            {
                map.OuterHeightDiff = null;
                map.InnerHeightDiff = null;
            }
            else
            {
                for (var y = 0; y < 9; y++)
                {
                    for (var x = 0; x < 9; x++)
                    {
                        map.OuterHeightDiff[x, y] = reader.ReadSingle();
                    }
                }

                for (var y = 0; y < 8; y++)
                {
                    for (var x = 0; x < 8; x++)
                    {
                        map.InnerHeightDiff[x, y] = reader.ReadSingle();
                    }
                }
            }

            if (readLiquid)
            {
                for (var y = 0; y < 9; y++)
                {
                    for (var x = 0; x < 9; x++)
                    {
                        map.LiquidHeight[x, y] = reader.ReadSingle();
                    }
                }
            }
            else
            {
                map.LiquidHeight = null;
            }

            return map;
        }
		#endregion

		#region Terrain Queries
		internal float GetInterpolatedHeight(Point2D chunkCoord, Point2D unitCoord, HeightMapFraction heightMapFraction)
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
			if (HeightMaps == null) return float.NaN;

			var heightMap = HeightMaps[chunkCoord.X, chunkCoord.Y];
			if (heightMap == null) return float.NaN;

			var medianHeight = heightMap.MedianHeight;
			if (heightMap.IsFlat) return medianHeight;

			// Fixme:  Indexing is backwards.
			var heightX = (unitCoord.Y == TerrainConstants.UnitsPerChunkSide)
							  ? (TerrainConstants.UnitsPerChunkSide - 1)
							  : unitCoord.Y;
			var heightY = (unitCoord.X == TerrainConstants.UnitsPerChunkSide)
							  ? (TerrainConstants.UnitsPerChunkSide - 1)
							  : unitCoord.X;


			// Determine what quad we're in
			var xf = heightMapFraction.FractionX;
			var yf = heightMapFraction.FractionY;

			float topLeft, topRight, bottomLeft, bottomRight;
			if (xf < 0.5f)
			{
				if (yf < 0.5)
				{
					// Calc the #1 quad
					// +--------------> X
					// | h1--------h2    Coordinates are:
					// | |    |    |      h1 0,0
					// | |  1 |    |      h2 0,1
					// | |----h5---|      h3 1,0
					// | |    |    |      h4 1,1
					// | |    |    |      h5 1/2,1/2
					// | h3--------h4
					// V Y
					var h1 = heightMap.OuterHeightDiff[heightX, heightY];
					var h2 = heightMap.OuterHeightDiff[heightX, heightY + 1];
					var h3 = heightMap.OuterHeightDiff[heightX + 1, heightY];
					var h5 = heightMap.InnerHeightDiff[heightX, heightY];

					topLeft = h1;
					topRight = (h1 + h2) / 2.0f;
					bottomLeft = (h1 + h3) / 2.0f;
					bottomRight = h5;
				}
				else
				{
					// Calc the #3 quad
					// +--------------> X
					// | h1--------h2    Coordinates are:
					// | |    |    |      h1 0,0
					// | |    |    |      h2 0,1
					// | |----h5---|      h3 1,0
					// | |  3 |    |      h4 1,1
					// | |    |    |      h5 1/2,1/2
					// | h3--------h4
					// V Y
					var h1 = heightMap.OuterHeightDiff[heightX, heightY];
					var h3 = heightMap.OuterHeightDiff[heightX + 1, heightY];
					var h4 = heightMap.OuterHeightDiff[heightX + 1, heightY + 1];
					var h5 = heightMap.InnerHeightDiff[heightX, heightY];

					topLeft = (h1 + h3) / 2.0f;
					topRight = h5;
					bottomLeft = h3;
					bottomRight = (h3 + h4) / 2.0f;

					yf -= 0.5f;
				}
			}
			else
			{
				if (yf < 0.5)
				{
					// Calc the #2 quad
					// +--------------> X
					// | h1--------h2    Coordinates are:
					// | |    |    |      h1 0,0
					// | |    |  2 |      h2 0,1
					// | |----h5---|      h3 1,0
					// | |    |    |      h4 1,1
					// | |    |    |      h5 1/2,1/2
					// | h3--------h4
					// V Y
					var h1 = heightMap.OuterHeightDiff[heightX, heightY];
					var h2 = heightMap.OuterHeightDiff[heightX, heightY + 1];
					var h4 = heightMap.OuterHeightDiff[heightX + 1, heightY + 1];
					var h5 = heightMap.InnerHeightDiff[heightX, heightY];

					topLeft = (h1 + h2) / 2.0f;
					topRight = h2;
					bottomLeft = h5;
					bottomRight = (h2 + h4) / 2.0f;

					xf -= 0.5f;
				}
				else
				{
					// Calc the #4 quad
					// +--------------> X
					// | h1--------h2    Coordinates are:
					// | |    |    |      h1 0,0
					// | |    |    |      h2 0,1
					// | |----h5---|      h3 1,0
					// | |    |  4 |      h4 1,1
					// | |    |    |      h5 1/2,1/2
					// | h3--------h4
					// V Y
					var h2 = heightMap.OuterHeightDiff[heightX, heightY + 1];
					var h3 = heightMap.OuterHeightDiff[heightX + 1, heightY];
					var h4 = heightMap.OuterHeightDiff[heightX + 1, heightY + 1];
					var h5 = heightMap.InnerHeightDiff[heightX, heightY];

					topLeft = h5;
					topRight = (h2 + h4) / 2.0f;
					bottomLeft = (h3 + h4) / 2.0f;
					bottomRight = h4;

					xf -= 0.5f;
					yf -= 0.5f;
				}
			}

			//var heightDiff = InterpolateTriangle(ref vec1, ref vec2, ref vec3, heightMapFraction);
			var heightDiff = BilinearInterpolate(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight, ref xf,
												 ref yf);
			return medianHeight + heightDiff;
		}

		internal float GetLiquidHeight(Point2D chunkCoord, Point2D point2D)
		{
			if (!LiquidProfile[chunkCoord.X, chunkCoord.Y])
				return float.MinValue;

			var heightMap = HeightMaps[chunkCoord.X, chunkCoord.Y];
			var liquidHeights = heightMap.LiquidHeight;

			return (liquidHeights == null)
					   ? float.MinValue
					   : liquidHeights[point2D.X, point2D.Y];
		}

		internal FluidType GetLiquidType(Point2D chunkCoord)
		{
			return LiquidTypes[chunkCoord.X, chunkCoord.Y];
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
            var xxf = xf*2.0f;
            var yyf = yf*2.0f;

            var hf0 = xxf*topRight + ((1 - xxf)*topLeft);
            var hf1 = xxf*bottomRight + ((1 - xxf)*bottomLeft);

            return ((yyf*hf1) + ((1 - yyf)*hf0));
		}
		#endregion

		#region Dump
		internal void DumpChunk(Point2D chunkCoord)
		{
			if (HeightMaps == null) return;

			var heightMap = HeightMaps[chunkCoord.X, chunkCoord.Y];
			if (heightMap == null) return;

			var medianHeight = heightMap.MedianHeight;

			var fileName = String.Format("Chunk_{0}_{1}.txt", chunkCoord.X, chunkCoord.Y);
			var filePath = Path.Combine("C:\\Users\\Nate\\Desktop", fileName);
			var writer = new StreamWriter(filePath);

			writer.WriteLine(String.Format("MedianHeight: {0}", medianHeight));
			if (heightMap.IsFlat)
			{
				writer.Close();
				return;
			}
			writer.Write(writer.NewLine);

			for (var row = 0; row < 9; row++)
			{
				for (var col = 0; col < 9; col++)
				{
					var height = heightMap.OuterHeightDiff[row, col];
					var value = Math.Round(height, 2);
					if (value >= 0) writer.Write(" ");
					writer.Write(String.Format("{0:00.00}    ", value));
				}
				writer.Write(writer.NewLine);
				writer.Write(writer.NewLine);

				// write 8 floats
				if (row < 8)
				{
					writer.Write("    ");
					for (var col = 0; col < 8; col++)
					{
						var height = heightMap.InnerHeightDiff[row, col];
						var value = Math.Round(height, 2);
						if (value >= 0) writer.Write(" ");
						writer.Write(String.Format("{0:00.00}    ", value));
					}
					writer.Write(writer.NewLine);
					writer.Write(writer.NewLine);
				}
			}

			writer.Close();
		}
		#endregion
    }

    public class HeightMap
    {
        public bool IsFlat;
        public float MedianHeight;
        public float[,] LiquidHeight = new float[9, 9];
        public float[,] OuterHeightDiff = new float[9, 9];
        public float[,] InnerHeightDiff = new float[8, 8];
    }
}