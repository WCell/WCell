using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using WCell.RealmServer.Database.Entities;

namespace WCell.RealmServer.Database.Mappings
{
	public class AchievementRecordMap : ClassMap<AchievementRecord>
	{
		public AchievementRecordMap()
		{
			Id(x => x.RecordId).GeneratedBy.Assigned();
			Map(x => x.CharacterId).Not.Nullable();
			Map(x => x.AchievementId).Not.Nullable();
			Map(x => x.CompleteDate);
		}
	}
}
