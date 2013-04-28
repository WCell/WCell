using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace WCell.RealmServer.Database.Mappings
{
	public class GuildBankTabItemMappingMap : ClassMap<GuildBankTabItemMapping>
	{
		public GuildBankTabItemMappingMap()
		{
			Id(x => x.Guid).GeneratedBy.Assigned();
			Map(x => x.BankTab); //TODO How can we to BelongsTo with fluent
			Map(x => x.TabSlot);
			Map(x => x.LastModifiedOn); //TODO: Work out if we should be using backing field instead (this is just a thought unrelated to this).
		}
	}
}
