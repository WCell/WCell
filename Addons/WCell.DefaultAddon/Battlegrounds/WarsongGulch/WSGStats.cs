using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Entities;
using WCell.RealmServer;

namespace WCell.Addons.Default.Battlegrounds.WarsongGulch
{
	public class WSGStats : BattlegroundStats
	{
		public int FlagCaptures;
		public int FlagReturns;

		public override int SpecialStatCount
		{
			get { return 2; }
		}

		public override void WriteSpecialStats(RealmPacketOut packet)
		{
			packet.WriteInt(FlagCaptures); // Flag caps
			packet.WriteInt(FlagReturns); // Flag returns
		}
	}
}