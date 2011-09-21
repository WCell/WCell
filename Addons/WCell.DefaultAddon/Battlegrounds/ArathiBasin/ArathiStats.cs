using WCell.RealmServer;
using WCell.RealmServer.Battlegrounds;

namespace WCell.Addons.Default.Battlegrounds.ArathiBasin
{
    class ArathiStats : BattlegroundStats
    {
        public int BasesAssaulted;

        public int BasesDefended;

        public override int SpecialStatCount
        {
            get
            {
                return 2;
            }
        }

        public override void WriteSpecialStats(RealmPacketOut packet)
        {
            packet.WriteInt(BasesAssaulted);
            packet.WriteInt(BasesDefended);
        }
    }
}