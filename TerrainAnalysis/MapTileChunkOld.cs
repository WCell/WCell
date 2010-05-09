using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Windows.Forms;
using System.Collections;
using Microsoft.Xna.Framework;

namespace TerrainAnalysis
{
    public class MapTileChunkOld
    {        
        TileVertex[][] chunkVerts = null;
        //TileVertex[][] triangleStrips = null;        
        private readonly int rowCount, columnCount;
        private float miniDX, miniDZ;
        private Microsoft.DirectX.Vector3 TileOffSet = new Microsoft.DirectX.Vector3(0, 0, 0);

        //Tile Chunk Oct Tree for fast searching
        public MapTileChunkOld top=null, bottom=null, left=null, right=null, topLeft=null, topRight=null, bottomLeft=null, bottomRight=null;



        /// <summary>
        /// Class that encapsulates the elements of Warcraft terrain altitudes represented as smaller chunks of a map tile
        /// </summary>
        /// <param name="numRows"></param>
        /// The number of rows of vertices in each chunk. This is typically 9+8
        /// <param name="numColumns"></param>
        /// The number of columns of vertices in each chunk. This is typically 9
        public MapTileChunkOld(int numRows, int numColumns)
        {            
            this.rowCount = numRows;
            this.columnCount = numColumns;
            this.miniDX = 1.0f / (float)(numColumns-1);
            this.miniDZ = -1.0f / (float)(numRows-1);

            this.chunkVerts = new TileVertex[numRows][];
            //this.triangleStrips = new TileVertex[numRows - 1][];
            

            int i = 0,j=0;
            for (i = 0; i < numRows; i++)
            {
                int colCount = (i % 2 == 1) ? numColumns - 1 : numColumns;
                chunkVerts[i] = new TileVertex[colCount];
                for (j = 0; j < colCount; j++)
                {
                    //Creates a grey-scale gradient across the map tile chunk
                    float iSqr = ((float)i / (float)numRows) * ((float)i / (float)numRows);
                    float jSqr = ((float)j / (float)numColumns) * ((float)j / (float)numColumns);
                    float baseColour = (float)Math.Sqrt(iSqr + jSqr);
                    int colourValue = (int)(180*baseColour);

                    //The actual data point structure encapsulated into a class
                    chunkVerts[i][j] = new TileVertex(new CustomVertex.PositionColored(0.0f, 0.0f, 0.0f, System.Drawing.Color.FromArgb(colourValue, colourValue, colourValue).ToArgb()), new Microsoft.Xna.Framework.Vector3(0, 1, 0));                    
                }
            }

            
            //CreateTriangleStrips();
            //CoordinateVertices();
        }

        private TileVertex[] CreateTriangleStrip(TileVertex[] topRow, TileVertex[] bottomRow, TileVertex endVertex, bool rightToLeft)
        {
            TileVertex[] vertStrip = new TileVertex[columnCount*2];
            int vertCount = 0;

            if (rightToLeft)
            {
                int i = 0;
                for (i = columnCount-1; i >= 0; i--)
                {                    
                    vertStrip[vertCount++] = topRow[i];                    
                    vertStrip[vertCount++] = i == 0 ? endVertex : bottomRow[i - 1];                    
                }
            }
            else
            {
                int i = 0;
                for (i = 0; i < this.columnCount; i++)
                {
                    vertStrip[vertCount++] = bottomRow[i];
                    vertStrip[vertCount++] = i == columnCount-1 ? endVertex : topRow[i];
                }
            }

            return vertStrip;
        }

        private TileVertex[][] CreateTriangleStrips()
        {
            int i=0;
            bool oddRow = false;
            TileVertex[][] triangleStrips = new TileVertex[this.rowCount - 1][];
            for (i = 0; i < this.rowCount-1; i++)
            {
                if (!oddRow)
                {
                    triangleStrips[i] = CreateTriangleStrip(chunkVerts[i], chunkVerts[i+1], chunkVerts[i+2][0], true);
                    foreach (TileVertex TV in triangleStrips[i])
                    {
                        CustomVertex.PositionColored pcV = TV.DataPoint;
                    }
                }
                else
                {
                    triangleStrips[i] = CreateTriangleStrip(chunkVerts[i], chunkVerts[i + 1], chunkVerts[i - 1][this.columnCount - 1], false);
                    foreach (TileVertex TV in triangleStrips[i])
                    {
                        CustomVertex.PositionColored pcV = TV.DataPoint;
                    }
                }
                oddRow = !oddRow;
            }

            return triangleStrips;
        }

        private void CoordinateVertices()
        {
            int x, z;
            for (z = 0; z < this.rowCount; z++)
            {
                int colCount = (z % 2 == 1) ? this.columnCount - 1 : this.columnCount;
                for (x = 0; x < colCount; x++)
                {
                    CustomVertex.PositionColored tempObj = chunkVerts[z][x].DataPoint;
                    tempObj.Y = 0.0f;
                    tempObj.X = x * this.miniDX;
                    tempObj.X += z % 2 == 1 ? this.miniDX / 2.0f : 0;
                    tempObj.Z = z * this.miniDZ;
                    tempObj.Color = System.Drawing.Color.White.ToArgb();

                    chunkVerts[z][x].SetX(tempObj.X);
                    chunkVerts[z][x].SetY(tempObj.Y);
                    chunkVerts[z][x].SetZ(tempObj.Z);
                }
            }
        }

        //Public Methods

        /// <summary>
        /// Gets the list of triangle strips for the map tile chunk containing vertex references
        /// </summary>
        public TileVertex[][] TriangleStrips
        {
            get
            {
                return CreateTriangleStrips();
            }
        }

        /// <summary>
        /// Set
        /// </summary>
        /// <param name="heightMap">
        /// Two-Dimensional array containing floating point numbers which determine the height of each point in the tile chunk
        /// </param>
        public void SetHeightMap(Microsoft.Xna.Framework.Vector3[,] heightMap)
        {
            int i, j;
            this.TileOffSet = new Microsoft.DirectX.Vector3(heightMap[0,0].X, heightMap[0,0].Y, heightMap[0,0].Z);
            for (i = 0; i < this.rowCount; i++)
            {
                int colCount = (i % 2 == 1) ? this.columnCount - 1 : this.columnCount;
                for (j = 0; j < colCount; j++)
                {
                    chunkVerts[i][j].SetY(heightMap[i, j].Y);
                    chunkVerts[i][j].SetX(heightMap[i, j].X);
                    chunkVerts[i][j].SetZ(heightMap[i, j].Z);
                }
            } 
        }

        public void SetNormals(Microsoft.Xna.Framework.Vector3[,] normalMap)
        {
            int i, j;

            for (i = 0; i < this.rowCount; i++)
            {
                int colCount = (i % 2 == 1) ? this.columnCount - 1 : this.columnCount;
                for (j = 0; j < colCount; j++)
                {
                    this.chunkVerts[i][j].NormalX = normalMap[i, j].X;
                    this.chunkVerts[i][j].NormalY = normalMap[i, j].Y;
                    this.chunkVerts[i][j].NormalZ = normalMap[i, j].Z;
                }
            } 
        }

        public CustomVertex.PositionColored[][] ChunkNormals
        {
            get
            {
                int normalCount = (this.columnCount * this.rowCount) - (this.columnCount - 1);
                CustomVertex.PositionColored[][] tempArray = new CustomVertex.PositionColored[normalCount][];
                int index = 0;
                for (int i = 0; i < this.rowCount; i++)
                {
                    int colCount = chunkVerts[i].Count();
                    
                    for (int j = 0; j < colCount; j++)
                    {
                        tempArray[index] = new CustomVertex.PositionColored[2];
                        tempArray[index][0] = chunkVerts[i][j].DataPoint;
                        CustomVertex.PositionColored tempNorm = chunkVerts[i][j].DataNormal;
                        tempNorm.X *= 4.0f;
                        tempNorm.Y *= 4.0f;
                        tempNorm.Z *= 4.0f;
                        tempNorm.X += chunkVerts[i][j].DataPoint.X;
                        tempNorm.Y += chunkVerts[i][j].DataPoint.Y;
                        tempNorm.Z += chunkVerts[i][j].DataPoint.Z;
                        tempNorm.Color = System.Drawing.Color.Red.ToArgb();

                        tempArray[index++][1] = tempNorm;
                    }
                }

                return tempArray;
            }
        }

        /// <summary>
        /// Used to shift the tile chunk to it's absolute rendering space
        /// </summary>
        /// <param name="topLeft">
        /// Two-dimensional point that governs the absolute rendering position of the top-left of the tile
        /// </param>
        /// <param name="scaleX">
        /// Floating point number that defines the rendering scale of points along the X-axis
        /// </param>
        /// <param name="scaleZ">
        /// Floating point number that defines the rendering scale of points along the Z-axis
        /// </param>
        public void Shift_Scale(System.Drawing.PointF topLeft, float scaleX, float scaleZ)
        {
            int row, col;
            for (row = 0; row < this.rowCount; row++)
            {
                int colCount = (row % 2 == 1) ? this.columnCount - 1 : this.columnCount;
                for (col = 0; col < colCount; col++)
                {
                    CustomVertex.PositionColored tempVert = this.chunkVerts[row][col].DataPoint;
                    tempVert.X *= scaleX;
                    tempVert.Z *= scaleZ;

                    tempVert.X += topLeft.X;
                    tempVert.Z += topLeft.Y;

                    this.chunkVerts[row][col].SetX(tempVert.X);
                    this.chunkVerts[row][col].SetZ(tempVert.Z);
                }
            }
        }

        /*
        public void WorldShift(Vector3 location)
        {

        }*/

        /// <summary>
        /// Grabs the bottom row of the map chunk to bridge with the map chunk under it
        /// </summary>
        /// <returns>1d array of data points</returns>
        public TileVertex[] BottomRow()
        {
            return this.chunkVerts[this.rowCount-1];
        }

        public TileVertex[] TopRow()
        {
            return this.chunkVerts[0];
        }

        public TileVertex[] LeftColumn()
        {
            int i = 0;
            ArrayList tempArray = new ArrayList();

            for (i = 0; i < rowCount; i += 2)
            {
                tempArray.Add(chunkVerts[i][0]);
            }

            TileVertex[] vertArray = new TileVertex[tempArray.Count];
            for (i = 0; i < tempArray.Count; i++)
                vertArray[i] = (TileVertex)tempArray[i];

            return vertArray;
        }

        /// <summary>
        /// Grabs the right column of the map chunk to bridge with the map chunk to the right of this one
        /// </summary>
        /// <returns></returns>
        public TileVertex[] RightColumn()
        {
            int i = 0;
            ArrayList tempArray = new ArrayList();

            for (i = 0; i < rowCount; i += 2)
            {
                tempArray.Add(chunkVerts[i][columnCount - 1]);
            }

            TileVertex[] vertArray = new TileVertex[tempArray.Count];
            for (i = 0; i < tempArray.Count; i++)
                vertArray[i] = (TileVertex)tempArray[i];

            return vertArray;
        }

        public Microsoft.DirectX.Vector3 ChunkOffset
        {
            get
            {
                return this.TileOffSet;
            }
        }
    }
}
