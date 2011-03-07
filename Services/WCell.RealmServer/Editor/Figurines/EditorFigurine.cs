using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Updates;
using WCell.RealmServer.Editor.Menus;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Spawns;
using WCell.RealmServer.Spells.Auras;
using WCell.Util.Variables;
using WCell.Constants.Factions;

namespace WCell.RealmServer.Editor.Figurines
{
	/// <summary>
	/// These figurines represent the 3D GUI elements of the editor
	/// </summary>
	public abstract class EditorFigurine : Unit
	{
		private static AuraCollection _sharedAuras;
		
		/// <summary>
		/// Whether to also spawn a DO to make this Figurine appear clearer
		/// </summary>
		[NotVariable]
		//public static bool AddDecoMarker = true;

		protected readonly NPCSpawnPoint m_SpawnPoint;

		protected EditorFigurine(MapEditor editor, NPCSpawnPoint spawnPoint)
		{
			if (_sharedAuras == null)
			{
				_sharedAuras = new AuraCollection(this);
			}

			m_auras = _sharedAuras;
			Editor = editor;
			m_SpawnPoint = spawnPoint;

			var entry = m_SpawnPoint.SpawnEntry.Entry;
			GenerateId(entry.Id);

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

		public MapEditor Editor
		{
			get;
			private set;
		}

		public abstract SpawnEditorMenu CreateEditorMenu();

		protected internal override void OnEncounteredBy(Character chr)
		{
			base.OnEncounteredBy(chr);

			// make sure, the GossipMenu exists when there is a Character nearby who could use it
			if (GossipMenu == null)
			{
				GossipMenu = CreateEditorMenu();
			}
		}

		/// <summary>
		/// Editor is only visible to staff members
		/// </summary>
		public override VisibilityStatus DetermineVisibilityFor(Unit observer)
		{
			if (observer is Character && Editor.Team.ContainsKey(observer.EntityId.Low)// && Editor.IsVisible
				)
			{
				// only those who are currently editing the map can see the figurines
				return VisibilityStatus.Visible;
			}
			return VisibilityStatus.Invisible;
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
			get { return m_SpawnPoint.SpawnEntry.Entry.DefaultName; }
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
			get { return m_SpawnPoint.SpawnEntry.Entry.Faction; }
			set {/* cannot be set */}
		}

		public override Faction DefaultFaction
		{
			get { return m_SpawnPoint.SpawnEntry.Entry.Faction; }
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
		#endregion
	}
}