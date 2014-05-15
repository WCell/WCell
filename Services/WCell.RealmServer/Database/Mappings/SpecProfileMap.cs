using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using WCell.RealmServer.Talents;

namespace WCell.RealmServer.Database.Mappings
{
	public class SpecProfileMap : ClassMap<SpecProfile>
	{
		public SpecProfileMap()
		{
			Not.LazyLoad();
			Id(x => x.SpecRecordId).GeneratedBy.Assigned();
			Map(x => x.CharacterGuid).Not.Nullable();
			Map(x => x.SpecIndex);
			Map(x => x.GlyphIds);
			Map(x => x.ActionButtons);
		}
	}
}
