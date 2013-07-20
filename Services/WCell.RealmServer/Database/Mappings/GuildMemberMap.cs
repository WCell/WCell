using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate;
using FluentNHibernate.Mapping;
using WCell.RealmServer.Database.Entities;

namespace WCell.RealmServer.Database.Mappings
{
	public class GuildMemberMap : ClassMap<GuildMember>
	{
		public GuildMemberMap()
		{
			Id(x => x.CharacterLowId).GeneratedBy.Assigned();
			Map(x => Reveal.Member<GuildMember>("_name")).Not.Nullable().Column("Name");
			Map(x => Reveal.Member<GuildMember>("_lastLevel")).Not.Nullable().Column("LastLvl");
			Map(x => Reveal.Member<GuildMember>("_lastLogin")).Not.Nullable().Column("LastLogin");
			Map(x => Reveal.Member<GuildMember>("_lastZoneId")).Not.Nullable().Column("LastZone");
			Map(x => Reveal.Member<GuildMember>("_class")).Not.Nullable().Column("Class");
			Map(x => Reveal.Member<GuildMember>("_rankId")).Not.Nullable().Column("Rank");
			Map(x => Reveal.Member<GuildMember>("m_GuildId")).Not.Nullable().Column("GuildId");
			Map(x => Reveal.Member<GuildMember>("_publicNote")).Column("PublicNote");
			Map(x => Reveal.Member<GuildMember>("_officerNote")).Column("OfficerNote");
			Map(x => Reveal.Member<GuildMember>("_remainingMoneyAllowance")).Column("BankRemainingMoneyAllowance");
			Map(x => Reveal.Member<GuildMember>("_moneyAllowanceResetTime")).Not.Nullable().Column("BankMoneyAllowanceResetTime");
		}
	}
}
