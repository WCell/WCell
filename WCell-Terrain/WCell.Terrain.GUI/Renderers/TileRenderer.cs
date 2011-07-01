using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WCell.Terrain.Collision;
using WCell.Terrain.GUI.Util;
using WCell.Terrain.MPQ;
using WCell.Util.Graphics;
using Color = Microsoft.Xna.Framework.Graphics.Color;
using Ray = Microsoft.Xna.Framework.Ray;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace WCell.Terrain.GUI.Renderers
{
	class TileRenderer : DrawableGameComponent
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

		/// <summary>
		/// Boolean variable representing if all the rendering data has been cached.
		/// </summary>
		private bool _renderCached;

		private VertexPositionNormalColored[] _cachedVertices;
		private int[] _cachedIndices;

		private VertexDeclaration _vertexDeclaration;


		public TileRenderer(Game game, TerrainTile tile)
			: base(game)
		{
			Tile = tile;
		}

		public TerrainTile Tile
		{
			get;
			private set;
		}

		public override void Initialize()
		{
			base.Initialize();

			_vertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionNormalColored.VertexElements);
		}

		public override void Draw(GameTime gameTime)
		{
			var vertices = GetRenderingVerticies();
			var indices = GetRenderingIndices();

			GraphicsDevice.VertexDeclaration = _vertexDeclaration;
			GraphicsDevice.DrawUserIndexedPrimitives(
				PrimitiveType.TriangleList,
				vertices,
				0, // vertex buffer offset to add to each element of the index buffer
				vertices.Length, // number of vertices to draw
				indices,
				0, // first index element to read
				indices.Length / 3 // number of primitives to draw
				);

			base.Draw(gameTime);
		}

		private VertexPositionNormalColored[] GetRenderingVerticies()
		{
			if (_renderCached)
			{
				return _cachedVertices;
			}

			BuildVerticiesAndIndicies();
			return _cachedVertices;
		}

		private int[] GetRenderingIndices()
		{
			if (_renderCached)
			{
				return _cachedIndices;
			}

			BuildVerticiesAndIndicies();
			return _cachedIndices;
		}

		#region Build polygons
		private void BuildVerticiesAndIndicies()
		{
			// Cycle through each ADT
			var tempVertices = new List<VertexPositionNormalColored>();
			var tempIndicies = new List<int>();
			var offset = 0;

			// Handle the ADTs
			for (var v = 0; v < Tile.TerrainVertices.Length; v++)
			{
				var vertex = Tile.TerrainVertices[v];
				var vertexPosNmlCol = new VertexPositionNormalColored(vertex.ToXna(),
																		TerrainColor,
																		Vector3.Down);
				tempVertices.Add(vertexPosNmlCol);
			}
			for (var i = 0; i < Tile.TerrainIndices.Length; i++)
			{
				tempIndicies.Add(Tile.TerrainIndices[i] + offset);
			}
			offset = tempVertices.Count;

			//if (!DrawLiquids) continue;
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
			offset = tempVertices.Count;

			_cachedIndices = tempIndicies.ToArray();
			_cachedVertices = tempVertices.ToArray();

			_renderCached = true;

			for (var i = 0; i < _cachedVertices.Length; i++)
			{
				XNAUtil.TransformWoWCoordsToXNACoords(ref _cachedVertices[i]);
			}
		}
		#endregion
	}
}
