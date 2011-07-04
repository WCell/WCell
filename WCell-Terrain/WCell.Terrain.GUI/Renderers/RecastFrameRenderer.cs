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
		private static Color MeshPolyColor { get { return Color.WhiteSmoke; } }

		public RecastSolidRenderer(Game game, GraphicsDeviceManager graphics, NavMesh mesh)
			: base(game, mesh)
		{
			_graphics = graphics;
		}

		public override void Draw(GameTime gameTime)
		{
			var depthBias = _graphics.GraphicsDevice.RenderState.DepthBias;
			var fillMode = _graphics.GraphicsDevice.RenderState.FillMode;

			if (_graphics != null)
			{
				//_graphics.GraphicsDevice.RenderState.DepthBias = 5;
				_graphics.GraphicsDevice.RenderState.FillMode = FillMode.Solid;
			}

			base.Draw(gameTime);

			if (_graphics != null)
			{
				_graphics.GraphicsDevice.RenderState.DepthBias = depthBias;
				_graphics.GraphicsDevice.RenderState.FillMode = fillMode;
			}
		}

		protected override void BuildVerticiesAndIndicies()
		{
			var vertices = Mesh.Vertices;
			List<int> indices;

			Mesh.GetTriangles(out indices);

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
