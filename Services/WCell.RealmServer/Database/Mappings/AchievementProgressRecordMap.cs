using FluentNHibernate.Mapping;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Database.Entities;

namespace WCell.RealmServer.Database.Mappings
{
	public class AchievementProgressRecordMap : ClassMap<AchievementProgressRecord>
	{
		public AchievementProgressRecordMap()
		{
			Id(x => x.RecordId).GeneratedBy.Assigned();
			Map(x => x.CharacterGuid).Not.Nullable();
			Map(x => x.AchievementCriteriaId).Not.Nullable();
			Map(x => x.Counter).Not.Nullable();
			Map(x => x.StartOrUpdateTime);
		}
	}
}
