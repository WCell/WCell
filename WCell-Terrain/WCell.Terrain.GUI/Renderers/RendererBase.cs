using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using WCell.Util;

namespace WCell.Terrain.GUI.Renderers
{
	public abstract class RendererBase : DrawableGameComponent
	{
		/// <summary>
		/// Boolean variable representing if all the rendering data has been cached.
		/// </summary>
		protected bool _renderCached;

		protected VertexPositionNormalColored[] _cachedVertices;
		protected int[] _cachedIndices;
		protected VertexDeclaration _vertexDeclaration;

		protected RendererBase(Game game)
			: base(game)
		{
		}

		public TerrainViewer Viewer
		{
			get { return (TerrainViewer)Game; }
		}

		public VertexPositionNormalColored[] RenderingVerticies
		{
			get
			{
				if (!_renderCached)
				{
					DoBuildVerticesAndIndices();
				}

				return _cachedVertices;
			}
		}

		public int[] RenderingIndices
		{
			get { return _cachedIndices; }
		}


		public override void Initialize()
		{
			base.Initialize();

			_vertexDeclaration = new VertexDeclaration(VertexPositionNormalColored.VertexElements);
		}

		public override void Draw(GameTime gameTime)
		{
			if (!Enabled) return;

			var vertices = RenderingVerticies;
			var indices = RenderingIndices;

			if (vertices.IsNullOrEmpty()) return;
			if (indices.IsNullOrEmpty()) return;

			// TODO: Replace this with a vertex buffer (we don't want to send the triangles to the GPU in every frame)
			GraphicsDevice.DrawUserIndexedPrimitives(
				PrimitiveType.TriangleList,
				vertices,
				0, // vertex buffer offset to add to each element of the index buffer
				vertices.Length, // number of vertices to draw
				indices,
				0, // first index element to read
				indices.Length / 3, // number of primitives to draw
				_vertexDeclaration);

			base.Draw(gameTime);
		}

		public virtual void Clear()
		{
			_renderCached = false;
		}

		void DoBuildVerticesAndIndices()
		{
			BuildVerticiesAndIndicies();

			// interpolate normals
			for (var i = 0; i < _cachedIndices.Length; i += 3)
			{
				var index1 = _cachedIndices[i];
				var index2 = _cachedIndices[i + 1];
				var index3 = _cachedIndices[i + 2];

				var vertex1 = _cachedVertices[index1];
				var vertex2 = _cachedVertices[index2];
				var vertex3 = _cachedVertices[index3];

				var normal = Vector3.Cross(vertex2.Position - vertex1.Position, vertex3.Position - vertex1.Position);

				vertex1.Normal += normal;
				vertex2.Normal += normal;
				vertex3.Normal += normal;

				_cachedVertices[index1] = vertex1;
				_cachedVertices[index2] = vertex2;
				_cachedVertices[index3] = vertex3;
			}

			for (int i = 0; i < _cachedVertices.Length; i++)
			{
				_cachedVertices[i].Normal.Normalize();
			}
		}
		protected abstract void BuildVerticiesAndIndicies();
	}
}