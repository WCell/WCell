using System;
using Castle.ActiveRecord;
using WCell.RealmServer.Guilds;

namespace WCell.RealmServer.Database
{
    [ActiveRecord("GuildBankItemMapping", Access = PropertyAccess.Property)]
    public class GuildBankTabItemMapping : ActiveRecordBase<GuildBankTabItemMapping>
    {
        [PrimaryKey(PrimaryKeyType.Assigned)]
		public long Guid
		{
			get;
			set;
		}

        [BelongsTo("Items")]
        public GuildBankTab BankTab
        {
            get;
            set;
        }

        [Property]
        public byte TabSlot
        {
            get;
            set;
        }

        [Version(UnsavedValue = "null")]
        public DateTime? LastModifiedOn
        { 
            get; 
            set;
        }
    }
}