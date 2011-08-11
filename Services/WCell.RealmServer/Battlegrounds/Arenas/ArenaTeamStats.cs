using WCell.Constants.ArenaTeams;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Battlegrounds.Arenas
{
    public partial class ArenaTeamStats
    {
        public ArenaTeam team
        {
            set 
                { 
                    team = value;
                    _teamId = (int)value.Id; 
                }
        }
                    
        public uint rating
        {
            get { return (uint)_rating; }
            set { _rating = (int)value; }
        }

        public uint gamesWeek
        {
            get { return (uint)_gamesWeek; }
            set { _gamesWeek = (int)value; }
        }

        public uint winsWeek
        {
            get { return (uint)_winsWeek; }
            set { _winsWeek = (int)value; }
        }

        public uint gamesSeason
        {
            get { return (uint)_gamesSeason; }
            set { _gamesSeason = (int)value; }
        }

        public uint winsSeason
        {
            get { return (uint)_winsSeason; }
            set { _winsSeason = (int)value; }
        }
        public uint rank
        {
            get { return (uint)_rank; }
            set { _rank = (int)value; }
        }

        public ArenaTeamStats()
        {
            
        }

        public ArenaTeamStats(ArenaTeam arenaTeam)
        {
            team = arenaTeam;
            rating = 1500;

            gamesWeek = 0;
            winsWeek = 0;
            gamesSeason = 0;
            winsSeason = 0;
            rank = 0;
        }

        public void SetStats(ArenaTeamStatsTypes stats, uint value)
        {
            switch(stats)
            {
                case ArenaTeamStatsTypes.STAT_TYPE_RATING:
                    rating = value;
                break;

                case ArenaTeamStatsTypes.STAT_TYPE_GAMES_WEEK:
                    gamesWeek = value;
                break;

                case ArenaTeamStatsTypes.STAT_TYPE_WINS_WEEK:
                    winsWeek = value;
                break;

                case ArenaTeamStatsTypes.STAT_TYPE_GAMES_SEASON:
                    gamesSeason = value;
                break;

                case ArenaTeamStatsTypes.STAT_TYPE_WINS_SEASON:
                    winsSeason = value;
                break;
            }
            UpdateAndFlush();          
        }
    }
}
