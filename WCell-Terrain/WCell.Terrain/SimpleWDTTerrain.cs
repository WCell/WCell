using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.World;
using WCell.Terrain.MPQ;
using WCell.Terrain.Serialization;
using WCell.Util.Graphics;

namespace WCell.Terrain
{
    public class SimpleWDTTerrain : SimpleTerrain
    {
        public WDT BackupTileSource;

        public TerrainTile GetOrCreateTile(TileIdentifier tileId)
        {
            return GetOrCreateTile(tileId.MapId, tileId.X, tileId.Y);
        }

        public TerrainTile GetOrCreateTile(MapId map, int x, int y)
        {
            Console.Write("Trying to load simple tile... ");
            var tile = LoadTile(map, x, y);

            if (tile == null)
            {
                // load it the slow way
                var start = DateTime.Now;
                Console.WriteLine();
                Console.Write("Tile could not be found - Decompressing...");

                if (!BackupTileSource.TileProfile[x, y])
                {
                    throw new ArgumentException(String.Format(
                        "Could not read tile (Map: {0} at ({1}, {2})",
                        map,
                        x,
                        y));
                }

                tile = BackupTileSource.GetTile(x, y);
                if (tile == null)
                {
                    throw new ArgumentException(String.Format(
                        "Could not read tile (Map: {0} at ({1}, {2})",
                        map,
                        x,
                        y));
                }

                Console.WriteLine("Done - Loading time: {0:0.000}s", (DateTime.Now - start).TotalSeconds);

                // write it back
                Console.Write("Saving decompressed tile... ");
                SimpleTileWriter.WriteADT((ADT)tile);

                tile = LoadTile(x, y);
                
                Console.WriteLine("Done");
            }
            else
            {
                Console.WriteLine("Done.");
            }

            tile.EnsureNavMeshLoaded();

            tile.TileId = new TileIdentifier(map, x, y);
            
            TileProfile[x, y] = true;
            Tiles[x, y] = tile;

            return tile;
        }

        public SimpleWDTTerrain(MapId mapId, bool loadOnDemand) : base(mapId, loadOnDemand)
        {
            Init(mapId);
        }

        public SimpleWDTTerrain(MapId mapId, TerrainLoader loader, bool loadOnDemand) : base(mapId, loader, loadOnDemand)
        {
            Init(mapId);
        }

        private void Init(MapId mapId)
        {
            BackupTileSource = new WDT(mapId);
            BackupTileSource.FillTileProfile();
        }
    }
}
