using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MPQNav;
using MPQNav.MPQ;

namespace TerrainDisplay
{
    class AxisRenderer : DrawableGameComponent
    {
        private VertexDeclaration _vertexDeclaration;
        private bool _renderCached = false;
        private VertexPositionNormalColored[] _cachedVertices;
        private int[] _cachedIndices;


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

        public AxisRenderer(Game game) : base(game)
        {
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
            var settingsReader = new System.Configuration.AppSettingsReader();
            var defaultMapX = (int)settingsReader.GetValue("defaultMapX", typeof(int));
            var defaultMapY = (int)settingsReader.GetValue("defaultMapY", typeof(int));
            var tempVertices = new List<VertexPositionNormalColored>();
            var tempIndices = new List<int>();
            var offset = 0;

            var baseXPos = TerrainConstants.CenterPoint - (defaultMapX + 1)*TerrainConstants.TileSize;
            var baseYPos = TerrainConstants.CenterPoint - (defaultMapY + 1)*TerrainConstants.TileSize;
            var baseZPos = -100.0f;

            // The Bottom-Righthand corner of a Tile in WoW coords
            var baseAxisVec = new Vector3(baseXPos, baseYPos, baseZPos);
            //var baseAxisVec = new Vector3(0.0f);

            // The Top-Lefthand corner of a Tile in WoW coords
            var endAxisVec = baseAxisVec + new Vector3(TerrainConstants.TileSize*2);
            
            PositionUtil.TransformWoWCoordsToXNACoords(ref baseAxisVec);
            PositionUtil.TransformWoWCoordsToXNACoords(ref endAxisVec);

            // The WoW X-axis drawn in XNA coords
            tempVertices.Add(new VertexPositionNormalColored(baseAxisVec, Color.Red, Vector3.Up));
            tempIndices.Add(0);
            tempVertices.Add(new VertexPositionNormalColored(new Vector3(baseAxisVec.X, baseAxisVec.Y, endAxisVec.Z), Color.Red, Vector3.Up));
            tempIndices.Add(1);
            tempIndices.Add(0);

            // The WoW Y-axis drawn in XNA coords
            tempVertices.Add(new VertexPositionNormalColored(baseAxisVec, Color.White, Vector3.Up));
            tempIndices.Add(2);
            tempVertices.Add(new VertexPositionNormalColored(new Vector3(endAxisVec.X, baseAxisVec.Y, baseAxisVec.Z), Color.White, Vector3.Up));
            tempIndices.Add(3);
            tempIndices.Add(2);

            // The WoW Z-axis
            tempVertices.Add(new VertexPositionNormalColored(baseAxisVec, Color.Blue, Vector3.Up));
            tempIndices.Add(4);
            tempVertices.Add(new VertexPositionNormalColored(new Vector3(baseAxisVec.X, endAxisVec.Y, baseAxisVec.Z), Color.Blue, Vector3.Up));
            tempIndices.Add(5);
            tempIndices.Add(4);

            _cachedIndices = tempIndices.ToArray();
            _cachedVertices = tempVertices.ToArray();

            _renderCached = true;
        }
    }
}
