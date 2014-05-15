using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Skills;

namespace WCell.RealmServer.Database.Entities
{
	public class SkillRecord
	{
	    public long Guid;

        public long OwnerId;

	    public SkillId SkillId;

	    public ushort CurrentValue;

	    public ushort MaxValue;

		public static IEnumerable<SkillRecord> GetAllSkillsFor(long charRecordId)
		{
			//return RealmWorldDBMgr.DatabaseProvider.Session.QueryOver<SkillRecord>().Where(x => x.OwnerId == (int)charRecordId).List();
			return RealmWorldDBMgr.DatabaseProvider.Query<SkillRecord>().Where(x => x.OwnerId == (int)charRecordId);
		}
	}
}