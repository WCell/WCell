using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Battlegrounds
{
	public class BattlegroundStats
	{
		public int KillingBlows, HonorableKills, Deaths, BonusHonor;
		public int TotalDamage, TotalHealing;

		public virtual int SpecialStatCount
		{
			get { return 0; }
		}

		/// <summary>
		/// Append bg-specific stats to pvp-stats Packet
		/// </summary>
		public virtual void WriteSpecialStats(RealmPacketOut packet)
		{
			// No specifics by default
		}
	}
}