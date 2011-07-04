using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WCell.Terrain.GUI.Util;
using Color = Microsoft.Xna.Framework.Graphics.Color;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace WCell.Terrain.GUI.Renderers
{
	class TileRenderer : RendererBase
	{
		private const bool DrawLiquids = false;
		private static Color TerrainColor
		{
			get { return Color.DarkSlateGray; }
			//get { return Color.Green; }
		}

		private static Color WaterColor
		{
			//get { return Color.DarkSlateGray; }
			get { return Color.Blue; }
		}


		public TileRenderer(Game game)
			: base(game)
		{
		}

		public TerrainTile Tile
		{
			get;
			private set;
		}

		#region Build polygons
		protected override void BuildVerticiesAndIndicies()
		{
			// Cycle through each ADT
			var viewer = (TerrainViewer) Game;
			Tile = viewer.Tile;

			var tempVertices = new List<VertexPositionNormalColored>();
			var tempIndicies = new List<int>();
			var offset = 0;

			// Handle the ADTs
			for (var v = 0; v < Tile.TerrainVertices.Length; v++)
			{
				var vertex1 = Tile.TerrainVertices[v];

				var vertexPosNmlCol1 = new VertexPositionNormalColored(vertex1.ToXna(),
																		TerrainColor,
																		Vector3.Up);
				tempVertices.Add(vertexPosNmlCol1);
			}

			for (var i = 0; i < Tile.TerrainIndices.Length; i += 3)
			{
				var index1 = Tile.TerrainIndices[i];
				var index2 = Tile.TerrainIndices[i+1];
				var index3 = Tile.TerrainIndices[i+2];
				tempIndicies.Add(index1);
				tempIndicies.Add(index2);
				tempIndicies.Add(index3);

				var vertex1 = tempVertices[index1];
				var vertex2 = tempVertices[index2];
				var vertex3 = tempVertices[index3];

				var normal = Vector3.Cross(vertex2.Position - vertex1.Position, vertex3.Position - vertex1.Position);

				vertex1.Normal += normal;
				vertex2.Normal += normal;
				vertex3.Normal += normal;

				tempVertices[index1] = vertex1;
				tempVertices[index2] = vertex2;
				tempVertices[index3] = vertex3;
			}
			offset = tempVertices.Count;

			//if (!DrawLiquids) continue;
			if (Tile.LiquidVertices != null)
			{
				for (var v = 0; v < Tile.LiquidVertices.Count; v++)
				{
					var vertex = Tile.LiquidVertices[v];
					var vertexPosNmlCol = new VertexPositionNormalColored(vertex.ToXna(),
					                                                      WaterColor,
					                                                      Vector3.Down);
					tempVertices.Add(vertexPosNmlCol);
				}
				for (var i = 0; i < Tile.LiquidIndices.Count; i++)
				{
					tempIndicies.Add(Tile.LiquidIndices[i] + offset);
				}
			}
			offset = tempVertices.Count;

			_cachedIndices = tempIndicies.ToArray();
			_cachedVertices = tempVertices.ToArray();

			_renderCached = true;

			for (var i = 0; i < _cachedVertices.Length; i++)
			{
				XNAUtil.TransformWoWCoordsToXNACoords(ref _cachedVertices[i]);
				_cachedVertices[i].Normal.Normalize();
			}
		}
		#endregion
	}
}
