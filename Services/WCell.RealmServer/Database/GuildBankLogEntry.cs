using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.ActiveRecord;
using WCell.Constants;
using WCell.Constants.Guilds;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Guilds
{
    [ActiveRecord("GuildBankLogEntries", Access = PropertyAccess.Property)]
    public class GuildBankLogEntry : ActiveRecordBase<GuildBankLogEntry>
    {
        [PrimaryKey(PrimaryKeyType.GuidComb)]
        public long BankLogEntryRecordId
        {
            get;
            set;
        }

        public GuildBankLog BankLog
        {
            get;
            set;
        }

        [Field]
        private int bankLogEntryType;

        [Field]
        private int actorEntityLowId;

        [Property]
        public int ItemEntryId
        {
            get;
            set;
        }

        [Property]
        public int ItemStackCount
        {
            get;
            set;
        }

        [Property]
        public int Money
        {
            get;
            set;
        }

        [Field]
        public int DestinationTabId;

        [Property]
        public DateTime TimeStamp
        {
            get;
            set;
        }


        public GuildBankLogEntryType Type
        {
            get { return (GuildBankLogEntryType)bankLogEntryType; }
            set { bankLogEntryType = (int)value; }
        }

        public Character Actor
        {
            get { return World.GetCharacter((uint)actorEntityLowId); }
            set { actorEntityLowId = (int)value.EntityId.Low; }
        }

        public GuildBankTab DestinationTab
        {
            get { return BankLog.Bank[DestinationTabId]; }
            set { DestinationTabId = value.BankSlot; }
        }
    }
}