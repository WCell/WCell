using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.Core.Terrain;
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
			query.Path.Reset(1);
			query.Path.Add(query.To);
			query.Reply();
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