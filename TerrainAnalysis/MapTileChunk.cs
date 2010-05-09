using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.DirectX.Direct3D;

namespace WCell.Collision
{
    public class MapTileChunk
    {
        private static float TILELENGTH = 533.3333f; //scaler for tile width in the rendering system
        private static float CHUNKLENGTH = TILELENGTH / 16.0f; //16x16 chunks per tile
        private static float UNITLENGTH = CHUNKLENGTH / 8.0f;

        private readonly bool highRes;
        ChunkPoint[,] chunkPoints = null;
        private Microsoft.Xna.Framework.Vector3 chunkOffset;
        private bool currentlySelected = false;

        //Oct-Tree Definition
        //Indexed by enum for tidier code

        public MapTileChunk[] treePointers = null;

        public MapTileChunk(float[,] heightMap, sbyte[,][] normalMap, bool highRes, Microsoft.Xna.Framework.Vector3 RegionOffset)
        {
            this.highRes = highRes;            
            this.chunkPoints = new ChunkPoint[(highRes ? 9 + 8 : 9), 9];

            for (int i = 0; i < (highRes ? 9 + 8 : 9); i++)
            {
                for (int j = 0; j < (highRes ? (i % 2 == 0 ? 9 : 8) : 9); j++)
                {                    
                    chunkPoints[i, j] = new ChunkPoint(heightMap[i,j], new ByteNormal(normalMap[i,j][0], normalMap[i,j][1], normalMap[i,j][2]));
                }
            }

            this.treePointers = new MapTileChunk[8];
            for (int i = 0; i < 8; i++)
                this.treePointers[i] = null;

            this.chunkOffset = new Vector3(RegionOffset.X,RegionOffset.Y + heightMap[0,0],RegionOffset.Z);
        }

        public CustomVertex.PositionColored[] TriangleStrip
        {
            get
            {
                return CreateTriangleStrips();
            }
        }

        //Used for Rendering only
        private CustomVertex.PositionColored[] CreateTriangleStrips()
        {
            CustomVertex.PositionColored[] vertexArray;
            int vertCount=0;
            if (this.highRes)
            {
                vertexArray = new CustomVertex.PositionColored[18 * 16];
                for (int i = 0; i < 16; i++)
                {
                    CustomVertex.PositionColored[] verts = GetStrip(i, i+1, i%2==0);
                    for (int j = 0; j < 18; j++)
                        vertexArray[vertCount++] = verts[j];
                }
            }
            else
            {
                vertexArray = new CustomVertex.PositionColored[18 * 8];
                for (int i = 0; i < 8; i++)
                {
                    CustomVertex.PositionColored[] verts = GetStrip(i, i + 1, true);
                    for (int j = 0; j < 18; j++)
                        vertexArray[vertCount++] = verts[j];
                }
            }

            return vertexArray;
        }

        private CustomVertex.PositionColored CreateVertex(int row, int column)
        {
            CustomVertex.PositionColored vert = new CustomVertex.PositionColored();
            vert.X = (column * UNITLENGTH) + this.chunkOffset.X;
            vert.Z = (row * UNITLENGTH);
            if (this.highRes)
            {
                vert.Z *= 0.5f;
                vert.X += UNITLENGTH * 0.5f;
            }
            vert.Z += this.chunkOffset.Z;
            vert.Y = this.chunkPoints[row, column].PointHeight;

            float colourValue;
            if (!this.currentlySelected)
            {
                if (highRes) colourValue = ((float)Math.Sqrt((row * row * 0.25) + (column * column)) * 255 * (float)Math.Sqrt(0.5f)) / 8.0f;
                else colourValue = ((float)Math.Sqrt((row * row) + (column * column)) * 255 * (float)Math.Sqrt(0.5f)) / 8.0f;
                vert.Color = System.Drawing.Color.FromArgb(150,(int)colourValue, (int)colourValue, (int)colourValue).ToArgb();                                
            }
            else
            {                
                vert.Color = System.Drawing.Color.FromArgb(50, System.Drawing.Color.CornflowerBlue).ToArgb();                
            }
            
            

            return vert;
        }
        //Used for rendering only
        private CustomVertex.PositionColored[] GetStrip(int topRow, int bottomRow, bool evenRow)
        {
            CustomVertex.PositionColored[] verts = new CustomVertex.PositionColored[18];
            int count=0;
            if (highRes)
            {
                if (evenRow)
                {
                    for (int i = 8; i >= 0; i--)
                    {
                        verts[count++] = CreateVertex(topRow,i);
                        verts[count++] = (i==0 ? CreateVertex(bottomRow+1,0) : CreateVertex(bottomRow,i-1));
                    }
                }
                else
                {                    
                    for (int i = 0; i < 9; i++)
                    {
                        verts[count++] = CreateVertex(bottomRow,i);
                        verts[count++] = (i==8 ? CreateVertex(topRow-1,8) : CreateVertex(topRow,i));
                    }
                }
            }
            else
            {
                for (int i = 8; i >= 0; i--)
                {
                    verts[count++] = CreateVertex(topRow, i);
                    verts[count++] = CreateVertex(bottomRow, i);
                }
            }

            return verts;
        }

        public CustomVertex.PositionColored[] NormalList
        {
            get
            {
                CustomVertex.PositionColored[] normals;
                if (this.highRes)
                {
                    int count = 0;
                    normals = new CustomVertex.PositionColored[2 * ((9 * 9) + (8 * 8))];
                    for (int i = 0; i < 9 + 8; i++)
                    {
                        for (int j = 0; j < (i % 2 == 0 ? 9 : 8); j++)
                        {
                            CustomVertex.PositionColored baseVert, normal;
                            baseVert = new CustomVertex.PositionColored();
                            baseVert.X = (j * UNITLENGTH) + this.chunkOffset.X;
                            if (i % 2 == 0)
                                baseVert.X += UNITLENGTH * 0.5f;
                            baseVert.Z = (i * UNITLENGTH * 0.5f)+this.chunkOffset.Z;
                            baseVert.Y = this.chunkPoints[i, j].PointHeight;
                            baseVert.Color = System.Drawing.Color.White.ToArgb();

                            normal = new CustomVertex.PositionColored();
                            normal.X = baseVert.X + this.chunkPoints[i, j].PointNormal.X;
                            normal.Y = baseVert.Y + this.chunkPoints[i, j].PointNormal.Y;
                            normal.Z = baseVert.Z + this.chunkPoints[i, j].PointNormal.Z;
                            normal.Color = System.Drawing.Color.Red.ToArgb();

                            normals[count++] = baseVert;
                            normals[count++] = normal;
                        }
                    }
                }
                else
                {
                    int count = 0;
                    normals = new CustomVertex.PositionColored[2 * 9 * 9];
                    for (int i = 0; i < 9; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            CustomVertex.PositionColored baseVert, normal;
                            baseVert = new CustomVertex.PositionColored();
                            baseVert.X = (j * UNITLENGTH) + this.chunkOffset.X;
                            if (i % 2 == 0)
                                baseVert.X += UNITLENGTH * 0.5f;
                            baseVert.Z = (i * UNITLENGTH) + this.chunkOffset.Z;
                            baseVert.Y = this.chunkPoints[i, j].PointHeight;
                            baseVert.Color = System.Drawing.Color.White.ToArgb();

                            normal = new CustomVertex.PositionColored();
                            normal.X = baseVert.X + this.chunkPoints[i, j].PointNormal.X;
                            normal.Y = baseVert.Y + this.chunkPoints[i, j].PointNormal.Y;
                            normal.Z = baseVert.Z + this.chunkPoints[i, j].PointNormal.Z;
                            normal.Color = System.Drawing.Color.Red.ToArgb();

                            normals[count++] = baseVert;
                            normals[count++] = normal;
                        }
                    }
                }



                return normals;
            }
        }
        
        public float IntersectsChunk(Microsoft.DirectX.Vector3 start, Microsoft.DirectX.Vector3 direction)
        {
            Microsoft.DirectX.Vector3 topLeft, bottomLeft, topRight;
            topLeft = new Microsoft.DirectX.Vector3(this.chunkOffset.X, chunkPoints[0, 0].PointHeight, this.chunkOffset.Z);
            bottomLeft = new Microsoft.DirectX.Vector3(this.chunkOffset.X, chunkPoints[(this.highRes ? 16 : 8), 0].PointHeight, this.chunkOffset.Z + CHUNKLENGTH);
            topRight = new Microsoft.DirectX.Vector3(this.chunkOffset.X + CHUNKLENGTH, this.chunkPoints[0, 8].PointHeight, this.chunkOffset.Z);

            Microsoft.DirectX.Plane chunkPlane = Microsoft.DirectX.Plane.FromPoints(topLeft, topRight, bottomLeft);
            Microsoft.DirectX.Vector3 intersection = Microsoft.DirectX.Plane.IntersectLine(chunkPlane, start, direction);

            Microsoft.DirectX.Vector3 testDirection1, testDirection2;
            testDirection1 = topRight - intersection;
            testDirection2 = bottomLeft - intersection;

            int signX,signY,signZ,numZeros=0;
            signX = Math.Sign(testDirection1.X) + Math.Sign(testDirection2.X);
            signY = Math.Sign(testDirection1.Y) + Math.Sign(testDirection2.Y);
            signZ = Math.Sign(testDirection1.Z) + Math.Sign(testDirection2.Z);

            if (testDirection1.Length() < CHUNKLENGTH || testDirection2.Length()<CHUNKLENGTH)
            {
                if (signZ == 0 || (int)(testDirection1.Z) == 0)
                    numZeros++;
                if (signY == 0 || (int)(testDirection1.Y) == 0)
                    numZeros++;
                if (signX == 0 || (int)(testDirection1.X) == 0)
                    numZeros++;
            }

            if (numZeros==3)
                return testDirection1.Length() + testDirection2.Length();
            else
                return float.MaxValue;
        }

        public Microsoft.DirectX.Vector3 ChunkCenter()
        {
            Microsoft.DirectX.Vector3 bottomLeft, topRight, center;
            bottomLeft = new Microsoft.DirectX.Vector3(this.chunkOffset.X, chunkPoints[(this.highRes ? 16 : 8), 0].PointHeight, this.chunkOffset.Z + CHUNKLENGTH);
            topRight = new Microsoft.DirectX.Vector3(this.chunkOffset.X + CHUNKLENGTH, this.chunkPoints[0, 8].PointHeight, this.chunkOffset.Z);

            center = topRight + bottomLeft;
            center.Multiply(0.5f);

            if (highRes)
                center.Y = this.chunkPoints[8, 4].PointHeight;
            else
                center.Y = this.chunkPoints[4, 4].PointHeight;


            return center;
        }

        public Microsoft.DirectX.Vector3 ChunkCenterNormal()
        {
            Microsoft.Xna.Framework.Vector3 tempVect;
            if (highRes)
                tempVect = this.chunkPoints[8, 4].PointNormal;
            else
                tempVect = this.chunkPoints[4, 4].PointNormal;

            return new Microsoft.DirectX.Vector3(tempVect.X, tempVect.Y, tempVect.Z);
        }

        public bool Selected
        {
            get
            {
                return this.currentlySelected;
            }
            set
            {
                this.currentlySelected = value;
            }
        }

        public Microsoft.DirectX.Vector3 ClosestNormal(Microsoft.DirectX.Vector3 position, Microsoft.DirectX.Vector2 direction)
        {
            
            int vertexRow = (int)((position.Z * (this.highRes ? 16.0f : 8.0f) / CHUNKLENGTH)+direction.Y);
            int vertexColumn = (int)(((position.X * 9.0f) / CHUNKLENGTH)+direction.X);

            int rowSwitch = (this.highRes ? vertexRow%2 : 0);
            rowSwitch = (int)Math.Sqrt(rowSwitch*rowSwitch);

            int columnLimit = (rowSwitch==1 ? 7 : 8);
            int rowLimit = (this.highRes ? 16 : 8);

            MapTileChunk destinationChunk = this;

            if (vertexColumn > columnLimit)
            {
                destinationChunk = this.treePointers[(int)ChunkTree.Right];
                vertexColumn -= columnLimit;
                if (vertexRow > rowLimit)
                {
                    destinationChunk = destinationChunk.treePointers[(int)ChunkTree.Bottom];
                    vertexRow -= rowLimit;
                }
                else if (vertexRow < 0)
                {
                    destinationChunk = destinationChunk.treePointers[(int)ChunkTree.Top];
                    vertexRow += rowLimit;
                }
            }
            else if (vertexColumn < 0)
            {
                destinationChunk = this.treePointers[(int)ChunkTree.Left];
                vertexColumn += columnLimit;
                if (vertexRow > rowLimit)
                {
                    destinationChunk = destinationChunk.treePointers[(int)ChunkTree.Bottom];
                    vertexRow -= rowLimit;
                }
                else if (vertexRow < 0)
                {
                    destinationChunk = destinationChunk.treePointers[(int)ChunkTree.Top];
                    vertexRow += rowLimit;
                }
            }

            ChunkPoint CP = destinationChunk.chunkPoints[vertexRow, vertexColumn];
            Microsoft.Xna.Framework.Vector3 norm = CP.PointNormal;
            return new Microsoft.DirectX.Vector3(norm.X, norm.Y, norm.Z);
        }
    }

    public enum ChunkTree
    {
        Top=0,
        Bottom=1,
        Left=2,
        Right=3,
        TopLeft=4,
        TopRight=5,
        BottomLeft=6,
        BottomRight=7
    };

    public class ChunkPoint
    {
        private readonly float pointHeight;
        private readonly ByteNormal pointNormal;

        public ChunkPoint(float height, ByteNormal normal)
        {
            this.pointHeight = height;
            this.pointNormal = normal;
        }

        public Microsoft.Xna.Framework.Vector3 PointNormal
        {
            get
            {
                return this.pointNormal.NormalVector;
            }
        }

        public float PointHeight
        {
            get
            {
                return this.pointHeight;
            }
        }
    }

    public class ByteNormal
    {
        private readonly sbyte x, y, z;

        public ByteNormal(sbyte x, sbyte y, sbyte z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Microsoft.Xna.Framework.Vector3 NormalVector
        {
            get
            {
                return new Vector3(-((float)x / 127.0f), (float)y / 127.0f, -((float)z / 127.0f));
            }
        }
    }
}
