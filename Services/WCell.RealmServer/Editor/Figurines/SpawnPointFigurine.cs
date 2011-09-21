using WCell.Constants.NPCs;
using WCell.RealmServer.Editor.Menus;
using WCell.RealmServer.Factions;
using WCell.RealmServer.NPCs.Spawns;
using WCell.Util.Variables;

namespace WCell.RealmServer.Editor.Figurines
{
	/// <summary>
	/// The visual component of a spawnpoint
	/// </summary>
	public class SpawnPointFigurine : EditorFigurine
	{
		/// <summary>
		/// Scales the figurine in relation to its original version
		/// </summary>
		[NotVariable]
		public static float SpawnFigScale = 0.5f;

		///// <summary>
		///// Whether to also spawn a DO to make this Figurine appear clearer
		///// </summary>
		//[NotVariable]
		//public static bool AddDecoMarker = true;

		public SpawnPointFigurine(MapEditor editor, NPCSpawnPoint spawnPoint)
			: base(editor, spawnPoint)
		{
			m_position = spawnPoint.Position;

			NPCFlags = NPCFlags.Gossip;
		}

		public override SpawnEditorMenu CreateEditorMenu()
		{
			return new SpawnPointEditorMenu(Editor, SpawnPoint, this);
		}

		public override float DefaultScale
		{
			get { return SpawnFigScale; }
		}

		public override Faction Faction
		{
			get { return m_SpawnPoint.SpawnEntry.Entry.RandomFaction; }
			set { }
		}
	}
}