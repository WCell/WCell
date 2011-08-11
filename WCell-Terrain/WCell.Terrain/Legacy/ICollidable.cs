using WCell.Util.Graphics;

namespace WCell.Terrain.Legacy
{
    public interface ICollidable
    {
        bool IntersectWith(Ray ray, out float distance);
    }
}
