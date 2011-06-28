using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core.Paths;
using WCell.Core.Terrain;
using WCell.Core.TerrainAnalysis;
using WCell.Constants.World;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Global
{
	public class DefaultTerrainProvider : ITerrainProvider
	{
		public ITerrain CreateTerrain(MapId rgnId)
		{
			return new EmptyTerrain();
		}
	}

	public class EmptyTerrain : ITerrain
	{
	    public bool HasLOS(Vector3 startPos, Vector3 endPos)
	    {
	        return true;
	    }

	    public float QueryWorldHeight(Vector3 worldPos)
	    {
	        return worldPos.Z;
	    }

	    public void QueryDirectPath(PathQuery query)
		{
			query.Reply(new Path(query.To));
		}

	    public float QueryTerrainHeight(Vector3 worldPos)
	    {
	        return worldPos.Z;
	    }

		public float GetEvironmentHeight(float x, float y)
		{
			return 0;
		}

		public bool HasTerrainLOS(Vector3 startPos, Vector3 endPos)
	    {
	        return true;
	    }

	    public float? QueryWMOCollision(Vector3 startPos, Vector3 endPos)
	    {
	        return null;
	    }

	    public bool HasWMOLOS(Vector3 startPos, Vector3 endPos)
	    {
	        return true;
	    }

	    public float? QueryWMOHeight(Vector3 worldPos)
	    {
	        return worldPos.Z;
	    }

	    public float? QueryModelCollision(Vector3 startPos, Vector3 endPos)
	    {
	        return null;
	    }

	    public bool HasModelLOS(Vector3 startPos, Vector3 endPos)
	    {
	        return true;
	    }

	    public float? QueryModelHeight(Vector3 worldPos)
	    {
	        return worldPos.Z;
	    }
	}
}