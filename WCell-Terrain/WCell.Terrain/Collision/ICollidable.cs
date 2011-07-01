using WCell.Util.Graphics;

namespace WCell.Terrain.Collision
{
    public interface ICollidable
    {
        bool IntersectWith(Ray ray, out float distance);
    }
}
