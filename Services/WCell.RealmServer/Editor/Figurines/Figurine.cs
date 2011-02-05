using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Spawns;
using WCell.Util.Variables;
using WCell.Constants.Factions;

namespace WCell.RealmServer.Editor.Figurines
{
	public abstract class Figurine : Unit
	{
		/// <summary>
		/// Whether to also spawn a DO to make this Figurine appear clearer
		/// </summary>
		[NotVariable]
		//public static bool AddDecoMarker = true;

		protected readonly NPCEntry m_entry;

	    protected Figurine(NPCEntry entry)
		{
			m_entry = entry;
			GenerateId(m_entry.Id);
			UnitFlags = UnitFlags.SelectableNotAttackable | UnitFlags.Possessed;
			DynamicFlags = UnitDynamicFlags.TrackUnit;
			EmoteState = EmoteType.StateDead;
			NPCFlags |= NPCFlags.Gossip;
	    	Model = m_entry.GetRandomModel();
			EntryId = m_entry.Id;

			// speed must be > 0
			// because of the way the client works
			const float speed = 1f;
			m_runSpeed = speed;
			m_swimSpeed = speed;
			m_swimBackSpeed = speed;
			m_walkSpeed = speed;
			m_walkBackSpeed = speed;
			m_flightSpeed = speed;
			m_flightBackSpeed = speed;

			SetInt32(UnitFields.MAXHEALTH, int.MaxValue);
			SetInt32(UnitFields.BASE_HEALTH, int.MaxValue);
			SetInt32(UnitFields.HEALTH, int.MaxValue);

			// just make a smaller version of the creature to be spawned
			SetFloat(ObjectFields.SCALE_X, entry.Scale * DefaultScale);

			IsEvading = true;
		}

		public virtual float DefaultScale
		{
			get { return 1f; }
		}

		public override LinkedList<WaypointEntry> Waypoints
		{
			get { return WaypointEntry.EmptyList; }
		}

		public override NPCSpawnPoint SpawnPoint
		{
			get { return null; }
		}

		public override string Name
		{
			get { return m_entry.DefaultName; }
			set { /* cannot be set */ }
		}

		protected override bool OnBeforeDeath()
		{
			return true;
		}

		protected override void OnDeath()
		{
			Delete();
		}

		public override Faction Faction
		{
			get { return m_entry.Faction; }
			set {/* cannot be set */}
		}

		public override Faction DefaultFaction
		{
			get { return m_entry.Faction; }
		}

		public override FactionId FactionId
		{
			get { return Faction != null ? Faction.Id : FactionId.None; }
			set { }
		}

		public override void Dispose(bool disposing)
		{
			m_Map = null;
		}
	}
}