using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using WCell.RealmServer.Guilds;

namespace WCell.RealmServer.Database.Mappings
{
	public class GuildBankTabItemMap : ClassMap<GuildBankTabItem>
	{
		public GuildBankTabItemMap()
		{
			Not.LazyLoad();
			Id(x => x.Guid).GeneratedBy.Assigned();
			References(x => x.BankTab);
			Map(x => x.TabSlot);
			Map(x => x.LastModifiedOn); //TODO: Work out if we should be using backing field instead (this is just a thought unrelated to this).
		}
	}
}
