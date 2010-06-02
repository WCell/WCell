using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MPQNav.MPQ;
using MPQNav.MPQ.ADT;
using MPQNav.MPQ.M2;
using MPQNav.MPQ.WMO;
using TerrainDisplay;

namespace MPQNav
{
    /// <summary>
    /// Struct outlining our custom vertex
    /// </summary>
    public struct VertexPositionNormalColored
    {
        /// <summary>
        /// Vector3 Position for this vertex
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Color of this vertex
        /// </summary>
        public Color Color;
        /// <summary>
        /// Normal vector for this Vertex
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// Constructor for a VertexPositionNormalColored
        /// </summary>
        /// <param name="position">Vector3 Position of the vertex</param>
        /// <param name="color">Color of the vertex</param>
        /// <param name="normal">Normal vector of the vertex</param>
        public VertexPositionNormalColored(Vector3 position, Color color, Vector3 normal)
        {
            Position = position;
            Color = color;
            Normal = normal;
        }
        /// <summary>
        /// Memory size for a VertexPositionNormalColored
        /// </summary>
        public static int SizeInBytes = 7 * 4;
        /// <summary>
        /// VertexElement array (used for rendering)
        /// </summary>
        public static readonly VertexElement[] VertexElements = new[] {
            new VertexElement( 0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0 ),
            new VertexElement( 0, sizeof(float) * 3, VertexElementFormat.Color, VertexElementMethod.Default, VertexElementUsage.Color, 0 ),
            new VertexElement( 0, sizeof(float) * 4, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0 ),
        };
    }

    /// <summary>
    /// This is the class that controls the entire game.
    /// </summary>
    public class Game1 : Game
    {
        const float RotationSpeed = 1f / 60f;
        const float ForwardSpeed = 50f / 60f;
        const float ViewAngle = MathHelper.PiOver4;
        const float NearClip = 1.0f;
        const float FarClip = 2000.0f;

        // XNA uses this variable for graphics information
        private readonly GraphicsDeviceManager _graphics;
        // We use this to declare our verticies
        private VertexDeclaration _vertexDeclaration;
        // Another set of XNA Variables
        private Matrix _view;
        private Matrix _proj;

        BasicEffect _basicEffect;
        
        //private readonly ADTManager _manager;
        private readonly TerrainManager _terrainManager;
        /// <summary>
        /// Console used to execute commands while the game is running.
        /// </summary>
        public MpqConsole Console;

        // Camera Stuff
        Vector3 _avatarPosition = new Vector3(-100, 100, -100);
        Vector3 avatarHeadOffset = new Vector3(0, 10, 0);
        float _avatarYaw;
        Vector3 cameraReference = new Vector3(0, 0, 10);
        Vector3 _thirdPersonReference = new Vector3(0, 20, -20);
        
        SpriteBatch _spriteBatch;
        SpriteFont _spriteFont;

        /// <summary>
        /// Constructor for the game.
        /// </summary>
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            var settingsReader = new System.Configuration.AppSettingsReader();

            var mpqPath = (string)settingsReader.GetValue("mpqPath", typeof(string));
            //String mpqFile = "c:\\Program Files\\World of Warcraft\\Data\\common.MPQ";
            var defaultContinent = (string)settingsReader.GetValue("defaultContinent", typeof(string));
            var defaultMapX = (int)settingsReader.GetValue("defaultMapX", typeof(int));
            var defaultMapY = (int)settingsReader.GetValue("defaultMapY", typeof(int));
            var continent = (ContinentType) Enum.Parse(typeof (ContinentType), defaultContinent, true);


            _terrainManager = new TerrainManager(mpqPath, continent, 0);
            _terrainManager.ADTManager.LoadTile(defaultMapX, defaultMapY);
            //_terrainManager.ADTManager.LoadTile(defaultMapX - 1, defaultMapY);

            //var tree = new OctTree(_manager.GetRenderingVerticies(), _manager.GetRenderingIndices(), defaultMapX, defaultMapY);
            //tree = new KDTree<IShape>(defaultMapX, defaultMapY, 500);
            //for (var i = 0; i < _renderIndices.Length; i++)
            //{
            //    var triangle = new Triangle(_renderVertices[_renderIndices[i++]].Position, 
            //                                _renderVertices[_renderIndices[i++]].Position,
            //                                _renderVertices[_renderIndices[i]].Position);
            //    var min = triangle.Min;
            //    var max = triangle.Max;
            //    tree.Put(min, max, triangle);
            //}

            //_avatarPosition = _terrainManager.ADTManager.MapTiles[0].Vertices[0].Position;
            _avatarPosition = new Vector3(TerrainConstants.CenterPoint - (defaultMapX + 1) * TerrainConstants.TileSize, TerrainConstants.CenterPoint - (defaultMapY + 1)* TerrainConstants.TileSize, 100.0f);
            //_avatarPosition = new Vector3(0.0f);
            PositionUtil.TransformWoWCoordsToXNACoords(ref _avatarPosition);
            _avatarYaw = 90;
        }

        /// <summary>
        /// Executes a console command.
        /// </summary>
        private void DoCommand()
        {
            //if (!Console.Command.commandCode.Equals(MpqConsole.ConsoleCommandStruct.CommandCode.Load)) return;

            //var command = Console.Command.commandData.Split(' ');
            //var mapX = int.Parse(command[0]);
            //var mapY = int.Parse(command[1]);
            //Console.WriteLine("Loading map:" + mapX + " " + mapY);
            //_terrainManager.ADTManager.LoadTile(mapX, mapY);
        }

        /// <summary>
        /// Loads the content needed for the game.
        /// </summary>
        protected override void LoadContent()
        {
            Keyboard.GetState();
            //var thaFont = Content.Load<SpriteFont>("Courier New");
            //_spriteFont = thaFont;
            //Console = new MpqConsole(this, thaFont);            
            //Console.MyEvent += DoCommand;

            //_spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Components.Add(new AxisRenderer(this));
            Components.Add(new ADTRenderer(this, _terrainManager.ADTManager));
            Components.Add(new M2Renderer(this, _terrainManager.M2Manager));
            Components.Add(new WMORenderer(this, _terrainManager.WMOManager));

            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.PreferredBackBufferHeight = 768;
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();
            InitializeEffect();

            base.Initialize();
        }

        private void InitializeEffect()
        {
            _basicEffect = new BasicEffect(_graphics.GraphicsDevice, null)
            {
                VertexColorEnabled = true,
                View = _view,
                Projection = _proj
            };

            _vertexDeclaration = new VertexDeclaration(_graphics.GraphicsDevice, VertexPositionNormalColored.VertexElements);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            UpdateAvatarPosition();
            //var ray = new Ray(_avatarPosition, Vector3.Down);
            //collider = new Triangle(Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ);
            //var list = tree.GetPotentialColliders(ray, float.MaxValue);
            //foreach (var shape in list)
            //{
            //    if (!shape.IntersectsWith(ray).HasValue) continue;
            //    collider = (Triangle)shape;
            //    break;
            //}
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            _graphics.GraphicsDevice.Clear(Color.Black);
            //tree.Draw();
            _graphics.GraphicsDevice.VertexDeclaration = _vertexDeclaration;

            // Update our camera
            UpdateCameraThirdPerson();

            _basicEffect.Begin();
            _basicEffect.Projection = _proj;
            _basicEffect.View = _view;

            _basicEffect.Alpha = 1.0f;
            _basicEffect.DiffuseColor = new Vector3(.95f, .95f, .95f);
            _basicEffect.SpecularColor = new Vector3(0.05f, 0.05f, 0.05f);
            _basicEffect.SpecularPower = 5.0f;

            _basicEffect.AmbientLightColor = new Vector3(0.75f, 0.75f, 0.75f);
            _basicEffect.DirectionalLight0.Enabled = true;
            _basicEffect.DirectionalLight0.DiffuseColor = Vector3.One;
            _basicEffect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(1.0f, -1.0f, -1.0f));
            _basicEffect.DirectionalLight0.SpecularColor = Vector3.One;

            _basicEffect.DirectionalLight1.Enabled = true;
            _basicEffect.DirectionalLight1.DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f);
            _basicEffect.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(-1.0f, -1.0f, 1.0f));

            _basicEffect.LightingEnabled = true;

            foreach (var pass in _basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                _graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
                _graphics.GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;
                
                // Make the renderers draw their stuff
                base.Draw(gameTime);
                
                _graphics.GraphicsDevice.RenderState.FillMode = FillMode.Solid;
                _graphics.GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
                pass.End();
            }

            
            _basicEffect.End();

            DrawCameraState();
            //_spriteBatch.Begin();
            //_spriteBatch.DrawString(_spriteFont, "Esc: Opens the console.",
            //                        new Vector2(10, _graphics.GraphicsDevice.Viewport.Height - 30), Color.White);
            //_spriteBatch.End();

            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.AlphaBlendEnable = false;
        }

        private void DrawCameraState()
        {
            _graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
            _graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
        }

        /// <summary>
        /// This is the method that is called to move our avatar 
        /// </summary>
        /// <code>
        /// // create the class that does translations
        /// GiveHelpTransforms ght = new GiveHelpTransforms();
        /// // have it load our XML into the SourceXML property
        /// ght.LoadXMLFromFile(
        ///      "E:\\Inetpub\\wwwroot\\GiveHelp\\GiveHelpDoc.xml");
        /// </code>
        void UpdateAvatarPosition()
        {
            var keyboardState = Keyboard.GetState();
            var currentState = GamePad.GetState(PlayerIndex.One);
            //var mouseState = Mouse.GetState();

            //if (Console.IsOpen()) return;

            if(keyboardState.IsKeyDown(Keys.P))
            {
                System.Console.WriteLine("Open!");
            }

            if (keyboardState.IsKeyDown(Keys.A) || (currentState.DPad.Left == ButtonState.Pressed))
            {
                // Rotate left.
                _avatarYaw += RotationSpeed;
            }

            if (keyboardState.IsKeyDown(Keys.D) || (currentState.DPad.Right == ButtonState.Pressed))
            {
                // Rotate right.
                _avatarYaw -= RotationSpeed;
            }

            if (keyboardState.IsKeyDown(Keys.W) || (currentState.DPad.Up == ButtonState.Pressed))
            {
                var forwardMovement = Matrix.CreateRotationY(_avatarYaw);
                var v = new Vector3(0, 0, ForwardSpeed);
                v = Vector3.Transform(v, forwardMovement);
                _avatarPosition.Z += v.Z;
                _avatarPosition.X += v.X;
            }

            if (keyboardState.IsKeyDown(Keys.S) || (currentState.DPad.Down == ButtonState.Pressed))
            {
                var forwardMovement = Matrix.CreateRotationY(_avatarYaw);
                var v = new Vector3(0, 0, -ForwardSpeed);
                v = Vector3.Transform(v, forwardMovement);
                _avatarPosition.Z += v.Z;
                _avatarPosition.X += v.X;
            }

            if (keyboardState.IsKeyDown(Keys.F))
            {
                _avatarPosition.Y = _avatarPosition.Y - 1;
            }

            if (keyboardState.IsKeyDown(Keys.R) || keyboardState.IsKeyDown(Keys.Space))
            {
                _avatarPosition.Y = _avatarPosition.Y + 1;
            }

            if (keyboardState.IsKeyDown(Keys.E))
            {
                var forwardMovement = Matrix.CreateRotationY(_avatarYaw);
                var v = new Vector3(1, 0, 0);
                v = Vector3.Transform(v, forwardMovement);
                _avatarPosition.X -= v.X;
                _avatarPosition.Y -= v.Y;
                _avatarPosition.Z -= v.Z;
            }

            if (keyboardState.IsKeyDown(Keys.Q))
            {
                var forwardMovement = Matrix.CreateRotationY(_avatarYaw);
                var v = new Vector3(1, 0, 0);
                v = Vector3.Transform(v, forwardMovement);
                _avatarPosition.X += v.X;
                _avatarPosition.Y += v.Y;
                _avatarPosition.Z += v.Z;
            }

            if (keyboardState.IsKeyDown(Keys.T))
            {
                _thirdPersonReference.Y = _thirdPersonReference.Y - 0.25f;
            }
            if (keyboardState.IsKeyDown(Keys.G))
            {
                _thirdPersonReference.Y = _thirdPersonReference.Y + 0.25f;
            }
        }

        void UpdateCameraThirdPerson()
        {
            var rotationMatrix = Matrix.CreateRotationY(_avatarYaw);

            // Create a vector pointing the direction the camera is facing.
            var transformedReference = Vector3.Transform(_thirdPersonReference, rotationMatrix);

            // Calculate the position the camera is looking from.
            var cameraPosition = transformedReference + _avatarPosition;

            // Set up the view matrix and projection matrix.
            _view = Matrix.CreateLookAt(cameraPosition, _avatarPosition, new Vector3(0.0f, 1.0f, 0.0f));

            var viewport = _graphics.GraphicsDevice.Viewport;
            var aspectRatio = viewport.Width/(float)viewport.Height;

            _proj = Matrix.CreatePerspectiveFieldOfView(ViewAngle, aspectRatio, NearClip, FarClip);
        }
    }
}
