using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WCell.Terrain.GUI.Recast;
using WCell.Terrain.Recast.NavMesh;

namespace WCell.Terrain.GUI.Renderers
{
    public abstract class RecastRendererBase : DrawableGameComponent
    {
        protected NavMeshLoader loader;
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

        public RecastRendererBase(Game game, NavMeshLoader loader)
            : base(game)
        {
            this.loader = loader;
        }

        public RecastRendererBase(Game game, GraphicsDeviceManager graphics, NavMeshLoader loader)
            : this(game, loader)
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