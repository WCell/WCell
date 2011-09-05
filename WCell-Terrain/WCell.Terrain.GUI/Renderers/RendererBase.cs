using System;
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

	    protected VertexBuffer vertBuffer;
	    protected IndexBuffer idxBuffer;

		protected VertexPositionNormalColored[] _cachedVertices;
		protected int[] _cachedIndices;
	    protected VertexDeclaration _vertexDeclaration;

		protected RendererBase(Game game)
			: base(game)
		{
		    EnabledChanged += EnabledToggled;
            Disposed += (sender, args) => ClearBuffers();
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

            if (!_renderCached)
            {
                DoBuildVerticesAndIndices();
            }

            //if (_cachedVertices.IsNullOrEmpty()) return;
            //if (_cachedIndices.IsNullOrEmpty()) return;

            //GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
            //    _cachedVertices, 0, _cachedVertices.Length,
            //    _cachedIndices, 0, _cachedIndices.Length/3);

            var vertices = vertBuffer;
            var indices = idxBuffer;

            if (vertices == null || vertices.VertexCount == 0) return;
            if (indices == null || indices.IndexCount == 0) return;

            GraphicsDevice.Indices = idxBuffer;
            GraphicsDevice.SetVertexBuffer(vertBuffer);
            GraphicsDevice.DrawIndexedPrimitives(
                PrimitiveType.TriangleList,
                0, // the index of the base vertex, minVertexIndex and baseIndex are all relative to this
                0, // the first vertex position drawn 
                vertices.VertexCount, // number of vertices to draw
                0, // first index element to read
                indices.IndexCount / 3); // the number of primitives to draw
		    base.Draw(gameTime);
		}

	    public virtual void Clear()
		{
			_renderCached = false;
		}

		void DoBuildVerticesAndIndices()
		{
			BuildVerticiesAndIndicies();

            if (_cachedIndices.IsNullOrEmpty()) return;

			// interpolate normals
			for (var i = 0; i < _cachedIndices.Length; )
			{
				var index1 = _cachedIndices[i++];
				var index2 = _cachedIndices[i++];
				var index3 = _cachedIndices[i++];

				var vertex1 = _cachedVertices[index1];
				var vertex2 = _cachedVertices[index2];
				var vertex3 = _cachedVertices[index3];

			    var edge1 = vertex2.Position - vertex1.Position;
			    var edge2 = vertex3.Position - vertex1.Position;
			    var normal = Vector3.Cross(edge2, edge1);

                if (normal.Y < 0.0f)
                {
                   // normal = -normal;
                }
                //var normal = Vector3.Up;

                _cachedVertices[index1].Normal += normal;
				_cachedVertices[index2].Normal += normal;
				_cachedVertices[index3].Normal += normal;
			}

			for (var i = 0; i < _cachedVertices.Length; i++)
			{
				_cachedVertices[i].Normal.Normalize();
			}

            RebuildBuffers();
		}

        void EnabledToggled(object sender, EventArgs e)
        {
            if (Enabled) BuildBuffers();
            else ClearBuffers();
        }

        protected void RebuildBuffers()
        {
            ClearBuffers();
            BuildBuffers();
        }

        protected void ClearBuffers()
        {
            if (vertBuffer != null) vertBuffer.Dispose();
            if (idxBuffer != null) idxBuffer.Dispose();
        }

        protected void BuildBuffers()
        {
            if (_cachedVertices.IsNullOrEmpty()) return;
            if (_cachedIndices.IsNullOrEmpty()) return;

            vertBuffer = new VertexBuffer(GraphicsDevice, _vertexDeclaration, _cachedVertices.Length, BufferUsage.WriteOnly);
            idxBuffer = new IndexBuffer(GraphicsDevice, typeof(int), _cachedIndices.Length, BufferUsage.WriteOnly);

            vertBuffer.SetData(_cachedVertices);
            idxBuffer.SetData(_cachedIndices);
        }

		protected abstract void BuildVerticiesAndIndicies();
	}
}