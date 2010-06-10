using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MPQNav.MPQ.M2
{
    public class M2Renderer : DrawableGameComponent
    {
        private readonly IM2Manager _manager;

        /// <summary>
        /// Boolean variable representing if all the rendering data has been cached.
        /// </summary>
        private bool _renderCached;

        private VertexPositionNormalColored[] _cachedVertices;
        private int[] _cachedIndices;

        private VertexDeclaration _vertexDeclaration;


        public M2Renderer(Game game, IM2Manager manager) : base(game)
        {
            _manager = manager;
            
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


        private void BuildVerticiesAndIndicies()
        {
            // Cycle through each M2
            var tempVertices = _manager.RenderVertices;
            var tempIndicies = _manager.RenderIndices;
            
            _cachedIndices = tempIndicies.ToArray();
            _cachedVertices = tempVertices.ToArray();

            _renderCached = true;

            for (var i = 0; i < _cachedVertices.Length; i++)
            {
                PositionUtil.TransformWoWCoordsToXNACoords(ref _cachedVertices[i]);
            }
        }
    }
}
