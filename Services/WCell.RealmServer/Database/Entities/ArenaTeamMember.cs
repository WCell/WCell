using System.Collections.Generic;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Battlegrounds.Arenas
{
	public partial class ArenaTeamMember
	{
	    public int CharacterLowId;

	    public uint ArenaTeamId;

        public static IEnumerable<ArenaTeamMember> FindAll(ArenaTeam team)
        {
            //TODO: Use Detatched Criteria for this
			return RealmWorldDBMgr.DatabaseProvider.Session.QueryOver<ArenaTeamMember>().Where(x => x.ArenaTeamId == (int)team.Id).List();
        }

	    private string _name;

	    private int _class;

        public ArenaTeamMember() {}

		public ArenaTeamMember(CharacterRecord chr, ArenaTeam team, bool isLeader)
			: this()
		{
			ArenaTeam = team;
     
			CharacterLowId = (int)chr.EntityLowId;
            ArenaTeamId = team.Id;
			_name = chr.Name;
			Class = chr.Class;
            GamesWeek = 0;
            WinsWeek = 0;
            GamesSeason = 0;
            WinsSeason = 0;
            PersonalRating = 1500;
		}
   }
}
