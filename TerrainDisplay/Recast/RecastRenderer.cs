using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MPQNav;
using MPQNav.MPQ;
using MPQNav.MPQ.ADT;

namespace TerrainDisplay.Recast
{
    public class RecastRenderer : DrawableGameComponent
    {
        private readonly ITerrainManager _manager;

        /// <summary>
        /// Boolean variable representing if all the rendering data has been cached.
        /// </summary>
        private bool _renderCached;

        private VertexPositionNormalColored[] _cachedVertices;
        private int[] _cachedIndices;

        private VertexDeclaration _vertexDeclaration;


        public RecastRenderer(Game game, ITerrainManager manager)
            : base(game)
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
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList,
                                                _cachedVertices, // array of vertices
                                                0, // start vertex
                                                _cachedVertices.Length/3 // number of triangles to draw
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
            var vecArray = _manager.GetRecastTriangleMesh();
            _cachedVertices = new VertexPositionNormalColored[vecArray.Length];

            for (var i = 0; i < vecArray.Length; i++)
            {
                _cachedVertices[i] = new VertexPositionNormalColored(vecArray[i], Color.White, Vector3.Up);
            }

            _renderCached = true;
        }
    }
}
