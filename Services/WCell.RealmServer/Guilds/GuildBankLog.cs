using System;
using System.Collections.Generic;
using WCell.Constants.Guilds;
using WCell.RealmServer.Database;
using WCell.RealmServer.Database.Entities;
using WCell.RealmServer.Entities;
using WCell.Util;

namespace WCell.RealmServer.Guilds
{
	public class GuildBankLog
	{
		public const int MAX_ENTRIES = 24;

		private readonly StaticCircularList<GuildBankLogEntry> itemLogEntries;
		private readonly StaticCircularList<GuildBankLogEntry> moneyLogEntries;

		public GuildBankLog(GuildBank bank)
		{
			Bank = bank;
			itemLogEntries = new StaticCircularList<GuildBankLogEntry>(MAX_ENTRIES, OnEntryDeleted);
			moneyLogEntries = new StaticCircularList<GuildBankLogEntry>(MAX_ENTRIES, OnEntryDeleted);
		}

		private static void OnEntryDeleted(GuildBankLogEntry obj)
		{
			RealmWorldDBMgr.DatabaseProvider.Delete(obj);
		}

		public GuildBank Bank
		{
			get;
			internal set;
		}

		internal void LoadLogs()
		{
			var logEntries = GuildBankLogEntry.LoadAll((uint)Bank.Guild.Id); //TODO int -> uint conversion, check how to solve this
			foreach (var entry in logEntries)
			{
				switch (entry.Type)
				{
					case GuildBankLogEntryType.None:
						{
							break;
						}
					case GuildBankLogEntryType.DepositItem:
						{
							itemLogEntries.Insert(entry);
							break;
						}
					case GuildBankLogEntryType.WithdrawItem:
						{
							itemLogEntries.Insert(entry);
							break;
						}
					case GuildBankLogEntryType.MoveItem:
						{
							itemLogEntries.Insert(entry);
							break;
						}
					case GuildBankLogEntryType.DepositMoney:
						{
							moneyLogEntries.Insert(entry);
							break;
						}
					case GuildBankLogEntryType.WithdrawMoney:
						{
							moneyLogEntries.Insert(entry);
							break;
						}
					case GuildBankLogEntryType.MoneyUsedForRepairs:
						{
							moneyLogEntries.Insert(entry);
							break;
						}
					case GuildBankLogEntryType.MoveItem_2:
						{
							itemLogEntries.Insert(entry);
							break;
						}
					case GuildBankLogEntryType.Unknown1:
						{
							break;
						}
					case GuildBankLogEntryType.Unknown2:
						{
							break;
						}
					default:
						{
							break;
						}
				} // end switch
			} // end foreach
		}

		public void LogEvent(GuildBankLogEntryType type, Character chr, ItemRecord item, GuildBankTab intoTab)
		{
			LogEvent(type, chr, item, item.Amount, intoTab);
		}

		public void LogEvent(GuildBankLogEntryType type, Character chr, ItemRecord item, int amount, GuildBankTab intoTab)
		{
			LogEvent(type, chr, 0, item, amount, intoTab);
		}

		public void LogEvent(GuildBankLogEntryType type, Character member, uint money, ItemRecord item, int amount, GuildBankTab intoTab)
		{
			switch (type)
			{
				case GuildBankLogEntryType.None:
					{
						break;
					}
				case GuildBankLogEntryType.DepositItem:
					{
						LogItemEvent(type, member, item, amount, intoTab);
						break;
					}
				case GuildBankLogEntryType.WithdrawItem:
					{
						LogItemEvent(type, member, item, amount, intoTab);
						break;
					}
				case GuildBankLogEntryType.MoveItem:
					{
						LogItemEvent(type, member, item, amount, intoTab);
						break;
					}
				case GuildBankLogEntryType.DepositMoney:
					{
						LogMoneyEvent(type, member, money);
						break;
					}
				case GuildBankLogEntryType.WithdrawMoney:
					{
						LogMoneyEvent(type, member, money);
						break;
					}
				case GuildBankLogEntryType.MoneyUsedForRepairs:
					{
						LogMoneyEvent(type, member, money);
						break;
					}
				case GuildBankLogEntryType.MoveItem_2:
					{
						LogItemEvent(type, member, item, amount, intoTab);
						break;
					}
				case GuildBankLogEntryType.Unknown1:
					{
						break;
					}
				case GuildBankLogEntryType.Unknown2:
					{
						break;
					}
				default:
					{
						break;
					}
			} // end switch
		} // end method

		private void LogMoneyEvent(GuildBankLogEntryType type, Character actor, uint money)
		{
			var entry = new GuildBankLogEntry(Bank.Guild.Id)
			{
				Type = type,
				Actor = actor,
				BankLog = this,
				Money = (int)money,
				Created = DateTime.Now
			};

			RealmWorldDBMgr.DatabaseProvider.SaveOrUpdate(entry);

			lock (itemLogEntries)
			{
				itemLogEntries.Insert(entry);
			}
		}

		private void LogItemEvent(GuildBankLogEntryType type, Character actor, ItemRecord record, int amount, GuildBankTab intoTab)
		{
			var entry = new GuildBankLogEntry(Bank.Guild.Id)
			{
				Type = type,
				Actor = actor,
				BankLog = this,
				DestinationTab = intoTab,
				ItemEntryId = (int)record.EntryId,
				ItemStackCount = (int)amount,
				Created = DateTime.Now
			};

			lock (moneyLogEntries)
			{
				moneyLogEntries.Insert(entry);
			}
		}

		public IEnumerable<GuildBankLogEntry> GetBankLogEntries(byte tabId)
		{
			if (tabId == GuildMgr.MAX_BANK_TABS)
			{
				lock (moneyLogEntries)
				{
					foreach (var entry in moneyLogEntries)
					{
						if (entry.DestinationTabId != tabId) continue;
						yield return entry;
					}
				}
			}

			lock (itemLogEntries)
			{
				foreach (var entry in itemLogEntries)
				{
					if (entry.DestinationTabId != tabId) continue;
					yield return entry;
				}
			}
		}
	}
}