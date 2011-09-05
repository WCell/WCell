using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WCell.Util.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace WCell.Terrain.GUI.Renderers
{
	public class GenericRenderer : RendererBase
	{
		public static Color SelectedTriangleColor = new Color(50, 90, 90);

		private RasterizerState rasterState = new RasterizerState
		{
			CullMode = CullMode.None,
			FillMode = FillMode.Solid,
			DepthBias = 1e-6f
		};

		public GenericRenderer(Game game)
			: base(game)
		{
			_cachedVertices = new VertexPositionNormalColored[0];
			_cachedIndices = new int[0];
		}

		public void SelectLine(WCell.Util.Graphics.Vector3 p1, WCell.Util.Graphics.Vector3 p2, Color color, bool doNotReplace = true)
		{
			const float halfLineWidth = 0.5f;

			var s1 = p1 - halfLineWidth;
			var s2 = p1 + halfLineWidth;

			var t1 = p2 - halfLineWidth;
			var t2 = p2 + halfLineWidth;

			s1.Z = s2.Z = Math.Max(s1.Z, s2.Z);
			t1.Z = t2.Z = Math.Max(t1.Z, t2.Z);
			var tri1 = new Triangle(s1, s2, t2);
			var tri2 = new Triangle(t2, t1, s1);

			Select(ref tri1, doNotReplace, color);
			Select(ref tri2, true, color);
		}

		public void SelectPoint(WCell.Util.Graphics.Vector3 p, Color color, bool doNotReplace = true)
		{
			const float sideDist = 0.5f;

			var p1 = p;
			var p2 = p;
			var p3 = p;

			p1.X += sideDist;

			p2.X -= sideDist;
			p2.Y -= sideDist;

			p3.X -= sideDist;
			p3.Y += sideDist;

			var tri1 = new Triangle(p1, p2, p3);

			Select(ref tri1, doNotReplace, color);
		}

		public void Select(ref Triangle tri, bool doNotReplace =  true)
		{
			Select(ref tri, doNotReplace, SelectedTriangleColor);
		}

		public void Select(ref Triangle tri, bool doNotReplace, Color color)
		{
			var v = _cachedVertices.Length;
			var i = _cachedIndices.Length;

			if (doNotReplace)
			{
				Array.Resize(ref _cachedVertices, _cachedVertices.Length + 3);
				Array.Resize(ref _cachedIndices, _cachedIndices.Length + 3);
			}
			else
			{
				v = i = 0;
				_cachedVertices = new VertexPositionNormalColored[3];
				_cachedIndices = new int[3];
			}

			// add vertices
			var normal = WCell.Util.Graphics.Vector3.Cross(tri.Point3 - tri.Point1, tri.Point2 - tri.Point1).ToXna();
			normal.Normalize();

			_cachedVertices[v] = new VertexPositionNormalColored(tri.Point1.ToXna(),
																	color,
																	normal);

			_cachedVertices[v+1] = new VertexPositionNormalColored(tri.Point2.ToXna(),
																	color,
																	normal);

			_cachedVertices[v+2] = new VertexPositionNormalColored(tri.Point3.ToXna(),
																	color,
																	normal);


			// add indices
			_cachedIndices[i] = v;
			_cachedIndices[i+1] = v+1;
			_cachedIndices[i+2] = v+2;
		}

		public override void Clear()
		{
			base.Clear();

			Array.Resize(ref _cachedVertices, 0);
			Array.Resize(ref _cachedIndices, 0);
		}

		protected override void BuildVerticiesAndIndicies()
		{
			// do nothing
		}

		public override void Draw(GameTime gameTime)
		{
			var graphics = Game.GraphicsDevice;

		    var oldBlendState = graphics.BlendState;
			var oldRasterState = graphics.RasterizerState;
		    var oldDepthStencilState = graphics.DepthStencilState;

            graphics.RasterizerState = rasterState;

            base.Draw(gameTime);

		    graphics.BlendState = oldBlendState;
			graphics.RasterizerState = oldRasterState;
		    graphics.DepthStencilState = oldDepthStencilState;
		}
	}
}
