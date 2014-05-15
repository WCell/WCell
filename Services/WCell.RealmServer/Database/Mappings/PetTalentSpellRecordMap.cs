using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using WCell.RealmServer.NPCs.Pets;

namespace WCell.RealmServer.Database.Mappings
{
	public class PetTalentSpellRecordMap : ClassMap<PetTalentSpellRecord>
	{
		public PetTalentSpellRecordMap()
		{
			Not.LazyLoad();
			Id(x => x.RecordId);
			Map(x => x.SpellId).Not.Nullable();
			Map(x => x.CooldownUntil).Not.Nullable();
		}
	}
}
