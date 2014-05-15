using System;
using WCell.Constants.Guilds;

namespace WCell.RealmServer.Guilds
{
	//[ActiveRecord("GuildBankTabRights", Access = PropertyAccess.Property)]
	public class GuildBankTabRights //: ActiveRecordBase<GuildBankTabRights>
    {
		public GuildBankTabRights()
		{
		}

		public GuildBankTabRights(int tabId, uint rankId)
		{
			Privileges = GuildBankTabPrivileges.None;
			WithdrawlAllowance = 0;
			TabId = tabId;
			GuildRankId = rankId;
		}

		//[PrimaryKey(PrimaryKeyType.Increment)]
		internal long Id;

		//[Field("GuildRankId", NotNull = true)]
		//private int m_GuildRankId;

		public uint GuildRankId;

		//[Field("Privileges", NotNull = true)]
		//private int _priveleges;

		//[Field("WithdrawlAllowance", NotNull = true)]
		//private int _withdrawlAllowance;

		//[Property(NotNull = true)]
		public DateTime AllowanceResetTime;

		public GuildBankTabPrivileges Privileges;

		public uint WithdrawlAllowance;

		//[Property]
		public int TabId;
    }
}