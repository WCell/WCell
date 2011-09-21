using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Spawns;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Editor.Figurines
{
	/// <summary>
	/// A decorative Unit without a brain
	/// </summary>
	public class FigurineBase : Unit
	{
		private static AuraCollection _sharedAuras;

		protected FigurineBase(NPCId id)
		{
			if (_sharedAuras == null)
			{
				_sharedAuras = new AuraCollection(this);
			}

			m_auras = _sharedAuras;

			GenerateId((uint) id);

			var entry = NPCMgr.GetEntry(id);
			UnitFlags = UnitFlags.SelectableNotAttackable | UnitFlags.Possessed;
			DynamicFlags = UnitDynamicFlags.TrackUnit;
			EmoteState = EmoteType.StateDead;
			NPCFlags |= NPCFlags.Gossip;
			Model = entry.GetRandomModel();
			EntryId = entry.Id;

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

			m_evades = true;
		}


		/// <summary>
		/// Editor is only visible to staff members
		/// </summary>
		public override VisibilityStatus DetermineVisibilityFor(Unit observer)
		{
			return observer is Character && ((Character) observer).Role.IsStaff ? 
				VisibilityStatus.Visible : VisibilityStatus.Invisible;
		}


		#region Default Overrides
		public override LinkedList<WaypointEntry> Waypoints
		{
			get { return WaypointEntry.EmptyList; }
		}

		public override NPCSpawnPoint SpawnPoint
		{
			get { return null; }
		}

		public virtual float DefaultScale
		{
			get { return 1f; }
		}

		public override string Name
		{
			get { return "Dummy"; }
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
			get { return DefaultFaction; }
			set {/* cannot be set */}
		}

		public override Faction DefaultFaction
		{
			get { return FactionMgr.Get(FactionId.Friendly); }
		}

		public override FactionId FactionId
		{
			get { return Faction.Id; }
			set { }
		}

		public override void Dispose(bool disposing)
		{
			m_Map = null;
		}
		#endregion
	}
}
