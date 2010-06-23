using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrainDisplay;
using TerrainDisplay.MPQ;
using TerrainDisplay.MPQ.ADT;
using TerrainDisplay.Util;

namespace TerrainDisplay.Recast
{
	/// <summary>
	/// Render Recast NavMesh in XNA
	/// TODO: API will only need one more method to setup parameters and start parsing to support this class
	/// </summary>
    public class RecastSolidRenderer : RecastRendererBase
    {
	    public RecastSolidRenderer(Game game, NavMeshManager manager)
            : base(game, manager)
        {
        }

        public RecastSolidRenderer(Game game, GraphicsDeviceManager graphics, NavMeshManager manager) 
            : base(game, graphics, manager)
        {
        }

        public override void Draw(GameTime gameTime)
        {
            if (!RenderPolyCached ||
                _cachedPolyVertices.IsNullOrEmpty() ||
                _cachedPolyIndices.IsNullOrEmpty())
            {
                if (!BuildPolyVerticiesAndIndicies())
                {
                    base.Draw(gameTime);
                    return;
                }
            }

            if (_graphics != null)
            {
                _graphics.GraphicsDevice.RenderState.DepthBias = 0.0000001f;
                _graphics.GraphicsDevice.RenderState.FillMode = FillMode.Solid;
            }

            GraphicsDevice.VertexDeclaration = _vertexDeclaration;
            GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                                                     _cachedPolyVertices, 0, _cachedPolyVertices.Length,
                                                     _cachedPolyIndices, 0, _cachedPolyIndices.Length / 3);

            if (_graphics != null)
            {
                _graphics.GraphicsDevice.RenderState.DepthBias = 0.0f;
                _graphics.GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;
            }

            base.Draw(gameTime);
        }

	    protected override bool BuildPolyVerticiesAndIndicies()
        {
            _manager.GetMeshVerticesAndIndices(out _cachedPolyVertices, out _cachedPolyIndices);
            if (_cachedPolyVertices.IsNullOrEmpty() || _cachedPolyIndices.IsNullOrEmpty()) return false;

            for (var i = 0; i < _cachedPolyVertices.Length; i++)
            {
                PositionUtil.TransformWoWCoordsToXNACoords(ref _cachedPolyVertices[i].Position);
            }

            RenderPolyCached = true;
	        return true;
        }
    }

    public class RecastFrameRenderer : RecastRendererBase
    {
        public RecastFrameRenderer(Game game, NavMeshManager manager)
            : base(game, manager)
        {
        }

        public RecastFrameRenderer(Game game, GraphicsDeviceManager graphics, NavMeshManager manager)
            : base(game, graphics, manager)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            _vertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionNormalColored.VertexElements);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!RenderPolyCached ||
                _cachedPolyVertices.IsNullOrEmpty() ||
                _cachedPolyIndices.IsNullOrEmpty())
            {
                if (!BuildPolyVerticiesAndIndicies())
                {
                    base.Draw(gameTime);
                    return;
                }
            }

            if (_graphics != null)
            {
                _graphics.GraphicsDevice.RenderState.DepthBias = -0.00001f;
                _graphics.GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;
            }

            GraphicsDevice.VertexDeclaration = _vertexDeclaration;
            GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                                                     _cachedPolyVertices, 0, _cachedPolyVertices.Length,
                                                     _cachedPolyIndices, 0, _cachedPolyIndices.Length / 3);

            if (_graphics != null)
            {
                _graphics.GraphicsDevice.RenderState.DepthBias = 0.0f;
                _graphics.GraphicsDevice.RenderState.FillMode = FillMode.Solid;
            }


            base.Draw(gameTime);
        }

        protected override bool BuildPolyVerticiesAndIndicies()
        {
            _manager.GetMeshVerticesAndIndices(out _cachedPolyVertices, out _cachedPolyIndices);
            if (_cachedPolyVertices.IsNullOrEmpty() || _cachedPolyIndices == null) return false;
            
            for (var i = 0; i < _cachedPolyVertices.Length; i++)
            {
                _cachedPolyVertices[i].Color = Color.Green;
                _cachedPolyVertices[i].Normal = Vector3.Up;
                PositionUtil.TransformWoWCoordsToXNACoords(ref _cachedPolyVertices[i].Position);
            }

            RenderPolyCached = true;
            return true;
        }
    }
}
