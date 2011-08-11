using Castle.ActiveRecord;
using WCell.Core.Database;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Battlegrounds.Arenas
{
	[ActiveRecord("ArenaTeamMember", Access = PropertyAccess.Property)]
	public partial class ArenaTeamMember : WCellRecord<ArenaTeamMember>
	{
		[PrimaryKey(PrimaryKeyType.Assigned)]
		public int CharacterLowId
		{
			get;
			private set;
		}

        [Field("ArenaTeamId", NotNull = true)]
        private int _arenaTeamId;

        public uint ArenaTeamId
        {
            get { return (uint)_arenaTeamId; }
            set { _arenaTeamId = (int)value; }
        }

        [Field("Name", NotNull = true)]
		private string _name;

		[Field("Class", NotNull = true)]
		private int _class;

        [Field("GamesWeek", NotNull = true)]
        private int _gamesWeek;

        [Field("WinsWeek", NotNull = true)]
        private int _winsWeek;

        [Field("GamesSeason", NotNull = true)]
        private int _gamesSeason;

        [Field("WinsSeason", NotNull = true)]
        private int _winsSeason;

        [Field("PersonalRating", NotNull = true)]
        private int _personalRating;

        public static ArenaTeamMember[] FindAll(ArenaTeam team)
        {
            return FindAllByProperty("_arenaTeamId", (int)team.Id);
        }

        public ArenaTeamMember() {}

		public ArenaTeamMember(CharacterRecord chr, ArenaTeam team, bool isLeader)
			: this()
		{
			ArenaTeam = team;
     
			CharacterLowId = (int)chr.EntityLowId;
            ArenaTeamId = team.Id;
			_name = chr.Name;
			_class = (int)chr.Class;
            _gamesWeek = 0;
            _winsWeek = 0;
            _gamesSeason = 0;
            _winsSeason = 0;
            _personalRating = 1500;
		}
   }
}
