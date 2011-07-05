using WCell.Terrain.Recast.NavMesh;
using WCell.Util.Graphics;

namespace WCell.Terrain.Simple.ADT
{
    public class SimpleADT : TerrainTile
    {
        //public TileChunk[,] Chunks;

		public SimpleADT(int tileX, int tileY, Terrain terrain) :
			base(tileX, tileY, terrain)
        {
        }

    }
}
