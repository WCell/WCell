using WCell.Constants.World;
using WCell.Core.Initialization;
using WCell.Core.Terrain;
using WCell.RealmServer.Global;
using WCell.Terrain;
using WCell.Terrain.Recast.NavMesh;
using WCell.Terrain.Serialization;


namespace WCell.Addons.Terrain
{
	public class FullTerrainProvider : ITerrainProvider
    {
		
		[Initialization(InitializationPass.Second)]
		public static void InitializeTerrain()
		{
			// World initializes TerrainMgr in the third pass and this must happen before that
			TerrainMgr.Provider = new FullTerrainProvider();
		}

		public static TerrainTile LoadTile(SimpleTerrain terrain, int x, int y)
		{
			var tile = SimpleADTReader.ReadTile(terrain, x, y);

			if (tile != null)
			{
				// do not generate navmesh on the fly (use command instead)
				if (NavMeshBuilder.DoesNavMeshExist(terrain.MapId, x, y))
				{
					tile.EnsureNavMeshLoaded();
				}
			}

			return tile;
		}

		public ITerrain CreateTerrain(MapId id)
        {
        	var terrain = new SimpleTerrain(id, LoadTile);
			terrain.FillTileProfile();
			return terrain;
        }
    }
}