using System;
using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.Core.TerrainAnalysis;
using WCell.RealmServer.Global;
using WCell.Util.Graphics;

namespace WCell.Collision.Addon
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
        private readonly MapId id;

        public float QueryTerrainHeight(Vector3 worldPos)
        {
            return WorldMap.GetHeightAtPoint(id, worldPos);
        }

    	public bool HasTerrainLOS(Vector3 startPos, Vector3 endPos)
        {
            return WorldMap.HasLOS(id, startPos, endPos);
        }

        public bool HasLOS(Vector3 startPos, Vector3 endPos)
        {
            return HasModelLOS(startPos, endPos) && 
                   HasWMOLOS(startPos, endPos) && 
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

            return Math.Max(Math.Max(modelHeight.Value, wmoHeight.Value), terrainHeight);
        }

        public void QueryDirectPath(PathQuery query)
        {
            var wayPoints = WorldMap.GetDirectPath(id, query.From, query.To);
            query.Reply(new Path(wayPoints));
        }

        public bool HasWMOLOS(Vector3 startPos, Vector3 endPos)
        {
            return WorldMapObjects.HasLOS(id, startPos, endPos);
        }

        public float? QueryWMOHeight(Vector3 worldPos)
        {
            return WorldMapObjects.GetWMOHeight(id, worldPos);
        }

        public float? QueryModelCollision(Vector3 startPos, Vector3 endPos)
        {
            return WorldModels.GetCollisionDistance(id, startPos, endPos);
        }

        public bool HasModelLOS(Vector3 startPos, Vector3 endPos)
        {
            return WorldModels.HasLOS(id, startPos, endPos);
        }

        public float? QueryModelHeight(Vector3 worldPos)
        {
            return WorldModels.GetModelHeight(id, worldPos);
        }

        public float? QueryWMOCollision(Vector3 startPos, Vector3 endPos)
        {
            return WorldMapObjects.CheckCollision(id, startPos, endPos);
        }

        public FullTerrain(MapId mapId)
        {
            id = mapId;
        }
    }
}