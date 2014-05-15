using WCell.Constants.ArenaTeams;
using WCell.RealmServer.Database;
using WCell.RealmServer.Database.Entities;

namespace WCell.RealmServer.Battlegrounds.Arenas
{
    public partial class ArenaTeamStats
    {
        public ArenaTeam Team
        {
            set 
                { 
                    _team = value;
                    Id = (int)value.Id; 
                }
            get { return _team; }
        }

        private ArenaTeam _team;

        public ArenaTeamStats()
        {
            
        }

        public ArenaTeamStats(ArenaTeam arenaTeam)
        {
            Team = arenaTeam;
            Rating = 1500;

            GamesWeek = 0;
            WinsWeek = 0;
            GamesSeason = 0;
            WinsSeason = 0;
            Rank = 0;
        }

        public void SetStats(ArenaTeamStatsTypes stats, uint value)
        {
            switch(stats)
            {
                case ArenaTeamStatsTypes.STAT_TYPE_RATING:
                    Rating = value;
                break;

                case ArenaTeamStatsTypes.STAT_TYPE_GAMES_WEEK:
                    GamesWeek = value;
                break;

                case ArenaTeamStatsTypes.STAT_TYPE_WINS_WEEK:
                    WinsWeek = value;
                break;

                case ArenaTeamStatsTypes.STAT_TYPE_GAMES_SEASON:
                    GamesSeason = value;
                break;

                case ArenaTeamStatsTypes.STAT_TYPE_WINS_SEASON:
                    WinsSeason = value;
                break;
            }
            RealmWorldDBMgr.DatabaseProvider.Save(this);
        }
    }
}
