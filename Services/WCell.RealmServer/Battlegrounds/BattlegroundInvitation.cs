using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Battlegrounds
{
    public class BattlegroundInvitation
    {
        public readonly BattlegroundTeam Team;

        internal OneShotObjectUpdateTimer CancelTimer;

        public BattlegroundInvitation(BattlegroundTeam team, int queueIndex)
        {
            Team = team;
            QueueIndex = queueIndex;
        }

        public int QueueIndex
        {
            get;
            internal set;
        }
    }
}