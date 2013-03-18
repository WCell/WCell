using System.Collections.Generic;
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
			//TODO: Use Detatched Criteria for this
            return RealmWorldDBMgr.DatabaseProvider.Session.QueryOver<SkillRecord>().Where(x => x.OwnerId == (int)charRecordId).List();
		}
	}
}