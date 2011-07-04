using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WCell.Terrain.GUI.Util;
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

        public RendererBase(Game game)
            : base(game)
        {
        }

        public VertexPositionNormalColored[] RenderingVerticies
        {
            get
            {
                if (!_renderCached)
				{
					BuildVerticiesAndIndicies();
                }

                return _cachedVertices;
            }
        }

		public int[] RenderingIndices
        {
            get
            {
                if (_renderCached)
                {
                    return _cachedIndices;
                }

                BuildVerticiesAndIndicies();
                return _cachedIndices;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            var vertices = RenderingVerticies;
            var indices = RenderingIndices;

            if (vertices.IsNullOrEmpty()) return;
            if (indices.IsNullOrEmpty()) return;

			// TODO: Replace this with something faster (we don't want to send the triangles to the GPU in every frame)
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


		public override void Initialize()
		{
			base.Initialize();

			_vertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionNormalColored.VertexElements);
		}

		public virtual void Clear()
		{
			_renderCached = false;
		}

		protected abstract void BuildVerticiesAndIndicies();
    }
}