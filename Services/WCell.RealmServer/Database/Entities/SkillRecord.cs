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
			//TODO: Use Detatched Criteria for this -- should be put into the databaseprovider instead so as to keep nhibernate out of the code
            return RealmWorldDBMgr.DatabaseProvider.Session.QueryOver<SkillRecord>().Where(x => x.OwnerId == (int)charRecordId).List();
		}
	}
}