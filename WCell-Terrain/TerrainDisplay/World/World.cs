using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using WCell.Constants.World;
using WCell.Util.Graphics;

namespace TerrainDisplay.World
{
    public class World
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public static float MaxLOSRadius = 120.0f;
        private readonly float _maxLOSRadiusSquared;

        private Dictionary<MapId, Map> _maps;

        /// <summary>
        /// Checks of the two positions have a clear line of sight on the given MapId.
        /// </summary>
        /// <param name="mapId">The <see cref="WCell.Constants.World.MapId"/> that corresponds to the desired map.</param>
        /// <param name="pos1">The start position for the LOS check.</param>
        /// <param name="pos2">The end position for the LOS check.</param>
        /// <returns>True if the LOS check was successful.</returns>
        public bool HasLOS(MapId mapId, Vector3 pos1, Vector3 pos2)
        {
            if (pos1.DistanceSquared(pos2) > _maxLOSRadiusSquared)
            {
                log.Warn(
                    "LOS request on points whose distance exceeds the maximum: Point 1: {0}, Point 2: {1}, Max Distance: {2}",
                    pos1, pos2, MaxLOSRadius);
                return false;
            }

            Map map;
            return (((!TryGetMap(mapId, out map)) || (map.HasLOS(pos1, pos2))));
        }

        /// <summary>
        /// Get the height of the floor underneath the given position on the map with the given MapId.
        /// </summary>
        /// <param name="mapId">The <see cref="WCell.Constants.World.MapId"/> that corresponds to the desired map.</param>
        /// <param name="pos">The position on the map to get the height at.</param>
        /// <param name="height">The height of the floor at the given position on the map.</param>
        /// <returns>True if the height retrieval was successful.</returns>
        public bool GetHeightAtPosition(MapId mapId, Vector3 pos, out float height)
        {
            height = float.MinValue;
            
            Map map;
            return ((!TryGetMap(mapId, out map)) || (map.GetHeightAtPosition(pos, out height)));
        }

        /// <summary>
        /// Gets the nearest walkable (i.e., not water, model, air, etc) position to the given position.
        /// </summary>
        /// <param name="mapId">The <see cref="WCell.Constants.World.MapId"/> that corresponds to the desired map.</param>
        /// <param name="pos">The position to check against.</param>
        /// <param name="walkablePos">The nearest walkable position to the given position.</param>
        /// <returns>True if the check was successful.</returns>
        public bool GetNearestWalkablePositon(MapId mapId, Vector3 pos, out Vector3 walkablePos)
        {
            walkablePos = Vector3.Zero;

            Map map;
            return ((!TryGetMap(mapId, out map)) || (map.GetNearestWalkablePosition(pos, out walkablePos)));
        }

        /// <summary>
        /// Gets the nearest valid (i.e., not over a hole, outside map boundaries, etc) position to the given position.
        /// </summary>
        /// <param name="mapId">The <see cref="WCell.Constants.World.MapId"/> that corresponds to the desired map.</param>
        /// <param name="pos">The position to check against.</param>
        /// <param name="validPos">The nearest valid position to the given position.</param>
        /// <returns>True if the check was successful.</returns>
        public bool GetNearestValidPosition(MapId mapId, Vector3 pos, out Vector3 validPos)
        {
            validPos = Vector3.Zero;

            Map map;
            return ((!TryGetMap(mapId, out map)) || (map.GetNearestValidPosition(pos, out validPos)));
        }


        /// <summary>
        /// Tries to retrieve/load the map from/into the maps Dictionary.
        /// </summary>
        /// <param name="mapId">The <see cref="WCell.Constants.World.MapId"/> that corresponds to the desired map.</param>
        /// <param name="map">Holder for the Map object to return.</param>
        /// <returns>True if the map was encountered.</returns>
        private bool TryGetMap(MapId mapId, out Map map)
        {
            if (!_maps.TryGetValue(mapId, out map))
            {
                if (!Map.TryLoad(mapId, out map))
                {
                    log.Warn("Unable to load the requested map: {0}", mapId);
                    return false;
                }

                _maps.Add(mapId, map);
            }

            if (map == null)
            {
                log.Warn("Unable to load the requested map. Assuming LOS for now. Map: {0}", mapId);
                return false;
            }

            return true;
        }

        
        public World()
        {
            _maxLOSRadiusSquared = MaxLOSRadius*MaxLOSRadius;
            _maps = new Dictionary<MapId, Map>((int)MapId.End);
        }
    }
}
