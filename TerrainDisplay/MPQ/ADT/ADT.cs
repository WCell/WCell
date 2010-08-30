using System;
using System.Collections.Generic;
using System.IO;
using Terra;
using TerrainDisplay.Collision;
using TerrainDisplay.Collision._3D;
using TerrainDisplay.MPQ.ADT.Components;
using TerrainDisplay.MPQ.WMO;
using TerrainDisplay.Util;
using WCell.Util.Graphics;

namespace TerrainDisplay.MPQ.ADT
{
    public class ADT : ADTBase
    {
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
        /// Array of MCNK chunks which give the ADT vertex information for this ADT
        /// </summary>
        public readonly ADTChunk[,] MapChunks = new ADTChunk[16, 16];
        /// <summary>
        /// Array of MH20 chunks which give the ADT FLUID vertex information for this ADT
        /// </summary>
        public readonly MH2O[,] LiquidInfo = new MH2O[16, 16];
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

        #endregion

        #region Variables

        private MpqTerrainManager _mpqTerrainManager;

        /// <summary>
        /// The continent of the ADT
        /// </summary>
        private TileIdentifier _tileId;

        /// <summary>
        /// Filename of the ADT
        /// </summary>
        /// <example>Azeroth_32_32.adt</example>
        public string FileName
        {
            get { return TerrainConstants.GetMapFilename(_tileX, _tileY); }
        }

        /// <summary>
        /// The X offset of the map in the 64 x 64 grid
        /// </summary>
        private int _tileX
        {
            get { return _tileId.TileX; }
        }

        /// <summary>
        /// The Y offset of the map in the 64 x 64 grid
        /// </summary>
        private int _tileY
        {
            get { return _tileId.TileY; }
        }

        public Rect TileBounds2d
        {
            get
            {
                var tileBaseX = TerrainConstants.CenterPoint - (_tileX*TerrainConstants.TileSize);
                var tileBaseY = TerrainConstants.CenterPoint - (_tileY*TerrainConstants.TileSize);
                var tileBottomX = tileBaseX - TerrainConstants.TileSize;
                var tileBottonY = tileBaseY - TerrainConstants.TileSize;
                return new Rect(new Point(tileBaseX, tileBaseY), new Point(tileBottomX, tileBottonY));
            }
        }

        public bool IsWMOOnly;
        #endregion

        #region Constructors

        public ADT(TileIdentifier tileId, MpqTerrainManager mpqTerrainManager)
        {
            _mpqTerrainManager = mpqTerrainManager;
            _tileId = tileId;

            QuadTree = new QuadTree<TerrainTriangleHolder>(new Size(TerrainConstants.ChunkSize, TerrainConstants.ChunkSize),
                                          TerrainConstants.UnitsPerChunkSide * TerrainConstants.UnitsPerChunkSide);
        }

        public ADT(TileIdentifier tileId)
        {
            _tileId = tileId;
        }
        #endregion

        public override void GenerateLiquidVertexAndIndices()
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

        public override void GenerateHeightVertexAndIndices()
        {
            const int heightsPerTileSide = TerrainConstants.UnitsPerChunkSide * TerrainConstants.ChunksPerTileSide;
            var tileHeights = new float[heightsPerTileSide + 1, heightsPerTileSide + 1];
            var tileHoles = new List<Index2>();


            for (var chunkRow = 0; chunkRow < TerrainConstants.ChunksPerTileSide; chunkRow++)
            {
                for (var chunkCol = 0; chunkCol < TerrainConstants.ChunksPerTileSide; chunkCol++)
                {
                    var chunk = MapChunks[chunkCol, chunkRow];
                    var heights = chunk.Heights.GetLowResMapMatrix();
                    var holes = (chunk.Header.Holes > 0) ? chunk.Header.GetHolesMap() : new bool[4, 4];

                    // Add the height map values, inserting them into their correct positions
                    for (var unitRow = 0; unitRow <= TerrainConstants.UnitsPerChunkSide; unitRow++)
                    {
                        for (var unitCol = 0; unitCol <= TerrainConstants.UnitsPerChunkSide; unitCol++)
                        {
                            var tileRow = chunkRow * TerrainConstants.UnitsPerChunkSide + unitRow;
                            var tileCol = chunkCol * TerrainConstants.UnitsPerChunkSide + unitCol;

                            tileHeights[tileRow, tileCol] = heights[unitCol, unitRow] + chunk.Header.Z;

                            if (unitCol == TerrainConstants.UnitsPerChunkSide) continue;
                            if (unitRow == TerrainConstants.UnitsPerChunkSide) continue;
                            // Add the hole vertices to the pre-insertion 'script'
                            if (!holes[unitRow / 2, unitCol / 2]) continue;

                            tileHoles.AddUnique(new Index2
                            {
                                Y = tileCol,
                                X = tileRow
                            });

                            tileHoles.AddUnique(new Index2
                            {
                                Y = Math.Min(tileCol + 1, heightsPerTileSide),
                                X = tileRow
                            });

                            tileHoles.AddUnique(new Index2
                            {
                                Y = tileCol,
                                X = Math.Min(tileRow + 1, heightsPerTileSide)
                            });

                            tileHoles.AddUnique(new Index2
                            {
                                Y = Math.Min(tileCol + 1, heightsPerTileSide),
                                X = Math.Min(tileRow + 1, heightsPerTileSide)
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
            Indices = new List<int>();
            terra.GenerateOutput(allHoleIndices, out newVertices, out Indices);

            TerrainVertices = new List<Vector3>();
            foreach (var vertex in newVertices)
            {
                var xPos = TerrainConstants.CenterPoint - (_tileX * TerrainConstants.TileSize) - (vertex.X * TerrainConstants.UnitSize);
                var yPos = TerrainConstants.CenterPoint - (_tileY * TerrainConstants.TileSize) - (vertex.Y * TerrainConstants.UnitSize);
                TerrainVertices.Add(new Vector3(xPos, yPos, vertex.Z));
            }

            SortTrisIntoChunks();
        }

        /// <summary>
        /// Adds the rendering liquid vertices to the provided list for the MapChunk given by:
        /// </summary>
        /// <param name="indexY">The y index of the map chunk.</param>
        /// <param name="indexX">The x index of the map chunk</param>
        /// <param name="vertices">The Collection to add the vertices to.</param>
        /// <returns>The number of vertices added.</returns>
        public override int GenerateLiquidVertices(int indexY, int indexX, ICollection<Vector3> vertices)
        {
            var tempVertexCounter = 0;
            var mapChunk = MapChunks[indexY, indexX];

            var mh2O = LiquidInfo[indexY, indexX];

            if (mh2O == null) return tempVertexCounter;
            if (!mh2O.Header.Used) return tempVertexCounter;

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

            var mh2OHeightMap = mh2O.GetMapHeightsMatrix();
            for (var xStep = 0; xStep < 9; xStep++)
            {
                for (var yStep = 0; yStep < 9; yStep++)
                {
                    var xPos = TerrainConstants.CenterPoint - (_tileX * TerrainConstants.TileSize) -
                               (indexX * TerrainConstants.ChunkSize) - (xStep * TerrainConstants.UnitSize);
                    var yPos = TerrainConstants.CenterPoint - (_tileY * TerrainConstants.TileSize) -
                               (indexY * TerrainConstants.ChunkSize) - (yStep * TerrainConstants.UnitSize);

                    if (((xStep < mh2O.Header.XOffset) || ((xStep - mh2O.Header.XOffset) > mh2O.Header.Height)) ||
                        ((yStep < mh2O.Header.YOffset) || ((yStep - mh2O.Header.YOffset) > mh2O.Header.Width)))
                    {
                        continue;
                    }

                    var zPos = mh2OHeightMap[yStep - mh2O.Header.YOffset, xStep - mh2O.Header.XOffset];

                    var position = new Vector3(xPos, yPos, zPos);

                    vertices.Add(position);
                    tempVertexCounter++;
                }
            }
            return tempVertexCounter;
        }

        /// <summary>
        /// Adds the rendering liquid indices to the provided list for the MapChunk given by:
        /// </summary>
        /// <param name="indexY">The y index of the map chunk.</param>
        /// <param name="indexX">The x index of the map chunk</param>
        /// <param name="offset">The number to add to the indices so as to match the end of the Vertices list.</param>
        /// <param name="indices">The Collection to add the indices to.</param>
        public override void GenerateLiquidIndices(int indexY, int indexX, int offset, List<int> indices)
        {
            var mh2O = LiquidInfo[indexY, indexX];
            if (mh2O == null) return;
            if (!mh2O.Header.Used) return;

            var renderMap = mh2O.GetRenderBitMapMatrix();
            for (int r = mh2O.Header.XOffset; r < (mh2O.Header.XOffset + mh2O.Header.Height); r++)
            {
                for (int c = mh2O.Header.YOffset; c < (mh2O.Header.YOffset + mh2O.Header.Width); c++)
                {
                    var row = r - mh2O.Header.XOffset;
                    var col = c - mh2O.Header.YOffset;

                    if (!renderMap[col, row] && ((mh2O.Header.Height != 8) || (mh2O.Header.Width != 8))) continue;
                    indices.Add(offset + ((row + 1) * (mh2O.Header.Width + 1) + col));
                    indices.Add(offset + (row * (mh2O.Header.Width + 1) + col));
                    indices.Add(offset + (row * (mh2O.Header.Width + 1) + col + 1));
                    indices.Add(offset + ((row + 1) * (mh2O.Header.Width + 1) + col + 1));
                    indices.Add(offset + ((row + 1) * (mh2O.Header.Width + 1) + col));
                    indices.Add(offset + (row * (mh2O.Header.Width + 1) + col + 1));
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
        public override int GenerateHeightVertices(int indexY, int indexX, ICollection<Vector3> vertices)
        {
            var mcnk = MapChunks[indexY, indexX];
            var lowResMap = mcnk.Heights.GetLowResMapMatrix();
            var lowResNormal = mcnk.Normals.GetLowResNormalMatrix();

            var counter = 0;
            for (var xStep = 0; xStep < 9; xStep++)
            {
                for (var yStep = 0; yStep < 9; yStep++)
                {
                    var xPos = TerrainConstants.CenterPoint - (_tileX * TerrainConstants.TileSize) - (indexX * TerrainConstants.ChunkSize) -
                               (xStep * TerrainConstants.UnitSize);
                    var yPos = TerrainConstants.CenterPoint - (_tileY * TerrainConstants.TileSize) - (indexY * TerrainConstants.ChunkSize) -
                               (yStep * TerrainConstants.UnitSize);
                    var zPos = lowResMap[yStep, xStep] + mcnk.Header.Z;

                    var theNormal = lowResNormal[yStep, xStep];

                    //var cosAngle = Vector3.Dot(Vector3.Up, theNormal);
                    //var angle = MathHelper.ToDegrees((float)Math.Acos(cosAngle));

                    //if (angle > 50.0)
                    //{
                    //    color = Color.Brown;
                    //}

                    var position = new Vector3(xPos, yPos, zPos);
                    vertices.Add(position);
                    counter++;
                }
            }
            return counter;
        }

        /// <summary>
        /// Adds the rendering indices to the provided list for the MapChunk given by:
        /// </summary>
        /// <param name="indexY">The y index of the map chunk.</param>
        /// <param name="indexX">The x index of the map chunk</param>
        /// <param name="offset">The number to add to the indices so as to match the end of the Vertices list.</param>
        /// <param name="indices">The Collection to add the indices to.</param>
        public override void GenerateHeightIndices(int indexY, int indexX, int offset, List<int> indices)
        {
            var mcnk = MapChunks[indexY, indexX];

            var holesMap = new bool[4, 4];
            if (mcnk.Header.Holes > 0)
            {
                holesMap = mcnk.Header.GetHolesMap();
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

        private void SortTrisIntoChunks()
        {
            var baseXPos = TerrainConstants.CenterPoint - (_tileId.TileX)*TerrainConstants.TileSize;
            var baseYPos = TerrainConstants.CenterPoint - (_tileId.TileY)*TerrainConstants.TileSize;

            //Triangulate the indices
            var triList = new List<Index3>();
            for (var i = 0; i < Indices.Count; )
            {
                triList.Add(new Index3
                {
                    Index0 = (short)Indices[i++],
                    Index1 = (short)Indices[i++],
                    Index2 = (short)Indices[i++]
                });
            }

            for (var chunkX = 0; chunkX < TerrainConstants.ChunksPerTileSide; chunkX++)
            {
                for (var chunkY = 0; chunkY < TerrainConstants.ChunksPerTileSide; chunkY++)
                {
                    var chunk = MapChunks[chunkY, chunkX];
                    var chunkBaseX = baseXPos - TerrainConstants.ChunkSize*chunkX;
                    var chunkBaseY = baseYPos - TerrainConstants.ChunkSize*chunkY;
                    var chunkBottomX = chunkBaseX - TerrainConstants.ChunkSize;
                    var chunkBottomY = chunkBaseY - TerrainConstants.ChunkSize;
                    var chunkRect = new Rect(new Point(chunkBaseX, chunkBaseY),
                                             new Point(chunkBottomX, chunkBottomY));

                    foreach (var triangle in triList)
                    {
                        var vertex0 = TerrainVertices[triangle.Index0];
                        var vertex1 = TerrainVertices[triangle.Index1];
                        var vertex2 = TerrainVertices[triangle.Index2];
                        var min = Vector3.Min(Vector3.Min(vertex0, vertex1), vertex2);
                        var max = Vector3.Max(Vector3.Max(vertex0, vertex1), vertex2);

                        var triRect = new Rect(new Point(min.X, min.Y), new Point(max.X, max.Y));
                        if (!chunkRect.IntersectsWith(triRect)) continue;

                        // Check if any of the triangle's vertices are contained in the chunk's bounds
                        if (chunkRect.Contains(vertex0.X, vertex0.Y))
                        {
                            chunk.TerrainTris.Add(triangle);
                            continue;
                        }

                        if (chunkRect.Contains(vertex1.X, vertex1.Y))
                        {
                            chunk.TerrainTris.Add(triangle);
                            continue;
                        }

                        if (chunkRect.Contains(vertex2.X, vertex2.Y))
                        {
                            chunk.TerrainTris.Add(triangle);
                            continue;
                        }

                        // Check if any of the Chunk's corners are contained in the triangle
                        if (Intersection.PointInTriangle2DXY(chunkRect.TopLeft, vertex0, vertex1, vertex2))
                        {
                            chunk.TerrainTris.Add(triangle);
                            continue;
                        }

                        if (Intersection.PointInTriangle2DXY(chunkRect.TopRight, vertex0, vertex1, vertex2))
                        {
                            chunk.TerrainTris.Add(triangle);
                            continue;
                        }

                        if (Intersection.PointInTriangle2DXY(chunkRect.BottomLeft, vertex0, vertex1, vertex2))
                        {
                            chunk.TerrainTris.Add(triangle);
                            continue;
                        }

                        if (Intersection.PointInTriangle2DXY(chunkRect.BottomRight, vertex0, vertex1, vertex2))
                        {
                            chunk.TerrainTris.Add(triangle);
                            continue;
                        }

                        // Check if any of the triangle's line segments intersect the chunk's bounds
                        if (Intersection.IntersectSegmentRectangle2DXY(chunkRect, vertex0, vertex1))
                        {
                            chunk.TerrainTris.Add(triangle);
                            continue;
                        }

                        if (Intersection.IntersectSegmentRectangle2DXY(chunkRect, vertex0, vertex2))
                        {
                            chunk.TerrainTris.Add(triangle);
                            continue;
                        }

                        if (Intersection.IntersectSegmentRectangle2DXY(chunkRect, vertex1, vertex2))
                        {
                            chunk.TerrainTris.Add(triangle);
                            continue;
                        }
                    }
                }
            }
        }

        public bool GetPotentialColliders(Ray2D ray2D, List<Index3> colliders)
        {
            var tileRect = TileBounds2d;

            Vector2 start, end;
            if (!tileRect.ClipRay(ray2D, out start, out end)) return false;

            if (colliders == null)
            {
                colliders = new List<Index3>();
            }

            int startChunkX, startChunkY;
            GetChunkXY(start, out startChunkX, out startChunkY);

            int endChunkX, endChunkY;
            GetChunkXY(end, out endChunkX, out endChunkY);

            if (startChunkX > endChunkX)
            {
                var temp = startChunkX;
                startChunkX = endChunkX;
                endChunkX = temp;
            }

            if (startChunkY > endChunkY)
            {
                var temp = startChunkY;
                startChunkY = endChunkY;
                endChunkY = temp;
            }

            for (var chunkY = startChunkY; chunkY <= endChunkY; chunkY++)
            {
                for (var chunkX = startChunkX; chunkX <= endChunkX; chunkX++)
                {
                    var chunk = MapChunks[chunkY, chunkX];
                    var basePos = new Point(chunk.Header.X, chunk.Header.Y);
                    var bottomPos = new Point(basePos.X - TerrainConstants.ChunkSize,
                                              basePos.Y - TerrainConstants.ChunkSize);
                    var chunkRect = new Rect(basePos, bottomPos);

                    if (!chunkRect.IntersectsWith(ray2D)) continue;

                    colliders.AddRange(chunk.TerrainTris);
                }
            }

            return true;
        }

        public bool GetChunkXY(Vector2 pos, out int chunkX, out int chunkY)
        {
            var tileBaseX = TerrainConstants.CenterPoint - (_tileX*TerrainConstants.TileSize);
            var tileBaseY = TerrainConstants.CenterPoint - (_tileY*TerrainConstants.TileSize);

            var posX = (pos.X - tileBaseX)*-1;
            var posY = (pos.Y - tileBaseY)*-1;

            chunkX = (int) (posX/TerrainConstants.ChunkSize);
            chunkY = (int) (posY/TerrainConstants.ChunkSize);

            if (chunkX < 0) return false;
            if (chunkY < 0) return false;
            if (chunkX >= TerrainConstants.ChunksPerTileSide) return false;
            if (chunkY >= TerrainConstants.ChunksPerTileSide) return false;
            return true;
        }
    }
}
