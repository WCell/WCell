using System;
using System.IO;
using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.Core.Terrain;
using WCell.Core.TerrainAnalysis;
using WCell.RealmServer.Global;
using WCell.Util.Graphics;
using Path = WCell.Core.Paths.Path;

namespace WCell.Collision
{
    public class FullTerrainProvider : ITerrainProvider
    {
        public ITerrain CreateTerrain(MapId id)
        {
            if (!WorldMap.TerrainDataExists(id))
            {
                return new EmptyTerrain();
            }

            return new FullTerrain(id);
        }
    }

    public class FullTerrain : ITerrain
    {
        private readonly MapId MapId;
    	public readonly MapModelCollection Models;

		public FullTerrain(MapId mapId)
		{
			MapId = mapId;

			var mapPath = System.IO.Path.Combine(WorldMap.HeightMapFolder, ((uint)mapId).ToString());
			var exists = Directory.Exists(mapPath);
			if (exists)
			{
				Models = new MapModelCollection(mapId);
				// TODO: WorldMap 
				// TODO: WorldMapObjects
			}
		}

        public float QueryTerrainHeight(Vector3 worldPos)
        {
            return WorldMap.GetHeightAtPoint(MapId, worldPos);
        }

    	public bool HasTerrainLOS(Vector3 startPos, Vector3 endPos)
        {
            return WorldMap.HasLOS(MapId, startPos, endPos);
        }

        public bool HasLOS(Vector3 startPos, Vector3 endPos)
        {
            return HasModelLOS(startPos, endPos) ||
                   HasWMOLOS(startPos, endPos) ||
                   HasTerrainLOS(startPos, endPos);
		}

		public float GetEvironmentHeight(float x, float y)
		{
			// TODO: Improve
			return QueryWorldHeight(new Vector3(x, y));
		}

        public float QueryWorldHeight(Vector3 worldPos)
        {
            var modelHeight = QueryModelHeight(worldPos);
            var wmoHeight = QueryWMOHeight(worldPos);
            var terrainHeight = QueryTerrainHeight(worldPos);
            if (modelHeight == null)
            {
                return wmoHeight == null ? terrainHeight : Math.Max(wmoHeight.Value, terrainHeight);
            }
			if (wmoHeight == null)
			{
				return Math.Max(modelHeight.Value, terrainHeight);
			}
			return Math.Max(Math.Max(modelHeight.Value, wmoHeight.Value), terrainHeight);
        }

        public void QueryDirectPath(PathQuery query)
        {
            var wayPoints = WorldMap.GetDirectPath(MapId, query.From, query.To);
            query.Reply(new Path(wayPoints));
        }

        public bool HasWMOLOS(Vector3 startPos, Vector3 endPos)
        {
            return WorldMapObjects.HasLOS(MapId, startPos, endPos);
        }

        public float? QueryWMOHeight(Vector3 worldPos)
        {
            return WorldMapObjects.GetWMOHeight(MapId, worldPos);
        }

        public float? QueryModelCollision(Vector3 startPos, Vector3 endPos)
        {
			return Models != null ? Models.GetCollisionDistance(startPos, endPos) : null;
        }

        public bool HasModelLOS(Vector3 startPos, Vector3 endPos)
        {
			return Models != null ? Models.HasLOS(startPos, endPos) : true;
        }

        public float? QueryModelHeight(Vector3 worldPos)
        {
			return Models != null ? Models.GetModelHeight(worldPos) : null;
        }

        public float? QueryWMOCollision(Vector3 startPos, Vector3 endPos)
        {
            return WorldMapObjects.CheckCollision(MapId, startPos, endPos);
        }
    }
}