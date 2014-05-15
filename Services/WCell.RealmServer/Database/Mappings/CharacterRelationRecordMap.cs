using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using WCell.RealmServer.Database.Entities;

namespace WCell.RealmServer.Database.Mappings
{
	public class CharacterRelationRecordMap : ClassMap<CharacterRelationRecord>
	{
		public CharacterRelationRecordMap()
		{
			Not.LazyLoad();
			Id(x => x.CharacterId);
			Map(x => x.RelatedCharacterId).Not.Nullable();
			Map(x => x.RelationType).Not.Nullable();
			Map(x => x.Note);
		}
	}
}
