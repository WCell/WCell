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
	/// <summary>
	/// Render recast content in XNA
	/// </summary>
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
            GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices,
                                                     0,
                                                     indices.Length/3);
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
            Vector3[] vectors;
            int[] indices;
            _manager.GetRecastTriangleMesh(out vectors, out indices);



            _cachedVertices = new VertexPositionNormalColored[vectors.Length];
            _cachedIndices = indices;
            for (var i = 0; i < vectors.Length; i++)
            {
                _cachedVertices[i] = new VertexPositionNormalColored(vectors[i], Color.White, Vector3.Up);
            }

            _renderCached = true;
        }
    }
}
