using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate;
using FluentNHibernate.Mapping;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Database.Mappings
{
	public class PersistentSpellCategoryCooldownMap : ClassMap<PersistentSpellCategoryCooldown>
	{
		public PersistentSpellCategoryCooldownMap()
		{
			Not.LazyLoad();
			Id(Reveal.Member<PersistentSpellCategoryCooldown>("Id"));
			Map(x => x.CategoryId).Not.Nullable();
			Map(x => x.ItemId).Not.Nullable();
			Map(x => x.CharId).Not.Nullable();
			Map(x => x.SpellId).Not.Nullable();
			Map(x => x.Until);
		}
	}
}
