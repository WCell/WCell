using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate;
using FluentNHibernate.Mapping;
using WCell.RealmServer.Guilds;

namespace WCell.RealmServer.Database.Mappings
{
	public class GuildRankMap : ClassMap<GuildRank>
	{
		public GuildRankMap()
		{
			Not.LazyLoad();
			Id(Reveal.Member<GuildRank>("_id")).GeneratedBy.Assigned();
			Map(x => x.GuildId).Not.Nullable();
			Map(x => x.Name).Not.Nullable();
			Map(x => x.RankIndex).Not.Nullable();
			Map(x => x.Privileges).Not.Nullable();
			Map(Reveal.Member<GuildRank>("_moneyPerDay"));
		}
	}
}
