using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Collections;
using Microsoft.Xna.Framework;
using WCell.Collision;

namespace TerrainAnalysis
{
    public class MapTile
    {
        private readonly int chunkRows, chunkColumns;
        MapTileChunk[][] tileChunks = null;
        private readonly float scaleX, scaleZ;
        private Microsoft.Xna.Framework.Vector3 topLeft;
        private string mapFileName = "New Tile";        

        public MapTile(ADT m_adt, int tileRow, int tileColumn)
        {
            this.topLeft = new Microsoft.Xna.Framework.Vector3(tileColumn * TILELENGTH, 0.0f, tileRow * TILELENGTH);
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
                    this.tileChunks[i][j] = new MapTileChunk(m_adt.MapChunks[i, j].HeightMap, m_adt.MapChunks[i, j].Normals, m_adt.HighRes, new Microsoft.Xna.Framework.Vector3(j * CHUNKLENGTH, 0.0f, i * CHUNKLENGTH) + this.topLeft);

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

        /// <summary>
        /// Inserts a new tile chunk at the specified coordinates with relation to the rest of the tile. 0,0 is the top left
        /// </summary>
        /// <param name="mapTileChunk">
        /// Map chunk to insert
        /// </param>
        /// <param name="row">
        /// Chunk row index
        /// </param>
        /// <param name="column">
        /// Chunk column index
        /// </param>
        public void Insert(MapTileChunk mapTileChunk, int row, int column)
        {
            /*
            this.tileChunks[row][column] = mapTileChunk;

            System.Drawing.PointF chunkTopLeft = new System.Drawing.PointF();
            chunkTopLeft.X = this.topLeft.X;
            chunkTopLeft.Y = this.topLeft.Y;
            chunkTopLeft.X += (float)column * this.scaleX;
            chunkTopLeft.Y += -((float)row * this.scaleZ);

            mapTileChunk.Shift_Scale(chunkTopLeft, this.scaleX, this.scaleZ);*/
        }

 

        /// <summary>
        /// Assigns an ADT height-map to the designated tile chunk.
        /// </summary>
        /// <param name="heightMap">
        /// 2D non-jagged array containing the Y values
        /// </param>
        /// <param name="row">
        /// Chunk row
        /// </param>
        /// <param name="column">
        /// Chunk column
        /// </param>
        /// <param name="scaleY">
        /// Height scaling value. >0 decreases otherwise it increases. 
        /// </param>
        public void SetHeightMap(Microsoft.Xna.Framework.Vector3[,] heightMap, int row, int column,float scaleY, Microsoft.DirectX.Vector3 offSet)
        {
            //this.tileChunks[this.chunkRows - 1 -row][this.chunkColumns - 1 - column].SetHeightMap(heightMap, scaleY);
            //this.tileChunks[row][column].SetHeightMap(heightMap, scaleY, offSet);
        }

        public void SetNormalMap(Microsoft.Xna.Framework.Vector3[,] normalMap, int row, int column)
        {
            //this.tileChunks[row][column].SetNormals(normalMap);
        }

        /*
        public CustomVertex.PositionColored[][] TileNormals
        {
            
            get
            {
                
                int numNormals = 145 *chunkColumns * chunkRows;
                int index=0;
                CustomVertex.PositionColored[][] tempArray = new CustomVertex.PositionColored[numNormals][];
                for (int i = 0; i < chunkRows; i++)
                {
                    for (int j = 0; j < chunkColumns; j++)
                    {
                        CustomVertex.PositionColored[][] chunkArray = tileChunks[i][j].ChunkNormals;
                        for (int k = 0; k < 145; k++)
                        {
                            tempArray[index++] = chunkArray[k];
                        }
                    }
                }

                return tempArray;
                //return tileChunks[0][0].ChunkNormals;
            }
        }*/

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

        public Microsoft.Xna.Framework.Vector3 TilePosition
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

        private static float TILELENGTH = 533.3333f; //scaler for tile width in the rendering system
        private static float CHUNKLENGTH = TILELENGTH / 16.0f; //16x16 chunks per tile

        public bool OverThisTile(Microsoft.DirectX.Vector2 dataPoint)
        {
            if (dataPoint.X >= topLeft.X && dataPoint.X < topLeft.X + TILELENGTH)
            {
                if (dataPoint.Y >= topLeft.Y && dataPoint.Y < topLeft.Y + TILELENGTH)
                    return true;
            }

            return false;
        }

        public MapTileChunk ChunkBelow(Microsoft.DirectX.Vector2 dataPoint)
        {
            dataPoint.X -= topLeft.X;
            dataPoint.Y -= topLeft.Y;

            int columnIndex = (int)(dataPoint.X / CHUNKLENGTH);
            int rowIndex = (int)(dataPoint.Y / CHUNKLENGTH);

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
                    tempVector.X += CHUNKLENGTH * 0.5f;
                    tempVector.Y += CHUNKLENGTH * 0.5f;
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
