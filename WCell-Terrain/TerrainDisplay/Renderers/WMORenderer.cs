using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrainDisplay.Util;
using WCell.Terrain.MPQ.WMO;

namespace TerrainDisplay.Renderers
{
	public class WMORenderer : RendererBase
	{
		private readonly IWMOManager _manager;
		private static Color WMOColor { get { return Color.DarkSlateGray; } }
		private static Color WMOModelColor { get { return Color.DarkSlateGray; } }
		private static Color WMOWaterColor { get { return Color.DarkSlateGray; } }

		public WMORenderer(Game game, IWMOManager manager)
			: base(game)
		{
			_manager = manager;
		}

		protected override void BuildVerticiesAndIndicies()
		{
			var vertCount = _manager.WmoVertices.Count +
							_manager.WmoM2Vertices.Count +
							_manager.WmoLiquidVertices.Count;
			_cachedVertices = new VertexPositionNormalColored[vertCount];

			var idxCount = _manager.WmoIndices.Count +
						   _manager.WmoM2Indices.Count +
						   _manager.WmoLiquidIndices.Count;
			_cachedIndices = new int[idxCount];

			var vertices = _manager.WmoVertices;
			var indices = _manager.WmoIndices;

			for (var i = 0; i < vertices.Count; i++)
			{
				var vec = vertices[i];
				XNAUtil.TransformWoWCoordsToXNACoords(ref vec);
				_cachedVertices[i] = new VertexPositionNormalColored(vec.ToXna(), WMOColor, Vector3.Down);
			}
			for (var i = 0; i < indices.Count; i++)
			{
				_cachedIndices[i] = indices[i];
			}

			// Add the M2 stuff
			var vecOffset = vertices.Count;
			vertices = _manager.WmoM2Vertices;
			for (var i = 0; i < vertices.Count; i++)
			{
				var vec = vertices[i];
				XNAUtil.TransformWoWCoordsToXNACoords(ref vec);
				_cachedVertices[i + vecOffset] = new VertexPositionNormalColored(vec.ToXna(), WMOModelColor,
																				 Vector3.Down);
			}
			var idxOffset = indices.Count;
			indices = _manager.WmoM2Indices;
			for (var i = 0; i < indices.Count; i++)
			{
				_cachedIndices[i + idxOffset] = indices[i] + vecOffset;
			}

			// Add the Liquid stuff
			vecOffset += vertices.Count;
			vertices = _manager.WmoLiquidVertices;
			for (var i = 0; i < vertices.Count; i++)
			{
				var vec = vertices[i];
				XNAUtil.TransformWoWCoordsToXNACoords(ref vec);
				_cachedVertices[i + vecOffset] = new VertexPositionNormalColored(vec.ToXna(), WMOWaterColor,
																				 Vector3.Down);
			}
			idxOffset += indices.Count;
			indices = _manager.WmoLiquidIndices;
			for (var i = 0; i < indices.Count; i++)
			{
				_cachedIndices[i + idxOffset] = indices[i] + vecOffset;
			}

			_renderCached = true;
		}
	}
}