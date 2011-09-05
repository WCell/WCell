using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WCell.Util;

namespace WCell.Terrain.GUI.Renderers
{
    public class WireframeNormalRenderer : DrawableGameComponent
    {
        private static Color vertColor = Color.Red;
        protected bool _renderCached;
        protected EnvironmentRenderer environment;

        protected VertexBuffer vertBuffer;
        protected IndexBuffer idxBuffer;

        protected VertexPositionNormalColored[] _cachedVertices;
        protected int[] _cachedIndices;
        protected VertexDeclaration _vertexDeclaration;

        public WireframeNormalRenderer(Game game, EnvironmentRenderer environment) : base(game)
        {
            this.environment = environment;
            _vertexDeclaration = new VertexDeclaration(VertexPositionNormalColored.VertexElements);

            EnabledChanged += EnabledToggled;
            Disposed += (sender, args) => ClearBuffers();
        }

        public VertexPositionNormalColored[] RenderingVerticies
		{
			get
			{
				if (!_renderCached)
				{
					BuildVerticesAndIndices();
				}

				return _cachedVertices;
			}
		}

		public int[] RenderingIndices
		{
			get { return _cachedIndices; }
		}


		public override void Draw(GameTime gameTime)
		{
		    if (!Enabled) return;

            if (!_renderCached)
            {
                BuildVerticesAndIndices();
            }

            var vertices = vertBuffer;
            var indices = idxBuffer;

            if (vertices == null || vertices.VertexCount == 0) return;
            if (indices == null || indices.IndexCount == 0) return;

		    var graphics = Game.GraphicsDevice;
            if (graphics.RasterizerState.FillMode == FillMode.WireFrame)
            {
                GraphicsDevice.Indices = idxBuffer;
                GraphicsDevice.SetVertexBuffer(vertBuffer);
                GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.LineList,
                    0, // the index of the base vertex, minVertexIndex and baseIndex are all relative to this
                    0, // the first vertex position drawn 
                    vertices.VertexCount, // number of vertices to draw
                    0, // first index element to read
                    indices.IndexCount/2); // the number of primitives to draw
            }

		    base.Draw(gameTime);
        }

        void BuildVerticesAndIndices()
        {
            _cachedVertices = new VertexPositionNormalColored[environment.RenderingVerticies.Length*2];
            for (var i = 0; i < environment.RenderingVerticies.Length; i++)
            {
                var vertPosNmlClr = environment.RenderingVerticies[i];
                _cachedVertices[i*2] = new VertexPositionNormalColored(vertPosNmlClr.Position, vertColor,
                                                                     vertPosNmlClr.Normal);
                _cachedVertices[i*2 + 1] = new VertexPositionNormalColored(vertPosNmlClr.Position + vertPosNmlClr.Normal,
                                                                         vertColor, vertPosNmlClr.Normal);
            }

            _cachedIndices = new int[_cachedVertices.Length];
            for (var i = 0; i < _cachedVertices.Length; i++)
            {
                _cachedIndices[i] = i;
            }

            RebuildBuffers();
            _renderCached = true;
        }

        void EnabledToggled(object sender, EventArgs e)
        {
            if (Enabled) BuildBuffers();
            else ClearBuffers();
        }

        protected void RebuildBuffers()
        {
            ClearBuffers();
            BuildBuffers();
        }

        protected void ClearBuffers()
        {
            if (vertBuffer != null) vertBuffer.Dispose();
            if (idxBuffer != null) idxBuffer.Dispose();
        }

        protected void BuildBuffers()
        {
            if (_cachedVertices.IsNullOrEmpty()) return;
            if (_cachedIndices.IsNullOrEmpty()) return;

            vertBuffer = new VertexBuffer(GraphicsDevice, _vertexDeclaration, _cachedVertices.Length, BufferUsage.WriteOnly);
            idxBuffer = new IndexBuffer(GraphicsDevice, typeof(int), _cachedIndices.Length, BufferUsage.WriteOnly);

            vertBuffer.SetData(_cachedVertices);
            idxBuffer.SetData(_cachedIndices);
        }
    }
}
