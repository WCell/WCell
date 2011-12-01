using System;
using Castle.ActiveRecord;
using NHibernate.Criterion;
using WCell.Constants.Guilds;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Guilds
{
	[ActiveRecord("GuildBankLogEntries", Access = PropertyAccess.Property)]
	public class GuildBankLogEntry : ActiveRecordBase<GuildBankLogEntry>
	{
		private static readonly Order CreatedOrder = new Order("Created", false);

		public static GuildBankLogEntry[] LoadAll(uint guildId)
		{
			return FindAll(CreatedOrder, Restrictions.Eq("GuildId", (int)guildId));
		}

		public GuildBankLogEntry()
		{

		}

		public GuildBankLogEntry(uint guildId)
		{
			GuildId = (int) guildId;
		}

		//[PrimaryKey(PrimaryKeyType.Assigned)]
		[Property]
		public int GuildId
		{
			get;
			set;
		}

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
		public DateTime Created
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