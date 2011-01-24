using System;
using WCell.Constants.World;
using WCell.Core.Timers;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Spawns;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.NPCs.Spawns
{
	/// <summary>
	/// Represents a SpawnPoint that continuesly spawns NPCs.
	/// </summary>
	public class NPCSpawnPoint : SpawnPoint<NPCSpawnPoolTemplate, NPCSpawnEntry, NPC, NPCSpawnPoint, NPCSpawnPool>, IWorldLocation
	{
	}
}