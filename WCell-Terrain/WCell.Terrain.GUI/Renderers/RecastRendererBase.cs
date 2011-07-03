using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WCell.Terrain.Recast.NavMesh;

namespace WCell.Terrain.GUI.Renderers
{
    public abstract class RecastRendererBase : DrawableGameComponent
    {
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

        public RecastRendererBase(Game game, NavMesh mesh)
            : base(game)
        {
			Mesh = mesh;
        }

    	public NavMesh Mesh
    	{
    		get;
			private set;
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