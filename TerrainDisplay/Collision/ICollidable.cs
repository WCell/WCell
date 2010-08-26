using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Graphics;

namespace TerrainDisplay.Collision
{
    public interface ICollidable
    {
        bool IntersectWith(Ray ray, out float distance);
    }
}
