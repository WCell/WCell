using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector3 = WCell.Util.Graphics.Vector3;

namespace TerrainDisplay.MPQ.M2
{
    public class M2Renderer : RendererBase
    {
        private static Color M2Color
        {
            get { return Color.DarkSlateGray; }
        }

        private readonly IM2Manager _manager;

        public M2Renderer(Game game, IM2Manager manager) : base(game)
        {
            _manager = manager;
        }

        protected override void BuildVerticiesAndIndicies()
        {
            // Cycle through each M2
            var tempVertices = _manager.RenderVertices;
            var tempIndicies = _manager.RenderIndices;
            
            _cachedVertices = new VertexPositionNormalColored[tempVertices.Count];
            for (var i = 0; i < tempVertices.Count; i++)
            {
                var vec = tempVertices[i];
                PositionUtil.TransformWoWCoordsToXNACoords(ref vec);
                _cachedVertices[i] = new VertexPositionNormalColored(vec.ToXna(), M2Color,
                                                                     Microsoft.Xna.Framework.Vector3.Down);
            }

            _cachedIndices = new int[tempIndicies.Count];
            for (var i = 0; i < tempIndicies.Count; i++)
            {
                _cachedIndices[i] = tempIndicies[i];
            }

            _renderCached = true;
        }
    }
}
