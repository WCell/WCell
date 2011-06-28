using System;
using System.Collections.Generic;
using TerrainDisplay.Collision;
using TerrainDisplay.MPQ.WMO;
using WCell.Constants.World;
using WCell.Util.Graphics;
using TerrainDisplay.MPQ.ADT;
using TerrainDisplay.MPQ.ADT.Components;
using TerrainDisplay.Extracted.ADT;
using TerrainDisplay.Extracted.M2;
using TerrainDisplay.Extracted.WMO;

namespace TerrainDisplay.Extracted
{
    public class ExtractedADT : ADTBase
    {
        private static Color TerrainColor = Color.Green;
        private static Color Watercolor = Color.Blue;

        public bool IsWMOOnly;
        public List<ExtractedWMODefinition> WMODefs;
        public List<ExtractedMapM2Definition> M2Defs;
        public QuadTree<ExtractedADTChunk> QuadTree;
        public ExtractedADTChunk[,] Chunks;

        private MapId _mapId;
        private int _tileX;
        private int _tileY;

        public ExtractedADT(MapId mapId, int tileX, int tileY)
        {
            _mapId = mapId;
            _tileX = tileX;
            _tileY = tileY;
        }

        public override void GenerateLiquidVertexAndIndices()
        {
            LiquidVertices = new List<Vector3>();
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
            //TerrainVertices = new List<Vector3>();
            Indices = new List<int>();

            for (var indexX = 0; indexX < TerrainConstants.ChunksPerTileSide; indexX++)
            {
                for (var indexY = 0; indexY < TerrainConstants.ChunksPerTileSide; indexY++)
                {
                    GenerateHeightIndices(indexY, indexX, TerrainVertices.Count, Indices);
                    //GenerateHeightVertices(indexY, indexX, TerrainVertices);
                }
            }
        }

        public override int GenerateLiquidVertices(int indexY, int indexX, ICollection<Vector3> vertices)
        {
            var tempVertexCounter = 0;
            var mapChunk = Chunks[indexY, indexX];

            if (!mapChunk.HasLiquid) return tempVertexCounter;

            //var clr = Color.Green;

            //switch (mapChunk.LiquidType)
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

                    vertices.Add(position);
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

        public override int GenerateHeightVertices(int indexY, int indexX, ICollection<Vector3> vertices)
        {
            return 0;
            //var mcnk = Chunks[indexY, indexX];
            //var lowResMap = mcnk.HeightMap;
            //var medianHeight = mcnk.MedianHeight;

            //var counter = 0;
            //for (var xStep = 0; xStep < TerrainConstants.UnitsPerChunkSide + 1; xStep++)
            //{
            //    for (var yStep = 0; yStep < TerrainConstants.UnitsPerChunkSide + 1; yStep++)
            //    {
            //        var xPos = TerrainConstants.CenterPoint - (_tileX * TerrainConstants.TileSize) - (indexX * TerrainConstants.ChunkSize) -
            //                   (xStep * TerrainConstants.UnitSize);
            //        var yPos = TerrainConstants.CenterPoint - (_tileY * TerrainConstants.TileSize) - (indexY * TerrainConstants.ChunkSize) -
            //                   (yStep * TerrainConstants.UnitSize);

            //        float zPos;
            //        if (mcnk.IsFlat)
            //        {
            //            zPos = medianHeight;
            //        }
            //        else
            //        {
            //            zPos = lowResMap[yStep, xStep] + medianHeight;
            //        }

            //        var position = new Vector3(xPos, yPos, zPos);
            //        vertices.Add(position);
            //        counter++;
            //    }
            //}
            //return counter;
        }

        public override void GenerateHeightIndices(int indexY, int indexX, int offset, List<int> indices)
        {
            var chunk = Chunks[indexY, indexX];
            foreach (var index3 in chunk.TerrainTris)
            {
                indices.Add(index3.Index0);
                indices.Add(index3.Index1);
                indices.Add(index3.Index2);
            }

            //var holesMap = new bool[4, 4];
            //if (mcnk.HasHoles)
            //{
            //    holesMap = mcnk.HolesMap;
            //}

            //for (var row = 0; row < 8; row++)
            //{
            //    for (var col = 0; col < 8; col++)
            //    {
            //        if (holesMap[col / 2, row / 2]) continue;
            //        //{
            //        //    indices.Add(offset + ((row + 1) * (8 + 1) + col));
            //        //    indices.Add(offset + (row * (8 + 1) + col));
            //        //    indices.Add(offset + (row * (8 + 1) + col + 1));

            //        //    indices.Add(offset + ((row + 1) * (8 + 1) + col + 1));
            //        //    indices.Add(offset + ((row + 1) * (8 + 1) + col));
            //        //    indices.Add(offset + (row * (8 + 1) + col + 1));
            //        //    continue;
            //        //}
            //        //* The order metter*/
            //        /*This 3 index add the up triangle
            //                    *
            //                    *0--1--2
            //                    *| /| /
            //                    *|/ |/ 
            //                    *9  10 11
            //                    */

            //        indices.Add(offset + ((row + 1) * (8 + 1) + col)); //9 ... 10
            //        indices.Add(offset + (row * (8 + 1) + col)); //0 ... 1
            //        indices.Add(offset + (row * (8 + 1) + col + 1)); //1 ... 2

            //        /*This 3 index add the low triangle
            //                     *
            //                     *0  1   2
            //                     *  /|  /|
            //                     * / | / |
            //                     *9--10--11
            //                     */

            //        indices.Add(offset + ((row + 1) * (8 + 1) + col + 1));
            //        indices.Add(offset + ((row + 1) * (8 + 1) + col));
            //        indices.Add(offset + (row * (8 + 1) + col + 1));
            //    }
            //}
        }

        public override bool GetPotentialColliders(Ray2D ray2D, List<Index3> results)
        {
            if (results == null)
            {
                results = new List<Index3>();
            }

            var chunks = QuadTree.Query(ray2D);
            foreach (var chunk in chunks)
            {
                results.AddRange(chunk.TerrainTris);
            }

            return true;
        }
    }
}
