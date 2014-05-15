using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate;
using FluentNHibernate.Mapping;
using WCell.RealmServer.Database.Entities;

namespace WCell.RealmServer.Database.Mappings
{
	public class GuildBankLogEntryMap : ClassMap<GuildBankLogEntry>
	{
		public GuildBankLogEntryMap()
		{
			Not.LazyLoad();
			CompositeId().KeyProperty(x => x.GuildId).KeyProperty(x => x.BankLogEntryRecordId);
			//Map(x => x.BankLog); //TODO: This should be a relation? TODO: Find out how on earth this worked without being stored??
			Map(Reveal.Member<GuildBankLogEntry>("bankLogEntryType"));
			Map(Reveal.Member<GuildBankLogEntry>("actorEntityLowId"));
			Map(x => x.ItemEntryId);
			Map(x => x.ItemStackCount);
			Map(x => x.Money);
			Map(x => x.DestinationTabId);
			Map(x => x.Created);
		}
	}
}
