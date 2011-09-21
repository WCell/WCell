using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WCell.Terrain.GUI.Util;
using WCell.Terrain.Recast.NavMesh;
using WCell.Util;
using WVector3 = WCell.Util.Graphics.Vector3;

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
				CullMode = CullMode.None,
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
		    var tiles = Viewer.Tiles;

            List<int> indices;
            using (LargeObjectPools.IndexListPool.Borrow(out indices))
            {
                var tempIndices = new List<int>();
                var tempVertices = new List<VertexPositionNormalColored>();

                foreach (var tile in tiles)
                {
                    indices.Clear();

                    var mesh = tile.NavMesh;
                    mesh.GetTriangles(indices);

                    if (mesh.Vertices.Length == 0 || indices.Count == 0) continue;

                    var offset = tempVertices.Count;
                    foreach (var vertex in mesh.Vertices)
                    {
                        tempVertices.Add(new VertexPositionNormalColored(vertex.ToXna(), RenderColor, Vector3.Zero));
                    }

                    foreach (var index in indices)
                    {
                        tempIndices.Add(offset + index);
                    }
                }

                _cachedIndices = tempIndices.ToArray();
                _cachedVertices = tempVertices.ToArray();
            }

		    _renderCached = true;
		}
	}
}
