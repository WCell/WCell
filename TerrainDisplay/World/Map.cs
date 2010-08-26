using System;
using System.Collections.Generic;
using NLog;
using WCell.Constants.World;
using WCell.Util.Graphics;

namespace TerrainDisplay.World
{
    internal class Map
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        internal MapId MapId;
        private readonly Dictionary<TileIdentifier, Tile> _tiles; 


        internal static bool TryLoad(MapId mapId, out Map map)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks this Map for LOS between two positions.
        /// </summary>
        /// <param name="pos1">The start position.</param>
        /// <param name="pos2">The end position.</param>
        /// <returns>True if there is a clear LOS between the points.</returns>
        internal bool HasLOS(Vector3 pos1, Vector3 pos2)
        {
            // The max distance check has already been performed.
            var tile1Id = TileIdentifier.ByPosition(MapId, pos1);
            if (tile1Id == null)
            {
                log.Warn("Got an invalid Tile Coord for {0}. Assuming LOS for now.", pos1);
                return true;
            }

            var tile2Id = TileIdentifier.ByPosition(MapId, pos2);
            if (tile2Id == null)
            {
                log.Warn("Got an invalid Tile Coord for {0}. ", pos1);
                return true;
            }
            
            Tile tile;
            if (!TryGetTile(tile1Id, out tile))
            {
                log.Error("Unable to load the given tile: {0} to check for LOS. Assuming LOS for now.", tile1Id);
                return true;
            }

            // if the positions are on the same tile, things get easier
            if (tile1Id == tile2Id)
            {
                return tile.HasLOS(pos1, pos2);
            }
            
            // else the LOS line segment crosses tile boundaries (possibly more than one.)
            var lineRect = new Rect(new Point(pos1.X, pos1.Y), new Point(pos2.X, pos2.Y));
            var tile1Rect = tile.Bounds;
            lineRect.Intersect(tile1Rect);

            // we know that pos1 was internal to tile1, so lineRect contains pos1 and the new pos2.
            var newPoint = lineRect.BottomRight;
            var newPos2 = new Vector3(newPoint.X, newPoint.Y, pos2.Z);

            if (!tile.HasLOS(pos1, newPos2)) return false;

            // clip the LOS line segment against the intervening tiles until tile2 is reached
            pos1 = newPos2;
            var dir = (pos2 - pos1);
            dir.Normalize();
            while(true)
            {
                // advance the start position by a small amount to ensure that we're on the new tile
                pos1 += 1e-3f*dir;
                tile1Id = TileIdentifier.ByPosition(MapId, pos1);
                if (tile1Id == null)
                {
                    log.Warn("Got an invalid Tile Coord for {0}. Assuming LOS for now.", pos1);
                    return true;
                }

                if (!TryGetTile(tile1Id, out tile))
                {
                    log.Error("Unable to load the given tile: {0} to check for LOS. Assuming LOS for now.", tile1Id);
                    return true;
                }

                if (tile1Id == tile2Id)
                {
                    return tile.HasLOS(pos1, pos2);
                }

                lineRect = new Rect(new Point(pos1.X, pos1.Y), new Point(pos2.X, pos2.Y));
                tile1Rect = tile.Bounds;
                lineRect.Intersect(tile1Rect);

                newPoint = lineRect.BottomRight;
                newPos2 = new Vector3(newPoint.X, newPoint.Y, pos2.Z);
                if (!tile.HasLOS(pos1, newPos2)) return false;

                pos1 = newPos2;
            }
        }

        internal bool GetHeightAtPosition(Vector3 pos, out float height)
        {
            throw new NotImplementedException();
        }

        internal bool GetNearestWalkablePosition(Vector3 pos, out Vector3 walkablePos)
        {
            throw new NotImplementedException();
        }

        internal bool GetNearestValidPosition(Vector3 pos, out Vector3 validPos)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Tries to get the tile with the given TileId. If the tile is not loaded,
        /// tries to load the tile from disk.
        /// </summary>
        /// <param name="tileId"><see cref="TileIdentifier"/> that describes the desired Tile.</param>
        /// <param name="tile">Tile object to fill with data</param>
        /// <returns>True if able to find/load the tile.</returns>
        private bool TryGetTile(TileIdentifier tileId, out Tile tile)
        {
            if (!_tiles.TryGetValue(tileId, out tile))
            {
                if (!Tile.TryLoad(tileId, out tile))
                {
                    log.Warn("Unable to load requested tile: {0}", tileId);
                    return false;
                }

                _tiles.Add(tileId, tile);
            }
            return true;
        }

        internal Map(MapId mapId)
        {
            MapId = mapId;
            _tiles =
                new Dictionary<TileIdentifier, Tile>(TerrainConstants.TilesPerMapSide*TerrainConstants.TilesPerMapSide);
        }
    }
}
