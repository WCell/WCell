using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using WCell.RealmServer.Database.Entities;

namespace WCell.RealmServer.Database.Mappings
{
	public class QuestRecordMap : ClassMap<QuestRecord>
	{
		public QuestRecordMap()
		{
			Not.LazyLoad();
			Id(x => x.QuestRecordId).GeneratedBy.Assigned();
			Map(x => x.QuestTemplateId).Not.Nullable(); // TODO: Camelcase?
			Map(x => x.OwnerId).Not.Nullable();	// TODO: Camelcase?
			Map(x => x.Slot).Not.Nullable();
			Map(x => x.TimeUntil);
			Map(x => x.Interactions);
			Map(x => x.VisitedATs);
		}
	}
}
