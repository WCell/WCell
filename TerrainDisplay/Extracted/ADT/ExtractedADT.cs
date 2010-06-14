using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrainDisplay;
using TerrainDisplay.MPQ;
using TerrainDisplay.MPQ.ADT;
using TerrainDisplay.MPQ.ADT.Components;
using TerrainDisplay.MPQ.WMO.Components;
using TerrainDisplay.Extracted.ADT;
using TerrainDisplay.Extracted.M2;
using TerrainDisplay.Extracted.WMO;

namespace TerrainDisplay.Extracted
{
    public class ExtractedADT : ADTBase
    {
        public bool IsWMOOnly;
        public List<ExtractedWMODefinition> WMODefs;
        public List<ExtractedMapM2Definition> M2Defs;
        public ExtractedADTChunk[,] Chunks;

        private int _mapId;
        private int _tileX;
        private int _tileY;

        public ExtractedADT(int mapId, int tileX, int tileY)
        {
            _mapId = mapId;
            _tileX = tileX;
            _tileY = tileY;
        }

        public override void GenerateLiquidVertexAndIndices()
        {
            LiquidVertices = new List<VertexPositionNormalColored>();
            LiquidIndices = new List<int>();

            var vertexCounter = 0;
            for (var indexX = 0; indexX < TerrainConstants.ChunksPerTileSide; indexX++)
            {
                for (var indexY = 0; indexY < TerrainConstants.ChunksPerTileSide; indexY++)
                {
                    var tempVertexCounter = GenerateLiquidVertices(indexY, indexX, LiquidVertices);
                    GenerateLiquidIndices(indexY, indexX, vertexCounter, LiquidIndices);
                    vertexCounter += tempVertexCounter;
                }
            }
        }

        public override void GenerateHeightVertexAndIndices()
        {
            Vertices = new List<VertexPositionNormalColored>();
            Indices = new List<int>();

            for (var indexX = 0; indexX < TerrainConstants.ChunksPerTileSide; indexX++)
            {
                for (var indexY = 0; indexY < TerrainConstants.ChunksPerTileSide; indexY++)
                {
                    GenerateHeightIndices(indexY, indexX, Vertices.Count, Indices);
                    GenerateHeightVertices(indexY, indexX, Vertices);
                }
            }
        }

        public override int GenerateLiquidVertices(int indexY, int indexX, ICollection<VertexPositionNormalColored> vertices)
        {
            var tempVertexCounter = 0;
            var mapChunk = Chunks[indexY, indexX];

            if (!mapChunk.HasLiquid) return tempVertexCounter;

            var clr = Color.Green;

            switch (mapChunk.LiquidType)
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

            var medianLiqHeight = mapChunk.MedianLiquidHeight1;

            for (var xStep = 0; xStep < 9; xStep++)
            {
                for (var yStep = 0; yStep < 9; yStep++)
                {
                    var xPos = TerrainConstants.CenterPoint - (_tileX * TerrainConstants.TileSize) -
                               (indexX * TerrainConstants.ChunkSize) - (xStep * TerrainConstants.UnitSize);
                    var yPos = TerrainConstants.CenterPoint - (_tileY * TerrainConstants.TileSize) -
                               (indexY * TerrainConstants.ChunkSize) - (yStep * TerrainConstants.UnitSize);

                    float zPos;
                    if (mapChunk.IsLiquidFlat)
                    {
                        zPos = medianLiqHeight;
                    }
                    else
                    {
                        zPos = medianLiqHeight + mapChunk.LiquidHeights[yStep, xStep];
                    }

                    var position = new Vector3(xPos, yPos, zPos);

                    vertices.Add(new VertexPositionNormalColored(position, clr, Vector3.Up));
                    tempVertexCounter++;
                }
            }
            return tempVertexCounter;
        }

        public override void GenerateLiquidIndices(int indexY, int indexX, int offset, List<int> indices)
        {
            var chunk = Chunks[indexY, indexX];
            if (!chunk.HasLiquid) return;

            for (var row = 0; row < 8; row++)
            {
                for (var col = 0; col < 8; col++)
                {
                    if (!chunk.LiquidMap[col, row]) continue;

                    indices.Add(offset + ((row + 1) * (8 + 1) + col)); //9 ... 10
                    indices.Add(offset + (row * (8 + 1) + col)); //0 ... 1
                    indices.Add(offset + (row * (8 + 1) + col + 1)); //1 ... 2

                    indices.Add(offset + ((row + 1) * (8 + 1) + col + 1));
                    indices.Add(offset + ((row + 1) * (8 + 1) + col));
                    indices.Add(offset + (row * (8 + 1) + col + 1));
                }
            }
        }

        public override int GenerateHeightVertices(int indexY, int indexX, ICollection<VertexPositionNormalColored> vertices)
        {
            var mcnk = Chunks[indexY, indexX];
            var lowResMap = mcnk.HeightMap;
            var medianHeight = mcnk.MedianHeight;

            var counter = 0;
            for (var xStep = 0; xStep < TerrainConstants.UnitsPerChunkSide + 1; xStep++)
            {
                for (var yStep = 0; yStep < TerrainConstants.UnitsPerChunkSide + 1; yStep++)
                {
                    var xPos = TerrainConstants.CenterPoint - (_tileX * TerrainConstants.TileSize) - (indexX * TerrainConstants.ChunkSize) -
                               (xStep * TerrainConstants.UnitSize);
                    var yPos = TerrainConstants.CenterPoint - (_tileY * TerrainConstants.TileSize) - (indexY * TerrainConstants.ChunkSize) -
                               (yStep * TerrainConstants.UnitSize);

                    float zPos;
                    if (mcnk.IsFlat)
                    {
                        zPos = medianHeight;
                    }
                    else
                    {
                        zPos = lowResMap[yStep, xStep] + medianHeight;
                    }

                    var position = new Vector3(xPos, yPos, zPos);
                    vertices.Add(new VertexPositionNormalColored(position, Color.Green, Vector3.Up));
                    counter++;
                }
            }
            return counter;
        }

        public override void GenerateHeightIndices(int indexY, int indexX, int offset, List<int> indices)
        {
            var mcnk = Chunks[indexY, indexX];

            var holesMap = new bool[4, 4];
            if (mcnk.HasHoles)
            {
                holesMap = mcnk.HolesMap;
            }

            for (var row = 0; row < 8; row++)
            {
                for (var col = 0; col < 8; col++)
                {
                    if (holesMap[col / 2, row / 2]) continue;
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
    }
}
