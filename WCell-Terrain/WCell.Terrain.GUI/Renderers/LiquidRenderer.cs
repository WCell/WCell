using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WCell.Terrain.GUI.Util;
using WVector3 = WCell.Util.Graphics.Vector3;

namespace WCell.Terrain.GUI.Renderers
{
	public class LiquidRenderer : RendererBase
	{
		private static Color WaterColor = new Color(0, 0, 70, 50);
	    
	    private TerrainTile tile;

        readonly BlendState alphaBlendState = new BlendState()
		{
			AlphaBlendFunction = BlendFunction.Add,
			AlphaSourceBlend = Blend.SourceAlpha,
			ColorSourceBlend = Blend.SourceAlpha,
			AlphaDestinationBlend = Blend.InverseSourceAlpha,
			ColorDestinationBlend = Blend.InverseSourceAlpha
		};

		public LiquidRenderer(Game game, TerrainTile tile) : base(game)
		{
		    this.tile = tile;
		}

		public override void Draw(GameTime gameTime)
		{
			var oldBlendState = Game.GraphicsDevice.BlendState;

			Game.GraphicsDevice.BlendState = alphaBlendState;
			base.Draw(gameTime);
			Game.GraphicsDevice.BlendState = oldBlendState;
		}

		protected override void BuildVerticiesAndIndicies()
		{
		    List<int> indices;
			using (LargeObjectPools.IndexListPool.Borrow(out indices))
			{
				List <WCell.Util.Graphics.Vector3> vertices;
				
				using (LargeObjectPools.Vector3ListPool.Borrow(out vertices))
				{
				    indices.Clear();
				    vertices.Clear();
				    var tempVertices = new List<VertexPositionNormalColored>();
				    
                    tile.GenerateLiquidMesh(indices, vertices);

				    if (vertices.Count == 0) return;

				    foreach (var vertex in vertices)
				    {
				        var vertexPosNmlCol = new VertexPositionNormalColored(vertex.ToXna(),
				                                                              WaterColor,
				                                                              Vector3.Zero);
				        tempVertices.Add(vertexPosNmlCol);
				    }

				    _cachedIndices = indices.ToArray();
				    _cachedVertices = tempVertices.ToArray();
				}
			}

			_renderCached = true;
		}
	}
}
