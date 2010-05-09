using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Windows.Forms;
using System.Threading;
using System.Collections;
using System.IO;

namespace TerrainAnalysis
{
    public class Rasteriser
    {
        Device d3DDevice = null;
        Thread RenderCycle = null;
        bool DeviceActive = true;
        
        ArrayList worldObjectWireFrameObjects = new ArrayList();
        VertexBuffer normalsBuffer = null;
        VertexBuffer terrainBuffer = null;

        VertexBuffer backTerrainBuffer = null, backNormalBuffer=null;
        private bool backTerrainBufferWaiting = false, backNormalBufferWaiting = false, backObjectBufferWaiting = false;

        private float rotX = 0.0f, rotY = 0.0f;

        private bool usingExternalBuffer = false;
        private int stripsToRender = 0, normalsToRender=0;        

        private bool updatingMemoryBuffers = false, rendering=false;

        Microsoft.DirectX.Vector3 coordinateSystemVector = new Microsoft.DirectX.Vector3(0.0f, 1.0f, 0.0f);
        Microsoft.DirectX.Vector3 cameraOrigin = new Microsoft.DirectX.Vector3(0.0f, 0.0f, 0.0f);        
        Matrix ViewMatrix;

        /// <summary>
        /// Instanciates a Direct3D rendering device.
        /// </summary>
        /// <param name="viewPortControl">
        /// An appropriate form control to draw the scene to
        /// </param>
        public Rasteriser(Control viewPortControl)
        {
            ViewMatrix = Matrix.LookAtLH(this.cameraOrigin, new Vector3(0, 0, 1), new Vector3(0, 1, 0));
            PresentParameters presentParams = new PresentParameters();
            presentParams.Windowed = true;
            presentParams.SwapEffect = SwapEffect.Discard;
            presentParams.EnableAutoDepthStencil = true;
            presentParams.AutoDepthStencilFormat = DepthFormat.D16;

            this.d3DDevice = new Device(0, DeviceType.Hardware, viewPortControl, CreateFlags.SoftwareVertexProcessing, presentParams);
            this.d3DDevice.RenderState.Lighting = false;
            this.d3DDevice.RenderState.UseWBuffer = true;            
            this.d3DDevice.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, 1.0f, 1.0f, 40000.0f);
            this.d3DDevice.VertexFormat = CustomVertex.PositionColored.Format;
            this.d3DDevice.RenderState.CullMode = Cull.None;

            /*
            this.d3DDevice.RenderState.ColorVertex = true;
            this.d3DDevice.RenderState.DiffuseMaterialSource = ColorSource.Color1;
            this.d3DDevice.RenderState.AlphaBlendEnable = true;
            this.d3DDevice.RenderState.SourceBlend = Blend.One;
            this.d3DDevice.RenderState.DestinationBlend = Blend.DestinationAlpha;
            */

            this.AdjustCamera();                           
        }

        private void RenderCycleThread()
        {
            while (this.DeviceActive)
            {
                //Clear the Z-buffer and view port then render the next frame

                //Flip buffers if they are waiting before rendering
                if (backNormalBufferWaiting)
                {
                    this.normalsBuffer = backNormalBuffer;
                    backNormalBufferWaiting = false;
                }
                if (backTerrainBufferWaiting)
                {
                    this.terrainBuffer = backTerrainBuffer;
                    backTerrainBufferWaiting = false;
                }
                if (backObjectBufferWaiting)
                {
                    backObjectBufferWaiting = false;
                }

                if (!updatingMemoryBuffers)
                {
                    this.rendering = true;
                    this.d3DDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, System.Drawing.Color.Black, 1.0f, 0);
                    this.d3DDevice.BeginScene();                   
                    this.d3DDevice.Transform.View = ViewMatrix;

                    DrawVertices();
                    DrawNormals();
                    DrawWorldObjectsWireFrame();

                    this.d3DDevice.EndScene();
                    this.d3DDevice.Present();
                    this.rendering = false;
                }
            }

            //this.d3DDevice.RenderState.
            this.ReleaseBuffers();
            this.d3DDevice.Dispose();

            Thread.CurrentThread.Abort();
        }

        

        private void ReleaseBuffers()
        {
            //Release locked video memory
            
            /*
            int i = 0;
            if (this.renderBuffers != null)
            {
                for (i = 0; i < renderBuffers.Count(); i++)
                    renderBuffers[i].Dispose();
            }
            if(this.singleBuffer!=null)
                this.singleBuffer.Dispose();
            if (this.normalsBuffer != null)
                normalsBuffer.Dispose();*/
        }

        private void DrawNormals()
        {
            if (normalsBuffer != null)
            {
                this.d3DDevice.SetStreamSource(0, normalsBuffer, 0);
                int numLines=0;
                if(this.usingExternalBuffer)
                    numLines = this.normalsToRender;
                else
                    numLines = normalsBuffer.SizeInBytes / (16 * 2);

                if (!updatingMemoryBuffers && this.DeviceActive && numLines>0)
                {   
                    this.d3DDevice.DrawPrimitives(PrimitiveType.LineList, 0, numLines);
                }
            }
        }

        //Renders an array of triangle strips

        private void DrawVertices()
        {
            if (this.terrainBuffer != null)
            {
                this.d3DDevice.SetStreamSource(0, this.terrainBuffer, 0);
                
                int numStrips = 0;
                if (this.usingExternalBuffer)
                    numStrips = this.stripsToRender;
                else
                    numStrips = this.terrainBuffer.SizeInBytes / (16 * 18);

                for (int j = 0; j < numStrips; j++)
                {
                    if (!updatingMemoryBuffers && this.DeviceActive)
                    {
                        this.d3DDevice.DrawPrimitives(PrimitiveType.TriangleStrip, j * 18, 16);
                    }
                }
            }
        }

        private void DrawWorldObjectsWireFrame()
        {
            try
            {
                foreach (LineStripObject LSO in worldObjectWireFrameObjects)
                {
                    if (!LSO.UpdatingWorldObject)
                    {
                        VertexBuffer vb = LSO.BlockBuffer;
                        IndexBuffer ib = LSO.BlockIndices;
                        this.d3DDevice.Indices = ib;
                        this.d3DDevice.SetStreamSource(0, vb, 0);
                        int numLines = ib.SizeInBytes / 4;

                        this.d3DDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, numLines * 2, 0, numLines);                        
                    }
                }
            }
            catch
            {
            }
        }

 

        //Update the view port camera
        private void AdjustCamera()
        {              
            Microsoft.DirectX.Vector3 xAxis, yAxis, zAxis;
            xAxis = new Vector3(ViewMatrix.M11, ViewMatrix.M21, ViewMatrix.M31);
            yAxis = new Vector3(ViewMatrix.M12, ViewMatrix.M22, ViewMatrix.M32);
            zAxis = new Vector3(ViewMatrix.M13, ViewMatrix.M23, ViewMatrix.M33);

            ViewMatrix.M41 = -dotProduct(this.cameraOrigin, xAxis);
            ViewMatrix.M42 = -dotProduct(this.cameraOrigin, yAxis);
            ViewMatrix.M43 = -dotProduct(this.cameraOrigin, zAxis);             
        }

        private void RotateXAxis()
        {
            //Rotate the X-axis first
            Microsoft.DirectX.Vector3 zAxis;
            ViewMatrix.RotateX(this.rotX);
            zAxis = new Vector3(ViewMatrix.M13, ViewMatrix.M23, ViewMatrix.M33);
            float zPrime,xPrime;

            //The Y-axis but only on the Z-X plane
            /*
            zPrime = (zAxis.Z * (float)Math.Cos(rotY)) - (zAxis.X * (float)Math.Sin(rotY));
            xPrime = (zAxis.Z * (float)Math.Sin(rotY)) + (zAxis.X * (float)Math.Cos(rotY));
             */
            zPrime = (zAxis.X * (float)Math.Sin(rotY)) + (zAxis.Z * (float)Math.Cos(rotY));
            xPrime = (zAxis.X * (float)Math.Cos(rotY)) - (zAxis.Z * (float)Math.Sin(rotY));
            zAxis.X = xPrime;
            zAxis.Z = zPrime;

            //Takes the translated Z-axis and use it as the new direction to look at
            ViewMatrix = Matrix.LookAtLH(new Vector3(0, 0, 0), zAxis, new Vector3(0, 1, 0));
        }

        private float dotProduct(Microsoft.DirectX.Vector3 v1, Microsoft.DirectX.Vector3 v2)
        {
            float dX, dY, dZ;
            dX = v1.X * v2.X;
            dY = v1.Y * v2.Y;
            dZ = v1.Z * v2.Z;

            return dX + dY + dZ;
        }

        public Matrix RenderMatrix
        {
            get
            {
                return this.ViewMatrix;
            }
        }

        //Public Methods

        /// <summary>
        /// Instruct the Direct3D device to begin rendering the scene
        /// </summary>
        public void StartRendering()
        {
            RenderCycle = new Thread(new ThreadStart(RenderCycleThread));
            RenderCycle.Start();
        }

        /// <summary>
        /// Gets the reference of the Direct3D hardware device being used
        /// </summary>
        public Device Direct3dDevice
        {
            get
            {
                return this.d3DDevice;
            }
        }

        /// <summary>
        /// Stops rendering and releases video memory
        /// </summary>
        public void Shutdown()
        {
            this.DeviceActive = false;
        }

        /// <summary>
        /// Assign externally created triangle strip buffers for rendering
        /// </summary>
        public VertexBuffer TerrainBuffer
        {
            set
            {
                this.backTerrainBuffer = value;
                backTerrainBufferWaiting = true;
            }
        }

        public int StripsToRender
        {
            set
            {
                this.stripsToRender = value;
            }
        }

        public int NormalsToRender
        {
            set
            {
                this.normalsToRender = value;
            }
        }

        public VertexBuffer NormalList
        {
            set
            {
                this.backNormalBuffer = value;
                this.backNormalBufferWaiting = true;
            }
        }

        /// <summary>
        /// Sets or gets the current horizontal pan for the view port
        /// </summary>
        public float PanX
        {
            set
            {
                Vector3 xAxis = new Vector3(ViewMatrix.M11, ViewMatrix.M21, ViewMatrix.M31);
                xAxis.Multiply(value);
                this.cameraOrigin = this.cameraOrigin + xAxis;
                AdjustCamera();
            }
            get
            {
                return this.cameraOrigin.X;
            }
        }

        public void ResetViewPort(Vector3 location)
        {
            this.rotX = 0;
            this.rotY = 0;
            Vector3 direction = new Vector3(0, 0, 1);
            ViewMatrix = Matrix.LookAtLH(location, location+direction, new Vector3(0, 1, 0));
            this.cameraOrigin.X = location.X;
            this.cameraOrigin.Y = location.Y;
            this.cameraOrigin.Z = location.Z;
        }

        /// <summary>
        /// Sets or gets the current vertical pan for the view port
        /// </summary>
        public float PanY
        {
            set
            {
                this.cameraOrigin.Y = value;
                AdjustCamera();
            }
            get
            {
                return this.cameraOrigin.Y;
            }
        }

        /// <summary>
        /// Sets or gets the current zoom of the view port
        /// </summary>
        public float ZoomZ
        {
            set
            {
                //this.cameraOrigin.Z = value;
                Vector3 zAxis = new Vector3(ViewMatrix.M13, ViewMatrix.M23, ViewMatrix.M33);
                zAxis.Multiply(value);
                this.cameraOrigin = this.cameraOrigin + zAxis;
                AdjustCamera();
            }
            get
            {
                return this.cameraOrigin.Z;
            }
        }

        /// <summary>
        /// Sets the pitch of the view port (rotation of the X axis)
        /// </summary>
        public float Pitch
        {
            set
            {
                this.rotX+= value;
                //RotateYAxis();
                RotateXAxis();
                AdjustCamera();
            }
        }

        /// <summary>
        /// Sets the Yaw of the view port (rotation of the Y axis)
        /// </summary>
        public float Yaw
        {
            set
            {
                this.rotY += value;
                RotateXAxis();
                AdjustCamera();
            }
        }

        /// <summary>
        /// Sets the Roll of the view port (rotation of the Z axis)
        /// </summary>
        public float Roll
        {
            set
            {
                AdjustCamera();
            }
        }

        public void AddWorldObject_WireFrame(LineStripObject LSO)
        {
            worldObjectWireFrameObjects.Add(LSO);
        }

        public Microsoft.DirectX.Vector3 CameraPosition
        {
            get
            {
                return this.cameraOrigin;
            }
        }

        public bool ExternalBufferUpdate
        {
            set
            {
                this.updatingMemoryBuffers = value;
                while (this.rendering) ;                
            }
        }

        public void LookAtFrom(Microsoft.DirectX.Vector3 from, Microsoft.DirectX.Vector3 at)
        {
            this.ViewMatrix = Matrix.LookAtLH(from, at, new Vector3(0, 1, 0));
        }
    }
}
