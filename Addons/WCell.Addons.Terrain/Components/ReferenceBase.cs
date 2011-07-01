using WCell.Util.Graphics;

namespace WCell.Addons.Collision.Components
{
    public class ReferenceBase
    {
        public uint UniqueId;
        public BoundingBox WorldBounds;
        public Matrix WorldToModel;
        public Matrix ModelToWorld;
    }
}