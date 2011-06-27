using System;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TerrainDisplay.Collision;
using TerrainDisplay.MPQ.ADT;
using TerrainDisplay.MPQ.M2;
using TerrainDisplay.MPQ.WMO;
using TerrainDisplay.Recast;
using TerrainDisplay.Util;
using WCell.Util.Graphics;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Color = Microsoft.Xna.Framework.Graphics.Color;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using MathHelper = Microsoft.Xna.Framework.MathHelper;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Ray = WCell.Util.Graphics.Ray;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace TerrainDisplay
{
	/// <summary>
	/// This is the class that controls the entire game.
	/// </summary>
	public class Game1 : Game
	{
		const float ViewAngle = MathHelper.PiOver4;
		const float NearClip = 1.0f;
		const float FarClip = 2000.0f;
		const float MouseSensitivity = 0.008f;

		// XNA uses this variable for graphics information
		private readonly GraphicsDeviceManager _graphics;
		// We use this to declare our verticies
		private VertexDeclaration _vertexDeclaration;
		// Another set of XNA Variables
		private Matrix _view;
		private Matrix _proj;

		BasicEffect _basicEffect;
		float ForwardSpeed = 50f / 60f;
		
		//private readonly ADTManager _manager;
		/// <summary>
		/// Console used to execute commands while the game is running.
		/// </summary>
		public MpqConsole Console;

		// Camera Stuff
		Vector3 avatarHeadOffset = new Vector3(0, 10, 0);
		float avatarYaw, avatarPitch;
		Vector3 cameraReference = new Vector3(0, 0, 10);
		Vector3 _thirdPersonReference = new Vector3(0, 20, -20);
		private bool mouseLeftButtonDown;

		/// <summary>
		/// The form of this application
		/// </summary>
		public Form Form
		{
			get;
			private set;
		}

		public static Vector3 avatarPosition = new Vector3(-100, 100, -100);

		SpriteBatch _spriteBatch;
		SpriteFont _spriteFont;

		/// <summary>
		/// Constructor for the game.
		/// </summary>
		public Game1(Vector3 avatarPosition)
		{
			Game1.avatarPosition = avatarPosition;
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			avatarYaw = 90;
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
			Form = (Form)Form.FromHandle(Window.Handle);
		    IsMouseVisible = true;
			_graphics.PreferredBackBufferWidth = 1024;
			_graphics.PreferredBackBufferHeight = 768;
			_graphics.IsFullScreen = false;
			_graphics.ApplyChanges();
			InitializeEffect();

			// TODO: Add your initialization logic here
			Components.Add(new RecastFrameRenderer(this, _graphics, TerrainProgram.TerrainManager.MeshManager));
			Components.Add(new RecastSolidRenderer(this, _graphics, TerrainProgram.TerrainManager.MeshManager));
			Components.Add(new AxisRenderer(this));
			Components.Add(new ADTRenderer(this, TerrainProgram.TerrainManager.ADTManager));
			Components.Add(new M2Renderer(this, TerrainProgram.TerrainManager.M2Manager));
			Components.Add(new WMORenderer(this, TerrainProgram.TerrainManager.WMOManager));
			Components.Add(new SelectedTriangleRender(this, TerrainProgram.TerrainManager.SelectedTriangleManager));
			
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

			// use mouse navigation

			var w = Form.Width;
			var h = Form.Height;

			if (w != 0 && h != 0 && Form.ActiveForm == Form)
			{
				var x = Mouse.GetState().X;
				var y = Mouse.GetState().Y;

				var cx = w/2;
				var cy = h/2;

				avatarPitch += (y - cy) * MouseSensitivity;
				avatarYaw += (cx - x) * MouseSensitivity;

				Mouse.SetPosition(cx, cy); // move back to center
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
			_basicEffect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(1.0f, -1.0f, 0.0f));
			_basicEffect.DirectionalLight0.SpecularColor = Vector3.One;

			_basicEffect.DirectionalLight1.Enabled = true;
			_basicEffect.DirectionalLight1.DiffuseColor = new Vector3(0.1f, 0.1f, 0.1f);
			_basicEffect.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(-1.0f, -1.0f, 1.0f));

			_basicEffect.LightingEnabled = true;
			
			foreach (var pass in _basicEffect.CurrentTechnique.Passes)
			{
				pass.Begin();
				_graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
				_graphics.GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;
				_graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
				
				// Make the renderers draw their stuff
				base.Draw(gameTime);
				
				_graphics.GraphicsDevice.RenderState.FillMode = FillMode.Solid;
				_graphics.GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
				_graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
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
			var mouseState = Mouse.GetState();

			//if (Console.IsOpen()) return;

			if(keyboardState.IsKeyDown(Keys.P))
			{
				System.Console.WriteLine("Open!");
			}

			if (keyboardState.IsKeyDown(Keys.A) || (currentState.DPad.Left == ButtonState.Pressed))
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

			if (keyboardState.IsKeyDown(Keys.D) || (currentState.DPad.Right == ButtonState.Pressed))
			{
				// move right
				//avatarYaw -= RotationSpeed;

				var forwardMovement = Matrix.CreateRotationY(avatarYaw);
				var v = new Vector3(1, 0, 0);
				v = Vector3.Transform(v, forwardMovement);
				avatarPosition.X -= v.X;
				avatarPosition.Y -= v.Y;
				avatarPosition.Z -= v.Z;
			}

			if (keyboardState.IsKeyDown(Keys.W) || (currentState.DPad.Up == ButtonState.Pressed))
			{
				var forwardMovement = Matrix.CreateRotationY(avatarYaw);
				var v = new Vector3(0, 0, ForwardSpeed);
				v = Vector3.Transform(v, forwardMovement);
				avatarPosition.Z += v.Z;
				avatarPosition.X += v.X;
			}

			if (keyboardState.IsKeyDown(Keys.S) || (currentState.DPad.Down == ButtonState.Pressed))
			{
				var forwardMovement = Matrix.CreateRotationY(avatarYaw);
				var v = new Vector3(0, 0, -ForwardSpeed);
				v = Vector3.Transform(v, forwardMovement);
				avatarPosition.Z += v.Z;
				avatarPosition.X += v.X;
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


		    if (mouseState.LeftButton == ButtonState.Pressed && !mouseLeftButtonDown)
		    {
				// select polygon
		        mouseLeftButtonDown = true;

		        var width = _graphics.GraphicsDevice.Viewport.Width;
		        var height = _graphics.GraphicsDevice.Viewport.Height;
                var x = mouseState.X;
		        var y = mouseState.Y;

                if (x < 0 || y < 0 || x > width || y > height) return;
                
                var add = (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift));

		        var startPos = new Vector3(x, y, 0);
		        var endPos = new Vector3(x, y, 1);

		        var near = _graphics.GraphicsDevice.Viewport.Unproject(startPos, _proj, _view, Matrix.Identity).ToWCell();
		        var far = _graphics.GraphicsDevice.Viewport.Unproject(endPos, _proj, _view, Matrix.Identity).ToWCell();

		        var dir = (far - near).NormalizedCopy();
		        var ray = new Ray(near, dir);

		        TerrainProgram.TerrainManager.SelectedTriangleManager.UpdateSelectedTriangle(ray, add);
		    }

		    if (mouseState.LeftButton == ButtonState.Released && mouseLeftButtonDown)
		    {
		        mouseLeftButtonDown = false;
		    }
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
			var aspectRatio = viewport.Width/(float)viewport.Height;
			_proj = Matrix.CreatePerspectiveFieldOfView(ViewAngle, aspectRatio, NearClip, FarClip);
		}
	}
}
