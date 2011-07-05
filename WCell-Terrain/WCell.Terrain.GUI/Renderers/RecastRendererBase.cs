using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WCell.Terrain.Recast.NavMesh;

namespace WCell.Terrain.GUI.Renderers
{
	public abstract class RecastRendererBase : RendererBase
    {
        public RecastRendererBase(Game game)
            : base(game)
        {
        }
    }
}