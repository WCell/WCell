using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Threading;
using System.IO;
using WCell.Collision;
using System.Collections;
using TerrainAnalysis.ThreadSafeControls;
using TerrainAnalysis.TerrainObjects;

namespace TerrainAnalysis
{
    public partial class TerrainViewer : Form
    {
        private Rasteriser D3dDevice = null;
        //ArrayList MapTiles = new ArrayList();
        private Thread BackBuffering = null;
        private bool ApplicationDone = false;

        private static int dynamicChunkSize = 18;
        private static float dynamicRadius = (float)dynamicChunkSize * TerrainConstants.HighResChunk.ChunkSize;

        //Used for multi-buffering frames to increase interlocking performance
        private static int numBuffers = 3;
        private int bufferNumber = 0;
        private VertexBuffer[] dynamicBuffers = new VertexBuffer[3];
        private VertexBuffer[] normalBuffers = new VertexBuffer[3];
        private Microsoft.DirectX.Vector2 previousPosition = new Microsoft.DirectX.Vector2(0, 0);
        Microsoft.DirectX.Vector3 min = new Microsoft.DirectX.Vector3(999999, 999999, 999999), max = new Microsoft.DirectX.Vector3(-999999, -999999, -999999);

        //Prevents overlapping updates from key press repeating
        private bool performingBufferUpdate = false, pushedMovementKey=false, loadingNewZone=false;       

        //private TileVertex[,][][] dynamicTriangleStrips = new TileVertex[dynamicChunkSize, dynamicChunkSize][][];

        private int mouseStartX=0, mouseStartY=0;
        private float rotX = 0.0f, rotY = 0.0f, viewOffSetY=0.0f, prevRotX=0.0f, prevRotY=0.0f;

        WorldZone currentWorld = null;
        ArrayList CurrentChunks = null;
        MapTileChunk currentSelectedChunk = null;
        LineStripObject currentSelectedObject = null, currentHoverObject=null;

        ArrayList WorldWireFrameUnits = new ArrayList(), WorldWireFrameObsticles = new ArrayList();

        private ViewPortMode currentViewPortMode = ViewPortMode.Terrain;
        private ObjectMode currentObjectMode = ObjectMode.None;

        private enum ViewPortMode
        {
            Terrain,
            Object
        };

        private enum ObjectMode
        {
            None,
            Move,
            Rotate
        };

        private Microsoft.DirectX.Vector3 CameraPosition;

        public TerrainViewer()
        {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);            
            this.LoadMapFiles();

            this.viewRadiusSlider.Maximum = dynamicChunkSize;
            this.viewRadiusSlider.Minimum = 1;
            this.viewRadiusSlider.Value = dynamicChunkSize / 2;

            SetupWorld();
            BackBuffering = new Thread(new ThreadStart(BackBufferThread));
            BackBuffering.Start();
        }

        private int ChunkCalculator()
        {
            int chunks = 1;

            /*
            //using a formula I created where x = the radius in units of chunks. 3x^2 + x + 1
            int i = 0, exponent = 2;
            for (i = 0; i < exponent; i++)
            {
                chunks *= dynamicChunkSize;
            }
            chunks *= 3;
            chunks += dynamicChunkSize + 1;*/

            //Going to use a circular method instead
            chunks = (int)((float)(dynamicChunkSize+1) * (float)(dynamicChunkSize+1) * (float)Math.PI);

            return chunks;
        }

        private void SetupWorld()
        {
            if (File.Exists("heightstuff.txl"))
                File.Delete("heightstuff.txl");

            
            //Create the DirectX rendering enginer
            D3dDevice = new Rasteriser(this.pictureBox1);
            int numChunks = ChunkCalculator();
            //this.maximumChunkLabel.Text = "Maximum Chunks in Radius: " + numChunks.ToString();

            int bufferSize = 18 * 16 * numChunks;
            int normalsSize = numChunks * ((8*8)+(9*9)) * 2;
            for (int i = 0; i < numBuffers; i++)
            {
                this.dynamicBuffers[i] = new VertexBuffer(typeof(CustomVertex.PositionColored), bufferSize, this.D3dDevice.Direct3dDevice,0, CustomVertex.PositionColored.Format, Pool.Default);
                this.normalBuffers[i] = new VertexBuffer(typeof(CustomVertex.PositionColored), normalsSize, this.D3dDevice.Direct3dDevice, 0, CustomVertex.PositionColored.Format, Pool.Default);
            }
            
            

            D3dDevice.StartRendering();
            D3dDevice.Pitch = (float)Math.PI * -0.5f;
            D3dDevice.ZoomZ = -7000.0f;                       
        }

        private void DestroyWorld()
        {
            this.D3dDevice.Shutdown();
            for (int i = 0; i < numBuffers; i++)
            {
                normalBuffers[i].Dispose();
                dynamicBuffers[i].Dispose();
            }
        }

        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Shudown the rendering engine and release all locked memory in the video card used for rendering
            this.ApplicationDone = true;
            this.D3dDevice.Shutdown();
            foreach (VertexBuffer vb in dynamicBuffers)
            {
                try
                {
                    vb.Dispose();
                }
                catch
                {
                }
            }
            foreach (VertexBuffer nb in normalBuffers)
            {
                try
                {
                    nb.Dispose();
                }
                catch
                {
                }
            }
        }

        //UI Functions

        //Puts a list of ADT files from the /maps directory in a list box to chose from
        private void LoadMapFiles()
        {
			string[] mapFiles = Directory.GetFileSystemEntries("World\\Maps", "*");
            foreach (string mFile in mapFiles)
            {
                string[] split = mFile.Split(new char[]{'\\'});
                mapListBox.Items.Add(split[split.Count()-1]);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private int currentMouseX = 0, currentMouseY = 0;
        private bool MouseIsDown = false, leftMouseButton=false;

        //Used for marking a vector that determines the rotation angle in the rendering port
        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (!MouseIsDown)
            {
                if (e.Button == MouseButtons.Left)
                    leftMouseButton = true;
                else
                {
                    leftMouseButton = false; 
                }

                this.mouseStartX = e.X;
                this.mouseStartY = e.Y;
                this.MouseIsDown = true;
            }
        }

        //Performs an incremental rotation so that the view port rotates from it's current axis offsets rather than from 0,0,0
        private void DoRotation()
        {
            //delta X and Y of the mouse client coordinates relative to the rendering port container
            int mickeysX, mickeysY;
            
            mickeysX = currentMouseX - this.mouseStartX;
            mickeysY = currentMouseY - this.mouseStartY;

            float newAngleY = ((float)mickeysX / (float)pictureBox1.Width) * 2.0f;
            float newAngleX = ((float)mickeysY / (float)pictureBox1.Height) * 2.0f;

            //Rotation angles calculated since the mouse was pressed
            this.prevRotX = -((float)Math.PI * newAngleX)%((float)Math.PI*2.0f);
            this.prevRotY = -((float)Math.PI * newAngleY) % ((float)Math.PI*2.0f);

            //this.rotationLabel.Text = ((float)(this.rotX + this.prevRotX)).ToString() + "," + ((float)(this.rotY + this.prevRotY)).ToString();

            //Update the Direct3D view port rotations

            if (this.currentViewPortMode == ViewPortMode.Terrain)
            {
                if (leftMouseButton)
                {
                    this.D3dDevice.Pitch = this.rotX + this.prevRotX;
                    this.D3dDevice.Yaw = this.rotY + this.prevRotY;
                    navigatorBox.Invalidate();
                }
                else
                {

                }
            }
            else
            {
                if (leftMouseButton)
                {
                    //this.currentSelectedObject.RotateXY(prevRotX, prevRotY);
                }
                else
                {
                    //this.currentSelectedObject.RotateXY(prevRotX, 0);
                }
            }
        }

        //Fix the new axes rotation values after lifting the mouse button
        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            this.MouseIsDown = false; 
            this.prevRotX = 0.0f;
            this.prevRotY = 0.0f;
            
        }

        private void FindIntersect(int x, int y)
        {
            Microsoft.DirectX.Vector3 nearSource = new Microsoft.DirectX.Vector3((float)x, (float)y, 0.0f);
            Microsoft.DirectX.Vector3 farSource = new Microsoft.DirectX.Vector3((float)x, (float)y, 1.0f);

            Microsoft.DirectX.Matrix world = this.D3dDevice.Direct3dDevice.Transform.World;
            Microsoft.DirectX.Matrix projection = this.D3dDevice.Direct3dDevice.Transform.Projection;
            Microsoft.DirectX.Matrix view = this.D3dDevice.Direct3dDevice.Transform.View;

            Microsoft.DirectX.Vector3 nearPoint = Microsoft.DirectX.Vector3.Unproject(nearSource,this.D3dDevice.Direct3dDevice.Viewport, projection, view, world);
            Microsoft.DirectX.Vector3 farPoint = Microsoft.DirectX.Vector3.Unproject(farSource, this.D3dDevice.Direct3dDevice.Viewport, projection, view, world);
            Microsoft.DirectX.Vector3 projectionVector = farPoint - nearPoint;

            if (this.currentViewPortMode == ViewPortMode.Terrain)
            {
                MapTileChunk closestValidChunk = null;
                float smallestDistance = float.MaxValue;

                if (this.CurrentChunks != null)
                {
                    foreach (MapTileChunk mtChunk in this.CurrentChunks)
                    {
                        //Microsoft.DirectX.Plane chunkPlane = mtChunk.ChunkPlane();
                        //Microsoft.DirectX.Vector3 intersection = Microsoft.DirectX.Plane.IntersectLine(chunkPlane, nearPoint, nearPoint + projectionVector);
                        float distance;
                        if ((distance = mtChunk.IntersectsChunk(nearPoint, nearPoint + projectionVector)) < smallestDistance)
                        {
                            smallestDistance = distance;
                            if (closestValidChunk != null)
                                closestValidChunk.Selected = false;
                            closestValidChunk = mtChunk;
                        }
                        else
                            mtChunk.Selected = false;
                    }
                }

                if (closestValidChunk != null)
                {
                    if (!closestValidChunk.Selected)
                    {
                        closestValidChunk.Selected = true;
                        this.bufferNumber++;
                        if (bufferNumber == numBuffers)
                            bufferNumber = 0;
                        this.UpdateMemoryBuffer(dynamicBuffers[bufferNumber], normalBuffers[bufferNumber], this.CurrentChunks);
                    }
                }
                this.currentSelectedChunk = closestValidChunk;
            }
            else
            {

            }
        }

        private void FindObjectIntersect(int x, int y)
        {
            Microsoft.DirectX.Vector3 nearSource = new Microsoft.DirectX.Vector3((float)x, (float)y, 0.0f);
            Microsoft.DirectX.Vector3 farSource = new Microsoft.DirectX.Vector3((float)x, (float)y, 1.0f);

            Microsoft.DirectX.Matrix world = this.D3dDevice.Direct3dDevice.Transform.World;
            Microsoft.DirectX.Matrix projection = this.D3dDevice.Direct3dDevice.Transform.Projection;
            Microsoft.DirectX.Matrix view = this.D3dDevice.Direct3dDevice.Transform.View;

            Microsoft.DirectX.Vector3 nearPoint = Microsoft.DirectX.Vector3.Unproject(nearSource, this.D3dDevice.Direct3dDevice.Viewport, projection, view, world);
            Microsoft.DirectX.Vector3 farPoint = Microsoft.DirectX.Vector3.Unproject(farSource, this.D3dDevice.Direct3dDevice.Viewport, projection, view, world);
            Microsoft.DirectX.Vector3 projectionVector = farPoint - nearPoint;

            float closestObjectDistance = float.MaxValue;
            LineStripObject closestObject = null;

            foreach (LineStripObject LSO in this.WorldWireFrameUnits)
            {
                float planeDistance = LSO.IntersectsObject(nearPoint, projectionVector);
                if (planeDistance < closestObjectDistance)
                {
                    closestObjectDistance = planeDistance;
                    if (closestObject != null)
                        closestObject.Hover = false;
                    closestObject = LSO;
                    LSO.Hover = true;
                    //throw new Exception("MOTHER FUCKERS");
                }
                else
                    LSO.Hover = false;
            }

            this.currentHoverObject = closestObject;
            if (this.currentHoverObject != null)
                this.selectObjectToolStripMenuItem.Enabled = true;
            else
                this.selectObjectToolStripMenuItem.Enabled = false;
        }

        //If the mouse button is down and the mouse moves, update the view port rotations
        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            //label1.Text = e.X.ToString() + "," + e.Y.ToString();
            currentMouseX = e.X;
            currentMouseY = e.Y;
            if (this.MouseIsDown)
            {
                if (this.currentViewPortMode == ViewPortMode.Terrain)
                {
                    this.DoRotation();
                    this.mouseStartX = e.X;
                    this.mouseStartY = e.Y;
                }
                else if (this.currentViewPortMode == ViewPortMode.Object)
                {
                    if(this.currentObjectMode == ObjectMode.Rotate)
                    {
                        if (this.currentSelectedObject != null)
                        {
                            DoRotation();
                        }
                    }
                }

            }
            else
            {
                if (this.currentViewPortMode == ViewPortMode.Terrain)
                    FindIntersect(e.X, e.Y);
                else if (this.currentViewPortMode == ViewPortMode.Object)
                {
                    if(this.currentObjectMode== ObjectMode.None)
                        FindObjectIntersect(e.X, e.Y); 
                }
            }
        }

        //Used for loading an ADT file selected from the list
        private delegate void MapLoaderThreadDelegate(string fileName, int tileIndex);

        private void MapFile_Changed(object sender, EventArgs e)
        {
            if (mapListBox.SelectedIndex != -1)
            {                
                mapListBox.Enabled = false; 
                tileListBox.Items.Clear();


                min = new Microsoft.DirectX.Vector3(999999, 999999, 999999);
                max = new Microsoft.DirectX.Vector3(-999999, -999999, -999999);

                Thread tempThread = new Thread(new ParameterizedThreadStart(MapLoader));
                tempThread.Start(mapListBox.SelectedItem);
            }
        }

        //A scalar that defines the degree in which the view port pans or rotates.
        private static float viewPortScaleDelta = 50.0f;

        private void upButton_Click(object sender, EventArgs e)
        {
            this.viewOffSetY += viewPortScaleDelta;
            this.D3dDevice.PanY = this.viewOffSetY;       
        }

        private void downButton_Click(object sender, EventArgs e)
        {
            this.viewOffSetY -= viewPortScaleDelta;
            this.D3dDevice.PanY = this.viewOffSetY;         
        }         

        private void Key_Down(object sender, KeyEventArgs e)
        {
            bool validKeyPressed = true;
            if (e.KeyCode == Keys.W)
                this.D3dDevice.ZoomZ = 4.0f;
            else if (e.KeyCode == Keys.S)
                this.D3dDevice.ZoomZ = -4.0f;
            else if (e.KeyCode == Keys.A)
                this.D3dDevice.PanX = -2.0f;
            else if (e.KeyCode == Keys.D)
                this.D3dDevice.PanX = 2.0f;
            else
                validKeyPressed = false;

            if (e.KeyCode == Keys.Escape)
                this.currentObjectMode = ObjectMode.None;
            else if (e.KeyCode == Keys.Left)
            {
                if (this.currentSelectedObject != null)
                    this.currentSelectedObject.rotateAboutNormal(-(float)Math.PI / 20.0f);

            }
            else if (e.KeyCode == Keys.Right)
            {
                if (this.currentSelectedObject != null)
                    this.currentSelectedObject.rotateAboutNormal((float)Math.PI / 20.0f);
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (this.currentSelectedObject != null)
                {
                    this.currentSelectedObject.MoveObject(0.2f);
                    Microsoft.DirectX.Vector3 objectPosition = this.currentSelectedObject.ObjectPosition;
                    Microsoft.DirectX.Vector3 objectDirection = this.currentSelectedObject.ObjectDirection;

                    TerrainCorrection TC = this.currentWorld.NearestNormal(objectPosition, objectDirection);
                    this.currentSelectedObject.SnapToNormal(TC.newNormal);
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (this.currentSelectedObject != null)
                {
                    this.currentSelectedObject.MoveObject(-0.2f);
                    Microsoft.DirectX.Vector3 objectPosition = this.currentSelectedObject.ObjectPosition;
                    Microsoft.DirectX.Vector3 objectDirection = this.currentSelectedObject.ObjectDirection;

                    TerrainCorrection TC = this.currentWorld.NearestNormal(objectPosition, objectDirection);
                    this.currentSelectedObject.SnapToNormal(TC.newNormal);
                }
            }

            if (validKeyPressed)
            {
                Microsoft.DirectX.Vector2 cameraPos = new Microsoft.DirectX.Vector2(this.CameraPosition.X, this.CameraPosition.Z);
                string currentTileText = "Current Tile: " + ((float)(cameraPos.X / TerrainConstants.Tile.TileSize)).ToString() + "," + ((float)(cameraPos.Y / TerrainConstants.Tile.TileSize)).ToString();
                ThreadSafe.UpdateItemText(this.currentTileLabel, currentTileText);
                pushedMovementKey = true;                               
            }
        }

        private bool TravelledEnough(Microsoft.DirectX.Vector2 p1, Microsoft.DirectX.Vector2 p2, float distance)
        {
            Microsoft.DirectX.Vector2 delta = p2 - p1;
            if (delta.Length() > distance)
                return true;
            else
                return false;
        }

        private void UpdateMemoryBuffer(VertexBuffer vb, VertexBuffer nb, ArrayList chunkList)
        {
            //ArrayList chunkList = GetChunksInRadius(dynamicRadius);
            
            this.CurrentChunks = chunkList;
            int numStrips = 0, numNormals=0, byteOffset=0, normalOffset=0;

           
            if (chunkList != null)
            {
                ThreadSafe.UpdateItemText(this.chunksInRadiusLabel, "Chunks in Radius: " + chunkList.Count.ToString());

                foreach (MapTileChunk mtChunk in chunkList)
                {
                    if (terrainCheck.Checked)
                    {
                        CustomVertex.PositionColored[] tempVerts = mtChunk.TriangleStrip;
                        int stripCount = tempVerts.Count() / 18;
                        vb.SetData(tempVerts, byteOffset, 0);
                        byteOffset += (16 * 18 * stripCount);
                        numStrips += stripCount;

                    }

                    if (normalsCheck.Checked)
                    {
                        CustomVertex.PositionColored[] tempNormals = mtChunk.NormalList;
                        int normalCount = tempNormals.Count() / 2;
                        nb.SetData(tempNormals, normalOffset, 0);
                        normalOffset += normalCount * 2 * 16;
                        numNormals += normalCount;

                    }
                }
                if (terrainCheck.Checked)
                {
                    this.D3dDevice.TerrainBuffer = vb;
                    this.D3dDevice.StripsToRender = numStrips;
                }
                if (normalsCheck.Checked)
                {
                    this.D3dDevice.NormalList = nb;
                    this.D3dDevice.NormalsToRender = numNormals;
                }
            }
        }

        private ArrayList GetChunksInRadius(float radius)
        {
            Microsoft.DirectX.Vector2 cameraPos = new Microsoft.DirectX.Vector2(this.CameraPosition.X, this.CameraPosition.Z);

            if (currentWorld != null)
            {

                return currentWorld.GetChunksInRadius(cameraPos, radius);
            }
            return null;
        }

        private void terrainCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (terrainCheck.Checked)
            {
                this.bufferNumber++;
                if (bufferNumber == numBuffers)
                    bufferNumber = 0;
                ArrayList chunkList = GetChunksInRadius(ThreadSafe.GetSliderValue(this.viewRadiusSlider) * TerrainConstants.HighResChunk.ChunkSize);
                this.UpdateMemoryBuffer(dynamicBuffers[bufferNumber], normalBuffers[bufferNumber], chunkList);
            }
            else
            {
                this.D3dDevice.StripsToRender = 0;
                this.D3dDevice.TerrainBuffer = null;
            }
        }

        private void normalsCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (normalsCheck.Checked)
            {
                this.bufferNumber++;
                if (bufferNumber == numBuffers)
                    bufferNumber = 0;

                ArrayList chunkList = GetChunksInRadius(ThreadSafe.GetSliderValue(this.viewRadiusSlider) * TerrainConstants.HighResChunk.ChunkSize);
                this.UpdateMemoryBuffer(dynamicBuffers[bufferNumber], normalBuffers[bufferNumber], chunkList);
            }
            else
            {
                this.D3dDevice.NormalsToRender = 0;
                this.D3dDevice.NormalList = null;
            }
        }

        
                  
        private void MapLoader(object zoneName)//, int tileIndex)
        {
            this.loadingNewZone = true;
            WorldZone worldZone = new WorldZone((string)zoneName, false);
            worldZone.OnLoadEvent += new WorldZone.ZoneLoadEventHandler(worldZone_OnLoadEvent);
            worldZone.OnZoneTypeEvent += new WorldZone.ZoneTypeEventhandler(worldZone_OnZoneTypeEvent);
            worldZone.ProcessZone();
            Thread.CurrentThread.Abort();
        }

        void worldZone_OnZoneTypeEvent(WorldZone.ZoneType zoneType)
        {
            if (zoneType == WorldZone.ZoneType.NoTerrain)
            {
                MessageBox.Show("The zone you have selected contains no terrain data");
                ThreadSafe.UpdateControlVisibility(loadingLabel, false);
                ThreadSafe.UpdateControlEnabled(mapListBox, true);
            }
            else
            {
                ThreadSafe.UpdateItemText(this.loadingLabel, "Loading Terrain Data...");
                ThreadSafe.UpdateControlVisibility(this.loadingLabel, true);
            }
        }

        void worldZone_OnLoadEvent(WorldZone.LoadEventType loadEvent, object eventObject)
        {
            if (loadEvent == WorldZone.LoadEventType.TileLoaded)
            {
                ThreadSafe.AddContainerItem(tileListBox, eventObject);
            }
            else if (loadEvent == WorldZone.LoadEventType.ZoneLoaded)
            {
                ThreadSafe.UpdateItemText(this.loadingLabel, "Linking Zone Chunks...");

            }
            else if (loadEvent == WorldZone.LoadEventType.ChunksLinked)
            {
                ThreadSafe.UpdateControlVisibility(this.loadingLabel, false);
                this.currentWorld = (WorldZone)eventObject;

                this.CameraPosition.X = currentWorld.ZoneCenter.X;
                this.CameraPosition.Z = currentWorld.ZoneCenter.Y;
                this.CameraPosition.Y = 300;

                this.D3dDevice.ResetViewPort(this.CameraPosition);
                this.CameraPosition = this.D3dDevice.CameraPosition;
                this.previousPosition = new Microsoft.DirectX.Vector2(CameraPosition.X, CameraPosition.Y);
                D3dDevice.Pitch = (float)Math.PI * -0.5f;

                this.bufferNumber++;
                if (bufferNumber == numBuffers)
                    bufferNumber = 0;

                ArrayList chunkList = GetChunksInRadius(ThreadSafe.GetSliderValue(this.viewRadiusSlider) * TerrainConstants.HighResChunk.ChunkSize);
                this.UpdateMemoryBuffer(dynamicBuffers[bufferNumber], normalBuffers[bufferNumber], chunkList);
                this.loadingNewZone = false;
                ThreadSafe.UpdateControlEnabled(mapListBox, true);
                this.miniMapBox.Invalidate();
            }
        }

  

        private void BackBufferThread()
        {
            while (!ApplicationDone)
            {
                if (pushedMovementKey && !loadingNewZone)
                {
                    CameraPosition = this.D3dDevice.CameraPosition;
                    var camPosition2D = new Vector2(CameraPosition.X, CameraPosition.Y);
                    this.miniMapBox.Invalidate();
                    ThreadSafe.UpdateItemText(this.sliderRadiusLabel, "View Radius: " + ThreadSafe.GetSliderValue(this.viewRadiusSlider).ToString() + " chunks");
                    if (!performingBufferUpdate)
                    {
                        performingBufferUpdate = true;
                        if (TravelledEnough(this.previousPosition, camPosition2D, TerrainConstants.HighResChunk.ChunkSize))//dynamicRadius * 0.25f))
                        {
                            this.bufferNumber++;
                            if (bufferNumber == numBuffers)
                                bufferNumber = 0;

                            var chunkList = GetChunksInRadius(ThreadSafe.GetSliderValue(this.viewRadiusSlider) * TerrainConstants.HighResChunk.ChunkSize);
                            this.UpdateMemoryBuffer(dynamicBuffers[bufferNumber], normalBuffers[bufferNumber], chunkList);

                            this.previousPosition = camPosition2D;

                        }
                        performingBufferUpdate = false;
                    }

                    pushedMovementKey = false;
                }
            }

            Thread.CurrentThread.Abort();
        }

        private void MiniMap_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.FillRectangle(Brushes.Black, e.ClipRectangle);

            if (this.currentWorld != null)
            {                
                MapTile[,] worldTiles = currentWorld.WorldGrid;
                for (int i = 0; i < 64; i++)
                {
                    for (int j = 0; j < 64; j++)
                    {
                        if (worldTiles[i, j] != null)
                        {
                            System.Drawing.Rectangle tileRect = new System.Drawing.Rectangle(i * 5, j * 5, 5, 5);
                            g.FillRectangle(Brushes.Gray, tileRect);
                        }
                    }
                }
            }
            float x = this.CameraPosition.X, z = this.CameraPosition.Z;
            int tileX = (int)(x / TerrainConstants.Tile.TileSize);
            int tileZ = (int)(z / TerrainConstants.Tile.TileSize);
            x -= tileX*TerrainConstants.Tile.TileSize;
            z -= tileZ*TerrainConstants.Tile.TileSize;
            x /= TerrainConstants.HighResChunk.ChunkSize;
            z /= TerrainConstants.HighResChunk.ChunkSize;
            x *= 5.0f/16.0f;
            z *= 5.0f/16.0f;
            tileX *= 5;
            tileZ *= 5;

            g.FillRectangle(Brushes.Red, new System.Drawing.Rectangle(tileX + (int)x, tileZ + (int)z, 2, 2));
        }

        private void Navigation_Paint(object sender, PaintEventArgs e)
        {
            float x, z;
            Microsoft.DirectX.Matrix viewPort = this.D3dDevice.RenderMatrix;
            x = viewPort.M13;
            z = viewPort.M33;

            Graphics g = e.Graphics;
            g.DrawLine(Pens.Black, new PointF(50.0f, 50.0f), new PointF(50.0f + (x * 50.0f), 50.0f + (z * 50.0f)));
            g.DrawEllipse(Pens.Black,0,0,99,99);
            //g.DrawLine(Pens.Black, 0.0f, 0.0f, zAxis.X, zAxis.Y);
        }

        private void MiniMap_Click(object sender, EventArgs e)
        {

        }

        private void MiniMap_MouseClick(object sender, MouseEventArgs e)
        {
            int tileRow, tileColumn;
            tileRow = (int)(e.Y / 5.0f);
            tileColumn = (int)(e.X / 5.0f);

            Microsoft.DirectX.Vector3 newCam = new Microsoft.DirectX.Vector3(tileColumn * TerrainConstants.Tile.TileSize, CameraPosition.Y, tileRow * TerrainConstants.Tile.TileSize);
            this.D3dDevice.ResetViewPort(newCam);
            this.CameraPosition = this.D3dDevice.CameraPosition;

            this.pushedMovementKey = true;
        }

        private void viewRadiusSlider_Scroll(object sender, EventArgs e)
        {
            this.previousPosition = new Microsoft.DirectX.Vector2(0, 0);
            this.pushedMovementKey = true;
        }

        private void addObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
 
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void terrainModeMenuItem_Click(object sender, EventArgs e)
        {
            if (terrainModeMenuItem.Checked)
            {
                objectModeMenuItem.Checked = false;
                this.currentViewPortMode = ViewPortMode.Terrain;
                pictureBox1.ContextMenuStrip = this.terrainModeContextMenu;
            }
        }

        private void objectModeMenuItem_Click(object sender, EventArgs e)
        {
            if (objectModeMenuItem.Checked)
            {
                terrainModeMenuItem.Checked = false;
                this.currentViewPortMode = ViewPortMode.Object;
                this.pictureBox1.ContextMenuStrip = objectModeContextMenu;
            }                           
        }

        private void selectObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.currentSelectedObject != null)
                this.currentSelectedObject.Selected = false;
            this.currentHoverObject.Selected = true;
            this.currentSelectedObject = this.currentHoverObject;
        }

        private void moveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.currentObjectMode = ObjectMode.Move;
        }

        private void rotateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.currentObjectMode = ObjectMode.Rotate;
        }

        private void unitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.currentSelectedChunk != null && this.currentViewPortMode == ViewPortMode.Terrain)
            {
                Microsoft.DirectX.Vector3 centerPoint = currentSelectedChunk.ChunkCenter();
                Microsoft.DirectX.Vector3 centerNormal = currentSelectedChunk.ChunkCenterNormal();
                LineStripObject LSO = new LineStripObject(2, 2, 2, centerPoint, new Microsoft.DirectX.Vector3(0,1,0), ((float)Math.PI)/4.0f, this.D3dDevice.Direct3dDevice);
                LSO.SnapToNormal(centerNormal);
                this.D3dDevice.AddWorldObject_WireFrame(LSO);
                this.WorldWireFrameUnits.Add(LSO);
            }
        }

        private void applyTestNormalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.currentSelectedObject != null)
            {
                Microsoft.DirectX.Vector3 testNormal = new Microsoft.DirectX.Vector3(float.Parse(normX.Text), float.Parse(normY.Text), float.Parse(normZ.Text));
                testNormal.Normalize();
                currentSelectedObject.SnapToNormal(testNormal);
            }
        }

        private void unitMovementTimer_Tick(object sender, EventArgs e)
        {
            if (this.currentSelectedObject != null && this.currentObjectMode == ObjectMode.Move)
            {
                this.currentSelectedObject.MoveObject(0.2f);
                Microsoft.DirectX.Vector3 objectPosition = this.currentSelectedObject.ObjectPosition;
                Microsoft.DirectX.Vector3 objectDirection = this.currentSelectedObject.ObjectDirection;

                TerrainCorrection TC = this.currentWorld.NearestNormal(objectPosition, objectDirection);
                this.currentSelectedObject.SnapToNormal(TC.newNormal);
                this.currentSelectedObject.CorrectObjectPosition(TC.surfaceCorrection, 0.1f);

                this.D3dDevice.LookAtFrom(objectPosition - ((new Microsoft.DirectX.Vector3(objectDirection.X,0,objectDirection.Z)) * 20) + (new Microsoft.DirectX.Vector3(0, 20, 0)), objectPosition);
            }
        }
    }
}
