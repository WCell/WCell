using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Guilds;
using WCell.Constants.Items;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.Util;
using WCell.Util.Collections;

namespace WCell.RealmServer.Guilds
{
	public class GuildBank
	{
		private static readonly int[] BankTabPrices = new int[] {
            1000000,
            2500000,
            5000000,
            10000000,
            25000000,
            50000000
        };

		private GuildBankTab[] bankTabs;

		/// <summary>
		/// 
		/// </summary>
		internal GuildBank(Database.Entities.Guild guild, bool isNew)
		{
			Guild = guild;
			BankLog = new GuildBankLog(this);
			if (isNew)
			{
				bankTabs = new [] {
					new GuildBankTab(this)
					{
						BankSlot = 0,
						Icon = "",
						Name = "Slot 0",
						Text = ""
					}
				};
			}
			else
			{
				// load an existing guild bank
				bankTabs = GuildBankTab.FindAllByProperty("_guildId", (int)guild.Id);
				BankLog.LoadLogs();
			}
		}

		public Database.Entities.Guild Guild
		{
			get;
			private set;
		}

		public GuildBankLog BankLog
		{
			get;
			private set;
		}

		public GuildBankTab this[int tabId]
		{
			get
			{
				var tabs = bankTabs;
				return tabId < tabs.Length ? tabs[tabId] : null;
			}
		}

		public void DepositMoney(Character depositer, GameObject bankObj, uint deposit)
		{
			if (!CheckBankObj(depositer, bankObj)) return;

			if (deposit == 0) return;
			if (depositer.Money < deposit) return;

			Guild.Money += deposit;
			depositer.Money -= deposit;

			BankLog.LogEvent(GuildBankLogEntryType.DepositMoney, depositer, deposit, null, 0, null);
			GuildHandler.SendGuildBankTabNames(depositer, bankObj);
			GuildHandler.SendGuildBankTabContents(depositer, bankObj, 0);
			GuildHandler.SendGuildBankMoneyUpdate(depositer, bankObj);
		}

		public void WithdrawMoney(Character withdrawer, GameObject bankObj, uint withdrawl)
		{
			if (!CheckBankObj(withdrawer, bankObj)) return;

			if (withdrawl == 0) return;
			if (Guild.Money < withdrawl) return;

			var member = withdrawer.GuildMember;
			if (member == null) return;

			if (!member.HasRight(GuildPrivileges.WITHDRAW_GOLD) ||
				member.HasRight(GuildPrivileges.WITHDRAW_GOLD_LOCK))
			{
				return;
			}
			if (member.BankMoneyWithdrawlAllowance < withdrawl)
			{
				return;
			}

			Guild.Money -= withdrawl;
			withdrawer.Money += withdrawl;
			member.BankMoneyWithdrawlAllowance -= withdrawl;

			BankLog.LogEvent(GuildBankLogEntryType.WithdrawMoney, withdrawer, withdrawl, null, 0, null);

			GuildHandler.SendMemberRemainingDailyWithdrawlAllowance(withdrawer, member.BankMoneyWithdrawlAllowance);
			GuildHandler.SendGuildBankTabNames(withdrawer, bankObj);
			GuildHandler.SendGuildBankTabContents(withdrawer, bankObj, 0);
			GuildHandler.SendGuildBankMoneyUpdate(withdrawer, bankObj);
		}

		public void SwapItemsManualBankToBank(Character chr, GameObject bankObj, byte fromBankTabId, byte fromTabSlot, byte toBankTabId, byte toTabSlot, uint itemEntryId, byte amount)
		{
			if (!CheckBankObj(chr, bankObj)) return;

			var member = chr.GuildMember;
			if (member == null) return;

			var rank = member.Rank;
			if (rank == null) return;

			//check for valid tab / tab slot
			var fromBankTab = this[fromBankTabId];
			if (fromBankTab == null) return;

			var toBankTab = this[toBankTabId];
			if (toBankTab == null) return;

			if (fromTabSlot >= GuildMgr.MAX_BANK_TAB_SLOTS) return;
			if (toTabSlot >= GuildMgr.MAX_BANK_TAB_SLOTS) return;

			var bankTabRights = rank.BankTabRights;

			if (!bankTabRights[fromBankTabId].Privileges.HasFlag(GuildBankTabPrivileges.ViewTab))
				return;
			if (bankTabRights[fromBankTabId].WithdrawlAllowance <= 0)
				return;
			if (!bankTabRights[toBankTabId].Privileges.HasFlag(GuildBankTabPrivileges.DepositItem))
				return;

			var fromItem = fromBankTab[fromTabSlot];
			if (fromItem == null) return;

			if (fromItem.EntryId != itemEntryId) return; // Cheater!
			if (fromItem.Amount < amount) return; // Cheater!
			if (amount == 0)
			{
				amount = (byte)fromItem.Amount;
			}

			var isIntraTab = (fromBankTabId == toBankTabId);
			// moving to same tab
			if (isIntraTab)
			{
				if (fromTabSlot == toTabSlot)
				{
					// no can do kemosabe
					return;
				}

				var retItem = toBankTab.StoreItemInSlot(fromItem, amount, toTabSlot, true);
				fromBankTab[fromTabSlot] = retItem;
			}
			// moving to separate tab
			else
			{
				// This may require an Item swap, if so then extra permissions need to be checked.
				if (toBankTab.CheckStoreItemInSlot(fromItem, amount, toTabSlot, true))
				{
					// No item swap needed, continue as normal.
					var retItem = toBankTab.StoreItemInSlot(fromItem, amount, toTabSlot, true);
					fromBankTab[fromTabSlot] = retItem;
				}
				else
				{
					// Check that the character can deposit to the source BankTab
					if (!bankTabRights[fromBankTabId].Privileges.HasFlag(GuildBankTabPrivileges.DepositItem))
					{
						return;
					}

					// Check that the character can withdraw from the target BankTab
					if (!(bankTabRights[toBankTabId].WithdrawlAllowance > 0))
					{
						return;
					}

					var retItem = toBankTab.StoreItemInSlot(fromItem, amount, toTabSlot, true);
					fromBankTab[fromTabSlot] = retItem;
					if (retItem != fromItem)
					{
						// Item swap took place
						// Dock the player the extra withdrawl
						bankTabRights[toTabSlot].WithdrawlAllowance--;
						bankTabRights[fromBankTabId].WithdrawlAllowance--;

						// Log the extra item-move
						BankLog.LogEvent(GuildBankLogEntryType.MoveItem, chr, fromItem, amount, toBankTab);
						BankLog.LogEvent(GuildBankLogEntryType.MoveItem, chr, retItem, fromBankTab);

						Guild.SendGuildBankTabContentUpdateToAll(fromBankTabId, fromTabSlot);
						Guild.SendGuildBankTabContentUpdateToAll(toBankTabId, toTabSlot);
						return;
					}
				}

				// Moving from one tab to another costs you a withdrawl point.
				bankTabRights[fromBankTabId].WithdrawlAllowance--;
			}

			// Log the item move.
			BankLog.LogEvent(GuildBankLogEntryType.MoveItem, chr, fromItem, amount, toBankTab);

			Guild.SendGuildBankTabContentUpdateToAll(fromBankTabId, fromTabSlot, isIntraTab ? toTabSlot : -1);
		}

		public void SwapItemsAutoStoreBankToChar(Character chr, GameObject bank, byte fromBankTabId, byte fromTabSlot, uint itemEntryId, byte autoStoreCount)
		{
			if (!CheckBankObj(chr, bank)) return;

			var member = chr.GuildMember;
			if (member == null) return;

			var rank = member.Rank;
			if (rank == null) return;

			var fromBankTab = this[fromBankTabId];
			if (fromBankTab == null) return;

			if (fromTabSlot >= GuildMgr.MAX_BANK_TAB_SLOTS) return;

			var bankTabRights = rank.BankTabRights;

			if (!bankTabRights[fromBankTabId].Privileges.HasFlag(GuildBankTabPrivileges.ViewTab))
				return;
			if (!(bankTabRights[fromBankTabId].WithdrawlAllowance > 0))
				return;

			var fromItem = fromBankTab[fromTabSlot];
			if (fromItem == null) return;

			if (fromItem.EntryId != itemEntryId) return; // Cheater!

			var newItem = Item.CreateItem(fromItem, fromItem.Template);
			var preAmt = newItem.Amount;
			var msg = chr.Inventory.TryAddAmount(newItem, newItem.Amount, true);

			var postAmt = preAmt;
			if (msg != InventoryError.OK)
			{
				postAmt = newItem.Amount;
				// Check to see if any part of the item was added to the inventory
				if (postAmt == preAmt)
				{
					ItemHandler.SendInventoryError(chr, msg);
					return;
				}

				// If at least part of the item was added, then we should record the transaction
				// and dock the character and Withdrawl point.
			}
			else
			{
				// The entire stack fit into the character's Inventory
				// Remove the stack from the bank and charge/log the transaction.
				fromBankTab[fromTabSlot] = null;
			}

			bankTabRights[fromBankTabId].WithdrawlAllowance--;
			BankLog.LogEvent(GuildBankLogEntryType.WithdrawItem, chr, fromItem, postAmt, fromBankTab);

			Guild.SendGuildBankTabContentUpdateToAll(fromBankTabId, fromTabSlot);
		}

		public void SwapItemsManualBankToChar(Character chr, GameObject bank, byte fromBankTabId, byte fromTabSlot, byte bagSlot, byte slot, uint itemEntryId, byte amount)
		{
			if (!CheckBankObj(chr, bank)) return;
			var member = chr.GuildMember;
			if (member == null) return;

			var rank = member.Rank;
			if (rank == null) return;

			//check for valid tab / tab slot
			var fromBankTab = this[fromBankTabId];
			if (fromBankTab == null) return;

			if (fromTabSlot >= GuildMgr.MAX_BANK_TAB_SLOTS) return;

			var bankTabRights = rank.BankTabRights;

			if (!bankTabRights[fromBankTabId].Privileges.HasFlag(GuildBankTabPrivileges.ViewTab))
				return;
			if (!(bankTabRights[fromBankTabId].WithdrawlAllowance > 0))
				return;

			var fromItem = fromBankTab[fromTabSlot];
			if (fromItem == null) return;

			if (fromItem.EntryId != itemEntryId) return; // Cheater!
			if (fromItem.Amount < amount) return; // Cheater!
			if (amount == 0)
			{
				amount = (byte)fromItem.Amount;
			}

			var bag = chr.Inventory.GetContainer((InventorySlot)bagSlot, false);
			if (bag == null)
			{
				return;
			}

			if (!bag.IsValidSlot(slot))
			{
				return;
			}

			var bagItem = bag[slot];
			if (bagItem == null)
			{
				// unoccupied slot
				var newBagAmount = (int)amount;
                var newBankAmount = fromItem.Amount - (int)amount;
				var msg = bag.TryAdd(fromItem.Template, ref newBagAmount, slot);
				if (msg != InventoryError.OK)
				{
					ItemHandler.SendInventoryError(chr, msg);
					return;
				}

                // Does this slot have any of this item left in the bank?
                if (newBankAmount <= 0)
                {
                    //None left in this slot in the bank, get rid of it
                    fromBankTab[fromTabSlot] = null;
                }
                else
                {
                    fromItem.Amount -= amount;
                }
			}
			else
			{
				// occupied slot
				if (bagItem.EntryId == fromItem.EntryId)
				{
					// Same Item, try to merge the stacks
					var amtAdded = (int)amount;
					var msg = bag.TryMerge(fromItem.Template, ref amtAdded, slot, true);
					if (msg != InventoryError.OK)
					{
						ItemHandler.SendInventoryError(chr, msg);
						return;
					}

                    // Decrement the stack count of the item in the bank
                    fromItem.Amount -= amtAdded;

                    // Does this slot have any of this item left in the bank?
                    if (fromItem.Amount <= 0)
                    {
                        //None left in this slot in the bank, get rid of it
                        fromBankTab[fromTabSlot] = null;
                    }

				}
				else
				{
					// Different Item
					if (amount != fromItem.Amount)
					{
						// Can't Swap a partial stack.
						ItemHandler.SendInventoryError(chr, InventoryError.COULDNT_SPLIT_ITEMS);
						return;
					}

					// This necessitates a stack swap, check that the character has deposit permissions for the GuildBank
					// Check that the character can deposit to the source BankTab
					if (!bankTabRights[fromBankTabId].Privileges.HasFlag(GuildBankTabPrivileges.DepositItem))
					{
						return;
					}

					// Take the Item from the player's bag
					var item = bag.Remove(slot, true);

					// Take the bank Item and add it to the character.
					var amtAdded = (int)amount;
					var msg = bag.TryAdd(fromItem.Template, ref amtAdded, slot);
					if (msg != InventoryError.OK)
					{
						// put the old item back.
						bag.AddUnchecked(slot, item, true);

						ItemHandler.SendInventoryError(chr, msg);
						return;
					}

                    // Decrement the stack count of the item in the bank
                    fromItem.Amount -= amtAdded;

                    // Does this slot have any of this item left in the bank?
                    if (fromItem.Amount <= 0)
                    {
                        //None left in this slot in the bank, get rid of it
                        fromBankTab[fromTabSlot] = null;
                    }

					// add the Item from the character's bag to the bank.
					fromBankTab[fromTabSlot] = item.Record;

					BankLog.LogEvent(GuildBankLogEntryType.WithdrawItem, chr, fromItem, amount, fromBankTab);
					BankLog.LogEvent(GuildBankLogEntryType.DepositItem, chr, item.Record, fromBankTab);

					Guild.SendGuildBankTabContentUpdateToAll(fromBankTabId, fromTabSlot);
					return;
				}
			}

			BankLog.LogEvent(GuildBankLogEntryType.WithdrawItem, chr, fromItem, amount, fromBankTab);
			Guild.SendGuildBankTabContentUpdateToAll(fromBankTabId, fromTabSlot);
		}

		public void SwapItemsAutoStoreCharToBank(Character chr, GameObject bank, byte toBankTabId, byte bagSlot, byte slot, uint itemEntryId, byte autoStoreCount)
		{
			if (!CheckBankObj(chr, bank)) return;
			var member = chr.GuildMember;
			if (member == null) return;

			var rank = member.Rank;
			if (rank == null) return;

			//check for valid tab / tab slot
			var toBankTab = this[toBankTabId];
			if (toBankTab == null) return;

			var bankTabRights = rank.BankTabRights;

			if (!bankTabRights[toBankTabId].Privileges.HasFlag(GuildBankTabPrivileges.DepositItem))
				return;

			var bag = chr.Inventory.GetContainer((InventorySlot)bagSlot, false);
			if (bag == null)
			{
				return;
			}

			if (!bag.IsValidSlot(slot))
			{
				return;
			}

			var bagItem = bag.Remove(bagSlot, true);
			if (bagItem == null) return;

			var freeSlot = toBankTab.FindFreeSlot();
			if (freeSlot == -1)
			{
				bag.AddUnchecked(bagSlot, bagItem, true);
				ItemHandler.SendInventoryError(chr, InventoryError.BANK_FULL);
				return;
			}

			toBankTab[freeSlot] = bagItem.Record;

			bankTabRights[toBankTabId].WithdrawlAllowance++;

			BankLog.LogEvent(GuildBankLogEntryType.DepositItem, chr, bagItem.Record, toBankTab);

			Guild.SendGuildBankTabContentUpdateToAll(toBankTabId, freeSlot);
		}

		public void SwapItemsManualCharToBank(Character chr, GameObject bank, byte bagSlot, byte slot, uint itemEntryId, byte toBankTabId, byte toTabSlot, byte amount)
		{
			if (!CheckBankObj(chr, bank)) return;
			var member = chr.GuildMember;
			if (member == null) return;

			var rank = member.Rank;
			if (rank == null) return;

			//check for valid tab / tab slot
			var toBankTab = this[toBankTabId];
			if (toBankTab == null) return;

			if (toTabSlot >= GuildMgr.MAX_BANK_TAB_SLOTS) return;

			var bankTabRights = rank.BankTabRights;

			if (!bankTabRights[toBankTabId].Privileges.HasFlag(GuildBankTabPrivileges.DepositItem))
				return;

			var toItem = toBankTab[toTabSlot];

			var bag = chr.Inventory.GetContainer((InventorySlot)bagSlot, false);
			if (bag == null) return;
			if (!bag.IsValidSlot(slot)) return;

			var bagItem = bag[slot];
			if (bagItem == null) return;
			//if (bagItem.EntryId != itemEntryId) return; // Cheater!
			if (bagItem.Amount < amount) return; // Cheater!
			if (amount == 0)
			{
				amount = (byte)bagItem.Amount;
			}

			if (toItem == null)
			{
				// unoccupied slot
                bagItem = bagItem.Amount > amount ? bagItem.Split(amount) : bag.Remove(slot, true);

				toBankTab[toTabSlot] = bagItem.Record;
                BankLog.LogEvent(GuildBankLogEntryType.DepositItem, chr, bagItem.Record, toBankTab);
                Guild.SendGuildBankTabContentUpdateToAll(toBankTabId, toTabSlot);
                return;
			}

				// occupied slot
            if (bagItem.EntryId == toItem.EntryId)
            {
                // Same Item, try to merge the stacks
                // If the whole stack is merged, the bagItem.Amount goes to zero and the bagItem is destroyed
                // If a partial stack is merged, the bagItem.Amount is automatically updated.
                var retItem = toBankTab.StoreItemInSlot(bagItem.Record, amount, toTabSlot, true);
				// TODO: What about the returned Item?
            }
            else
            {
                // Different Item
                if (amount != bagItem.Amount)
                {
                    // Can't Swap a partial stack.
                    ItemHandler.SendInventoryError(chr, InventoryError.COULDNT_SPLIT_ITEMS);
                    return;
                }

                // This necessitates a stack swap, check that the character has withdrawl permissions for the GuildBank
                // Check that the character can deposit to the source BankTab
                if (!(bankTabRights[toBankTabId].WithdrawlAllowance > 0))
                {
                    return;
                }

                // Take the Item from the player's bag
                var item = bagItem.Amount > amount ? bagItem.Split(amount) : bag.Remove(slot, true);

                // Take the bank Item and add it to the character.
                var amtAdded = (int)amount;
                var msg = bag.TryAdd(toItem.Template, ref amtAdded, slot);
                if (msg != InventoryError.OK)
                {
                    // put the old item back.
                    bag.AddUnchecked(slot, item, true);

                    ItemHandler.SendInventoryError(chr, msg);
                    return;
                }

                // add the Item from the character's bag to the bank.
                toBankTab[toTabSlot] = item.Record;

                BankLog.LogEvent(GuildBankLogEntryType.WithdrawItem, chr, toItem, amount, toBankTab);
                BankLog.LogEvent(GuildBankLogEntryType.DepositItem, chr, item.Record, toBankTab);

                Guild.SendGuildBankTabContentUpdateToAll(toBankTabId, toTabSlot);
                return;
            }

			BankLog.LogEvent(GuildBankLogEntryType.WithdrawItem, chr, toItem, amount, toBankTab);
			Guild.SendGuildBankTabContentUpdateToAll(toBankTabId, toTabSlot);
		}

		public void BuyTab(Character chr, GameObject bank, byte tabId)
		{
			if (!CheckBankObj(chr, bank)) return;
			var member = chr.GuildMember;
			if (member == null) return;

			var rank = member.Rank;
			if (rank == null) return;

			if (!member.IsLeader) return;
			if (tabId >= GuildMgr.MAX_BANK_TABS) return;
			if (Guild.PurchasedBankTabCount >= GuildMgr.MAX_BANK_TABS) return;
			if (tabId != Guild.PurchasedBankTabCount) return;

			var tabPrice = BankTabPrices[tabId];
			if (chr.Money < tabPrice) return;

			if (AddNewBankTab(tabId))
			{
			    rank.BankTabRights[tabId].Privileges = GuildBankTabPrivileges.Full;
			    rank.BankTabRights[tabId].WithdrawlAllowance = 0xFFFFFFFF;
                GuildHandler.SendGuildRosterToGuildMembers(Guild);
				GuildHandler.SendGuildBankTabNames(chr, bank);
			}
		}

		public void ModifyTabInfo(Character chr, GameObject bank, byte tabId, string newName, string newIcon)
		{
			if (!CheckBankObj(chr, bank)) return;
			var member = chr.GuildMember;
			if (member == null) return;

			if (!member.IsLeader) return;

			if (tabId < 0 || tabId > Guild.PurchasedBankTabCount) return;

			var tab = this[tabId];
			if (tab == null) return;

			tab.Name = newName;
			tab.Icon = newIcon;

			tab.UpdateLater();

			GuildHandler.SendGuildBankTabNames(chr, bank);
			GuildHandler.SendGuildBankTabContents(chr, bank, tabId);
		}

		public void GetBankTabText(Character chr, byte tabId)
		{
            if (tabId < 0 || tabId >= GuildMgr.MAX_BANK_TABS) return;
            if (tabId > Guild.PurchasedBankTabCount) return;

			var tab = this[tabId];
			if (tab == null) return;

			GuildHandler.SendGuildBankTabText(chr, tabId, tab.Text);
		}

		public void SetBankTabText(Character chr, byte tabId, string newText)
		{
			var member = chr.GuildMember;
			if (member == null) return;

			var rank = member.Rank;
			if (rank == null) return;

			if (tabId < 0 || tabId >= GuildMgr.MAX_BANK_TABS) return;
			if (tabId > Guild.PurchasedBankTabCount) return;

			var tab = this[tabId];
			if (tab == null) return;

			if (!rank.BankTabRights[tabId].Privileges.HasFlag(GuildBankTabPrivileges.UpdateText))
				return;

			tab.Text = newText.Length < 501 ? newText : newText.Substring(0, 500);
			tab.UpdateLater();

			Guild.Broadcast(GuildHandler.CreateBankTabTextPacket(tabId, newText));
		}

		public void QueryBankLog(Character chr, byte tabId)
		{
			if (tabId < 0 || tabId >= GuildMgr.MAX_BANK_TABS) return;
			if (tabId > Guild.PurchasedBankTabCount) return;

			var tab = this[tabId];
			if (tab == null) return;

			GuildHandler.SendGuildBankLog(chr, BankLog, tabId);
		}

		private static bool CheckBankObj(Character chr, GameObject bankObj)
		{
			chr.EnsureContext();
			if (bankObj == null) return false;
			bankObj.EnsureContext();
			if (!bankObj.CanBeUsedBy(chr)) return false;

			return true;
		}

		private bool AddNewBankTab(int tabId)
		{
			if (tabId < 0 || tabId >= GuildMgr.MAX_BANK_TABS) return false;

			var bankTab = this[tabId];
			if (bankTab != null) return false;

			Guild.PurchasedBankTabCount++;
			var tab = new GuildBankTab
			{
				Bank = this,
				BankSlot = tabId,
                Icon = "",
                Name = "Slot " + (tabId + 1),
                Text = ""
			};
			ArrayUtil.AddOnlyOne(ref bankTabs, tab);

			tab.CreateLater();

			return true;
		}
	}
}