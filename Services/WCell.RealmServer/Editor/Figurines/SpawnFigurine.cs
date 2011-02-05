using WCell.Constants.NPCs;
using WCell.RealmServer.Factions;
using WCell.RealmServer.NPCs.Spawns;
using WCell.Util.Variables;

namespace WCell.RealmServer.Editor.Figurines
{
	/// <summary>
	/// The visual component of a spawnpoint
	/// </summary>
	public class SpawnFigurine : EditorFigurine
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

		private readonly NPCSpawnPoint m_SpawnPoint;

		public SpawnFigurine(MapEditor editor, NPCSpawnPoint spawnPoint)
			: base(editor, spawnPoint.SpawnEntry.Entry)
		{
			m_SpawnPoint = spawnPoint;
			m_position = spawnPoint.SpawnEntry.Position;

			//GossipMenu = m_SpawnPoint.GossipMenu;
			NPCFlags = NPCFlags.Gossip;
		}

		public override float DefaultScale
		{
			get
			{
				return SpawnFigScale;
			}
		}

		public override Faction Faction
		{
			get { return m_entry.Faction; }
			set { }
		}
	}
}