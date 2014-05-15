using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate;
using FluentNHibernate.Mapping;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Database.Mappings
{
	public class PersistentSpellIdCooldownMap : ClassMap<PersistentSpellIdCooldown>
	{
		public PersistentSpellIdCooldownMap()
		{
			Not.LazyLoad();
			Id(Reveal.Member<PersistentSpellIdCooldown>("Id"));
			Map(x => x.ItemId).Not.Nullable();
			Map(x => x.CharId).Not.Nullable();
			Map(x => x.SpellId).Not.Nullable();
			Map(x => x.Until);
		}
	}
}
