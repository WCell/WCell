using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace TerrainAnalysis
{
    //Encapsulates a class for rendering a wire-frame object in the world. Has it's own coordinate, movement and rotation functionality
    public class LineStripObject
    {
        private Vector3 position;
        private Vector3[] objectVertices = new Vector3[8], renderVertices = new Vector3[8];
        private Vector3 objectCenter, pointer, pointerBase, normal;
        private float pointerLength;
        private float rotationY = 0.0f, rotationX = 0.0f;
        

        private VertexBuffer objectBuffer = null;
        private IndexBuffer objectIndices = null;

        private bool UpdatingObject = false, selected=false, hover=false;

        /// <summary>
        /// Creates a wire-frame block with directional vector pointing in the direction the front of the cube is facing
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="depth"></param>
        /// <param name="initialPosition"></param>
        public LineStripObject(float width, float height, float depth, Vector3 initialPosition, Vector3 surfaceNormal, float unitRotation, Device d3DDevice)
        {
            this.position = initialPosition;
            float dWidth=width*0.5f, dHeight=height*0.5f, dDepth=depth*0.5f;
            this.normal = surfaceNormal;
            this.rotationY = unitRotation;

            //Convention is top-left, top-right, bottom-left, bottom-right
            //Block is initally facing along the positive Z-axis away from the origin.

            objectVertices[0] = new Vector3(dWidth, height, dDepth); //top left
            objectVertices[1] = new Vector3(-dWidth, height, dDepth); //top right
            objectVertices[2] = new Vector3(dWidth, 0.0f, dDepth); //bottom left
            objectVertices[3] = new Vector3(-dWidth, 0.0f, dDepth); //bottom right

            objectVertices[4] = new Vector3(dWidth, height, -dDepth);
            objectVertices[5] = new Vector3(-dWidth, height, -dDepth);
            objectVertices[6] = new Vector3(dWidth, 0.0f, -dDepth);
            objectVertices[7] = new Vector3(-dWidth, 0.0f, -dDepth);

            objectCenter = new Vector3(0, dHeight, 0);
            pointerBase = new Vector3(0, dHeight, 0);
            pointer = new Vector3(0, 0, 1);
            pointerLength = depth;

            this.normal.Normalize();
            //this.pointer = this.orthogonalPointer(this.normal);

            createLineStripBuffer(d3DDevice);
            createIndexBuffer(d3DDevice);

            SnapToNormal(this.normal);
            //rotateUnit(unitRotation);

        }

        private void rotateUnit(float rotY)
        {
            this.rotationY = rotY;
            Microsoft.DirectX.Vector3 start = new Vector3(0, 0, 1);
            start.TransformCoordinate(Matrix.RotationY(rotY));
            //this.pointer = start;

            //RotateXY(this.rotationXFromNormal(this.normal), rotY);
        }

        private float rotationXFromNormal(Vector3 surfaceNormal)
        {
            float height = surfaceNormal.Y;

            return (float)Math.Asin(height);            
        }

        private void createIndexBuffer(Device d3Device)
        {
            //26 vertex indices
            Int16[] objectIndexArray = new Int16[]{0,1, 1,3, 3,2, 2,0, //front
                                                    4,5, 5,7, 7,6, 6,4, //back
                                                    0,4, 1,5, 2,6, 3,7, //lines joining front and back
                                                    8,9}; //pointer line
            this.objectIndices = new IndexBuffer(d3Device, 52, 0, Pool.Default, true);
            objectIndices.SetData(objectIndexArray, 0, LockFlags.None);            
        }

        private void createLineStripBuffer(Device d3DDevice)
        {
            this.objectBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), 10, d3DDevice, 0, CustomVertex.PositionColored.Format, Pool.Default);


            CustomVertex.PositionColored[] verts = (CustomVertex.PositionColored[])objectBuffer.Lock(0, 0);

            int colourValue;            
            colourValue = System.Drawing.Color.White.ToArgb();

            for (int i = 0; i < 8; i++)
            {
                verts[i].Color = System.Drawing.Color.White.ToArgb();
                verts[i].X = objectVertices[i].X + position.X;
                verts[i].Y = objectVertices[i].Y + position.Y;
                verts[i].Z = objectVertices[i].Z + position.Z;

                renderVertices[i].X = objectVertices[i].X;
                renderVertices[i].Y = objectVertices[i].Y;
                renderVertices[i].Z = objectVertices[i].Z;
            }

            verts[8] = new CustomVertex.PositionColored(objectCenter + this.position, colourValue);
            verts[9] = new CustomVertex.PositionColored(objectCenter + this.position + (pointer * pointerLength), colourValue);

            objectBuffer.Unlock();            
        }



        private void RefreshVertexBuffer()
        {
            this.UpdatingObject = true;

            CustomVertex.PositionColored[] verts = (CustomVertex.PositionColored[])objectBuffer.Lock(0, 0);
            int colourValue;

            if(this.hover)
                colourValue = System.Drawing.Color.Yellow.ToArgb();
            else if(!this.selected)
                colourValue = System.Drawing.Color.White.ToArgb();
            else
                colourValue = System.Drawing.Color.Red.ToArgb();

            for (int i = 0; i < 8; i++)
            {
                verts[i].Color = colourValue;
                verts[i].X = renderVertices[i].X + position.X;
                verts[i].Y = renderVertices[i].Y + position.Y;
                verts[i].Z = renderVertices[i].Z + position.Z;
            }

            verts[8] = new CustomVertex.PositionColored(pointerBase + position, colourValue);
            verts[9] = new CustomVertex.PositionColored(pointerBase + (pointer * pointerLength) + position, colourValue);

            objectBuffer.Unlock();

            this.UpdatingObject = false;
        }

        public VertexBuffer BlockBuffer
        {
            get
            {
                return this.objectBuffer;
            }
        }

        public IndexBuffer BlockIndices
        {
            get
            {
                return this.objectIndices;
            }
        }

        public bool UpdatingWorldObject
        {
            get
            {
                return this.UpdatingObject;
            }
        }

        public bool Selected
        {
            get
            {
                return this.selected;
            }
            set
            {
                if (this.selected != value)
                {
                    this.selected = value;
                    this.RefreshVertexBuffer();
                }
            }
        }

        public bool Hover
        {
            set
            {
                if (this.hover != value)
                {
                    this.hover = value;
                    RefreshVertexBuffer();
                    //throw new Exception("CUNT FACE");
                }
            }
            get
            {
                return this.hover;
            }
        }

        public float IntersectsObject(Microsoft.DirectX.Vector3 origin, Microsoft.DirectX.Vector3 direction)
        {
            int[][] faceIndices = new int[6][]; //Check all 6 faces

            faceIndices[0] = new int[] { 0, 1, 2 }; //front
            faceIndices[1] = new int[] { 4, 5, 6 }; //back
            faceIndices[2] = new int[] { 1, 0, 5 }; //top
            faceIndices[3] = new int[] { 3, 2, 7 }; //bottom
            faceIndices[4] = new int[] { 4, 0, 6 }; //left
            faceIndices[5] = new int[] { 5, 1, 7 }; //right

            float closestIntersectionDistance = float.MaxValue;

            for (int i = 0; i < 6; i++)
            {

                Microsoft.DirectX.Vector3[] planeVerts = new Vector3[3];
                for (int j = 0; j < 3; j++)
                {
                    planeVerts[j] = new Vector3(this.position.X, this.position.Y, this.position.Z);
                    planeVerts[j].X += this.objectVertices[faceIndices[i][j]].X;
                    planeVerts[j].Y += this.objectVertices[faceIndices[i][j]].Y;
                    planeVerts[j].Z += this.objectVertices[faceIndices[i][j]].Z;
                }

                Microsoft.DirectX.Plane chunkPlane = Microsoft.DirectX.Plane.FromPoints(planeVerts[0], planeVerts[1], planeVerts[2]);
                Microsoft.DirectX.Vector3 intersection = Microsoft.DirectX.Plane.IntersectLine(chunkPlane, origin, origin + direction);

                Microsoft.DirectX.Vector3 testDirection1, testDirection2;
                testDirection1 = planeVerts[1] - intersection;
                testDirection2 = planeVerts[2] - intersection;

                int signX, signY, signZ, numZeros = 0;
                signX = Math.Sign(testDirection1.X) + Math.Sign(testDirection2.X);
                signY = Math.Sign(testDirection1.Y) + Math.Sign(testDirection2.Y);
                signZ = Math.Sign(testDirection1.Z) + Math.Sign(testDirection2.Z);

                float planeLength = (planeVerts[1]-planeVerts[2]).Length();

                if (testDirection1.Length() < planeLength || testDirection2.Length() < planeLength)
                {
                    if (signZ == 0 || (int)(testDirection1.Z) == 0)
                        numZeros++;
                    if (signY == 0 || (int)(testDirection1.Y) == 0)
                        numZeros++;
                    if (signX == 0 || (int)(testDirection1.X) == 0)
                        numZeros++;
                }

                if (numZeros == 3)
                    if ((intersection - origin).Length() < closestIntersectionDistance)
                        closestIntersectionDistance = (intersection - origin).Length();
            }
            

            return closestIntersectionDistance;
        }

        public void rotateAboutNormal(float rotYPrime)
        {
            Microsoft.DirectX.Vector3 xPrime = this.normal;
            float rotX = rotYPrime;

            int i = 0;
            for (i = 0; i < 8; i++)
                this.renderVertices[i].TransformCoordinate(Matrix.RotationAxis(xPrime, rotX));
            this.pointerBase.TransformCoordinate(Matrix.RotationAxis(xPrime, rotX));
            this.pointer.TransformCoordinate(Matrix.RotationAxis(xPrime, rotX));

            this.RefreshVertexBuffer();
        }

        private void rotateXPrime(float rotX, Vector3 xPrime)
        {            
            int i = 0;
            for (i = 0; i < 8; i++)
                this.renderVertices[i].TransformCoordinate(Matrix.RotationAxis(xPrime,rotX));
            this.pointerBase.TransformCoordinate(Matrix.RotationAxis(xPrime, rotX));
            this.pointer.TransformCoordinate(Matrix.RotationAxis(xPrime, rotX));

            this.RefreshVertexBuffer();
        }

        public void SnapToNormal(Microsoft.DirectX.Vector3 normal)
        {
            float theta=0.0f;
            Vector3 xPrime;
            
            xPrime = Vector3.Cross(this.normal, normal);
            xPrime.Normalize();
            
            //theta = (float)Math.Acos(dotProduct)*(float)Math.Sign(dotProduct);            
            theta = (float)Math.Atan2(Vector3.Dot(xPrime, Vector3.Cross(this.normal, normal)), Vector3.Dot(this.normal, normal));            
            rotateXPrime(theta, xPrime);
            this.normal = normal;
            
        }

        public void MoveObject(float magnitude)
        {
            this.position += magnitude * this.pointer;
            RefreshVertexBuffer();
        }

        public Microsoft.DirectX.Vector3 ObjectPosition
        {
            get
            {
                return this.position;
            }
        }

        public Microsoft.DirectX.Vector2 ObjectDirection
        {
            get
            {
                return new Microsoft.DirectX.Vector2(this.pointer.X, this.pointer.Z);
            }
        }

        
    }
}
