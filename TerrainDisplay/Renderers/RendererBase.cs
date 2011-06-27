using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrainDisplay.MPQ.WMO;
using TerrainDisplay.Util;

namespace TerrainDisplay
{
    public abstract class RendererBase : DrawableGameComponent
    {
        /// <summary>
        /// Boolean variable representing if all the rendering data has been cached.
        /// </summary>
        protected bool _renderCached;

        protected VertexPositionNormalColored[] _cachedVertices;
        protected int[] _cachedIndices;

        public RendererBase(Game game)
            : base(game)
        {
        }

        private VertexPositionNormalColored[] RenderingVerticies
        {
            get
            {
                if (_renderCached)
                {
                    return _cachedVertices;
                }

                BuildVerticiesAndIndicies();
                return _cachedVertices;
            }
        }

        private int[] RenderingIndices
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

        protected abstract void BuildVerticiesAndIndicies();
    }
}