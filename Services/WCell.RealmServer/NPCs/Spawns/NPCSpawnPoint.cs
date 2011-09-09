using WCell.RealmServer.Entities;
using WCell.RealmServer.Spawns;

namespace WCell.RealmServer.NPCs.Spawns
{
	/// <summary>
	/// Represents a SpawnPoint that continuesly spawns NPCs.
	/// </summary>
	public class NPCSpawnPoint : SpawnPoint<NPCSpawnPoolTemplate, NPCSpawnEntry, NPC, NPCSpawnPoint, NPCSpawnPool>, IWorldLocation
	{
	}
}