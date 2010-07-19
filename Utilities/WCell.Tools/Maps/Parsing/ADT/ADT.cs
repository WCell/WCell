using System;
using System.Collections.Generic;
using System.IO;
using WCell.Tools.Maps.Constants;
using WCell.Tools.Maps.Parsing.ADT.Components;
using WCell.Util.Graphics;

namespace WCell.Tools.Maps.Parsing.ADT
{
    public class ADT
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
        /// Filename of the ADT
        /// </summary>
        /// <example>Azeroth_32_32.adt</example>
        public string FileName;

        /// <summary>
        /// The X offset of the map in the 64 x 64 grid
        /// </summary>
        private int _tileX;

        /// <summary>
        /// The Y offset of the map in the 64 x 64 grid
        /// </summary>
        private int _tileY;

        public bool IsWMOOnly;
        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the ADT class
        /// </summary>
        /// <param name="filePath">Filepath of the ADT</param>
        /// <example>ADT myADT = new ADT("Azeroth_32_32.adt");</example>
        public ADT(string fileName)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);
            FileName = fileName;

            string[] fileNameParts = fileName.Split('_');
            _tileX = Int32.Parse(fileNameParts[2]);
            _tileY = Int32.Parse(fileNameParts[1]);
        }

        #endregion

        //public void GenerateLiquidVertexAndIndices()
        //{
        //    LiquidVertices = new List<VertexPositionNormalColored>();
        //    LiquidIndices = new List<int>();
           
        //    var vertexCounter = 0;
        //    for (var indexX = 0; indexX < 16; indexX++)
        //    {
        //        for (var indexY = 0; indexY < 16; indexY++)
        //        {
        //            var tempVertexCounter = GenerateLiquidVertices(indexY, indexX, LiquidVertices);
        //            GenerateLiquidIndices(indexY, indexX, vertexCounter, LiquidIndices);
        //            vertexCounter += tempVertexCounter;
        //        }
        //    }
        //}
        
        //public void GenerateHeightVertexAndIndices()
        //{
        //    Vertices = new List<VertexPositionNormalColored>();
        //    Indices = new List<int>();

        //    for (var indexX = 0; indexX < 16; indexX++)
        //    {
        //        for (var indexY = 0; indexY < 16; indexY++)
        //        {
        //            GenerateHeightIndices(indexY, indexX, Vertices.Count, Indices);
        //            GenerateHeightVertices(indexY, indexX, Vertices);
        //        }
        //    }
        //}

        ///// <summary>
        ///// Adds the rendering liquid vertices to the provided list for the MapChunk given by:
        ///// </summary>
        ///// <param name="indexY">The y index of the map chunk.</param>
        ///// <param name="indexX">The x index of the map chunk</param>
        ///// <param name="vertices">The Collection to add the vertices to.</param>
        ///// <returns>The number of vertices added.</returns>
        //public int GenerateLiquidVertices(int indexY, int indexX, ICollection<VertexPositionNormalColored> vertices)
        //{
        //    var tempVertexCounter = 0;
        //    var mapChunk = MapChunks[indexY, indexX];
            
        //    var mh2O = LiquidInfo[indexY, indexX];

        //    if (mh2O == null) return tempVertexCounter;
        //    if (!mh2O.Header.Used) return tempVertexCounter;

        //    var clr = Color.Green;

        //    switch (mh2O.Header.Type)
        //    {
        //        case FluidType.Water:
        //            clr = Color.Blue;
        //            break;
        //        case FluidType.Lava:
        //            clr = Color.Red;
        //            break;
        //        case FluidType.OceanWater:
        //            clr = Color.Coral;
        //            break;
        //    }

        //    var mh2OHeightMap = mh2O.GetMapHeightsMatrix();
        //    for (var xStep = 0; xStep < 9; xStep++)
        //    {
        //        for (var yStep = 0; yStep < 9; yStep++)
        //        {
        //            var xPos = TerrainConstants.CenterPoint - (_tileX*TerrainConstants.TileSize) -
        //                       (indexX*TerrainConstants.ChunkSize) - (xStep*TerrainConstants.UnitSize);
        //            var yPos = TerrainConstants.CenterPoint - (_tileY*TerrainConstants.TileSize) -
        //                       (indexY*TerrainConstants.ChunkSize) - (yStep*TerrainConstants.UnitSize);

        //            if (((xStep < mh2O.Header.XOffset) || ((xStep - mh2O.Header.XOffset) > mh2O.Header.Height)) ||
        //                ((yStep < mh2O.Header.YOffset) || ((yStep - mh2O.Header.YOffset) > mh2O.Header.Width)))
        //            {
        //                continue;
        //            }

        //            var zPos = mh2OHeightMap[yStep - mh2O.Header.YOffset, xStep - mh2O.Header.XOffset];

        //            var position = new Vector3(xPos, yPos, zPos);

        //            vertices.Add(new VertexPositionNormalColored(position, clr, Vector3.Up));
        //            tempVertexCounter++;
        //        }
        //    }
        //    return tempVertexCounter;
        //}

        ///// <summary>
        ///// Adds the rendering liquid indices to the provided list for the MapChunk given by:
        ///// </summary>
        ///// <param name="indexY">The y index of the map chunk.</param>
        ///// <param name="indexX">The x index of the map chunk</param>
        ///// <param name="offset">The number to add to the indices so as to match the end of the Vertices list.</param>
        ///// <param name="indices">The Collection to add the indices to.</param>
        //public void GenerateLiquidIndices(int indexY, int indexX, int offset, List<int> indices)
        //{
        //    var mh2O = LiquidInfo[indexY, indexX];
        //    if (mh2O == null) return;
        //    if (!mh2O.Header.Used) return;

        //    var renderMap = mh2O.GetRenderBitMapMatrix();
        //    for (int r = mh2O.Header.XOffset; r < mh2O.Header.XOffset + mh2O.Header.Height; r++)
        //    {
        //        for (int c = mh2O.Header.YOffset; c < mh2O.Header.YOffset + mh2O.Header.Width; c++)
        //        {
        //            var row = r - mh2O.Header.XOffset;
        //            var col = c - mh2O.Header.YOffset;

        //            if (!renderMap[col, row] && ((mh2O.Header.Height != 8) || (mh2O.Header.Width != 8))) continue;
        //            indices.Add(offset + ((row + 1) * (mh2O.Header.Width + 1) + col));
        //            indices.Add(offset + (row * (mh2O.Header.Width + 1) + col));
        //            indices.Add(offset + (row * (mh2O.Header.Width + 1) + col + 1));
        //            indices.Add(offset + ((row + 1) * (mh2O.Header.Width + 1) + col + 1));
        //            indices.Add(offset + ((row + 1) * (mh2O.Header.Width + 1) + col));
        //            indices.Add(offset + (row * (mh2O.Header.Width + 1) + col + 1));
        //        }
        //    }
        //}

        ///// <summary>
        ///// Adds the rendering liquid vertices to the provided list for the MapChunk given by:
        ///// </summary>
        ///// <param name="indexY">The y index of the map chunk.</param>
        ///// <param name="indexX">The x index of the map chunk</param>
        ///// <param name="vertices">The Collection to add the vertices to.</param>
        ///// <returns>The number of vertices added.</returns>
        //public int GenerateHeightVertices(int indexY, int indexX, ICollection<VertexPositionNormalColored> vertices)
        //{
        //    var mcnk = MapChunks[indexY, indexX];
        //    var lowResMap = mcnk.Heights.GetLowResMapMatrix();
        //    //var lowResNormal = mcnk.Normals.GetLowResNormalMatrix();

        //    var counter = 0;
        //    for (var xStep = 0; xStep < 9; xStep++)
        //    {
        //        for (var yStep = 0; yStep < 9; yStep++)
        //        {
        //            var xPos = TerrainConstants.CenterPoint - (_tileX*TerrainConstants.TileSize) - (indexX*TerrainConstants.ChunkSize) -
        //                       (xStep*TerrainConstants.UnitSize);
        //            var yPos = TerrainConstants.CenterPoint - (_tileY*TerrainConstants.TileSize) - (indexY*TerrainConstants.ChunkSize) -
        //                       (yStep*TerrainConstants.UnitSize);
        //            var zPos = lowResMap[yStep, xStep] + mcnk.Header.Z;

        //            var color = Color.Green;

        //            //var theNormal = lowResNormal[r, c];

        //            //var cosAngle = Vector3.Dot(Vector3.Up, theNormal);
        //            //var angle = MathHelper.ToDegrees((float)Math.Acos(cosAngle));

        //            //if (angle > 50.0)
        //            //{
        //            //    color = Color.Brown;
        //            //}

        //            var position = new Vector3(xPos, yPos, zPos);
        //            //vertices.Add(new VertexPositionNormalColored(position, color, theNormal));
        //            vertices.Add(new VertexPositionNormalColored(position, color, Vector3.Up));
        //            counter++;
        //        }
        //    }
        //    return counter;
        //}

        ///// <summary>
        ///// Adds the rendering indices to the provided list for the MapChunk given by:
        ///// </summary>
        ///// <param name="indexY">The y index of the map chunk.</param>
        ///// <param name="indexX">The x index of the map chunk</param>
        ///// <param name="offset">The number to add to the indices so as to match the end of the Vertices list.</param>
        ///// <param name="indices">The Collection to add the indices to.</param>
        //public void GenerateHeightIndices(int indexY, int indexX, int offset, List<int> indices)
        //{
        //    var mcnk = MapChunks[indexY, indexX];

        //    var holesMap = new bool[4,4];
        //    if (mcnk.Header.Holes > 0)
        //    {
        //        holesMap = mcnk.Header.GetHolesMap();
        //    }

        //    for (var row = 0; row < 8; row++)
        //    {
        //        for (var col = 0; col < 8; col++)
        //        {
        //            if (holesMap[row/2, col/2]) continue;
        //            //{
        //            //    indices.Add(offset + ((row + 1) * (8 + 1) + col));
        //            //    indices.Add(offset + (row * (8 + 1) + col));
        //            //    indices.Add(offset + (row * (8 + 1) + col + 1));

        //            //    indices.Add(offset + ((row + 1) * (8 + 1) + col + 1));
        //            //    indices.Add(offset + ((row + 1) * (8 + 1) + col));
        //            //    indices.Add(offset + (row * (8 + 1) + col + 1));
        //            //    continue;
        //            //}
        //            //* The order metter*/
        //            /*This 3 index add the up triangle
        //                        *
        //                        *0--1--2
        //                        *| /| /
        //                        *|/ |/ 
        //                        *9  10 11
        //                        */

        //            indices.Add(offset + ((row + 1) * (8 + 1) + col)); //9 ... 10
        //            indices.Add(offset + (row * (8 + 1) + col)); //0 ... 1
        //            indices.Add(offset + (row * (8 + 1) + col + 1)); //1 ... 2

        //            /*This 3 index add the low triangle
        //                         *
        //                         *0  1   2
        //                         *  /|  /|
        //                         * / | / |
        //                         *9--10--11
        //                         */

        //            indices.Add(offset + ((row + 1) * (8 + 1) + col + 1));
        //            indices.Add(offset + ((row + 1) * (8 + 1) + col));
        //            indices.Add(offset + (row * (8 + 1) + col + 1));
        //        }
        //    }
        //}
    }
}