using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using TerrainDisplay.Collision;
using TerrainDisplay.Collision.QuadTree;
using TerrainDisplay.MPQ;
using TerrainDisplay.MPQ.ADT;
using TerrainDisplay.MPQ.M2;
using TerrainDisplay.MPQ.WMO;
using WCell.Util.Graphics;

namespace TerrainDisplay.World
{
    internal class Tile
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private QuadTree<TerrainTriangleHolder> _terrainTree;
        private QuadTree<M2Holder> _m2Tree;
        private QuadTree<WMOHolder> _wmoTree;

        public Rect Bounds { get; private set; }


        internal static bool TryLoad(TileIdentifier tileId, out Tile tile)
        {
            tile = null;
            var mpqManager = new MpqTerrainManager(tileId);
            if (!mpqManager.LoadTile(tileId)) return false;
            
            tile = new Tile
            {
                _terrainTree = mpqManager.ADTManager.MapTiles[0].QuadTree,
                //_m2Tree = ((M2Manager) mpqManager.M2Manager).QuadTree,
                //_wmoTree = ((WMOManager) mpqManager.WMOManager).QuadTree,
                Bounds = PositionUtil.GetTileBoundingRect(tileId),
            };

            return true;
        }


        private Tile()
        {
            _terrainTree = new QuadTree<TerrainTriangleHolder>(new Size(TerrainConstants.ChunkSize, TerrainConstants.ChunkSize), 64);
            _m2Tree = new QuadTree<M2Holder>(new Size(TerrainConstants.ChunkSize, TerrainConstants.ChunkSize), 64);
            _wmoTree = new QuadTree<WMOHolder>(new Size(TerrainConstants.ChunkSize, TerrainConstants.ChunkSize), 64);
        }

        internal bool HasLOS(Vector3 pos1, Vector3 pos2)
        {
            var pos2d = new Vector2(pos1.X, pos1.Y);
            var dir2d = (new Vector2(pos2.X - pos1.X, pos2.Y - pos1.Y)).NormalizedCopy();
            var ray2d = new Ray2D(pos2d, dir2d);


            // try to eliminate the big stuff first
            var potentialColliders = _wmoTree.Query(ray2d);
            foreach (var holder in potentialColliders)
            {
                //holder.WMO.
            }
            return false;
        }
    }
}
