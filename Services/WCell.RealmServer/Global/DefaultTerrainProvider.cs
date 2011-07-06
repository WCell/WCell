using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core.Paths;
using WCell.Core.Terrain.Paths;
using WCell.Constants.World;
using WCell.Util.Graphics;
using WCell.Core.Terrain;

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
		public bool IsAvailable(int tileX, int tileY)
		{
			return false;
		}

		public bool HasLOS(Vector3 startPos, Vector3 endPos)
		{
			return true;
		}

		public void FindPath(PathQuery query)
		{
			query.Reply(new[] { query.To });
		}

		public float GetGroundHeightUnderneath(Vector3 worldPos)
		{
			return worldPos.Z;
		}

		public bool ForceLoadTile(int x, int y)
		{
			return true;
		}
	}
}