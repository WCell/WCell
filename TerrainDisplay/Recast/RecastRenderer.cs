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
    public class RecastRenderer : DrawableGameComponent
    {
        private readonly NavMeshManager _manager;

        /// <summary>
        /// Boolean variable representing if all the rendering data has been cached.
        /// </summary>
        private bool _renderPolyCached;
        private bool _renderTileCached;

        private VertexPositionNormalColored[] _cachedPolyVertices;
        private VertexPositionNormalColored[] _cachedTileVertices;
        private int[] _cachedPolyIndices;
        private int[] _cachedTileIndices;

        private VertexDeclaration _vertexDeclaration;
        private GraphicsDeviceManager _graphics;
        

        public RecastRenderer(Game game, NavMeshManager manager)
            : base(game)
        {
            _manager = manager;
        }

        public RecastRenderer(Game game, GraphicsDeviceManager graphics, NavMeshManager manager) : this(game, manager)
        {
            // TODO: Complete member initialization
            _graphics = graphics;
        }

        public void RefreshRenderData()
        {
            _renderPolyCached = false;
            _renderTileCached = false;
        }

        public override void Initialize()
        {
            base.Initialize();

            _vertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionNormalColored.VertexElements);
        }

        public override void Draw(GameTime gameTime)
        {
            var polyVertices = PolyRenderingVertices;
            var polyIndices = PolyRenderingIndices;

            if (polyVertices.IsNullOrEmpty())
            {
                base.Draw(gameTime);
                return;
            }

            if (_graphics != null)
            {
                _graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                _graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                _graphics.GraphicsDevice.RenderState.ColorWriteChannels = ColorWriteChannels.None;
                _graphics.GraphicsDevice.RenderState.FillMode = FillMode.Solid;
            }

            GraphicsDevice.VertexDeclaration = _vertexDeclaration;
            GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, 
                                                     polyVertices, 0, polyVertices.Length, 
                                                     polyIndices, 0, polyIndices.Length/3);

            if (_graphics != null)
            {
                _graphics.GraphicsDevice.RenderState.DepthBufferEnable = false;
                _graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = false;
                _graphics.GraphicsDevice.RenderState.ColorWriteChannels = ColorWriteChannels.All;
                _graphics.GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;
            }

            var tileVertices = TileRenderingVertices;
            var tileIndices = TileRenderingIndices;

            if (_cachedTileVertices.IsNullOrEmpty())
            {
                base.Draw(gameTime);
                return;
            }

            if (_graphics != null)
            {
                _graphics.GraphicsDevice.RenderState.DepthBufferFunction = CompareFunction.Never;
                _graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = false;
                _graphics.GraphicsDevice.RenderState.ColorWriteChannels = ColorWriteChannels.All;
                _graphics.GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;
            }

            GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, 
                                                     tileVertices, 0, tileVertices.Length, 
                                                     tileIndices, 0, tileIndices.Length/3);

            if (_graphics != null)
            {
                _graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                _graphics.GraphicsDevice.RenderState.DepthBufferFunction = CompareFunction.Always;
                _graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                _graphics.GraphicsDevice.RenderState.ColorWriteChannels = ColorWriteChannels.All;
                _graphics.GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;
            }

            base.Draw(gameTime);
        }

        private VertexPositionNormalColored[] PolyRenderingVertices
        {
            get
            {
                if (_renderPolyCached)
                {
                    return _cachedPolyVertices;
                }

                BuildPolyVerticiesAndIndicies();
                return _cachedPolyVertices;
            }
        }

        private VertexPositionNormalColored[] TileRenderingVertices
        {
            get
            {
                if (_renderTileCached)
                {
                    return _cachedTileVertices;
                }

                //BuildTileVerticiesAndIndicies();
                return _cachedPolyVertices;
            }
        }

	    private int[] PolyRenderingIndices
        {
            get
            {
                if (_renderPolyCached)
                {
                    return _cachedPolyIndices;
                }

                BuildPolyVerticiesAndIndicies();
                return _cachedPolyIndices;
            }
        }

        private int[] TileRenderingIndices
        {
            get
            {
                if (_renderTileCached)
                {
                    return _cachedTileIndices;
                }

                //BuildTileVerticiesAndIndicies();
                return _cachedPolyIndices;
            }
        }


        private void BuildPolyVerticiesAndIndicies()
        {
            _manager.GetMeshVerticesAndIndices(out _cachedPolyVertices, out _cachedPolyIndices);
            if (_cachedPolyVertices == null || _cachedPolyVertices.Length == 0)
            {
                _cachedPolyVertices = new VertexPositionNormalColored[1] {
                                                                         new VertexPositionNormalColored(
                                                                             new Vector3(0f, 0f, 0f), Color.White,
                                                                             Vector3.Up),
                                                                     };
                _cachedPolyIndices = new int[3] {0, 0, 0};
            }

            _cachedTileVertices = new VertexPositionNormalColored[_cachedPolyVertices.Length];
            _cachedTileIndices = new int[_cachedPolyIndices.Length];
            Array.Copy(_cachedPolyIndices, _cachedTileIndices, _cachedPolyIndices.Length);
            for (var i = 0; i < _cachedPolyVertices.Length; i++)
            {
                PositionUtil.TransformWoWCoordsToXNACoords(ref _cachedPolyVertices[i].Position);
                _cachedTileVertices[i] = new VertexPositionNormalColored(_cachedPolyVertices[i].Position, Color.Black, Vector3.Up);
            }

            _renderPolyCached = true;
        }
    }
}
