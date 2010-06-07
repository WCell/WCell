using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MPQNav.MPQ.ADT
{
    class ADTRenderer : DrawableGameComponent
    {
        private readonly IADTManager _manager;

        /// <summary>
        /// Boolean variable representing if all the rendering data has been cached.
        /// </summary>
        private bool _renderCached;

        private VertexPositionNormalColored[] _cachedVertices;
        private int[] _cachedIndices;

        private VertexDeclaration _vertexDeclaration;


        public ADTRenderer(Game game, IADTManager manager) : base(game)
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
            // Cycle through each ADT
            var tempVertices = new List<VertexPositionNormalColored>();
            var tempIndicies = new List<int>();
            var offset = 0;
            foreach (var adt in _manager.MapTiles)
            {
                // Handle the ADTs
                for (var v = 0; v < adt.Vertices.Count; v++)
                {
                    tempVertices.Add(adt.Vertices[v]);
                }
                for (var i = 0; i < adt.Indices.Count; i++)
                {
                    tempIndicies.Add(adt.Indices[i] + offset);
                }
                offset = tempVertices.Count;

                for (var v = 0; v < adt.LiquidVertices.Count; v++)
                {
                    tempVertices.Add(adt.LiquidVertices[v]);
                }
                for (var i = 0; i < adt.LiquidIndices.Count; i++)
                {
                    tempIndicies.Add(adt.LiquidIndices[i] + offset);
                }
                offset = tempVertices.Count;
            }

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
