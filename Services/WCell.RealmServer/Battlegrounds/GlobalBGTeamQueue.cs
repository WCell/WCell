using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Battlegrounds
{
    public class GlobalBGTeamQueue : BattlegroundTeamQueue
    {
        public GlobalBGTeamQueue(GlobalBattlegroundQueue parentQueue, BattlegroundSide side)
            : base(parentQueue, side)
        {
        }

        public GlobalBattlegroundQueue GlobalQueue
        {
            get { return (GlobalBattlegroundQueue)ParentQueue; }
        }

        public override BattlegroundRelation Enqueue(ICharacterSet chrs)
        {
            var relation = base.Enqueue(chrs);

            ((GlobalBattlegroundQueue)ParentQueue).CheckBGCreation();

            return relation;
        }
    }
}