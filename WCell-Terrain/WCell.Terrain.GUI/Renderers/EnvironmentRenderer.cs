using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WCell.Terrain.GUI.Util;
using Color = Microsoft.Xna.Framework.Graphics.Color;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace WCell.Terrain.GUI.Renderers
{
	class EnvironmentRenderer : RendererBase
	{
		private static Color TerrainColor
		{
			get { return Color.DarkSlateGray; }
			//get { return Color.Green; }
		}


		public EnvironmentRenderer(Game game)
			: base(game)
		{
		}

		#region Build polygons
		protected override void BuildVerticiesAndIndicies()
		{
			var viewer = (TerrainViewer) Game;
			var tile = viewer.Tile;

			var tempVertices = new List<VertexPositionNormalColored>();
			var tempIndicies = new List<int>();

			for (var v = 0; v < tile.TerrainVertices.Length; v++)
			{
				var vertex1 = tile.TerrainVertices[v];
				XNAUtil.TransformWoWCoordsToXNACoords(ref vertex1);
				var vertexPosNmlCol1 = new VertexPositionNormalColored(vertex1.ToXna(),
																		TerrainColor,
																		Vector3.Zero);
				tempVertices.Add(vertexPosNmlCol1);
			}

			for (var i = 0; i < tile.TerrainIndices.Length; i += 3)
			{
				var index1 = tile.TerrainIndices[i];
				var index2 = tile.TerrainIndices[i+1];
				var index3 = tile.TerrainIndices[i+2];
				tempIndicies.Add(index1);
				tempIndicies.Add(index2);
				tempIndicies.Add(index3);
			}

			_cachedIndices = tempIndicies.ToArray();
			_cachedVertices = tempVertices.ToArray();

			_renderCached = true;
		}
		#endregion
	}
}
