using System;
using Castle.ActiveRecord;
using WCell.Core.Database;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Guilds
{
    [ActiveRecord("GuildMember", Access = PropertyAccess.Property)]
    public partial class GuildMember : WCellRecord<GuildMember>
    {
        [PrimaryKey(PrimaryKeyType.Assigned)]
        public long CharacterLowId
        {
            get;
            private set;
        }

        [Field("Name", NotNull = true)]
        private string _name;

        [Field("LastLvl", NotNull = true)]
        private int _lastLevel;

        [Field("LastLogin", NotNull = true)]
        private DateTime _lastLogin;

        [Field("LastZone", NotNull = true)]
        private int _lastZoneId;

        [Field("Class", NotNull = true)]
        private int _class;

        [Field("Rank", NotNull = true)]
        private int _rankId;

        [Field("GuildId", NotNull = true)]
        private int m_GuildId;

        public uint GuildId
        {
            get { return (uint)m_GuildId; }
            set { m_GuildId = (int)value; }
        }

        [Field("PublicNote")]
        private string _publicNote;

        [Field("OfficerNote")]
        private string _officerNote;

        [Field("BankRemainingMoneyAllowance")]
        private int _remainingMoneyAllowance;

        [Field("BankMoneyAllowanceResetTime", NotNull = true)]
        private DateTime _moneyAllowanceResetTime;

        /// <summary>
        /// Loads all members of the given guild from the DB
        /// </summary>
        public static GuildMember[] FindAll(uint guildId)
        {
            return FindAllByProperty("m_GuildId", (int)guildId);
        }

        public GuildMember(CharacterRecord chr, Guild guild, GuildRank rank)
            : this()
        {
            var zoneId = (int)chr.Zone;

            Guild = guild;

            CharacterLowId = (int)chr.EntityLowId;
            _rankId = rank.RankIndex;
            _name = chr.Name;
            _lastLevel = chr.Level;
            _lastLogin = DateTime.Now;
            _lastZoneId = zoneId;
            _class = (int)chr.Class;
            _publicNote = string.Empty;
            _officerNote = string.Empty;
        }
    }
}