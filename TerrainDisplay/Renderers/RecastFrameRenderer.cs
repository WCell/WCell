using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrainDisplay.Util;
using Vector3 = WCell.Util.Graphics.Vector3;

namespace TerrainDisplay.Recast
{
	/// <summary>
    /// Render Recast NavMesh in XNA
	/// TODO: API will only need one more method to setup parameters and start parsing to support this class
	/// </summary>
    public class RecastSolidRenderer : RecastRendererBase
    {
        private Color MeshPolyColor { get { return Color.Black; } }

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
	        List<Vector3> vertices;
	        List<int> indices;
            _manager.GetMeshVerticesAndIndices(out vertices, out indices);

            if (vertices.Count == 0 || indices.Count == 0) return false;

            _cachedPolyVertices = new VertexPositionNormalColored[vertices.Count];
            for (var i = 0; i < vertices.Count; i++)
            {
                _cachedPolyVertices[i] = new VertexPositionNormalColored(vertices[i].ToXna(), MeshPolyColor,
                                                                         Vector3.Up.ToXna());
                PositionUtil.TransformWoWCoordsToXNACoords(ref _cachedPolyVertices[i]);
            }

            _cachedPolyIndices = new int[indices.Count];
	        for (int i = 0; i < indices.Count; i++)
	        {
	            _cachedPolyIndices[i] = indices[i];
	        }

            RenderPolyCached = true;
	        return true;
        }
    }

    public class RecastFrameRenderer : RecastRendererBase
    {
        public Color RecastFrameColor { get { return Color.Green; } }

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
            List<Vector3> polyVertices;
            List<int> polyIndices;
            _manager.GetMeshVerticesAndIndices(out polyVertices, out polyIndices);
            if (polyVertices.Count == 0 || polyIndices == null) return false;
            
            _cachedPolyVertices = new VertexPositionNormalColored[polyVertices.Count];
            for (var i = 0; i < polyVertices.Count; i++)
            {
                _cachedPolyVertices[i] = new VertexPositionNormalColored(polyVertices[i].ToXna(), RecastFrameColor,
                                                                         Vector3.Up.ToXna());
                PositionUtil.TransformWoWCoordsToXNACoords(ref _cachedPolyVertices[i]);
            }

            _cachedPolyIndices = new int[polyIndices.Count];
            for (int i = 0; i < polyIndices.Count; i++)
            {
                _cachedPolyIndices[i] = polyIndices[i];
            }

            RenderPolyCached = true;
            return true;
        }
    }
}
