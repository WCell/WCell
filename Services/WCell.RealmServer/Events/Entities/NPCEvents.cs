using System;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

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
		/// Called when the given NPC is added to the world or when resurrected.
		/// </summary>
		public event Action<Character, NPC> Interacting;

		public event Func<NPC, bool> BeforeDeath;

		public event NPCHandler Died;

		/// <summary>
		/// Is called when this NPC's level changed, only if the NPC of this NPCEntry may gain levels (<see cref="NPC.MayGainExperience"/>).
		/// </summary>
    	public event NPCHandler LevelChanged;
    }

	public partial class NPCSpawnEntry
	{
		/// <summary>
		/// Called when a new NPC of this Spawn has been added to the world (also called on Teleport to another Map).
		/// </summary>
		public event Action<NPC> Spawned;
	}
}