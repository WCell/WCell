using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate;
using FluentNHibernate.Mapping;
using WCell.RealmServer.Database.Entities;
using WCell.RealmServer.Guilds;

namespace WCell.RealmServer.Database.Mappings
{
	public class GuildEventLogEntryMap : ClassMap<GuildEventLogEntry>
	{
		public GuildEventLogEntryMap()
		{
			Id(x => Reveal.Member<GuildEventLogEntry>("Guild"));
			Map(x => x.GuildId).Not.Nullable();
			Map(x => x.Type).Not.Nullable();
			Map(x => x.Character1LowId).Not.Nullable();
			Map(x => x.Character2LowId).Not.Nullable();
			Map(x => x.NewRankId).Not.Nullable();
			Map(x => x.TimeStamp).Not.Nullable();

		}
	}
}
