using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WCell.Constants.World;
using WCell.MPQTool;
using WCell.Terrain.GUI.UI;
using WCell.Terrain.GUI.Util;
using WCell.Terrain.GUI.Renderers;
using WCell.Terrain.MPQ.DBC;
using WCell.Terrain.Pathfinding;
using WCell.Terrain.Serialization;
using WCell.Util;
using WCell.Util.Graphics;

using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Color = Microsoft.Xna.Framework.Graphics.Color;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using MathHelper = Microsoft.Xna.Framework.MathHelper;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Ray = WCell.Util.Graphics.Ray;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using MenuItem = System.Windows.Forms.MenuItem;

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
		private VertexDeclaration _vertexDeclaration;
		// Another set of XNA Variables
		private Matrix _view;
		private Matrix _proj;

		BasicEffect effect;

		//private readonly ADTManager _manager;
		/// <summary>
		/// Console used to execute commands while the game is running.
		/// </summary>
		//public MpqConsole Console;

		// Camera Stuff
		float avatarYaw, avatarPitch;
		Vector3 _thirdPersonReference = new Vector3(0, 20, -20);
		private bool mouseLeftButtonDown, escapeDown;

		public static Vector3 avatarPosition = new Vector3(-100, 100, -100);

		SpriteBatch _spriteBatch;
		SpriteFont _spriteFont;

		private Pathfinder pathfinder;
		private float globalIlluminationLevel;

		/// <summary>
		/// Constructor for the game.
		/// <param name="tile">The tile to be displayed</param>
		/// </summary>
		public TerrainViewer(Vector3 avatarPosition, TerrainTile tile)
		{
			TerrainViewer.avatarPosition = avatarPosition;
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			avatarYaw = 90;

			Tile = tile;
			pathfinder = new Pathfinder(Tile);
		}

		#region Properties
		public float GlobalIlluminationLevel
		{
			get { return globalIlluminationLevel; }
			set
			{
				globalIlluminationLevel = value;

				effect.DiffuseColor = new Vector3(.95f, .95f, .95f) * value;
				effect.SpecularColor = new Vector3(0.05f, 0.05f, 0.05f) * value;
				effect.AmbientLightColor = new Vector3(0.35f, 0.35f, 0.35f) * value;
			}
		}

		public TerrainTile Tile
		{
			get;
			private set;
		}

		/// <summary>
		/// The form of this application
		/// </summary>
		public Form Form
		{
			get;
			private set;
		}

		public GenericRenderer TriangleSelector
		{
			get;
			private set;
		}
		#endregion


		#region Initialization
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
			InitGUI();

			Form.Activated += (sender, args) => RecenterMouse();

			IsMouseVisible = true;
			_graphics.PreferredBackBufferWidth = 1024;
			_graphics.PreferredBackBufferHeight = 768;
			_graphics.IsFullScreen = false;
			_graphics.ApplyChanges();
			InitializeEffect();


			Components.Add(new AxisRenderer(this));
			Components.Add(new TileRenderer(this));
			if (Tile.Terrain.NavMesh != null)
			{
				Components.Add(new RecastSolidRenderer(this, _graphics, Tile.Terrain.NavMesh));
			}
			Components.Add(TriangleSelector = new GenericRenderer(this));

			base.Initialize();
		}

		private void InitializeEffect()
		{
			effect = new BasicEffect(_graphics.GraphicsDevice, null)
			{
				VertexColorEnabled = true,
				LightingEnabled = true,

				Alpha = 1.0f,
				SpecularPower = 5.0f
			};

			effect.DirectionalLight0.Enabled = true;
			effect.DirectionalLight0.DiffuseColor = Vector3.One;
			effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(1.0f, -1.0f, 0.0f));
			effect.DirectionalLight0.SpecularColor = Vector3.One;

			effect.DirectionalLight1.Enabled = true;
			effect.DirectionalLight1.DiffuseColor = new Vector3(0.1f, 0.1f, 0.1f);
			effect.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(-1.0f, -1.0f, 1.0f));

			GlobalIlluminationLevel = 1.5f;


			_vertexDeclaration = new VertexDeclaration(_graphics.GraphicsDevice, VertexPositionNormalColored.VertexElements);
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
			UpdateAvatarPosition();

			// use mouse navigation

			var w = Form.Width;
			var h = Form.Height;

			if (w != 0 && h != 0 && Form.ActiveForm == Form && !IsMenuVisible)
			{
				var x = Mouse.GetState().X;
				var y = Mouse.GetState().Y;

				var cx = w / 2;
				var cy = h / 2;

				avatarPitch += (y - cy) * (MouseSensitivity / 1000);
				avatarYaw += (cx - x) * (MouseSensitivity / 1000);

				RecenterMouse();
			}


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
			var w = Form.Width;
			var h = Form.Height;
			var cx = w / 2;
			var cy = h / 2;
			Mouse.SetPosition(cx, cy); // move back to center
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			_graphics.GraphicsDevice.Clear(Color.DeepSkyBlue);
			//tree.Draw();
			_graphics.GraphicsDevice.VertexDeclaration = _vertexDeclaration;

			// Update our camera
			UpdateCameraThirdPerson();


			effect.Begin();
			effect.Projection = _proj;
			effect.View = _view;

			foreach (var pass in effect.CurrentTechnique.Passes)
			{
				pass.Begin();
				//_graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
				//_graphics.GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;
				//_graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;

				// Make the renderers draw their stuff
				base.Draw(gameTime);

				//_graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
				pass.End();
			}


			effect.End();

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

		void UpdateCameraThirdPerson()
		{
			// Create a vector pointing the direction the camera is facing.
			//var transformedReference = Vector3.Transform(, Matrix.CreateRotationY(avatarYaw));
			//transformedReference = Vector3.Transform(transformedReference, Matrix.CreateRotationX(avatarPitch));

			// Calculate the position the camera is looking from
			var target = avatarPosition + Vector3.Transform(Vector3.UnitZ, Matrix.CreateFromYawPitchRoll(avatarYaw, avatarPitch, 0));

			// Set up the view matrix and projection matrix
			_view = Matrix.CreateLookAt(avatarPosition, target, new Vector3(0.0f, 1.0f, 0.0f));

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
		void UpdateAvatarPosition()
		{
			var keyboardState = Keyboard.GetState();
			var gamePadState = GamePad.GetState(PlayerIndex.One);
			var mouseState = Mouse.GetState();

			//if (Console.IsOpen()) return;

			if (keyboardState.IsKeyDown(Keys.P))
			{
				System.Console.WriteLine("Open!");
			}

			if (keyboardState.IsKeyDown(Keys.A) || (gamePadState.DPad.Left == ButtonState.Pressed))
			{
				// move left
				//avatarYaw += RotationSpeed;
				var forwardMovement = Matrix.CreateRotationY(avatarYaw);
				var v = new Vector3(1, 0, 0);
				v = Vector3.Transform(v, forwardMovement);
				avatarPosition.X += v.X;
				avatarPosition.Y += v.Y;
				avatarPosition.Z += v.Z;
			}

			if (keyboardState.IsKeyDown(Keys.D) || (gamePadState.DPad.Right == ButtonState.Pressed))
			{
				// move right
				//avatarYaw -= RotationSpeed;

				var forwardMovement = Matrix.CreateRotationY(avatarYaw);
				var v = new Vector3(1, 0, 0);
				avatarPosition -= Vector3.Transform(v, forwardMovement);
			}

			if (keyboardState.IsKeyDown(Keys.W) || (gamePadState.DPad.Up == ButtonState.Pressed))
			{
				//var forwardMovement = Matrix.CreateRotationY(avatarYaw);
				//var v = new Vector3(0, 0, ForwardSpeed);
				//v = Vector3.Transform(v, forwardMovement);
				//avatarPosition.Z += v.Z;
				//avatarPosition.X += v.X;
				var horizontal = avatarPitch.Cos();
				var vertical = -avatarPitch.Sin();
				var v = new Vector3(avatarYaw.Sin() * horizontal, vertical, avatarYaw.Cos() * horizontal);
				v.Normalize();

				var newPos = avatarPosition + v * ForwardSpeed;
				Ray ray;
				var canMove = true;
				if (GetRayToCursor(out ray))
				{
					// something is in front of us
					float dist;
					if (Tile.FindFirstHitTriangle(ray, out dist) != -1)
					{
						// stop moving, if too close
						canMove = dist > CollisionDistance;
					}
				}
				if (canMove)
				{
					avatarPosition = newPos;
				}
			}

			if (keyboardState.IsKeyDown(Keys.S) || (gamePadState.DPad.Down == ButtonState.Pressed))
			{
				var horizontal = avatarPitch.Cos();
				var vertical = -avatarPitch.Sin();
				var v = new Vector3(avatarYaw.Sin() * horizontal, vertical, avatarYaw.Cos() * horizontal);
				v.Normalize();
				avatarPosition -= v * ForwardSpeed;
			}

			if (keyboardState.IsKeyDown(Keys.F))
			{
				avatarPosition.Y = avatarPosition.Y - 1;
			}

			if (keyboardState.IsKeyDown(Keys.R) || keyboardState.IsKeyDown(Keys.Space))
			{
				avatarPosition.Y = avatarPosition.Y + 1;
			}

			if (keyboardState.IsKeyDown(Keys.T))
			{
				_thirdPersonReference.Y = _thirdPersonReference.Y - 0.25f;
			}
			if (keyboardState.IsKeyDown(Keys.G))
			{
				_thirdPersonReference.Y = _thirdPersonReference.Y + 0.25f;
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


			if (mouseState.LeftButton == ButtonState.Pressed && !mouseLeftButtonDown)
			{
				// select polygon
				mouseLeftButtonDown = true;

				//SelectPolygon();

				if (selectedPoints.Count >= 2)
				{
					// clear
					selectedPoints.Clear();
					TriangleSelector.Clear();
				}
				else
				{
					// select
					SelectOnPath();
				}
			}

			if (mouseState.LeftButton == ButtonState.Released && mouseLeftButtonDown)
			{
				mouseLeftButtonDown = false;
			}
		}
		#endregion

		#region Mouse selection
		private readonly List<WCell.Util.Graphics.Vector3> selectedPoints = new List<WCell.Util.Graphics.Vector3>();

		private void SelectOnPath()
		{
			Ray ray;
			if (!GetRayToCursor(out ray))
			{
				// Outside of current map
				return;
			}
			selectedPoints.Add(Tile.IntersectFirstTriangle(ray));

			if (selectedPoints.Count > 1)
			{
				var visited = new HashSet<int>();
				var current = pathfinder.FindPath(1, selectedPoints[0], selectedPoints[1], out visited);

				if (current.IsNull) return;

				foreach (var tri in visited)
				{
					SelectTriangle(tri, true, Color.Red);
				}

				while (!current.IsNull)
				{
					//var tri = Tile.FindFirstTriangleUnderneath(curren);
					SelectTriangle(current.Triangle, true);

					var p1 = current.EnterPos;
					var p2 = current.Previous.IsNull ? selectedPoints[0] : current.Previous.EnterPos;
					TriangleSelector.SelectLine(p1, p2, Color.Green);
					current = current.Previous;
				}
			}
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

			var index = Tile.FindFirstHitTriangle(ray);
			if (index != -1)
			{
				// mark the selected triangle
				SelectTriangle(index, add);

				// also mark it's neighbors
				var neighbors = Tile.GetEdgeNeighborsOf(index);
				foreach (var neighbor in neighbors)
				{
					SelectTriangle(neighbor, true);
				}
			}
		}

		private void SelectTriangle(int index, bool add)
		{
			SelectTriangle(index, add, Color.Yellow);
		}

		void SelectTriangle(int index, bool add, Color color)
		{
			if (index == -1) return;

			Triangle triangle;
			Tile.GetTriangle(index, out triangle);

			// elevate them just a tiny bit, so they do not collide with the original triangle
			triangle.Point1.Z += 0.0001f;
			triangle.Point2.Z += 0.0001f;
			triangle.Point3.Z += 0.0001f;

			TriangleSelector.Select(ref triangle, add, color);
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
			var endPos = new Vector3(x, y, 1);

			//var near = _graphics.GraphicsDevice.Viewport.Unproject(startPos, _proj, _view, Matrix.Identity).ToWCell();
			var near = avatarPosition.ToWCell();
			var far = _graphics.GraphicsDevice.Viewport.Unproject(endPos, _proj, _view, Matrix.Identity).ToWCell();

			XNAUtil.TransformXnaCoordsToWoWCoords(ref near);
			XNAUtil.TransformXnaCoordsToWoWCoords(ref far);

			var dir = (far - near).NormalizedCopy();

			ray = new Ray(near, dir);
			return true;
		}
		#endregion

		private void ToggleSolidRenderingMode()
		{
			if (solidRenderingMode)
			{
				_graphics.GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
				_graphics.GraphicsDevice.RenderState.FillMode = FillMode.Solid;
			}
			else
			{
				_graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
				_graphics.GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;
			}
		}

		#region GUI
		private MainMenu menu;
		private MenuItem renderingModeButton;
		private bool solidRenderingMode = true;
		private TreeView treeView;

		public bool IsMenuVisible
		{
			get
			{
				return Form.Menu != null;
			}
			set
			{
				if (value == IsMenuVisible) return;

				treeView.Visible = value;
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

		/// <summary>
		/// Add some basic GUI controls
		/// </summary>
		void InitGUI()
		{
			Console.WriteLine("");
			Console.WriteLine("Help:");
			Console.WriteLine("  Press ESCAPE to enter the menu");

			Form = (Form)Form.FromHandle(Window.Handle);

			menu = new MainMenu();

			renderingModeButton = new MenuItem();
			renderingModeButton.Click += ClickedRenderingModeButton;
			menu.MenuItems.Add(renderingModeButton);
			ClickedRenderingModeButton(null, null);

			//var exportRecastMeshButton = new MenuItem("Export Tile mesh");
			//exportRecastMeshButton.Click += ExportRecastMesh;
			//menu.MenuItems.Add(exportRecastMeshButton);


			treeView = new TreeView();
			treeView.Dock = DockStyle.Left;
			treeView.DoubleClick += ClickedTreeView;
			treeView.Width = 200;
			treeView.Visible = IsMenuVisible;

			// build list of zones
			var zoneTileSets = ZoneBoundaries.GetZoneTileSets();
			var zoneNodes = new Tuple<ZoneId, List<Point2D>>[(int)ZoneId.End];
			for (var map = 0; map < zoneTileSets.Length; map++)
			{
				var tileSet = zoneTileSets[map];
				if (tileSet == null || MapInfo.GetMapInfo((MapId) map) == null) continue;	// map does not exist

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
								var node = zoneNodes[(int) zone];
								var coords = new Point2D(x, y);
								if (!WCellTerrainSettings.GetDefaultMPQFinder().FileExists(ADTReader.GetFilename((MapId) map, coords)))
								{
									// tile does not exist
									continue;
								}

								if (node == null)
								{
									// Only add Zone node if it has at least one tile
									children.Add(zoneNodes[(int) zone] = node = new Tuple<ZoneId, List<Point2D>>(zone, new List<Point2D>()));
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
					treeView.Nodes.Add(new MapTreeNode((MapId)map, children.ToArray().TransformArray(tuple =>
							new ZoneTreeNode((MapId)map, tuple.Item1, tuple.Item2))));
				}
			}

			Form.Controls.Add(treeView);
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

		private TextBox waitingBox;

		private void ClickedTreeView(object sender, EventArgs e)
		{
			var node = treeView.SelectedNode;
			if (node is TileTreeNode)
			{
				var tnode = ((TileTreeNode)node);

				// GUI stuff
				IsMenuVisible = false;
				if (waitingBox == null)
				{
					waitingBox = new TextBox();
					waitingBox.Text = "Loading Tile - Please wait...";
					waitingBox.Dock = DockStyle.Fill;
					waitingBox.ForeColor = System.Drawing.Color.DarkGreen;
					waitingBox.BackColor = System.Drawing.Color.Black;
					waitingBox.Margin = new Padding(0, 0, 0, 0);
					waitingBox.BorderStyle = BorderStyle.None;
					waitingBox.TextAlign = HorizontalAlignment.Center;
					Form.Controls.Add(waitingBox);
				}
				else
				{
					waitingBox.Visible = true;
				}

				// load new tile
				Tile = TerrainViewerProgram.GetOrCreateTile(tnode.Map, tnode.Coords);

				// reset renderers
				foreach (var component in Components)
				{
					if (component is RendererBase)
					{
						((RendererBase)component).Clear();
					}
				}

				// update node
				tnode.BackColor = System.Drawing.Color.White;

				// reset GUI
				waitingBox.Visible = false;

				// update avatar
				var topRight = Tile.Bounds.TopRight;
				var bottomLeft = Tile.Bounds.BottomLeft;
				avatarPosition = new Vector3(topRight.X, topRight.Y, 200);
				XNAUtil.TransformWoWCoordsToXNACoords(ref avatarPosition);


				avatarYaw = 45;
			}
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
	}
}
