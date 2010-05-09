using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.ActiveRecord;
using WCell.Constants;
using WCell.Constants.Guilds;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Guilds
{
    public class GuildBankLog
    {
        public const int MAX_ENTRIES = 24;
        
        private Stack<GuildBankLogEntry> itemLogEntries;
		private Stack<GuildBankLogEntry> moneyLogEntries;

		public readonly IList<GuildBankLogEntry> LogEntries;

		public GuildBank Bank
		{
			get;
			internal set;
		}

        public Stack<GuildBankLogEntry> ItemLogEntries
        {
            get
            {
                if (itemLogEntries == null)
                {
                    InitLogQueues(itemLogEntries, moneyLogEntries);
                }
                return itemLogEntries;
            }
            set { itemLogEntries = value; }
        }

        public Stack<GuildBankLogEntry> MoneyLogEntries
        {
            get
            {
                if (moneyLogEntries == null)
                {
                    InitLogQueues(itemLogEntries, moneyLogEntries);
                }
                return moneyLogEntries;
            }
            set { moneyLogEntries = value; }
        }


        public GuildBankLog()
        {
            ItemLogEntries = new Stack<GuildBankLogEntry>(MAX_ENTRIES);
            MoneyLogEntries = new Stack<GuildBankLogEntry>(MAX_ENTRIES);
            LogEntries = new List<GuildBankLogEntry>(2 * MAX_ENTRIES);
        }

        private void InitLogQueues(Stack<GuildBankLogEntry> itemLog, Stack<GuildBankLogEntry> moneyLog)
        {
            var itemLogSort = new SortedList<DateTime, GuildBankLogEntry>(MAX_ENTRIES);
            var moneyLogSort = new SortedList<DateTime, GuildBankLogEntry>(MAX_ENTRIES);

            foreach (var entry in LogEntries)
            {
                switch (entry.Type)
                {
                    case GuildBankLogEntryType.None:
                        {
                            break;
                        }
                    case GuildBankLogEntryType.DepositItem:
                        {
                            itemLogSort.Add(entry.TimeStamp, entry);
                            break;
                        }
                    case GuildBankLogEntryType.WithdrawItem:
                        {
                            itemLogSort.Add(entry.TimeStamp, entry);
                            break;
                        }
                    case GuildBankLogEntryType.MoveItem:
                        {
                            itemLogSort.Add(entry.TimeStamp, entry);
                            break;
                        }
                    case GuildBankLogEntryType.DepositMoney:
                        {
                            moneyLogSort.Add(entry.TimeStamp, entry);
                            break;
                        }
                    case GuildBankLogEntryType.WithdrawMoney:
                        {
                            moneyLogSort.Add(entry.TimeStamp, entry);
                            break;
                        }
                    case GuildBankLogEntryType.MoneyUsedForRepairs:
                        {
                            moneyLogSort.Add(entry.TimeStamp, entry);
                            break;
                        }
                    case GuildBankLogEntryType.MoveItem_2:
                        {
                            itemLogSort.Add(entry.TimeStamp, entry);
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

            // SortedList sorts in ascending order by default
            ItemLogEntries = InitStack(itemLogSort);
            MoneyLogEntries = InitStack(moneyLogSort);
        }

        private static Stack<GuildBankLogEntry> InitStack(SortedList<DateTime, GuildBankLogEntry> list)
        {
            var newStack = new Stack<GuildBankLogEntry>(MAX_ENTRIES);
            var values = list.Values;
            var startIndex = Math.Max(0, values.Count - MAX_ENTRIES);
            
            for (var i = startIndex; i < values.Count; --i)
            {
                newStack.Push(values[i]);
            }
            return newStack;
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
            var entry = new GuildBankLogEntry {
                Type = type,
                Actor = actor,
                BankLog = this,
                Money = (int)money,
                TimeStamp = DateTime.Now
            };

            ItemLogEntries.Push(entry);
            ItemLogEntries = EnsureStackSize(ItemLogEntries);
        }

        private void LogItemEvent(GuildBankLogEntryType type, Character actor, ItemRecord record, int amount, GuildBankTab intoTab)
        {
            var entry = new GuildBankLogEntry {
                Type = type,
                Actor = actor,
                BankLog = this,
                DestinationTab = intoTab,
                ItemEntryId = (int)record.EntryId,
                ItemStackCount = (int)amount,
                TimeStamp = DateTime.Now
            };

            MoneyLogEntries.Push(entry);
            MoneyLogEntries = EnsureStackSize(MoneyLogEntries);
        }

        private static Stack<GuildBankLogEntry> EnsureStackSize(Stack<GuildBankLogEntry> stack)
        {
            if (stack.Count <= MAX_ENTRIES) return stack;
            return (Stack<GuildBankLogEntry>)stack.Take(MAX_ENTRIES);
        }

        public List<GuildBankLogEntry> GetBankLogEntries(byte tabId)
        {
            var retList = new List<GuildBankLogEntry>(MAX_ENTRIES);
            if (tabId == GuildMgr.MAX_BANK_TABS)
            {
                foreach(var entry in moneyLogEntries)
                {
                    if (entry.DestinationTabId != tabId) continue;
                    retList.Add(entry);
                }
                return retList;
            }

            foreach(var entry in itemLogEntries)
            {
                if (entry.DestinationTabId != tabId) continue;
                retList.Add(entry);
            }
            return retList;
        }
    }
}
