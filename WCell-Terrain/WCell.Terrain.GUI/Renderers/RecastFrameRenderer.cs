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
	public class RecastSolidRenderer : RecastRendererBase
	{
		private static Color MeshPolyColor { get { return new Color(65, 50, 50, 100); } }

		public RecastSolidRenderer(Game game)
			: base(game)
		{
		}

		public override void Draw(GameTime gameTime)
		{
			var graphics = Game.GraphicsDevice;
			var depthBias = graphics.RenderState.DepthBias;
			var fillMode = graphics.RenderState.FillMode;

			graphics.RenderState.DepthBias = 5e-7f;
			graphics.RenderState.FillMode = FillMode.Solid;

			base.Draw(gameTime);
			
			graphics.RenderState.DepthBias = depthBias;
			graphics.RenderState.FillMode = fillMode;
		}

		protected override void BuildVerticiesAndIndicies()
		{
			var mesh = ((TerrainViewer)Game).Tile.NavMesh;
			var vertices = mesh.Vertices;
			List<int> indices;

			mesh.GetTriangles(out indices);

			if (vertices.Length == 0 || indices.Count == 0) return;


			// TODO: Interpolate normals

			_cachedVertices = new VertexPositionNormalColored[vertices.Length];
			for (var i = 0; i < vertices.Length; i++)
			{
				var vertex = vertices[i];
				XNAUtil.TransformWoWCoordsToXNACoords(ref vertex);
				_cachedVertices[i] = new VertexPositionNormalColored(vertex.ToXna(),
																		MeshPolyColor,
																		Vector3.Up.ToXna());
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
