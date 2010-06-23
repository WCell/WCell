using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TerrainDisplay.MPQ.WMO
{
    public class WMORenderer : DrawableGameComponent
    {
        /// <summary>
        /// Boolean variable representing if all the rendering data has been cached.
        /// </summary>
        private bool _renderCached;

        private VertexPositionNormalColored[] _cachedVertices;
        private int[] _cachedIndices;
        private readonly IWMOManager _manager;


        public WMORenderer(Game game, IWMOManager manager)
            : base(game)
        {
            _manager = manager;
        }

        public override void Draw(GameTime gameTime)
        {
            var vertices = GetRenderingVerticies();
            var indices = GetRenderingIndices();

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
            var vertices = _manager.RenderVertices;
            var indices = _manager.RenderIndices;
            
            _cachedIndices = indices.ToArray();
            _cachedVertices = vertices.ToArray();

            _renderCached = true;

            for (var i = 0; i < _cachedVertices.Length; i++)
            {
                _cachedVertices[i].Normal = Vector3.Down;
                PositionUtil.TransformWoWCoordsToXNACoords(ref _cachedVertices[i]);
            }
        }
    }
}