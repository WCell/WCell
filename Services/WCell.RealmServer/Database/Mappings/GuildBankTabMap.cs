using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate;
using FluentNHibernate.Mapping;
using WCell.RealmServer.Guilds;

namespace WCell.RealmServer.Database.Mappings
{
	public class GuildBankTabMap : ClassMap<GuildBankTab>
	{
		public GuildBankTabMap()
		{
			Not.LazyLoad();
			Id(Reveal.Member<GuildBankTab>("Id"));
			Map(x => x.Name);
			Map(x => x.Text);
			Map(x => x.Icon);
			Map(x => x.BankSlot);
			//Map(Reveal.Member<GuildBankTab>("_guildId")).Not.Nullable();
			//Map(Reveal.Member<GuildBankTab>("Items")); //TODO: Assuming mapping is still required?
			References(x => x.Guild);
			HasMany<GuildBankTab>(Reveal.Member<GuildBankTab>("Items")).Inverse().Cascade.AllDeleteOrphan();
		}
	}
}
