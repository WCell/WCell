using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrainDisplay.Util;

namespace TerrainDisplay.Collision
{
    public class SelectedTriangleRender : RendererBase
    {
        private static readonly Color SelectedTriangleColor = Color.Yellow;
        private readonly SelectedTriangleManager _manager;

        public SelectedTriangleRender(Game game, SelectedTriangleManager manager) : base(game)
        {
            _manager = manager;
        }

        protected override void BuildVerticiesAndIndicies()
        {
            _cachedVertices = new VertexPositionNormalColored[_manager.Vertices.Count];
            _cachedIndices = new int[_manager.Indices.Count];

            for (var i = 0; i < _manager.Vertices.Count; i++)
            {
                var position = _manager.Vertices[i];
                PositionUtil.TransformWoWCoordsToXNACoords(ref position);
                _cachedVertices[i] = new VertexPositionNormalColored(position.ToXna(), SelectedTriangleColor,
                                                                       Vector3.Up);
            }

            for (var i = 0; i < _manager.Indices.Count; i++)
            {
                _cachedIndices[i] = _manager.Indices[i];
            }
        }
    }
}
