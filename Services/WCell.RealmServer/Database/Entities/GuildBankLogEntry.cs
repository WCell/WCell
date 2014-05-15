using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using WCell.Constants.Guilds;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Guilds;

namespace WCell.RealmServer.Database.Entities
{
	public class GuildBankLogEntry
	{
		//private static readonly Order CreatedOrder = new Order("Created", false);
		private GuildBankLogEntry()
		{
			
		}
		public static IEnumerable<GuildBankLogEntry> LoadAll(uint guildId)
		{
			return RealmWorldDBMgr.DatabaseProvider.FindAll<GuildBankLogEntry>(x => x.GuildId == guildId).OrderBy(entry => entry.Created); //TODO: Check this is going to do things as we want
		}
		
		public GuildBankLogEntry(int guildId)
		{
			GuildId = guildId;
		}

		//[PrimaryKey(PrimaryKeyType.Assigned)]
		public int GuildId
		{
			get;
			set;
		}

		//[PrimaryKey(PrimaryKeyType.GuidComb)]
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

		//[Field]
		private int bankLogEntryType;

		//[Field]
		private int actorEntityLowId;

		//[Property]
		public int ItemEntryId
		{
			get;
			set;
		}

		//[Property]
		public int ItemStackCount
		{
			get;
			set;
		}

		//[Property]
		public int Money
		{
			get;
			set;
		}

		//[Field]
		public int DestinationTabId;

		//[Property]
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

		public override bool Equals(object obj)
		{
			var other = obj as GuildBankLogEntry;

			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;

			return GuildId == other.GuildId &&
				BankLogEntryRecordId == other.BankLogEntryRecordId;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = GetType().GetHashCode();
				hash = (hash * 31) ^ GuildId.GetHashCode();
				hash = (hash * 31) ^ BankLogEntryRecordId.GetHashCode();

				return hash;
			}
		}
	}
}