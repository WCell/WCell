using Castle.ActiveRecord;
using WCell.Core.Database;

namespace WCell.RealmServer.ArenaTeams
{
	[ActiveRecord("ArenaTeamStats", Access = PropertyAccess.Property)]
	public partial class ArenaTeamStats : WCellRecord<ArenaTeamStats>
	{
        [PrimaryKey(PrimaryKeyType.Assigned, "ArenaTeamId")]
        private int _teamId
        {
            get;
            set;
        }

        [Field("Rating", NotNull = true)]
        private int _rating;

        [Field("GamesWeek", NotNull = true)]
        private int _gamesWeek;

        [Field("WinsWeek", NotNull = true)]
        private int _winsWeek;

        [Field("GamesSeason", NotNull = true)]
        private int _gamesSeason;

        [Field("WinsSeason", NotNull = true)]
        private int _winsSeason;

        [Field("Rank", NotNull = true)]
        private int _rank;

    }
}
