using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.MPQTool;
using WCell.Terrain.GUI.Util;
using WCell.Terrain.Legacy;
using WCell.Terrain.GUI.UI;

using WCell.Terrain.GUI.Renderers;
using WCell.Terrain.MPQ.DBC;
using WCell.Terrain.Pathfinding;
using WCell.Terrain.Recast.NavMesh;
using WCell.Terrain.Serialization;
using WCell.Util;
using WCell.Util.Graphics;

using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Color = Microsoft.Xna.Framework.Color;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using MathHelper = Microsoft.Xna.Framework.MathHelper;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Ray = WCell.Util.Graphics.Ray;

using XVector3 = Microsoft.Xna.Framework.Vector3;
using WVector3 = WCell.Util.Graphics.Vector3;

using MenuItem = System.Windows.Forms.MenuItem;
using Point = System.Drawing.Point;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace WCell.Terrain.GUI
{
	/// <summary>
	/// GUI component that lets us view and navigate through a map
	/// </summary>
	public class TerrainViewer : Game
	{
		const float ViewAngle = MathHelper.PiOver4;
		const float NearClip = 1.0f;
		const float FarClip = 2000.0f;
		/// <summary>
		/// Can't get closer than this to any object.
		/// Must be > ForwardSpeed
		/// </summary>
		private const float CollisionDistance = 2;
		/// <summary>
		/// Default sensitivity for most games is 3
		/// </summary>
		public static float MouseSensitivity = 3;

		//float ForwardSpeed = 50f / 60f;
		float ForwardSpeed = 1.2f;


		// XNA uses this variable for graphics information
		private readonly GraphicsDeviceManager _graphics;
		// We use this to declare our verticies
		//private VertexDeclaration _vertexDeclaration;

		// Another set of XNA Variables
		private Matrix _view;
		private Matrix _proj;

		Effect effect;

		//private readonly ADTManager _manager;
		/// <summary>
		/// Console used to execute commands while the game is running.
		/// </summary>
		//public MpqConsole Console;

		// Camera Stuff
		float avatarYaw, avatarPitch;
		private bool mouseLeftButtonDown, escapeDown;

		public static XVector3 avatarPosition = new XVector3(-100, 100, -100);

		//SpriteBatch spriteBatch;
		//SpriteFont font;
	    public RasterizerState solidRasterizerState, frameRasterizerState;

		public readonly DepthStencilState defaultStencilState = new DepthStencilState()
		{
			DepthBufferEnable = true
		};

		public readonly DepthStencilState disabledDepthStencilState = new DepthStencilState()
		{
			DepthBufferEnable = false
		};

	    private AxisRenderer AxisRenderer;
		private float globalIlluminationLevel;
	    private World world;
	    private SimpleWDTTerrain activeTerrain;
		private List<TerrainTile> m_Tiles;

		/// <summary>
		/// Constructor for the game.
		/// </summary>
		public TerrainViewer(XVector3 avatarPosition, World world, TileIdentifier tileId)
		{
			TerrainViewer.avatarPosition = avatarPosition;
			_graphics = new GraphicsDeviceManager(this);
			_graphics.GraphicsProfile = GraphicsProfile.HiDef;
			Content.RootDirectory = "Content";

			avatarYaw = 90;

			Form = (Form)Control.FromHandle(Window.Handle);

		    this.world = world;
		    activeTerrain = (SimpleWDTTerrain)world.WorldTerrain[tileId.MapId];
		    var tile = activeTerrain.Tiles[tileId.X, tileId.Y];
		    ActiveTile = tile;
            m_Tiles = new List<TerrainTile> { tile };
            TileRenderers = new Dictionary<TileIdentifier, TileRenderer>((int)MapId.End);
		}

		#region Properties
		public float GlobalIlluminationLevel
		{
			get { return globalIlluminationLevel; }
			set
			{
				globalIlluminationLevel = value;
			    effect.Parameters["xAmbient"].SetValue((0.8f)*value);

			    //effect.DiffuseColor = new XVector3(.95f, .95f, .95f) * value;
			    //effect.SpecularColor = new XVector3(0.05f, 0.05f, 0.05f) * value;
			    //effect.AmbientLightColor = new XVector3(0.35f, 0.35f, 0.35f) * value;
			}
		}

        public List<TerrainTile> Tiles
		{
			get { return m_Tiles; }
			private set { m_Tiles = value; }
		}
        public Dictionary<TileIdentifier, TileRenderer> TileRenderers; 

		public List<IShape> Shapes
		{
            get
            {
                var retList = new List<IShape>(m_Tiles.Count);
                foreach (var tile in m_Tiles)
                {
                    retList.Add(tile.NavMesh);
                }
                return retList;
            }
		}

        public TerrainTile ActiveTile { get; private set; }

		/// <summary>
		/// The form of this application
		/// </summary>
		public Form Form
		{
			get;
			private set;
		}

		public GenericRenderer TriangleSelectionRenderer
		{
			get;
			private set;
		}

		/// <summary>
		/// Lines are always rendered above everything else
		/// </summary>
		public GenericRenderer LineSelectionRenderer
		{
			get;
			private set;
		}
		#endregion


		#region Initialization
		/// <summary>
		/// Loads the content needed for the game.
		/// </summary>
		protected override void LoadContent()
		{
		    //effect = Content.Load<Effect>("effects");
		    //Keyboard.GetState();
		    //font = Content.Load<SpriteFont>("mfont");
		    //_spriteFont = thaFont;
		    //Console = new MpqConsole(this, thaFont);            
		    //Console.MyEvent += DoCommand;

		    //spriteBatch = new SpriteBatch(GraphicsDevice);
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
            // reset mouse, so we don't suddenly pan off to the sides
			Form.Activated += (sender, args) => RecenterMouse();
            
			IsMouseVisible = true;
			_graphics.PreferredBackBufferWidth = 1024;
			_graphics.PreferredBackBufferHeight = 768;
			_graphics.IsFullScreen = false;

			var device = _graphics.GraphicsDevice;
		    device.RasterizerState = solidRasterizerState =
		                             new RasterizerState
		                             {
		                                 CullMode = CullMode.None,
		                                 FillMode = FillMode.Solid
		                             };

		    frameRasterizerState = new RasterizerState()
		    {
		        CullMode = CullMode.None,
		        FillMode = FillMode.WireFrame
		    };

			device.DepthStencilState = defaultStencilState;

			_graphics.ApplyChanges();

			//_graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
			//_graphics.GraphicsDevice.RenderState.AlphaDestinationBlend = Blend.InverseSourceAlpha;
			//_graphics.GraphicsDevice.RenderState.AlphaDestinationBlend = Blend.InverseSourceAlpha;

			//_graphics.GraphicsDevice.RenderState.SourceBlend = Blend.One;
			//_graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha; 

		    effect = Content.Load<Effect>("effects");
			InitializeEffect();


			Components.Add(AxisRenderer = new AxisRenderer(this));
            Components.Add(TriangleSelectionRenderer = new GenericRenderer(this));
			Components.Add(LineSelectionRenderer = new GenericRenderer(this));

		    var tileRenderer = new TileRenderer(this, ActiveTile);
            TileRenderers.Add(ActiveTile.TileId, tileRenderer);
            Components.Add(tileRenderer);

			InitGUI();

            base.Initialize();
		}

		private void InitializeEffect()
		{
		    effect.CurrentTechnique = effect.Techniques["Colored"];

		    var lightDirection = new XVector3(-1.0f, 1.0f, 1.0f);
            lightDirection.Normalize();
            effect.Parameters["xLightDirection"].SetValue(lightDirection);
            //effect.Parameters["xAmbient"].SetValue(0.1f);
            effect.Parameters["xEnableLighting"].SetValue(true);
            effect.Parameters["xWorld"].SetValue(Matrix.Identity);

            /*effect = new BasicEffect(_graphics.GraphicsDevice)
			{
				VertexColorEnabled = true,
				LightingEnabled = true,

				Alpha = 1.0f,
				SpecularPower = 5.0f
			};

			effect.DirectionalLight0.Enabled = true;
			effect.DirectionalLight0.DiffuseColor = XVector3.One;
			effect.DirectionalLight0.Direction = XVector3.Normalize(new XVector3(1.0f, -1.0f, 0.0f));
			effect.DirectionalLight0.SpecularColor = XVector3.One;

			effect.DirectionalLight1.Enabled = true;
			effect.DirectionalLight1.DiffuseColor = new XVector3(0.1f, 0.1f, 0.1f);
			effect.DirectionalLight1.Direction = XVector3.Normalize(new XVector3(-1.0f, -1.0f, 1.0f));
            */

			GlobalIlluminationLevel = 1.5f;
            //_vertexDeclaration = new VertexDeclaration(VertexPositionNormalColored.VertexElements);
		}
		#endregion

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
		}

		#region Update & Draw
		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
            if (!IsActive) return;

			UpdateState();

			// use mouse navigation
		    var bounds = Window.ClientBounds;
            var w = bounds.Width;
			var h = bounds.Height;

			if (w != 0 && h != 0 && !IsMenuVisible)
			{
				var x = Mouse.GetState().X;
				var y = Mouse.GetState().Y;
                
                if (x > 0 && x < w && y > 0 && y < h)
                {
                    var cx = w/2;
                    var cy = h/2;

                    avatarPitch += (y - cy)*(MouseSensitivity/1000);
                    avatarYaw += (cx - x)*(MouseSensitivity/1000);

                    RecenterMouse();
                }
			}

			UpdateTexts();

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

		private void RecenterMouse()
		{
            var bounds = Window.ClientBounds;
            var x = Mouse.GetState().X;
		    if (x < 0 || x > bounds.Width) return;

            var y = Mouse.GetState().Y;
            if (y < 0 || y > bounds.Height) return;
            
            var cx = bounds.Width / 2;
			var cy = bounds.Height / 2;
			Mouse.SetPosition(cx, cy); // move back to center
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			_graphics.GraphicsDevice.Clear(Color.Black);

			GraphicsDevice.DepthStencilState = defaultStencilState;
			UpdateProjection();

            effect.Parameters["xProjection"].SetValue(_proj);
			//effect.Projection = _proj;
			effect.Parameters["xView"].SetValue(_view);
            //effect.View = _view;

			foreach (var pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				base.Draw(gameTime);
			}
			//_spriteBatch.Begin();
			//_spriteBatch.DrawString(_spriteFont, "Esc: Opens the console.",
			//                        new Vector2(10, _graphics.GraphicsDevice.Viewport.Height - 30), Color.White);
			//_spriteBatch.End();
		}

		void UpdateProjection()
		{
			// Create a vector pointing the direction the camera is facing.
			//var transformedReference = Vector3.Transform(, Matrix.CreateRotationY(avatarYaw));
			//transformedReference = Vector3.Transform(transformedReference, Matrix.CreateRotationX(avatarPitch));

			// Calculate the position the camera is looking from
			var target = avatarPosition + XVector3.Transform(XVector3.UnitZ, Matrix.CreateFromYawPitchRoll(avatarYaw, avatarPitch, 0));

			// Set up the view matrix and projection matrix
			_view = Matrix.CreateLookAt(avatarPosition, target, new XVector3(0.0f, 1.0f, 0.0f));

			var viewport = _graphics.GraphicsDevice.Viewport;
			var aspectRatio = viewport.Width / (float)viewport.Height;
			_proj = Matrix.CreatePerspectiveFieldOfView(ViewAngle, aspectRatio, NearClip, FarClip);
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
		void UpdateState()
		{
			var keyboardState = Keyboard.GetState();
			var gamePadState = GamePad.GetState(PlayerIndex.One);
			var mouseState = Mouse.GetState();

			//if (Console.IsOpen()) return;

			if (keyboardState.IsKeyDown(Keys.P))
			{
				Console.WriteLine("Open!");
			}

			// movement
			if (!IsMenuVisible 
                && (keyboardState.IsKeyDown(Keys.A) 
                    || (gamePadState.DPad.Left == ButtonState.Pressed)))
			{
			    var forwardSpeed = ForwardSpeed;
                if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
                {
                    forwardSpeed *= 0.5f;
                }

				// move left
				//avatarYaw += RotationSpeed;
				var forwardMovement = Matrix.CreateRotationY(avatarYaw);
				var v = new XVector3(1, 0, 0)*forwardSpeed;
				v = XVector3.Transform(v, forwardMovement);
				avatarPosition += v;
			}

            if (!IsMenuVisible 
                && (keyboardState.IsKeyDown(Keys.D) 
                    || (gamePadState.DPad.Right == ButtonState.Pressed)))
			{
                var forwardSpeed = ForwardSpeed;
                if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
                {
                    forwardSpeed *= 0.5f;
                }

				// move right
				//avatarYaw -= RotationSpeed;
                var forwardMovement = Matrix.CreateRotationY(avatarYaw);
				var v = new XVector3(-1, 0, 0)*forwardSpeed;
			    v = XVector3.Transform(v, forwardMovement);
				avatarPosition += v;
			}

            if (!IsMenuVisible 
                && (keyboardState.IsKeyDown(Keys.W) 
                    || (gamePadState.DPad.Up == ButtonState.Pressed)))
			{
                var forwardSpeed = ForwardSpeed;
                if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
                {
                    forwardSpeed *= 0.5f;
                }

				//var forwardMovement = Matrix.CreateRotationY(avatarYaw);
				//var v = new Vector3(0, 0, ForwardSpeed);
				//v = Vector3.Transform(v, forwardMovement);
				//avatarPosition.Z += v.Z;
				//avatarPosition.X += v.X;
				var horizontal = avatarPitch.Cos();
				var vertical = -avatarPitch.Sin();
				var v = new XVector3(avatarYaw.Sin() * horizontal, vertical, avatarYaw.Cos() * horizontal);
				v.Normalize();

			    var newPos = avatarPosition + v*forwardSpeed;
				var canMove = true;
				//Ray ray;
				//if (GetRayToCursor(out ray))
				//{
				//    // collision test
				//    float dist;
				//    if (Tile.FindFirstHitTriangle(ray, out dist) != -1)
				//    {
				//        // stop moving, if we run against something
				//        canMove = dist > CollisionDistance;
				//    }
				//}
				if (canMove)
				{
					avatarPosition = newPos;
				}
			}

			if (!IsMenuVisible 
                && (keyboardState.IsKeyDown(Keys.S) 
                    || (gamePadState.DPad.Down == ButtonState.Pressed)))
			{
                var forwardSpeed = ForwardSpeed;
                if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
                {
                    forwardSpeed *= 0.5f;
                }

				var horizontal = avatarPitch.Cos();
				var vertical = -avatarPitch.Sin();
				var v = new XVector3(avatarYaw.Sin() * horizontal, vertical, avatarYaw.Cos() * horizontal);
				v.Normalize();
			    avatarPosition -= v*forwardSpeed;
			}

			if (!IsMenuVisible && keyboardState.IsKeyDown(Keys.F))
			{
                var forwardSpeed = ForwardSpeed;
                if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
                {
                    forwardSpeed *= 0.5f;
                }
				avatarPosition.Y = avatarPosition.Y - forwardSpeed;
			}

			if (!IsMenuVisible 
                && (keyboardState.IsKeyDown(Keys.R) 
                    || keyboardState.IsKeyDown(Keys.Space)))
			{
                var forwardSpeed = ForwardSpeed;
                if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
                {
                    forwardSpeed *= 0.5f;
                }
				avatarPosition.Y = avatarPosition.Y + forwardSpeed;
			}


			// adjust speed
			if (keyboardState.IsKeyDown(Keys.OemPlus))
			{
				ForwardSpeed = Math.Min(ForwardSpeed + 0.1f, 15);
			}
			else if (keyboardState.IsKeyDown(Keys.OemMinus))
			{
				ForwardSpeed = Math.Max(ForwardSpeed - 0.1f, 0.1f);
			}

			// menu
			if (keyboardState.IsKeyDown(Keys.Escape))
			{
				if (!escapeDown)
				{
					escapeDown = true;
					IsMenuVisible = !IsMenuVisible;
				}
			}
			else if (escapeDown)
			{
				escapeDown = false;
			}


			// highlight
			if (keyboardState.IsKeyDown(Keys.H))
			{
				HighlightSurroundings(100);
			}

			if (keyboardState.IsKeyDown(Keys.T) && TriangleSelectionRenderer.RenderingVerticies.Length > 2)
			{
				// test intersection with the first selected triangle
				var t = new Triangle(TriangleSelectionRenderer.RenderingVerticies[0].Position.ToWCell(),
									 TriangleSelectionRenderer.RenderingVerticies[1].Position.ToWCell(),
									 TriangleSelectionRenderer.RenderingVerticies[2].Position.ToWCell());

				Ray ray;
				GetRayToCursor(out ray);
				float distance;
				if (Intersection.RayTriangleIntersect(ray, t.Point1, t.Point2, t.Point3, out distance))
				{
					Console.WriteLine("Hit - Distance: " + distance);
				}
				else
				{
					Console.WriteLine("No hit");
				}
			}

			// clear
			if (keyboardState.IsKeyDown(Keys.C))
			{
				ClearSelection();
			}

            if (!IsMenuVisible 
                && (keyboardState.IsKeyDown(Keys.LeftControl) 
                    || keyboardState.IsKeyDown(Keys.RightControl)))
            {
                if (keyboardState.IsKeyDown(Keys.LeftAlt) 
                    || keyboardState.IsKeyDown(Keys.RightAlt))
                {
                    SetEnvironmentVisibility(true);
                }
                else if (keyboardState.IsKeyDown(Keys.LeftShift) 
                         || keyboardState.IsKeyDown(Keys.RightShift))
                {
                    SetEnvironmentVisibility(true);
                }
                else if (keyboardState.IsKeyDown(Keys.LeftWindows) 
                         || keyboardState.IsKeyDown(Keys.RightWindows))
                {
                    SetEnvironmentVisibility(true);
                }
                else if (keyboardState.IsKeyDown(Keys.PrintScreen))
                {
                    SetEnvironmentVisibility(true);
                }
                else
                {
                    SetEnvironmentVisibility(false);
                }
            }
            else
            {
                SetEnvironmentVisibility(true);
            }

			// mouse
            if (!IsMenuVisible 
                && mouseState.LeftButton == ButtonState.Pressed 
                && !mouseLeftButtonDown)
			{
				// select polygon
				mouseLeftButtonDown = true;

				if (keyboardState.IsKeyDown(Keys.LeftControl))
				{
					SelectPolygon();
				}
				else
				{
					if (selectedPoints.Count >= 2)
					{
						// clear
						ClearSelection();
					}
					else
					{
						// select a path
						SelectOnPath();
					}
				}
			}

            if (!IsMenuVisible 
                && mouseState.LeftButton == ButtonState.Released 
                && mouseLeftButtonDown)
			{
				mouseLeftButtonDown = false;
			}
		}

		/// <summary>
		/// Clears all currently selected triangles
		/// </summary>
		private void ClearSelection()
		{
			selectedPoints.Clear();
			TriangleSelectionRenderer.Clear();
			LineSelectionRenderer.Clear();
		}

		private void HighlightSurroundings(float distance)
		{
			Ray ray;
			if (GetRayToCursor(out ray))
			{
                foreach (var shape in Shapes)
                {
                    foreach (var trii in shape.GetPotentialColliders(ray))
                    {
                        var tri = shape.GetTriangle(trii);
                        foreach (var vert in tri.Vertices)
                        {
                            var pos = ray.Position;
                            if (vert.GetDistance(pos) < distance)
                            {
                                SelectTriangle(shape, trii, true);
                            }
                        }
                    }
                }
			}
		}

        private void UpdateAvatarPosition()
        {
            var topRight = Tiles[0].Bounds.TopRight; 
            foreach (var tile in Tiles)
            {
                topRight = new WCell.Util.Graphics.Point
                {
                    X = Math.Max(topRight.X, tile.Bounds.TopRight.X),
                    Y = Math.Max(topRight.Y, tile.Bounds.TopRight.Y)
                };
            }

            avatarPosition = new XVector3(topRight.X, topRight.Y, 200);
            XNAUtil.TransformWoWCoordsToXNACoords(ref avatarPosition);
            avatarYaw = 45;
        }
		#endregion

		#region Mouse selection
		private readonly List<RegionalPoint> selectedPoints = new List<RegionalPoint>();

		private void SelectOnPath()
		{
			Ray ray;
			if (!GetRayToCursor(out ray))
			{
				// Outside of current map
				return;
			}

			WVector3 v = new WVector3(float.MinValue, float.MinValue, float.MinValue);
		    IShape found = null;
		    foreach (var shape in Shapes)
		    {
                if (shape.IntersectFirstTriangle(ray, out v) == -1) continue;

		        found = shape;
                break;
		    }

            if (found == null) 
                return;

		    selectedPoints.Add(new RegionalPoint
		    {
                Shape = found,
                Point = v
		    });

		    if (selectedPoints.Count <= 1) return;

		    // highlight corridor and visited fringe
		    var start = selectedPoints[0];
            if (!(start.Shape is NavMesh)) return;
            
            var dest = selectedPoints[1];
            if (!(dest.Shape is NavMesh)) return;

            var visited = new HashSet<int>();
		    var path = new Path();

		    var corridor = (start.Shape as NavMesh).Tile.Pathfinder.FindCorridor(start.Point, dest.Point, out visited);

		    if (corridor.IsNull) return;

		    // highlight fringe
		    /*foreach (var tri in visited)
				{
					SelectTriangle(tri, true, new Color(120, 10, 10, 128));
				}*/

		    // highlight corridor
		    var current = corridor;
		    while (!current.IsNull)
		    {
                // TODO: manage paths that cross tile boundaries.
		        SelectTriangle(start.Shape, current.Triangle, true);
		        current = current.Previous;
		    }

		    // draw line to along the path
		    (start.Shape as NavMesh).Tile.Pathfinder.FindPathStringPull(start.Point, dest.Point, corridor, path);
		    
            var p = start.Point;
		    LineSelectionRenderer.SelectPoint(start.Point, Color.Black);
		    while (path.HasNext())
		    {
		        var q = path.Next();
		        LineSelectionRenderer.SelectLine(p, q, Color.Green);
		        LineSelectionRenderer.SelectPoint(q, Color.Black);
		        p = q;
		    }

		    // highlight corners
		    /*current = corridor;
				while (!current.IsNull)
				{
					//var tri = Tile.NavMesh.FindFirstTriangleUnderneath(curren);
					SelectTriangle(current.Triangle, true);

					if (current.Edge != -1 && current.Previous != null)
					{
						WVector3 left, right, apex;
						Tile.NavMesh.GetOutsideOrderedEdgePointsPlusApex(current.Previous.Triangle, current.Edge, out left, out right, out apex);
						LineSelectionRenderer.SelectPoint(left, Color.LimeGreen);
						LineSelectionRenderer.SelectPoint(right, Color.Red);
                        LineSelectionRenderer.SelectPoint(apex, Color.Azure);
					}
					current = current.Previous;
				}*/
		}

		/// <summary>
		/// Selects the polygon under the cursor and all it's neighbors
		/// </summary>
		private void SelectPolygon()
		{
			var keyboardState = Keyboard.GetState();
			var add = (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift));
			Ray ray;
			if (!GetRayToCursor(out ray))
			{
				// Outside of current map
				return;
			}

		    var index = -1;
		    IShape found = null;
            foreach(var shape in Shapes)
            {
                index = shape.FindFirstHitTriangle(ray);
                if (index == -1) continue;
                
                found = shape;
                break;
            }

			//var index = Utility.Random(Shape.Indices.Length-2);
		    if (found == null) return;

		    // mark the selected triangle
		    SelectTriangle(found, index, add);

		    // also mark it's neighbors
		    var neighbors = found.GetNeighborsOf(index);
		    foreach (var neighbor in neighbors)
		    {
		        if (neighbor > 0)
		        {
		            SelectTriangle(found, neighbor*3, true, Color.Red);
		        }
		    }
		}

		private void SelectTriangle(IShape shape, int index, bool add)
		{
			SelectTriangle(shape, index, add, new Color(120, 120, 20));
		}

		void SelectTriangle(IShape shape, int index, bool add, Color color)
		{
			if (index == -1) return;

			var triangle = shape.GetTriangle(index);

			// elevate them just a tiny bit, so they do not collide with the original triangle
			triangle.Point1.Z += 0.0001f;
			triangle.Point2.Z += 0.0001f;
			triangle.Point3.Z += 0.0001f;

			TriangleSelectionRenderer.Select(ref triangle, add, color);
		}

		/// <summary>
		/// Computes a ray from the current viewer's location to the mouse cursor
		/// </summary>
		bool GetRayToCursor(out Ray ray)
		{
			var mouseState = Mouse.GetState();

			var width = _graphics.GraphicsDevice.Viewport.Width;
			var height = _graphics.GraphicsDevice.Viewport.Height;
			var x = mouseState.X;
			var y = mouseState.Y;

			if (x < 0 || y < 0 || x > width || y > height)
			{
				ray = default(Ray);
				return false;
			}

			//var startPos = new Vector3(x, y, 0);
			var endPos = new XVector3(x, y, 1);

			//var near = _graphics.GraphicsDevice.Viewport.Unproject(startPos, _proj, _view, Matrix.Identity).ToWCell();
			var near = avatarPosition.ToWCell();
			var far = _graphics.GraphicsDevice.Viewport.Unproject(endPos, _proj, _view, Matrix.Identity).ToWCell();

			var dir = (far - near).NormalizedCopy();

			ray = new Ray(near, dir);
			return true;
		}
		#endregion

		private void ToggleSolidRenderingMode()
		{
			if (solidRenderingMode)
			{
				_graphics.GraphicsDevice.RasterizerState = solidRasterizerState;
			}
			else
			{
				_graphics.GraphicsDevice.RasterizerState = frameRasterizerState;
			}
		}

        #region GUI
		private MainMenu menu;
		private MenuItem renderingModeButton;
	    private MenuItem toggleWaterButton;
	    private MenuItem toggleNavMeshButton;
		private bool solidRenderingMode = true;
	    private bool liquidRenderingEnabled = true;
	    private bool navMeshRenderingEnabled = false;
		private TreeView treeView;
		private TextBox waitingBox;
		readonly BackgroundWorker worker = new BackgroundWorker();

		private TextBox LiquidBox;
	    private Button NorthWestButton;
	    private Button NorthButton;
	    private Button NorthEastButton;
	    private Button EastButton;
	    private Button SouthEastButton;
	    private Button SouthButton;
	    private Button SouthWestButton;
	    private Button WestButton;

		public bool IsMenuVisible
		{
			get
			{
				return Form.Menu != null;
			}
			set
			{
				if (value == IsMenuVisible) return;

				SetTreeViewVisible(value);
			    SetButtonsVisible(value);
				if (value)
				{
					Form.Menu = menu;
					GlobalIlluminationLevel -= 0.5f;		// dampen the light
				}
				else
				{
					Form.Menu = null;
					GlobalIlluminationLevel += 0.5f;		// back to original illumination
					RecenterMouse();
				}
			}
		}

		private void SetTreeViewVisible(bool value)
		{
			if (treeView.Parent == null)
			{
				// did not add treeview yet
				if (worker.IsBusy) return;		// did not finish treeview yet

			    Form.Controls.Add(treeView);

			    treeView.Visible = false;
			    treeView.Visible = true;
                UpdateDirectionButtonPositions();
			}

			treeView.Visible = value;
		}

        private void SetButtonsVisible(bool value)
        {
            if (NorthWestButton.Parent == null) Form.Controls.Add(NorthWestButton);
            if (NorthButton.Parent == null) Form.Controls.Add(NorthButton);
            if (NorthEastButton.Parent == null) Form.Controls.Add(NorthEastButton);
            if (EastButton.Parent == null) Form.Controls.Add(EastButton);
            if (SouthEastButton.Parent == null) Form.Controls.Add(SouthEastButton);
            if (SouthButton.Parent == null) Form.Controls.Add(SouthButton);
            if (SouthWestButton.Parent == null) Form.Controls.Add(SouthWestButton);
            if (WestButton.Parent == null) Form.Controls.Add(WestButton);
            
            NorthWestButton.Visible = value;
            NorthButton.Visible = value;
            NorthEastButton.Visible = value;
            EastButton.Visible = value;
            SouthEastButton.Visible = value;
            SouthButton.Visible = value;
            SouthWestButton.Visible = value;
            WestButton.Visible = value;
        }

		/// <summary>
		/// Add some basic GUI controls
		/// </summary>
		void InitGUI()
		{
			Console.WriteLine("");
			Console.WriteLine("Help:");
			Console.WriteLine("  Press ESCAPE to enter the menu");

		    LiquidBox = new TransparentTextBox
		    {
		        Dock = DockStyle.None,
		        BackColor = System.Drawing.Color.Transparent,
		        Location = new Point(Form.Width - 60, 40),
		        Width = 50
		    };
		    TextBoxUtil.DisableDecoration(LiquidBox);
		    Form.Controls.Add(LiquidBox);

		    NorthWestButton = new DirectionButton(Point2D.NorthWest)
		    {
		        Text = "NorthWest",
		        Location = new Point(10, 10)
		    };
		    NorthWestButton.Click += ClickedDirectionButton;

		    NorthButton = new DirectionButton(Point2D.North)
		    {
		        Text = "North"
		    };
		    NorthButton.Location = new Point((Form.Width - NorthButton.Size.Width)/2, 10);
		    NorthButton.Click += ClickedDirectionButton;

		    NorthEastButton = new DirectionButton(Point2D.NorthEast)
		    {
		        Text = "NorthEast"
		    };
		    NorthEastButton.Location = new Point(Form.Width - NorthEastButton.Size.Width - 10, 10);
		    NorthEastButton.Click += ClickedDirectionButton;

		    EastButton = new DirectionButton(Point2D.East)
		    {
		        Text = "East"
		    };
		    EastButton.Location = new Point(Form.Width - EastButton.Size.Width - 10,
		                                    (Form.Height - EastButton.Size.Height)/2);
		    EastButton.Click += ClickedDirectionButton;

		    SouthEastButton = new DirectionButton(Point2D.SouthEast)
		    {
		        Text = "SouthEast"
		    };
		    SouthEastButton.Location = new Point(Form.Width - SouthEastButton.Size.Width - 10,
		                                         Form.Height - SouthEastButton.Size.Height - 75);
		    SouthEastButton.Click += ClickedDirectionButton;

		    SouthButton = new DirectionButton(Point2D.South)
		    {
		        Text = "South"
		    };
		    SouthButton.Location = new Point((Form.Width - SouthButton.Size.Width)/2,
		                                     Form.Height - SouthButton.Size.Height - 75);
		    SouthButton.Click += ClickedDirectionButton;

		    SouthWestButton = new DirectionButton(Point2D.SouthWest)
		    {
		        Text = "SouthWest"
		    };
		    SouthWestButton.Location = new Point(10, Form.Height - SouthWestButton.Size.Height - 75);
		    SouthWestButton.Click += ClickedDirectionButton;

		    WestButton = new DirectionButton(Point2D.West)
		    {
		        Text = "West"
		    };
		    WestButton.Location = new Point(10, (Form.Height - WestButton.Size.Height)/2);
		    WestButton.Click += ClickedDirectionButton;

		    menu = new MainMenu();

			renderingModeButton = new MenuItem();
			renderingModeButton.Click += ClickedRenderingModeButton;
			menu.MenuItems.Add(renderingModeButton);
			ClickedRenderingModeButton(null, null);

			toggleWaterButton = new MenuItem("Toggle Water");
			toggleWaterButton.Click += ClickedToggleWaterButton;
			menu.MenuItems.Add(toggleWaterButton);
            ClickedToggleWaterButton(null, null);

		    toggleNavMeshButton = new MenuItem();
		    toggleNavMeshButton.Click += ClickedToggleNavMeshButton;
		    menu.MenuItems.Add(toggleNavMeshButton);
		    ClickedToggleNavMeshButton(null, null);

			//var exportRecastMeshButton = new MenuItem("Export Tile mesh");
			//exportRecastMeshButton.Click += ExportRecastMesh;
			//menu.MenuItems.Add(exportRecastMeshButton);

			// build treeview asynchronously
		    treeView = new TreeView
		    {
		        Dock = DockStyle.Left
		    };
		    treeView.DoubleClick += DoubleClickedTreeView;
			treeView.Width = 200;

			GetOrCreateWaitingBox("Loading zone list...").Visible = true;
			worker.DoWork += DoBuildTreeView;
			worker.RunWorkerCompleted += OnTreeViewBuilt;

			worker.RunWorkerAsync();

            UpdateFormText();
		}

		void DoBuildTreeView(object sender, DoWorkEventArgs e)
		{
			// build list of maps, zones & tiles
			var zoneTileSets = ZoneBoundaries.GetZoneTileSets();
			var zoneNodes = new Tuple<ZoneId, List<Point2D>>[(int)ZoneId.End];
			for (var map = 0; map < zoneTileSets.Length; map++)
			{
				var tileSet = zoneTileSets[map];
				if (tileSet == null || MapInfo.GetMapInfo((MapId)map) == null) continue;	// map does not exist

				var children = new List<Tuple<ZoneId, List<Point2D>>>();
				for (var x = 0; x < tileSet.ZoneGrids.GetUpperBound(0); x++)
				{
					for (var y = 0; y < tileSet.ZoneGrids.GetUpperBound(1); y++)
					{
						var grid = tileSet.ZoneGrids[x, y];
						if (grid != null)
						{
							foreach (var zone in grid.GetAllZoneIds())
							{
								var node = zoneNodes[(int)zone];
								var coords = new Point2D(x, y);
								if (!WCellTerrainSettings.GetDefaultMPQFinder().FileExists(ADTReader.GetFilename((MapId)map, coords.X, coords.Y)))
								{
									// tile does not exist
									continue;
								}

								if (node == null)
								{
									// Only add Zone node if it has at least one tile
									children.Add(zoneNodes[(int)zone] = node = new Tuple<ZoneId, List<Point2D>>(zone, new List<Point2D>()));
								}
								node.Item2.Add(coords);
							}
						}
					}
				}


				if (children.Count == 1 && ((MapId)map).ToString() == children[0].Item1.ToString())
				{
					// map is only a single zone: Ommit the map node
					var tuple = children[0];
					treeView.Nodes.Add(new ZoneTreeNode((MapId)map, tuple.Item1, tuple.Item2));
				}
				else
				{
				    treeView.Nodes.Add(new MapTreeNode((MapId) map, children.ToArray().TransformArray(tuple =>
				                                                                                      new ZoneTreeNode((MapId) map,
				                                                                                                       tuple.Item1,
				                                                                                                       tuple.Item2))));
				}
			}
		}

		void OnTreeViewBuilt(object sender, EventArgs args)
		{
			// fix visibility
			waitingBox.Visible = false;
			SetTreeViewVisible(IsMenuVisible);
			
            // remove events
			worker.DoWork -= DoBuildTreeView;
			worker.RunWorkerCompleted -= OnTreeViewBuilt;
		}

		private void ClickedRenderingModeButton(object sender, EventArgs e)
		{
			ToggleSolidRenderingMode();
			solidRenderingMode = !solidRenderingMode;			// flip
			if (solidRenderingMode)
			{
				renderingModeButton.Text = "Solid Mesh";
			}
			else
			{
				renderingModeButton.Text = "Wireframe Mesh";
			}
		}

        private void ClickedToggleWaterButton(object sender, EventArgs e)
        {
            liquidRenderingEnabled = !liquidRenderingEnabled;

            foreach (var renderer in TileRenderers.Values)
            {
                renderer.LiquidEnabled = liquidRenderingEnabled;
            }
            
            if (liquidRenderingEnabled)
            {
                toggleWaterButton.Text = "Water Off";
            }
            else
            {
                toggleWaterButton.Text = "Water On";
            }
        }

        private void ClickedToggleNavMeshButton(object sender, EventArgs e)
        {
            navMeshRenderingEnabled = !navMeshRenderingEnabled;
            foreach (var renderer in TileRenderers.Values)
            {
                renderer.NavMeshEnabled = navMeshRenderingEnabled;
            }

            if (navMeshRenderingEnabled)
            {
                toggleNavMeshButton.Text = "NavMesh Off";
            }
            else
            {
                toggleNavMeshButton.Text = "NavMesh On";
            }
        }

        private void ClickedDirectionButton(object sender, EventArgs e)
        {
            var button = (DirectionButton)sender;

            var tile = ActiveTile;
            if (tile == null) return;

            var newCoords = tile.Coords + button.Direction;
            ToggleDisplayedTile(activeTerrain.MapId, newCoords);
        }

        private Control GetOrCreateWaitingBox(string text)
		{
			if (waitingBox == null)
			{
				waitingBox = new TextBox();
				waitingBox.Dock = DockStyle.Bottom;
				waitingBox.ForeColor = System.Drawing.Color.DarkGreen;
				waitingBox.BackColor = System.Drawing.Color.Black;
				TextBoxUtil.DisableDecoration(waitingBox);

				Form.Controls.Add(waitingBox);
			}

			waitingBox.Text = text;
			return waitingBox;
		}

		private void DoubleClickedTreeView(object sender, EventArgs e)
		{
            var node = treeView.SelectedNode;
		    if (!(node is TileTreeNode)) return;
            
            var tnode = node as TileTreeNode;

		    tnode.BackColor = (!ToggleDisplayedTile(tnode.Map, tnode.Coords))
		                          ? TileTreeNode.NotLoadedColor
		                          : TileTreeNode.LoadedColor;
		}

	    private DoWorkEventHandler addingTile;
	    private RunWorkerCompletedEventHandler completedLoad;
	    private bool ToggleDisplayedTile(MapId map, Point2D coords)
	    {
	        var tileId = new TileIdentifier(map, coords);
            if (TileRenderers.ContainsKey(tileId))
            {
                RemoveTileFromDisplay(tileId);
                IsMenuVisible = false;
                return false;
            }
            
	        // GUI stuff
            if (worker.IsBusy) return false;
            
            IsMenuVisible = false;
            GetOrCreateWaitingBox("Loading tile - Please wait...").Visible = true;
	        
            addingTile = (sender, b) =>  AddTileToDisplay(tileId);
            worker.DoWork += addingTile;

	        completedLoad = (sender, e) =>
	        {
	            UpdateTerrainViewer(false);
	            worker.DoWork -= addingTile;
	            worker.RunWorkerCompleted -= completedLoad;
	        };
	        worker.RunWorkerCompleted += completedLoad;

	        worker.RunWorkerAsync();
	        return true;
	    }

        private void SetEnvironmentVisibility(bool visible)
        {
            foreach (var renderer in TileRenderers.Values)
            {
                renderer.EnvironsEnabled = visible;
            }
        }

	    class TransparentTextBox : TextBox
		{
			public TransparentTextBox()
			{
				SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			}
		}

        class DirectionButton : Button
        {
            public readonly Point2D Direction;

            public DirectionButton(Point2D direction)
            {
                Direction = direction;
            }
        }

        public static class TextBoxUtil
		{
			public static void DisableDecoration(TextBox box)
			{
				box.Margin = new Padding(0, 0, 0, 0);
				box.BorderStyle = BorderStyle.None;
				box.TextAlign = HorizontalAlignment.Center;
				box.Enabled = false;
				box.HideSelection = true;
				box.GotFocus += (a, b) => box.Select(0, 0);
				box.LostFocus += (a, b) => box.Select(0, 0);
				box.Leave += (a, b) => box.Select(0, 0);
				box.Click += (a, b) => box.Select(0, 0);
			}
		}

		void UpdateTexts()
		{
			var actualPos = avatarPosition.ToWCell();

		    TerrainTile found = null;
		    foreach (var tile in Tiles)
		    {
                if (!tile.Bounds.Contains(actualPos.X, actualPos.Y)) continue;

		        found = tile;
		        break;
		    }

            if (found == null) return;

			var fluid = found.Terrain.GetLiquidType(actualPos);
			if (fluid != LiquidType.None)
			{
				LiquidBox.Text = "In " + fluid;
				switch (fluid)
				{
					case LiquidType.Lava:
						LiquidBox.ForeColor = System.Drawing.Color.Orange;
						break;
					case LiquidType.Water:
						LiquidBox.ForeColor = System.Drawing.Color.LightBlue;
						break;
					case LiquidType.OceanWater:
						LiquidBox.ForeColor = System.Drawing.Color.DarkBlue;
						break;
				}

				//spriteBatch.Begin();
				//spriteBatch.DrawString(font, , new Vector2(Window.ClientBounds.Width - 100, 45), Color.White);
				//spriteBatch.End();
			}
			else
			{
				LiquidBox.Text = string.Empty;
			}
		}

        private void AddTileToDisplay(TileIdentifier tileId)
        {
            var terrain = world.GetOrLoadSimpleWDTTerrain(tileId.MapId);
            var tile = terrain.GetOrCreateTile(tileId);
            if (tile == null) return;

            var renderer = new TileRenderer(this, tile);
            TileRenderers.Add(tileId, renderer);
            Tiles.Add(tile);
            Components.Add(renderer);
        }

        private void RemoveTileFromDisplay(TileIdentifier tileId)
        {
            var renderer = TileRenderers[tileId];
            TileRenderers.Remove(tileId);
            Tiles.RemoveFirst(tile => (tile.TileId == tileId));
            Components.Remove(renderer);
            
            renderer.Dispose();

            UpdateTerrainViewer(false);
        }

        private void UpdateTerrainViewer(bool updateAvatar)
        {
            UpdateFormText();

            // reset GUI
            waitingBox.Visible = false;

            // update avatar
            if (updateAvatar) UpdateAvatarPosition();
        }

        private void UpdateFormText()
        {
            var titleString = "TerrainViewer - ";
            foreach (var tile in m_Tiles)
            {
                titleString += string.Format("{0} (Tile X={1}, Y={2}, ",
                                    tile.Terrain.MapId, tile.TileX, tile.TileY);
            }
            titleString = titleString.TrimEnd(", ".ToCharArray());
            titleString += ")";
            Form.Text = titleString;
        }

        private void UpdateDirectionButtonPositions()
        {
            Form.Controls.Remove(NorthWestButton);
            Form.Controls.Remove(NorthButton);
            Form.Controls.Remove(SouthButton);
            Form.Controls.Remove(SouthWestButton);
            Form.Controls.Remove(WestButton);
            
            NorthWestButton.Location = new Point(NorthWestButton.Location.X + treeView.Size.Width, NorthWestButton.Location.Y);
            NorthButton.Location = new Point(NorthButton.Location.X + treeView.Size.Width/2, NorthButton.Location.Y);
            SouthButton.Location = new Point(SouthButton.Location.X + treeView.Size.Width/2, SouthButton.Location.Y);
            SouthWestButton.Location = new Point(SouthWestButton.Location.X + treeView.Size.Width, SouthWestButton.Location.Y);
            WestButton.Location = new Point(WestButton.Location.X + treeView.Size.Width, WestButton.Location.Y);
            
            Form.Controls.Add(NorthWestButton);
            Form.Controls.Add(NorthButton);
            Form.Controls.Add(SouthButton);
            Form.Controls.Add(SouthWestButton);           
            Form.Controls.Add(WestButton);

            SetButtonsVisible(IsMenuVisible);
        }

	    #endregion

		#region DrawBoundingBox
		//private static void DrawBoundingBox(BoundingBox boundingBox, Color color, WMORoot currentWMO)
		//{
		//    var min = boundingBox.Min;
		//    var max = boundingBox.Max;
		//    DrawBoundingBox(min, max, color, currentWMO);
		//}

		//private static void DrawBoundingBox(Vector3 min, Vector3 max, Color color, WMORoot currentWMO)
		//{
		//    var zero = min;
		//    var one = new Vector3(min.X, max.Y, min.Z);
		//    var two = new Vector3(min.X, max.Y, max.Z);
		//    var three = new Vector3(min.X, min.Y, max.Z);
		//    var four = new Vector3(max.X, min.Y, min.Z);
		//    var five = new Vector3(max.X, max.Y, min.Z);
		//    var six = max;
		//    var seven = new Vector3(max.X, min.Y, max.Z);

		//    var offset = currentWMO.WmoVertices.Count;
		//    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(zero, color, Vector3.Up));
		//    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(one, color, Vector3.Up));
		//    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(two, color, Vector3.Up));
		//    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(three, color, Vector3.Up));
		//    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(four, color, Vector3.Up));
		//    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(five, color, Vector3.Up));
		//    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(six, color, Vector3.Up));
		//    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(seven, color, Vector3.Up));

		//    // Bottom Face
		//    currentWMO.WmoIndices.Add(offset + 0);
		//    currentWMO.WmoIndices.Add(offset + 1);
		//    currentWMO.WmoIndices.Add(offset + 5);
		//    currentWMO.WmoIndices.Add(offset + 0);
		//    currentWMO.WmoIndices.Add(offset + 5);
		//    currentWMO.WmoIndices.Add(offset + 4);

		//    // Front face
		//    currentWMO.WmoIndices.Add(offset + 0);
		//    currentWMO.WmoIndices.Add(offset + 1);
		//    currentWMO.WmoIndices.Add(offset + 2);
		//    currentWMO.WmoIndices.Add(offset + 0);
		//    currentWMO.WmoIndices.Add(offset + 2);
		//    currentWMO.WmoIndices.Add(offset + 3);

		//    // Left face
		//    currentWMO.WmoIndices.Add(offset + 1);
		//    currentWMO.WmoIndices.Add(offset + 2);
		//    currentWMO.WmoIndices.Add(offset + 6);
		//    currentWMO.WmoIndices.Add(offset + 1);
		//    currentWMO.WmoIndices.Add(offset + 6);
		//    currentWMO.WmoIndices.Add(offset + 5);

		//    // Back face
		//    currentWMO.WmoIndices.Add(offset + 5);
		//    currentWMO.WmoIndices.Add(offset + 6);
		//    currentWMO.WmoIndices.Add(offset + 7);
		//    currentWMO.WmoIndices.Add(offset + 5);
		//    currentWMO.WmoIndices.Add(offset + 7);
		//    currentWMO.WmoIndices.Add(offset + 4);

		//    // Right face
		//    currentWMO.WmoIndices.Add(offset + 0);
		//    currentWMO.WmoIndices.Add(offset + 3);
		//    currentWMO.WmoIndices.Add(offset + 7);
		//    currentWMO.WmoIndices.Add(offset + 0);
		//    currentWMO.WmoIndices.Add(offset + 7);
		//    currentWMO.WmoIndices.Add(offset + 4);

		//    // Top face
		//    currentWMO.WmoIndices.Add(offset + 3);
		//    currentWMO.WmoIndices.Add(offset + 2);
		//    currentWMO.WmoIndices.Add(offset + 6);
		//    currentWMO.WmoIndices.Add(offset + 3);
		//    currentWMO.WmoIndices.Add(offset + 6);
		//    currentWMO.WmoIndices.Add(offset + 7);
		//}

		//private static void DrawPositionPoint(Vector3 position, WMORoot currentWMO)
		//{
		//    var color = Color.Green;
		//    var step = TerrainConstants.UnitSize/2;
		//    var topRight = new Vector3(position.X + step, position.Y + step, position.Z);
		//    var topLeft = new Vector3(position.X + step, position.Y - step, position.Z);
		//    var bottomRight = new Vector3(position.X - step, position.Y + step, position.Z);
		//    var bottomLeft = new Vector3(position.X - step, position.Y - step, position.Z);

		//    var offset = currentWMO.WmoVertices.Count;
		//    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(bottomRight, color, Vector3.Up));
		//    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(topRight, color, Vector3.Up));
		//    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(topLeft, color, Vector3.Up));
		//    currentWMO.WmoIndices.Add(offset + 0);
		//    currentWMO.WmoIndices.Add(offset + 1);
		//    currentWMO.WmoIndices.Add(offset + 2);

		//    offset = currentWMO.WmoVertices.Count;
		//    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(bottomRight, color, Vector3.Up));
		//    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(topLeft, color, Vector3.Up));
		//    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(bottomLeft, color, Vector3.Up));
		//    currentWMO.WmoIndices.Add(offset + 0);
		//    currentWMO.WmoIndices.Add(offset + 1);
		//    currentWMO.WmoIndices.Add(offset + 2);
		//}
		#endregion

        private struct RegionalPoint
        {
            public IShape Shape;
            public WVector3 Point;
        }
	}
}
