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
			Id(x => x.RecordId).GeneratedBy.Native();
			Map(x => x.CharacterLow).Not.Nullable().Access.CamelCaseField();
			Map(x => x.InstanceId).Not.Nullable().Access.CamelCaseField();
			Map(x => x.MapId).Not.Nullable().Access.CamelCaseField();
			Map(x => x.Until).Not.Nullable();
		}
	}
}
