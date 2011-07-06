using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WCell.Terrain.GUI.Util;

namespace WCell.Terrain.GUI.Renderers
{
	public class LiquidRenderer : RendererBase
	{
		private static Color WaterColor
		{
			//get { return Color.DarkSlateGray; }
			get { return new Color(0, 0, 70, 100); }
		}

		public LiquidRenderer(Game game) : base(game)
		{
		}

		protected override void BuildVerticiesAndIndicies()
		{
			var viewer = (TerrainViewer) Game;
			var tile = viewer.Tile;

			List<int> indices;
			using (LargeObjectPools.IndexListPool.Borrow(out indices))
			{
				List <WCell.Util.Graphics.Vector3> vertices;
				
				using (LargeObjectPools.Vector3ListPool.Borrow(out vertices))
				{
					var tempVertices = new List<VertexPositionNormalColored>();

					tile.GenerateLiquidMesh(indices, vertices);

					if (vertices != null)
					{	
						for (var v = 0; v < vertices.Count; v++)
						{
							var vertex = vertices[v];
							XNAUtil.TransformWoWCoordsToXNACoords(ref vertex);
							var vertexPosNmlCol = new VertexPositionNormalColored(vertex.ToXna(),
							                                                      WaterColor,
							                                                      Vector3.Zero);
							tempVertices.Add(vertexPosNmlCol);
						}
					}

					_cachedIndices = indices.ToArray();
					_cachedVertices = tempVertices.ToArray();
				}
			}
			_renderCached = true;
		}
	}
}
