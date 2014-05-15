using FluentNHibernate;
using FluentNHibernate.Mapping;
using WCell.RealmServer.Guilds;

namespace WCell.RealmServer.Database.Mappings
{
	public class GuildMemberMap : ClassMap<GuildMember>
	{
		public GuildMemberMap()
		{
			Not.LazyLoad();
			Id(x => x.CharacterLowId).GeneratedBy.Assigned();
			Map(Reveal.Member<GuildMember>("_name"),"Name").Not.Nullable();
			Map(Reveal.Member<GuildMember>("_lastLevel"),"LastLvl").Not.Nullable();
			Map(Reveal.Member<GuildMember>("_lastLogin"),"LastLogin").Not.Nullable();
			Map(Reveal.Member<GuildMember>("_lastZoneId"),"LastZone").Not.Nullable();
			Map(Reveal.Member<GuildMember>("_class"),"Class").Not.Nullable();
			Map(Reveal.Member<GuildMember>("_rankId"),"Rank").Not.Nullable();
			Map(Reveal.Member<GuildMember>("m_GuildId"),"GuildId").Not.Nullable();
			Map(Reveal.Member<GuildMember>("_publicNote"),"PublicNote");
			Map(Reveal.Member<GuildMember>("_officerNote"),"OfficerNote");
			Map(Reveal.Member<GuildMember>("_remainingMoneyAllowance"),"BankRemainingMoneyAllowance");
			Map(Reveal.Member<GuildMember>("_moneyAllowanceResetTime"),"BankMoneyAllowanceResetTime").Not.Nullable();
		}
	}
}
