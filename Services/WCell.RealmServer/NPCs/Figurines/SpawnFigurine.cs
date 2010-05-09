using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.UpdateFields;
using WCell.Util.Variables;
using System.Collections.Generic;

namespace WCell.RealmServer.NPCs.Figurines
{
	/// <summary>
	/// The visual component of a spawnpoint
	/// </summary>
	public class SpawnFigurine : Figurine
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

		private readonly SpawnPoint m_SpawnPoint;

		public SpawnFigurine(SpawnPoint spawnPoint)
			: base(spawnPoint.SpawnEntry.Entry)
		{
			m_SpawnPoint = spawnPoint;
			m_position = spawnPoint.SpawnEntry.Position;

			GossipMenu = m_SpawnPoint.GossipMenu;
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