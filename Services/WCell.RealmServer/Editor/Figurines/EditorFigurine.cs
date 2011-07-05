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
	public abstract class EditorFigurine : FigurineBase
	{
		/// <summary>
		/// Whether to also spawn a DO to make this Figurine appear clearer
		/// </summary>
		[NotVariable]
		//public static bool AddDecoMarker = true;

		protected readonly NPCSpawnPoint m_SpawnPoint;

		protected EditorFigurine(MapEditor editor, NPCSpawnPoint spawnPoint) : 
			base(spawnPoint.SpawnEntry.Entry.NPCId)
		{
			Editor = editor;
			m_SpawnPoint = spawnPoint;
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
	}
}