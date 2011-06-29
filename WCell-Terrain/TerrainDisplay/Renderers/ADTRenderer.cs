using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrainDisplay.Util;
using WCell.Terrain.MPQ.ADT;

namespace TerrainDisplay.Renderers
{
    class ADTRenderer : DrawableGameComponent
    {
        private readonly IADTManager _manager;

        private const bool DrawLiquids = false;
        private static Color TerrainColor
        {
            get { return Color.DarkSlateGray; }
            //get { return Color.Green; }
        }

        private static Color WaterColor
        {
            //get { return Color.DarkSlateGray; }
			get { return Color.Blue; }
        }

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
                for (var v = 0; v < adt.TerrainVertices.Count; v++)
                {
                    var vertex = adt.TerrainVertices[v];
                    var vertexPosNmlCol = new VertexPositionNormalColored(vertex.ToXna(), TerrainColor,
                                                                          Vector3.Down);
                    tempVertices.Add(vertexPosNmlCol);
                }
                for (var i = 0; i < adt.Indices.Count; i++)
                {
                    tempIndicies.Add(adt.Indices[i] + offset);
                }
                offset = tempVertices.Count;

                //if (!DrawLiquids) continue;
                for (var v = 0; v < adt.LiquidVertices.Count; v++)
                {
                    var vertex = adt.LiquidVertices[v];
                    var vertexPosNmlCol = new VertexPositionNormalColored(vertex.ToXna(), WaterColor,
                                                                          Vector3.Down);
                    tempVertices.Add(vertexPosNmlCol);
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
				XNAUtil.TransformWoWCoordsToXNACoords(ref _cachedVertices[i]);
            }
        }
    }
}
