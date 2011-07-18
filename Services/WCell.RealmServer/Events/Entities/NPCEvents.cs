using System;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.NPCs
{
    public partial class NPCEntry
    {
		public delegate void NPCHandler(NPC npc);

		/// <summary>
		/// Called when the given NPC is added to the world or when resurrected.
		/// </summary>
		public event NPCHandler Activated;

		/// <summary>
		/// Called when NPC is deleted
		/// </summary>
    	public event NPCHandler Deleted;

		/// <summary>
		/// Called when any Character interacts with the given NPC such as quests, vendors, trainers, bankers, anything that causes a gossip.
		/// </summary>
		public event Action<Character, NPC> Interacting;

		public event Func<NPC, bool> BeforeDeath;

		public event NPCHandler Died;

		/// <summary>
		/// Is called when this NPC's level changed, only if the NPC of this NPCEntry may gain levels (<see cref="NPC.MayGainExperience"/>).
		/// </summary>
    	public event NPCHandler LevelChanged;
    }
}

namespace WCell.RealmServer.NPCs.Spawns
{
	public partial class NPCSpawnEntry
	{
		/// <summary>
		/// Called when a new NPC of this Spawn has been added to the world (also called on Teleport to another Map).
		/// </summary>
		public event Action<NPC> Spawned;
	}
}