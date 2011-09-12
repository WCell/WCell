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

		public SolidNavMeshRenderer(Game game, TerrainTile tile)
			: base(game, tile)
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
		public WireframeNavMeshRenderer(Game game, TerrainTile tile)
			: base(game, tile)
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
	    private TerrainTile tile;
		private RasterizerState rasterState;
        //private readonly BlendState alphaBlendState = BlendState.Additive;
        readonly BlendState alphaBlendState = new BlendState
        {
            AlphaBlendFunction = BlendFunction.Add,
            AlphaSourceBlend = Blend.SourceAlpha,
            ColorSourceBlend = Blend.SourceAlpha,
            AlphaDestinationBlend = Blend.InverseSourceAlpha,
            ColorDestinationBlend = Blend.InverseSourceAlpha
        };


		internal NavMeshRenderer(Game game, TerrainTile tile)
			: base(game)
		{
		    this.tile = tile;
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
		    var oldDepthStencilState = GraphicsDevice.DepthStencilState;
			var oldRasterState = GraphicsDevice.RasterizerState;
			var oldBlendState = GraphicsDevice.BlendState;

            GraphicsDevice.RasterizerState = rasterState;
			GraphicsDevice.BlendState = alphaBlendState;
            
			base.Draw(gameTime);

		    GraphicsDevice.DepthStencilState = oldDepthStencilState;
			GraphicsDevice.RasterizerState = oldRasterState;
			GraphicsDevice.BlendState = oldBlendState;
		}

		protected override void BuildVerticiesAndIndicies()
		{
            List<int> indices;
            using (LargeObjectPools.IndexListPool.Borrow(out indices))
            {
                var tempVertices = new List<VertexPositionNormalColored>();
                
                indices.Clear();

                var mesh = tile.NavMesh;
                mesh.GetTriangles(indices);

                if (mesh.Vertices.Length == 0 || indices.Count == 0) return;

                foreach (var vertex in mesh.Vertices)
                {
                    tempVertices.Add(new VertexPositionNormalColored(vertex.ToXna(), RenderColor, Vector3.Zero));
                }

                _cachedIndices = indices.ToArray();
                _cachedVertices = tempVertices.ToArray();
            }

		    _renderCached = true;
		}
	}
}
