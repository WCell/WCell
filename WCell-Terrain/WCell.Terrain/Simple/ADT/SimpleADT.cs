using System.Collections.Generic;
using WCell.Terrain.Collision.QuadTree;
using WCell.Terrain.Simple.M2;
using WCell.Terrain.Simple.WMO;
using WCell.Util.Graphics;

namespace WCell.Terrain.Simple.ADT
{
    public class SimpleADT : TerrainTile
    {
        public bool IsWMOOnly;
        public List<SimpleWMODefinition> WMODefs;
        public List<SimpleMapM2Definition> M2Defs;
        public QuadTree<SimpleTerrainChunk> QuadTree;
        //public TileChunk[,] Chunks;

		public SimpleADT(int tileX, int tileY, Terrain terrain) :
			base(new Point2D(tileX, tileY), terrain)
        {
        }

    }
}
