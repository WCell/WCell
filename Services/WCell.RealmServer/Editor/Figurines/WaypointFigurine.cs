using System;
using WCell.Constants.NPCs;
using WCell.RealmServer.Editor.Menus;
using WCell.RealmServer.Factions;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Spawns;
using WCell.Util.Variables;

namespace WCell.RealmServer.Editor.Figurines
{
	public class WaypointFigurine : EditorFigurine
	{
		/// <summary>
		/// Scales the figurine in relation to its original version
		/// </summary>
		[NotVariable]
		public static float WPFigurineScale = 0.4f;

		private readonly WaypointEntry m_Waypoint;

		public WaypointFigurine(MapEditor editor, NPCSpawnPoint spawnPoint, WaypointEntry wp)
			: base(editor, spawnPoint)
		{	
			m_Waypoint = wp;

			//GossipMenu = m_SpawnPoint.GossipMenu;
			NPCFlags = NPCFlags.Gossip;
		}

		public WaypointEntry Waypoint
		{
			get { return m_Waypoint; }
		}

		public override SpawnEditorMenu CreateEditorMenu()
		{
			return new WaypointEditorMenu(Editor, SpawnPoint, this);
		}

		public override float DefaultScale
		{
			get
			{
				return WPFigurineScale;
			}
		}

		public override Faction Faction
		{
			get { return SpawnPoint.SpawnEntry.Entry.RandomFaction; }
			set { }
		}
	}
}