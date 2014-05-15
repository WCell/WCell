using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using WCell.RealmServer.Database.Entities;
using WCell.RealmServer.Guilds;

namespace WCell.RealmServer.Database.Mappings
{
	public class GuildBankTabRightsMap : ClassMap<GuildBankTabRights>
	{
		public GuildBankTabRightsMap()
		{
			Not.LazyLoad();
			Id(x => x.Id);
			Map(x => x.GuildRankId).Not.Nullable();
			Map(x => x.Privileges).Not.Nullable();
			Map(x => x.WithdrawlAllowance).Not.Nullable();
			Map(x => x.AllowanceResetTime).Not.Nullable();
			Map(x => x.TabId);
		}
	}
}
