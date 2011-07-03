using System.Collections.Generic;
using WCell.Terrain.Collision.QuadTree;
using WCell.Terrain.Simple.M2;
using WCell.Terrain.Simple.WMO;
using WCell.Util.Graphics;

namespace WCell.Terrain.Simple.ADT
{
    public class SimpleADT : TerrainTile
    {
        //public TileChunk[,] Chunks;

		public SimpleADT(int tileX, int tileY, Terrain terrain) :
			base(new Point2D(tileX, tileY), terrain)
        {
        }

    }
}
