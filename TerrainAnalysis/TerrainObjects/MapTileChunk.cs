using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TerrainAnalysis.TerrainObjects;

namespace WCell.Collision
{
    public class MapTileChunk
    {
        private static float UNITLENGTH = TerrainConstants.HighResChunk.ChunkSize / 8.0f;

        private readonly bool highRes;
        ChunkPoint[,] chunkPoints = null;
        bool[,] holes = new bool[4, 4];
        private Microsoft.DirectX.Vector3 chunkOffset;
        private bool currentlySelected = false;

        //Oct-Tree Definition
        //Indexed by enum for tidier code

        public MapTileChunk[] treePointers = null;

        public MapTileChunk(MapChunk mapChunk, bool highRes, Microsoft.DirectX.Vector3 RegionOffset)
        {
            
            this.highRes = highRes;            
            this.chunkPoints = new ChunkPoint[(highRes ? 9 + 8 : 9), 9];

            for (int i = 0; i < (highRes ? 9 + 8 : 9); i++)
            {
                for (int j = 0; j < (highRes ? (i % 2 == 0 ? 9 : 8) : 9); j++)
                {
                    chunkPoints[i, j] = new ChunkPoint(mapChunk.HeightMap[i, j], new ByteNormal(mapChunk.Normals[i, j][0], mapChunk.Normals[i, j][1], mapChunk.Normals[i, j][2]));
                }
            }

            ProcessHolesMask(mapChunk.Holes);

            this.treePointers = new MapTileChunk[8];
            for (int i = 0; i < 8; i++)
                this.treePointers[i] = null;

            this.chunkOffset = new Vector3(RegionOffset.X,RegionOffset.Y + mapChunk.HeightMap[0,0],RegionOffset.Z);
        }

        /// <summary>
        /// Extracts the holes in the map tile chunk.
        /// For low-res maps, a hole eliminates a square two columns and two rows in size
        /// For high-res maps, a hole eliminates a square two columns and four rows in size
        /// </summary>
        /// <param name="holesMask"></param>
        private void ProcessHolesMask(uint holesMask)
        {
            uint row;
            int i, j;
            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < 4; j++)
                {
                    row = (holesMask >> (i * 4)) & 0xF;
                    int bit = (int)((row>>j)&0x1);
                    holes[i, j] = (bit==0?false : true);
                }
            }
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

            if (this.highRes)
            {                
                vert.X = column * TerrainConstants.HighResChunk.UnitSizeX;
                vert.Z = row * TerrainConstants.HighResChunk.UnitSizeZ;
                if(row%2==1)
                    vert.X += TerrainConstants.HighResChunk.UnitSizeX * 0.5f;
            }
            else
            {
                vert.X = column * TerrainConstants.LowResChunk.UnitSizeX;
                vert.Z = row * TerrainConstants.LowResChunk.UnitSizeZ;
            }

            vert.X += this.chunkOffset.X;
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
        
        /// <summary>
        /// Checks to see if a ray collides with a particular map chunk given a point of origin and a direction towards the chunk
        /// </summary>
        /// <param name="start">Position of object origin</param>
        /// <param name="direction">Direction from object to chunk plane</param>
        /// <returns></returns>
        public float IntersectsChunk(Microsoft.DirectX.Vector3 start, Microsoft.DirectX.Vector3 direction)
        {
            //Create a 3d plane that governs the shape of the area based on three data points.
            //The plane is rectangular
            //Kaul: replace these assignments with any tree points from the area trigger
            Microsoft.DirectX.Vector3 topLeft, bottomLeft, topRight;
            topLeft = new Microsoft.DirectX.Vector3(this.chunkOffset.X, chunkPoints[0, 0].PointHeight, this.chunkOffset.Z);
            bottomLeft = new Microsoft.DirectX.Vector3(this.chunkOffset.X, chunkPoints[(this.highRes ? 16 : 8), 0].PointHeight, this.chunkOffset.Z + TerrainConstants.HighResChunk.ChunkSize);
            topRight = new Microsoft.DirectX.Vector3(this.chunkOffset.X + TerrainConstants.HighResChunk.ChunkSize, this.chunkPoints[0, 8].PointHeight, this.chunkOffset.Z);

            //Create the plane
            Microsoft.DirectX.Plane chunkPlane = Microsoft.DirectX.Plane.FromPoints(topLeft, topRight, bottomLeft);
            //Find the point on the plane where the direction ray intersects the plane. Planes are infinite and always return an intersection point
            //Returns 0,0,0 of the direction vector is parallel/antiparallel to the plane
            Microsoft.DirectX.Vector3 intersection = Microsoft.DirectX.Plane.IntersectLine(chunkPlane, start, direction);

            //Since the plane is infinite, we need to test if the intersection point is within the bounds of the finite plane
            //Create two test vectors. One pointing from the intersection point to the top right and one from the intersection point to the bottom left
            //The underlying theory being that if the point exists within the boundaries of the finite plane, the sign of each X,Y,Z in both test
            //vectors must be opposite.
            Microsoft.DirectX.Vector3 testDirection1, testDirection2;
            testDirection1 = topRight - intersection;
            testDirection2 = bottomLeft - intersection;

            //Add the signs of each vector component together
            int signX,signY,signZ,numZeros=0;
            signX = Math.Sign(testDirection1.X) + Math.Sign(testDirection2.X);
            signY = Math.Sign(testDirection1.Y) + Math.Sign(testDirection2.Y);
            signZ = Math.Sign(testDirection1.Z) + Math.Sign(testDirection2.Z);

            //Ignore situations where any of the components are zero. This means the plane sits perfectly on an axis plane
            if (testDirection1.Length() < TerrainConstants.HighResChunk.ChunkSize || testDirection2.Length()<TerrainConstants.HighResChunk.ChunkSize)
            {
                if (signZ == 0 || (int)(testDirection1.Z) == 0)
                    numZeros++;
                if (signY == 0 || (int)(testDirection1.Y) == 0)
                    numZeros++;
                if (signX == 0 || (int)(testDirection1.X) == 0)
                    numZeros++;
            }

            //If all the signs are opposite, return the lengths of both test direction vectors
            //Kaul: replace this with returning true or false for area triggers
            if (numZeros==3)
                return testDirection1.Length() + testDirection2.Length();
            else
                return float.MaxValue;
        }

        public Microsoft.DirectX.Vector3 ChunkCenter()
        {
            Microsoft.DirectX.Vector3 bottomLeft, topRight, center;
            bottomLeft = new Microsoft.DirectX.Vector3(this.chunkOffset.X, chunkPoints[(this.highRes ? 16 : 8), 0].PointHeight, this.chunkOffset.Z + TerrainConstants.HighResChunk.ChunkSize);
            topRight = new Microsoft.DirectX.Vector3(this.chunkOffset.X + TerrainConstants.HighResChunk.ChunkSize, this.chunkPoints[0, 8].PointHeight, this.chunkOffset.Z);

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
            Microsoft.DirectX.Vector3 tempVect;
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

        public TerrainCorrection ClosestNormal(Microsoft.DirectX.Vector3 position, Microsoft.DirectX.Vector3 direction)
        {
            
            int vertexRow = (int)((position.Z * (this.highRes ? 16.0f : 8.0f) / TerrainConstants.HighResChunk.ChunkSize)+direction.Z);
            int vertexColumn = (int)(((position.X * 9.0f) / TerrainConstants.HighResChunk.ChunkSize)+direction.X);

            int originalRow = vertexRow, originalColumn = vertexColumn;

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
            Microsoft.DirectX.Vector3 norm = CP.PointNormal;
            Microsoft.DirectX.Vector3 basePoint = new Microsoft.DirectX.Vector3(originalColumn*UNITLENGTH,CP.PointHeight,originalRow*UNITLENGTH), intersection, normal;

            normal = new Microsoft.DirectX.Vector3(norm.X, norm.Y, norm.Z);
            Microsoft.DirectX.Plane correctionPlane = Microsoft.DirectX.Plane.FromPointNormal(basePoint, normal);

            intersection = Microsoft.DirectX.Plane.IntersectLine(correctionPlane, position, position + normal);

            TerrainCorrection unitTerrainCorrection = new TerrainCorrection();

            unitTerrainCorrection.newNormal = normal;
            unitTerrainCorrection.surfaceCorrection = intersection - position;

            return unitTerrainCorrection;
        }
    }

    public struct TerrainCorrection
    {
        public Microsoft.DirectX.Vector3 newNormal;
        public Microsoft.DirectX.Vector3 surfaceCorrection;
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

        public Microsoft.DirectX.Vector3 PointNormal
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

        public Microsoft.DirectX.Vector3 NormalVector
        {
            get
            {
                return new Vector3(-((float)x / 127.0f), (float)y / 127.0f, -((float)z / 127.0f));
            }
        }
    }
}
