using System;
using WCell.Util.Graphics;

namespace WCell.Collision
{
    public interface IQuadObject
    {
        Rect Bounds { get; }
        event EventHandler BoundsChanged;
    }
}