using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using WCell.RealmServer.Database.Entities;

namespace WCell.RealmServer.Database.Mappings
{
	public class RaidRecordMap : ClassMap<RaidRecord>
	{
		public RaidRecordMap()
		{
			Not.LazyLoad();
			Id(x => x.RecordId).GeneratedBy.Native();
			Map(x => x.CharacterLow).Not.Nullable();
			Map(x => x.InstanceId).Not.Nullable();
			Map(x => x.MapId).Not.Nullable();
			Map(x => x.Until).Not.Nullable();
		}
	}
}
