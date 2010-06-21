using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrainDisplay.Collision._3D;
using TerrainDisplay.MPQ.ADT.Components;

namespace TerrainDisplay.MPQ.ADT
{
    public class ADT : ADTBase
    {
        private static Color TerrainColor = Color.SlateGray;
        private static Color WaterColor = Color.SlateGray;

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
        public readonly ADTChunk[,] MapChunks = new ADTChunk[16,16];
        /// <summary>
        /// Array of MH20 chunks which give the ADT FLUID vertex information for this ADT
        /// </summary>
        public readonly MH2O[,] LiquidInfo = new MH2O[16,16];
        /// <summary>
        /// List of MDDF Chunks which are placement information for M2s
        /// </summary>
        public readonly List<MapDoodadDefinition> DoodadDefinitions = new List<MapDoodadDefinition>(); // NEW
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

        /// <summary>
        /// The continent of the ADT
        /// </summary>
        private ContinentType _continent;

        /// <summary>
        /// Filename of the ADT
        /// </summary>
        /// <example>Azeroth_32_32.adt</example>
        public string FileName;

        private MpqTerrainManager _mpqTerrainManager;

        /// <summary>
        /// The X offset of the map in the 64 x 64 grid
        /// </summary>
        private int _tileX;

        /// <summary>
        /// The Y offset of the map in the 64 x 64 grid
        /// </summary>
        private int _tileY;

        #endregion

        #region Rendering Variables

        //public List<Triangle> Triangles;
        //public List<Triangle> LiquidTriangles;
        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the ADT class
        /// </summary>
        /// <param name="filePath">Filepath of the ADT</param>
        /// <example>ADT myADT = new ADT("Azeroth_32_32.adt");</example>
        public ADT(string filePath, MpqTerrainManager mpqTerrainManager)
        {
            _mpqTerrainManager = mpqTerrainManager;
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            FileName = fileName;

            string[] fileNameParts = fileName.Split('_');
            _continent = (ContinentType) Enum.Parse(typeof (ContinentType), fileNameParts[0], true);
            _tileX = Int32.Parse(fileNameParts[2]);
            _tileY = Int32.Parse(fileNameParts[1]);
        }

        #endregion

        public override void GenerateLiquidVertexAndIndices()
        {
            LiquidVertices = new List<VertexPositionNormalColored>();
            LiquidIndices = new List<int>();
           
            var vertexCounter = 0;
            for (var indexX = 0; indexX < 16; indexX++)
            {
                for (var indexY = 0; indexY < 16; indexY++)
                {
                    var tempVertexCounter = GenerateLiquidVertices(indexY, indexX, LiquidVertices, WaterColor);
                    GenerateLiquidIndices(indexY, indexX, vertexCounter, LiquidIndices);
                    vertexCounter += tempVertexCounter;
                }
            }
        }
        
        public override void GenerateHeightVertexAndIndices()
        {
            Vertices = new List<VertexPositionNormalColored>();
            Indices = new List<int>();

            for (var indexX = 0; indexX < 16; indexX++)
            {
                for (var indexY = 0; indexY < 16; indexY++)
                {
                    GenerateHeightIndices(indexY, indexX, Vertices.Count, Indices);
                    GenerateHeightVertices(indexY, indexX, Vertices, TerrainColor);
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
        public override int GenerateLiquidVertices(int indexY, int indexX, ICollection<VertexPositionNormalColored> vertices, Color color)
        {
            var tempVertexCounter = 0;
            var mapChunk = MapChunks[indexY, indexX];
            
            var mh2O = LiquidInfo[indexY, indexX];

            if (mh2O == null) return tempVertexCounter;
            if (!mh2O.Header.Used) return tempVertexCounter;

            var clr = Color.Green;

            switch (mh2O.Header.Type)
            {
                case FluidType.Water:
                    clr = Color.Blue;
                    break;
                case FluidType.Lava:
                    clr = Color.Red;
                    break;
                case FluidType.OceanWater:
                    clr = Color.Coral;
                    break;
            }

            var mh2OHeightMap = mh2O.GetMapHeightsMatrix();
            for (var xStep = 0; xStep < 9; xStep++)
            {
                for (var yStep = 0; yStep < 9; yStep++)
                {
                    var xPos = TerrainConstants.CenterPoint - (_tileX*TerrainConstants.TileSize) -
                               (indexX*TerrainConstants.ChunkSize) - (xStep*TerrainConstants.UnitSize);
                    var yPos = TerrainConstants.CenterPoint - (_tileY*TerrainConstants.TileSize) -
                               (indexY*TerrainConstants.ChunkSize) - (yStep*TerrainConstants.UnitSize);

                    if (((xStep < mh2O.Header.XOffset) || ((xStep - mh2O.Header.XOffset) > mh2O.Header.Height)) ||
                        ((yStep < mh2O.Header.YOffset) || ((yStep - mh2O.Header.YOffset) > mh2O.Header.Width)))
                    {
                        continue;
                    }

                    var zPos = mh2OHeightMap[yStep - mh2O.Header.YOffset, xStep - mh2O.Header.XOffset];

                    var position = new Vector3(xPos, yPos, zPos);

                    vertices.Add(new VertexPositionNormalColored(position, color, Vector3.Up));
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
                    indices.Add(offset + ((row + 1)*(mh2O.Header.Width + 1) + col));
                    indices.Add(offset + (row*(mh2O.Header.Width + 1) + col));
                    indices.Add(offset + (row*(mh2O.Header.Width + 1) + col + 1));
                    indices.Add(offset + ((row + 1)*(mh2O.Header.Width + 1) + col + 1));
                    indices.Add(offset + ((row + 1)*(mh2O.Header.Width + 1) + col));
                    indices.Add(offset + (row*(mh2O.Header.Width + 1) + col + 1));
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
        public override int GenerateHeightVertices(int indexY, int indexX, ICollection<VertexPositionNormalColored> vertices, Color terrainColor)
        {
            var mcnk = MapChunks[indexY, indexX];
            var lowResMap = mcnk.Heights.GetLowResMapMatrix();
            var lowResNormal = mcnk.Normals.GetLowResNormalMatrix();

            var counter = 0;
            for (var xStep = 0; xStep < 9; xStep++)
            {
                for (var yStep = 0; yStep < 9; yStep++)
                {
                    var xPos = TerrainConstants.CenterPoint - (_tileX*TerrainConstants.TileSize) - (indexX*TerrainConstants.ChunkSize) -
                               (xStep*TerrainConstants.UnitSize);
                    var yPos = TerrainConstants.CenterPoint - (_tileY*TerrainConstants.TileSize) - (indexY*TerrainConstants.ChunkSize) -
                               (yStep*TerrainConstants.UnitSize);
                    var zPos = lowResMap[yStep, xStep] + mcnk.Header.Z;

                    var theNormal = lowResNormal[yStep, xStep];

                    //var cosAngle = Vector3.Dot(Vector3.Up, theNormal);
                    //var angle = MathHelper.ToDegrees((float)Math.Acos(cosAngle));

                    //if (angle > 50.0)
                    //{
                    //    color = Color.Brown;
                    //}

                    var position = new Vector3(xPos, yPos, zPos);
                    vertices.Add(new VertexPositionNormalColored(position, terrainColor, theNormal));
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

            var holesMap = new bool[4,4];
            if (mcnk.Header.Holes > 0)
            {
                holesMap = mcnk.Header.GetHolesMap();
            }

            for (var row = 0; row < 8; row++)
            {
                for (var col = 0; col < 8; col++)
                {
                    if (holesMap[row/2, col/2]) continue;
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

        //public List<int> ReducePolygonCount(ADTChunk mcnk, List<int> tempIndices, int offset)
        //{
        //    // No polygons to cull
        //    if (tempIndices.Count == 0) return tempIndices;

        //    const float mapRes = MAX_FLAT_LAND_DELTA;
        //    var lowResHeightMap = mcnk.Heights.GetLowResMapMatrix();

        //    // thin the rows
        //    for (var row = 0; row < 9; row++)
        //    {
        //       for (var col = 1; col < 8; col++)
        //       {
        //           // check the heights of the three adjacent columns.
        //           // if they are within a tolerance, then the middle value can be culled.
        //           var leftHeight = lowResHeightMap[row, (col - 1)];
        //           var centerHeight = lowResHeightMap[row, col];
        //           var rightHeight = lowResHeightMap[row, (col + 1)];


        //           var leftDiff = Math.Abs(leftHeight - centerHeight);
        //           if (leftDiff >= mapRes) continue;

        //           var rightDiff = Math.Abs(rightHeight - centerHeight);
        //           if (rightDiff >= mapRes) continue;

        //           var oldIndex = (row*9) + col + offset;
        //           var newIndex = oldIndex + 1;

        //           // Here's the magic. We take the index of the point we're culling 
        //           // and increase its value by one. Now it points to the point on the right.
        //           tempIndices = AdjustTempIndices(tempIndices, oldIndex, newIndex);
        //       }
        //    }

        //    // Thin the columns
        //    for (var col = 0; col < 9; col++)
        //    {
        //        for (var row = 1; row < 8; row++)
        //        {
        //            var upperHeight = lowResHeightMap[(row - 1), col];
        //            var height = lowResHeightMap[row, col];
        //            var lowerHeight = lowResHeightMap[(row + 1), col];

        //            var upperDiff = Math.Abs(upperHeight - height);
        //            if (upperDiff >= mapRes) continue;
        //            var lowerDiff = Math.Abs(lowerHeight - height);
        //            if (lowerDiff >= mapRes) continue;

        //            var oldIndex = (row*9) + col + offset;
        //            var newIndex = ((row + 1)*9) + col + offset;

        //            tempIndices = AdjustTempIndices(tempIndices, oldIndex, newIndex);
        //        }
        //    }
        //    return CullTempIndices(tempIndices);
        //}

        //public List<int> ReducePolygonCount(MH2O mh2o, List<int> tempIndices, int offset)
        //{
        //    // No polygons to cull
        //    if (tempIndices.Count == 0) return tempIndices;

        //    const float mapRes = MAX_FLAT_WATER_DELTA;
        //    var lowResHeightMap = mh2o.GetMapHeightsMatrix();

        //    // thin the rows
        //    for (var row = 0; row < 8; row++)
        //    {
        //        for (var col = 1; col < 8; col++)
        //        {
        //            var adjRow = row - mh2o.Header.YOffset;
        //            var adjCol = col - mh2o.Header.XOffset;

        //            if (((row < mh2o.Header.YOffset) || (adjRow > mh2o.Header.Height)) ||
        //                ((col < mh2o.Header.XOffset) || (adjCol > mh2o.Header.Width)) ||
        //                (((col - 1) < mh2o.Header.XOffset) || ((adjCol - 1) > mh2o.Header.Width)) ||
        //                (((col + 1) < mh2o.Header.XOffset) || ((adjCol + 1) > mh2o.Header.Width)))
        //            {
        //                continue;
        //            }

        //            var leftHeight = lowResHeightMap[adjRow, (adjCol - 1)];
        //            var centerHeight = lowResHeightMap[adjRow, adjCol];
        //            //var rightHeight = lowResHeightMap[adjRow, (adjCol + 1)];

        //            var leftDiff = Math.Abs(leftHeight - centerHeight);
        //            if (leftDiff >= mapRes) continue;

        //            //var rightDiff = Math.Abs(rightHeight - centerHeight);
        //            //if (rightDiff >= mapRes) continue;

        //            var oldIndex = (adjRow * (mh2o.Header.Width + 1) + adjCol) + offset;
        //            var newIndex = oldIndex + 1;

        //            tempIndices = AdjustTempIndices(tempIndices, oldIndex, newIndex);
        //        }
        //    }
            
        //    // Thin the columns
        //    for (var col = 0; col < 8; col++)
        //    {
        //        for (var row = 1; row < 8; row++)
        //        {
        //            var adjRow = row - mh2o.Header.YOffset;
        //            var adjCol = col - mh2o.Header.XOffset;

        //            if ((((row - 1) < mh2o.Header.YOffset) || ((adjRow - 1) > mh2o.Header.Height)) ||
        //                ((row < mh2o.Header.YOffset) || (adjRow > mh2o.Header.Height)) ||
        //                (((row + 1) < mh2o.Header.YOffset) || ((adjRow + 1) > mh2o.Header.Height)) ||
        //                ((col < mh2o.Header.XOffset) || (adjCol > mh2o.Header.Width)))
        //            {
        //                continue;
        //            }

        //            var upperHeight = lowResHeightMap[(adjRow - 1), adjCol];
        //            var height = lowResHeightMap[adjRow, adjCol];
        //            //var lowerHeight = lowResHeightMap[(adjRow + 1), adjCol];

        //            var upperDiff = Math.Abs(upperHeight - height);
        //            if (upperDiff >= mapRes) continue;

        //            //var lowerDiff = Math.Abs(lowerHeight - height);
        //            //if (lowerDiff >= mapRes) continue;

        //            var oldIndex = (adjRow * (mh2o.Header.Width + 1) + adjCol) + offset;
        //            var newIndex = ((adjRow + 1) * (mh2o.Header.Width + 1) + adjCol) + offset;

        //            tempIndices = AdjustTempIndices(tempIndices, oldIndex, newIndex);
        //        }
        //    }
        //    return CullTempIndices(tempIndices);
        //}

        //private static List<int> CullTempIndices(IList<int> tempIndices)
        //{
        //    var newList = new List<int>();
        //    for (var i = 0; i < tempIndices.Count; i++)
        //    {
        //        var first = tempIndices[i++];
        //        var second = tempIndices[i++];
        //        var third = tempIndices[i];

        //        if (first == second || second == third || first == third) continue;
        //        newList.Add(first);
        //        newList.Add(second);
        //        newList.Add(third);
        //    }
        //    return newList;
        //}

        //private static List<int> AdjustTempIndices(IList<int> tempIndices, int oldIndex, int newIndex)
        //{
        //    var newList = new List<int>(tempIndices.Count);

        //    for (var i = 0; i < tempIndices.Count; i++)
        //    {
        //        if (tempIndices[i] != oldIndex)
        //        {
        //            newList.Add(tempIndices[i]);
        //        }
        //        else
        //        {
        //            newList.Add(newIndex);
        //        }
        //    }
        //    return newList;
        //}
    }
}