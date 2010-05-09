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

		/// <summary>
		/// Is called right before the given Attacker delivers the given <see cref="IDamageAction"/>
		/// </summary>
		public event AttackHandler HitDelivered;

		/// <summary>
		/// Is called right before the given Target receives the given <see cref="IDamageAction"/>
		/// </summary>
		public event AttackHandler HitReceived;

		public event Func<NPC, bool> BeforeDeath;

		public event NPCHandler Died;
    }

	public partial class SpawnEntry
	{
		/// <summary>
		/// Called when a new NPC of this Spawn has been added to the world (also called on Teleport to another Region).
		/// </summary>
		public event Action<NPC> Spawned;
	}
}
