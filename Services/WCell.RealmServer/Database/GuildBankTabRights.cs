using System;
using Castle.ActiveRecord;
using WCell.Constants.Guilds;

namespace WCell.RealmServer.Database
{
    [ActiveRecord("GuildBankTabRights", Access = PropertyAccess.Property)]
    public class GuildBankTabRights : ActiveRecordBase<GuildBankTabRights>
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

        [PrimaryKey(PrimaryKeyType.Increment)]
        internal long Id
        {
            get;
            set;
        }

        [Field("GuildRankId", NotNull = true)]
        private int m_GuildRankId;

        public uint GuildRankId
        {
            get { return (uint)m_GuildRankId; }
            set { m_GuildRankId = (int)value; }
        }

        [Field("Privileges", NotNull = true)]
        private int _priveleges;

        [Field("WithdrawlAllowance", NotNull = true)]
        private int _withdrawlAllowance;

        [Property(NotNull = true)]
        public DateTime AllowanceResetTime
        {
            get;
            set;
        }

        public GuildBankTabPrivileges Privileges
        {
            get { return (GuildBankTabPrivileges)_priveleges; }
            set { _priveleges = (int)value; }
        }

        public uint WithdrawlAllowance
        {
            get { return (uint)_withdrawlAllowance; }
            set { _withdrawlAllowance = (int)value; }
        }

        [Property]
        public int TabId
        {
            get;
            set;
        }
    }
}