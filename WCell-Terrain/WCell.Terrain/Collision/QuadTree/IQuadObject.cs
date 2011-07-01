using WCell.Util.Graphics;

namespace WCell.Terrain.Collision.QuadTree
{
    public interface IQuadObject
    {
		int NodeId { get; set; }
		Rect Bounds { get; }
    }
}