using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using WCell.RealmServer.Database.Entities;

namespace WCell.RealmServer.Database.Mappings
{
	public class ReputationRecordMap : ClassMap<ReputationRecord>
	{
		public ReputationRecordMap()
		{
			Id(x => x.RecordId).GeneratedBy.Native();
			Map(x => x.OwnerId).Not.Nullable();
			Map(x => x.ReputationIndex).Not.Nullable();
			Map(x => x.Value);
			Map(x => x.Flags).Not.Nullable();
		}
	}
}
