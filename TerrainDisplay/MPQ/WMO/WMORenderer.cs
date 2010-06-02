using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MPQNav.MPQ.WMO
{
    public class WMORenderer : DrawableGameComponent
    {
        /// <summary>
        /// Boolean variable representing if all the rendering data has been cached.
        /// </summary>
        private bool _renderCached;

        private VertexPositionNormalColored[] _cachedVertices;
        private int[] _cachedIndices;
        private readonly WMOManager _manager;


        public WMORenderer(Game game, WMOManager manager)
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
            var vertices = new List<VertexPositionNormalColored>();
            var indices = new List<int>();
            var offset = 0;
            foreach (var wmo in _manager.WMOs)
            {
                for (var v = 0; v < wmo.Vertices.Count; v++)
                {
                    vertices.Add(wmo.Vertices[v]);
                }
                
                for (var i = 0; i < wmo.Indices.Count; i++)
                {
                    indices.Add(wmo.Indices[i] + offset);
                }
                offset = vertices.Count;
            }

            _cachedIndices = indices.ToArray();
            _cachedVertices = vertices.ToArray();

            _renderCached = true;

            for (var i = 0; i < _cachedVertices.Length; i++)
            {
                PositionUtil.TransformWoWCoordsToXNACoords(ref _cachedVertices[i]);
            }
        }
    }
}