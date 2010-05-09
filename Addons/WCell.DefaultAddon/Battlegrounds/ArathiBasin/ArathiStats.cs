using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer;
using WCell.RealmServer.Battlegrounds;

namespace WCell.Addons.Default.Battlegrounds.ArathiBasin
{
    // TODO: finish this (placeholder)
    class ArathiStats : BattlegroundStats
    {
        public int BaseCaptures;

        public int BaseSaves;

        public override int SpecialStatCount
        {
            get
            {
                return 2;
            }
        }

        public override void WriteSpecialStats(RealmPacketOut packet)
        {
            packet.WriteInt(BaseCaptures); // correct?
            packet.WriteInt(BaseSaves); // correct?
        }
    }
}
