namespace WCell.Util.Graphics
{
    public struct Ray2D
    {
        public Vector2 Position;
        public Vector2 Direction;
        public Ray2D(Vector2 position, Vector2 direction)
        {
            Position = position;
            Direction = direction.NormalizedCopy();
        }


    }
}
