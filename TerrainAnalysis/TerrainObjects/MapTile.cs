using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Collections;
using WCell.Collision;
using TerrainAnalysis.TerrainObjects;

namespace TerrainAnalysis
{
    public class MapTile
    {        
        MapTileChunk[][] tileChunks = null;        
        private Microsoft.DirectX.Vector3 topLeft;
        private string mapFileName = "New Tile";        

        public MapTile(ADT m_adt, int tileRow, int tileColumn)
        {
            this.topLeft = new Microsoft.DirectX.Vector3(tileColumn * TerrainConstants.Tile.TileSize, 0.0f, tileRow * TerrainConstants.Tile.TileSize);
            this.tileChunks = new MapTileChunk[16][];
            int i = 0;

            for (i = 0; i < 16; i++)
            {
                this.tileChunks[i] = new MapTileChunk[16];
            }

            for (i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    this.tileChunks[i][j] = new MapTileChunk(m_adt.MapChunks[i, j], m_adt.HighRes, new Microsoft.DirectX.Vector3(j * TerrainConstants.HighResChunk.ChunkSize, 0.0f, i * TerrainConstants.HighResChunk.ChunkSize) + this.topLeft);

                    if (j > 0)
                    {
                        this.tileChunks[i][j].treePointers[(int)ChunkTree.Left] = this.tileChunks[i][j - 1];
                        this.tileChunks[i][j - 1].treePointers[(int)ChunkTree.Right] = this.tileChunks[i][j];
                        if (i > 0)
                        {
                            this.tileChunks[i][j].treePointers[(int)ChunkTree.TopLeft] = this.tileChunks[i - 1][j - 1];
                            this.tileChunks[i - 1][j - 1].treePointers[(int)ChunkTree.BottomRight] = this.tileChunks[i][j];
                        }
                    }

                    if (i > 0)
                    {
                        this.tileChunks[i][j].treePointers[(int)ChunkTree.Top] = this.tileChunks[i - 1][j];
                        this.tileChunks[i - 1][j].treePointers[(int)ChunkTree.Bottom] = this.tileChunks[i][j];
                        if (j < 15)
                        {
                            this.tileChunks[i][j].treePointers[(int)ChunkTree.TopRight] = this.tileChunks[i - 1][j + 1];
                            this.tileChunks[i - 1][j + 1].treePointers[(int)ChunkTree.BottomLeft] = this.tileChunks[i][j];
                        }
                    }
                }
            }
        }


        public string MapFileName
        {
            get
            {
                return this.mapFileName;
            }
            set
            {
                this.mapFileName = value;
            }
        }

        public override string ToString()
        {
            return this.mapFileName;
        }

        public Microsoft.DirectX.Vector3 TilePosition
        {
            get
            {
                return this.topLeft;
            }
            set
            {
                this.topLeft = value;
            }
        }

        public bool OverThisTile(Microsoft.DirectX.Vector2 dataPoint)
        {
            if (dataPoint.X >= topLeft.X && dataPoint.X < topLeft.X + TerrainConstants.Tile.TileSize)
            {
                if (dataPoint.Y >= topLeft.Y && dataPoint.Y < topLeft.Y + TerrainConstants.Tile.TileSize)
                    return true;
            }

            return false;
        }

        public MapTileChunk ChunkBelow(Microsoft.DirectX.Vector2 dataPoint)
        {
            dataPoint.X -= topLeft.X;
            dataPoint.Y -= topLeft.Y;

            int columnIndex = (int)(dataPoint.X / TerrainConstants.HighResChunk.ChunkSize);
            int rowIndex = (int)(dataPoint.Y / TerrainConstants.HighResChunk.ChunkSize);

            return this.tileChunks[rowIndex][columnIndex];
        }

        /*
        public void GrabChunksInRange(ArrayList chunkList, Microsoft.DirectX.Vector2 center, float radius)
        {
            for (int i = 0; i < chunkRows; i++)
            {
                for (int j = 0; j < chunkColumns; j++)
                {
                    Microsoft.DirectX.Vector2 tempVector = new Microsoft.DirectX.Vector2(tileChunks[i][j].ChunkOffset.X, tileChunks[i][j].ChunkOffset.Z);
                    tempVector.X += TerrainConstants.HighResChunk.ChunkSize * 0.5f;
                    tempVector.Y += TerrainConstants.HighResChunk.ChunkSize * 0.5f;
                    Microsoft.DirectX.Vector2 distanceVector = center - tempVector;
                    if (distanceVector.Length() < radius)
                        chunkList.Add(tileChunks[i][j]);
                }
            }
        }*/

        public MapTileChunk[] TileEdge(TileEdgeComponent edgeComponent)
        {
            if (edgeComponent == TileEdgeComponent.Top)
                return tileChunks[0];
            else if (edgeComponent == TileEdgeComponent.Bottom)
                return tileChunks[15];
            else if (edgeComponent == TileEdgeComponent.TopLeft)
                return new MapTileChunk[] { this.tileChunks[0][0] };
            else if (edgeComponent == TileEdgeComponent.TopRight)
                return new MapTileChunk[] { this.tileChunks[0][15] };
            else if (edgeComponent == TileEdgeComponent.BottomLeft)
                return new MapTileChunk[] { this.tileChunks[15][0] };
            else if (edgeComponent == TileEdgeComponent.BottomRight)
                return new MapTileChunk[] { this.tileChunks[15][15] };
            else
            {
                int columnIndex = edgeComponent == TileEdgeComponent.Left ? 0 : 15;
                int count = 0;
                MapTileChunk[] tempArray = new MapTileChunk[16];
                foreach (MapTileChunk[] sideArray in this.tileChunks)
                {
                    tempArray[count++] = sideArray[columnIndex];
                }
                return tempArray;
            }
        }

        public MapTileChunk GetChunk(int row, int column)
        {
            try
            {
                return this.tileChunks[row][column];
            }
            catch
            {
                return null;
            }
        }

    }

    public enum TileEdgeComponent
    {
        Top,
        Bottom,
        Left,
        Right,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    };
}
