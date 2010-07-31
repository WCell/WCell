using Microsoft.Xna.Framework;
using WCell.Util.Graphics;
using Microsoft.Xna.Framework.Graphics;
using TerrainDisplay.Util;

namespace TerrainDisplay.Recast
{
    public abstract class RecastRendererBase : DrawableGameComponent
    {
        protected NavMeshManager _manager;
        private static int _cachedCount;
        /// <summary>
        /// Boolean variable representing if all the rendering data has been cached.
        /// </summary>
        protected static bool RenderPolyCached
        {
            get { return (_cachedCount == 2); }
            set
            {
                if (value)
                {
                    _cachedCount++;
                    return;
                }
                _cachedCount = 0;
            }
        }

        protected VertexDeclaration _vertexDeclaration;
        protected GraphicsDeviceManager _graphics;
        protected VertexPositionNormalColored[] _cachedPolyVertices;
        protected int[] _cachedPolyIndices;

        public RecastRendererBase(Game game, NavMeshManager manager)
            : base(game)
        {
            _manager = manager;
        }

        public RecastRendererBase(Game game, GraphicsDeviceManager graphics, NavMeshManager manager)
            : this(game, manager)
        {
            _graphics = graphics;
        }

        public void RefreshRenderData()
        {
            _cachedCount = 0;
        }

        public override void Initialize()
        {
            base.Initialize();

            _vertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionNormalColored.VertexElements);
        }

        protected abstract bool BuildPolyVerticiesAndIndicies();
    }
}