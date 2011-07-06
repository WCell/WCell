using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WCell.Terrain.GUI.Util;
using WCell.Terrain.Recast.NavMesh;
using WCell.Util;
using Vector3 = WCell.Util.Graphics.Vector3;

namespace WCell.Terrain.GUI.Renderers
{
	/// <summary>
	/// Render Recast NavMesh in XNA
	/// </summary>
	public class SolidNavMeshRenderer : NavMeshRenderer
	{

		public SolidNavMeshRenderer(Game game)
			: base(game)
		{
		}

		protected override Color RenderColor
		{
			get { return new Color(65, 50, 50, 100); }
		}

		protected override FillMode RenderMode
		{
			get { return FillMode.Solid; }
		}
	}

	/// <summary>
	/// Wireframe renderer of navmesh
	/// </summary>
	public class WireframeNavMeshRenderer : NavMeshRenderer
	{
		public WireframeNavMeshRenderer(Game game)
			: base(game)
		{
		}

		protected override Color RenderColor
		{
			get { return new Color(180, 70, 70, 90); }
		}

		protected override FillMode RenderMode
		{
			get { return FillMode.WireFrame; }
		}
	}

	public abstract class NavMeshRenderer : RendererBase
	{
		private RasterizerState rasterState;
		readonly BlendState alphaBlendState = new BlendState()
		{
			AlphaBlendFunction = BlendFunction.Add,
			AlphaSourceBlend = Blend.SourceAlpha,
			ColorSourceBlend = Blend.SourceAlpha,
			AlphaDestinationBlend = Blend.InverseSourceAlpha,
			ColorDestinationBlend = Blend.InverseSourceAlpha
		};


		internal NavMeshRenderer(Game game)
			: base(game)
		{
		}

		protected abstract Color RenderColor { get; }

		protected abstract FillMode RenderMode { get; }

		public override void Initialize()
		{
			rasterState = new RasterizerState()
			{
				DepthBias = 1e-6f,
				FillMode = RenderMode
			};

			base.Initialize();
		}

		public override void Draw(GameTime gameTime)
		{
			var graphics = Game.GraphicsDevice;

			var oldRasterState = graphics.RasterizerState;
			var oldBlendState = graphics.BlendState;

			graphics.RasterizerState = rasterState;
			graphics.BlendState = alphaBlendState;

			base.Draw(gameTime);

			graphics.RasterizerState = oldRasterState;
			graphics.BlendState = oldBlendState;
		}

		protected override void BuildVerticiesAndIndicies()
		{
			var mesh = Viewer.Tile.NavMesh;
			var vertices = mesh.Vertices;

			List<int> indices;
			mesh.GetTriangles(out indices);

			if (vertices.Length == 0 || indices.Count == 0) return;

			_cachedVertices = new VertexPositionNormalColored[vertices.Length];
			for (var i = 0; i < vertices.Length; i++)
			{
				var vertex = vertices[i];
				_cachedVertices[i] = new VertexPositionNormalColored(vertex.ToXna(),
																		RenderColor,
																		Microsoft.Xna.Framework.Vector3.Zero);
			}

			_cachedIndices = new int[indices.Count];
			for (int i = 0; i < indices.Count; i++)
			{
				_cachedIndices[i] = indices[i];
			}

			_renderCached = true;
		}
	}
}
