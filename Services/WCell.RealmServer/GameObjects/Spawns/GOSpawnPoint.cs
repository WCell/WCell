using WCell.RealmServer.Entities;
using WCell.RealmServer.Spawns;

namespace WCell.RealmServer.GameObjects.Spawns
{
	/// <summary>
	/// Represents a SpawnPoint that continuesly spawns GOs.
	/// </summary>
	public class GOSpawnPoint : SpawnPoint<GOSpawnPoolTemplate, GOSpawnEntry, GameObject, GOSpawnPoint, GOSpawnPool>, IWorldLocation
	{
	}
}