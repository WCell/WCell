using System;
using System.IO;
using System.Text;
using WCell.Constants;
using WCell.Util.Graphics;

namespace WCell.Collision
{
    internal class WorldMapTile
    {
        private const string fileType = "ter";

        private readonly bool[,] LiquidProfile = 
            new bool[TerrainConstants.ChunksPerTileSide,TerrainConstants.ChunksPerTileSide];

        private readonly FluidType[,] LiquidTypes =
            new FluidType[TerrainConstants.ChunksPerTileSide,TerrainConstants.ChunksPerTileSide];

        private readonly HeightMap[,] HeightMaps =
            new HeightMap[TerrainConstants.ChunksPerTileSide,TerrainConstants.ChunksPerTileSide];

        
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="file">The full path to the TileInfo file to open.</param>
        internal WorldMapTile(string file)
        {
            if (!File.Exists(file))
                return;

            using (var fs = new FileStream(file, FileMode.Open))
            using (var br = new BinaryReader(fs))
            {
                var key = br.ReadString();
                if (key != fileType)
                {
                    Console.WriteLine("Invalid file format, suckah!");
                }

                ReadLiquidProfile(br, LiquidProfile);
                ReadLiquidTypes(br, LiquidTypes);
                ReadHeightMaps(br, LiquidProfile, HeightMaps);

                br.Close();
            }
        }

        internal float GetInterpolatedHeight(ChunkCoord chunkCoord, PointX2D coord, HeightMapFraction heightMapFraction)
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

            var heightMap = HeightMaps[chunkCoord.ChunkX, chunkCoord.ChunkY];
            if (heightMap == null) return float.NaN;

            var medianHeight = HalfUtils.Unpack(heightMap.MedianHeight);
            if (heightMap.IsFlat) return medianHeight;

            // Fixme:  Indexing is backwards.
            var heightX = (coord.Y == TerrainConstants.UnitsPerChunkSide)
                              ? (TerrainConstants.UnitsPerChunkSide - 1)
                              : coord.Y;
            var heightY = (coord.X == TerrainConstants.UnitsPerChunkSide)
                              ? (TerrainConstants.UnitsPerChunkSide - 1)
                              : coord.X;

            
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
                    var packed1 = heightMap.OuterHeightDiff[heightX, heightY];
                    var packed2 = heightMap.OuterHeightDiff[heightX, heightY + 1];
                    var packed3 = heightMap.OuterHeightDiff[heightX + 1, heightY];
                    var packed5 = heightMap.InnerHeightDiff[heightX, heightY];

                    var h1 = HalfUtils.Unpack(packed1);
                    var h2 = HalfUtils.Unpack(packed2);
                    var h3 = HalfUtils.Unpack(packed3);
                    var h5 = HalfUtils.Unpack(packed5);

                    topLeft = h1;
                    topRight = (h1 + h2)/2.0f;
                    bottomLeft = (h1 + h3)/2.0f;
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
                    var packed1 = heightMap.OuterHeightDiff[heightX, heightY];
                    var packed3 = heightMap.OuterHeightDiff[heightX + 1, heightY];
                    var packed4 = heightMap.OuterHeightDiff[heightX + 1, heightY + 1];
                    var packed5 = heightMap.InnerHeightDiff[heightX, heightY];

                    var h1 = HalfUtils.Unpack(packed1);
                    var h3 = HalfUtils.Unpack(packed3);
                    var h4 = HalfUtils.Unpack(packed4);
                    var h5 = HalfUtils.Unpack(packed5);

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
                    var packed1 = heightMap.OuterHeightDiff[heightX, heightY];
                    var packed2 = heightMap.OuterHeightDiff[heightX, heightY + 1];
                    var packed4 = heightMap.OuterHeightDiff[heightX + 1, heightY + 1];
                    var packed5 = heightMap.InnerHeightDiff[heightX, heightY];

                    var h1 = HalfUtils.Unpack(packed1);
                    var h2 = HalfUtils.Unpack(packed2);
                    var h4 = HalfUtils.Unpack(packed4);
                    var h5 = HalfUtils.Unpack(packed5);

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
                    var packed2 = heightMap.OuterHeightDiff[heightX, heightY + 1];
                    var packed3 = heightMap.OuterHeightDiff[heightX + 1, heightY];
                    var packed4 = heightMap.OuterHeightDiff[heightX + 1, heightY + 1];
                    var packed5 = heightMap.InnerHeightDiff[heightX, heightY];

                    var h2 = HalfUtils.Unpack(packed2);
                    var h3 = HalfUtils.Unpack(packed3);
                    var h4 = HalfUtils.Unpack(packed4);
                    var h5 = HalfUtils.Unpack(packed5);

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

        internal float GetLiquidHeight(ChunkCoord chunkCoord, PointX2D pointX2D)
        {
            if (!LiquidProfile[chunkCoord.ChunkX, chunkCoord.ChunkY])
                return float.MinValue;

            var heightMap = HeightMaps[chunkCoord.ChunkX, chunkCoord.ChunkY];
            var liquidHeights = heightMap.LiquidHeight;

            return (liquidHeights == null)
                       ? float.MinValue
                       : liquidHeights[pointX2D.X, pointX2D.Y];
        }

        internal FluidType GetLiquidType(ChunkCoord chunkCoord)
        {
            return LiquidTypes[chunkCoord.ChunkX, chunkCoord.ChunkY];
        }

        internal void DumpChunk(ChunkCoord chunkCoord)
        {
            if (HeightMaps == null) return;

            var heightMap = HeightMaps[chunkCoord.ChunkX, chunkCoord.ChunkY];
            if (heightMap == null) return;

            var medianHeight = HalfUtils.Unpack(heightMap.MedianHeight);
            
            var fileName = String.Format("Chunk_{0}_{1}.txt", chunkCoord.ChunkX, chunkCoord.ChunkY);
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
                    var packed = heightMap.OuterHeightDiff[row, col];
                    var unpacked = HalfUtils.Unpack(packed);
                    var value = Math.Round(unpacked, 2);
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
                        var packed = heightMap.InnerHeightDiff[row, col];
                        var unpacked = HalfUtils.Unpack(packed);
                        var value = Math.Round(unpacked, 2);
                        if (value >= 0) writer.Write(" ");
                        writer.Write(String.Format("{0:00.00}    ", value));
                    }
                    writer.Write(writer.NewLine);
                    writer.Write(writer.NewLine);
                }
            }

            writer.Close();
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
                        map.OuterHeightDiff[x, y] = reader.ReadUInt16();
                    }
                }

                for (var y = 0; y < 8; y++)
                {
                    for (var x = 0; x < 8; x++)
                    {
                        map.InnerHeightDiff[x, y] = reader.ReadUInt16();
                    }
                }
            }

            if (readLiquid)
            {
                for (var y = 0; y < 9; y++)
                {
                    for (var x = 0; x < 9; x++)
                    {
                        map.LiquidHeight[x, y] = reader.ReadUInt16();
                    }
                }
            }
            else
            {
                map.LiquidHeight = null;
            }

            return map;
        }

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
    }

    public class HeightMap
    {
        public bool IsFlat;
        public ushort MedianHeight;
        public ushort[,] LiquidHeight = new ushort[9, 9];
        public ushort[,] OuterHeightDiff = new ushort[9, 9];
        public ushort[,] InnerHeightDiff = new ushort[8, 8];
    }
}