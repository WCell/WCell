using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Guilds;
using WCell.Constants.Items;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
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

		public GuildBank()
		{
            BankTabs = new ImmutableList<GuildBankTab>(5);
            var tab = new GuildBankTab(this)
            {
                BankSlot = 0,
                Icon = "",
                Name = "Slot 0",
                Text = ""
            };
            BankTabs.Add(tab);

			BankLog = new GuildBankLog();
		}

		public Guild Guild
		{
			get;
			private set;
		}

		public IList<GuildBankTab> BankTabs
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
			get { return tabId < Guild.PurchasedBankTabCount ? BankTabs[tabId] : null; }
		}

		public void DepositMoney(Character depositer, GameObject bank, uint deposit)
		{
			if (deposit == 0) return;
			if (!bank.CanBeUsedBy(depositer)) return;
			if (depositer.Guild == null || depositer.GuildMember == null) return;
			if (depositer.Guild != Guild) return;
			if (depositer.Money < deposit) return;

			Guild.Money += deposit;
			depositer.Money -= deposit;

			BankLog.LogEvent(GuildBankLogEntryType.DepositMoney, depositer, deposit, null, 0, null);
			GuildHandler.SendGuildBankTabNames(depositer, bank);
			GuildHandler.SendGuildBankTabContents(depositer, bank, 0);
			GuildHandler.SendGuildBankMoneyUpdate(depositer, bank);
		}

		public void WithdrawMoney(Character withdrawer, GameObject bank, uint withdrawl)
		{
			if (withdrawl == 0) return;
			if (!bank.CanBeUsedBy(withdrawer)) return;
			if (withdrawer.Guild == null || withdrawer.GuildMember == null) return;
			if (withdrawer.Guild != Guild) return;
			if (Guild.Money < withdrawl) return;

			var member = withdrawer.GuildMember;
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
			GuildHandler.SendGuildBankTabNames(withdrawer, bank);
			GuildHandler.SendGuildBankTabContents(withdrawer, bank, 0);
			GuildHandler.SendGuildBankMoneyUpdate(withdrawer, bank);
		}

		public void SwapItemsManualBankToBank(Character chr, GameObject bank, byte fromBankTabId, byte fromTabSlot, byte toBankTabId, byte toTabSlot, uint itemEntryId, byte amount)
		{
			if (!DoBankChecks(chr, bank)) return;

			//check for valid tab / tab slot
			var fromBankTab = this[fromBankTabId];
			if (fromBankTab == null) return;

			var toBankTab = this[toBankTabId];
			if (toBankTab == null) return;

			if (fromTabSlot >= GuildMgr.MAX_BANK_TAB_SLOTS) return;
			if (toTabSlot >= GuildMgr.MAX_BANK_TAB_SLOTS) return;

			var bankTabRights = chr.GuildMember.Rank.BankTabRights;

			if (!bankTabRights[fromBankTabId].Privileges.HasFlag(GuildBankTabPrivileges.ViewTab))
				return;
			if (!(bankTabRights[fromBankTabId].WithdrawlAllowance > 0))
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

						chr.Guild.SendGuildBankTabContentUpdateToAll(fromBankTabId, fromTabSlot);
						chr.Guild.SendGuildBankTabContentUpdateToAll(toBankTabId, toTabSlot);
						return;
					}
				}

				// Moving from one tab to another costs you a withdrawl point.
				bankTabRights[fromBankTabId].WithdrawlAllowance--;
			}

			// Log the item move.
			BankLog.LogEvent(GuildBankLogEntryType.MoveItem, chr, fromItem, amount, toBankTab);

			chr.Guild.SendGuildBankTabContentUpdateToAll(fromBankTabId, fromTabSlot, isIntraTab ? toTabSlot : -1);

			if (!isIntraTab)
			{
				chr.Guild.SendGuildBankTabContentUpdateToAll(toBankTabId, toTabSlot);
			}

		}

		public void SwapItemsAutoStoreBankToChar(Character chr, GameObject bank, byte fromBankTabId, byte fromTabSlot, uint itemEntryId, byte autoStoreCount)
		{
			if (!DoBankChecks(chr, bank)) return;

			var fromBankTab = this[fromBankTabId];
			if (fromBankTab == null) return;

			if (fromTabSlot >= GuildMgr.MAX_BANK_TAB_SLOTS) return;

			var bankTabRights = chr.GuildMember.Rank.BankTabRights;

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

			chr.Guild.SendGuildBankTabContentUpdateToAll(fromBankTabId, fromTabSlot);
		}

		public void SwapItemsManualBankToChar(Character chr, GameObject bank, byte fromBankTabId, byte fromTabSlot, byte bagSlot, byte slot, uint itemEntryId, byte amount)
		{
			if (!DoBankChecks(chr, bank)) return;

			//check for valid tab / tab slot
			var fromBankTab = this[fromBankTabId];
			if (fromBankTab == null) return;

			if (fromTabSlot >= GuildMgr.MAX_BANK_TAB_SLOTS) return;

			var bankTabRights = chr.GuildMember.Rank.BankTabRights;

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
				var newAmt = (int)amount;
				var msg = bag.TryAdd(fromItem.Template, ref newAmt, slot);
				if (msg != InventoryError.OK)
				{
					ItemHandler.SendInventoryError(chr, msg);
					return;
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

					fromItem.Amount -= (int)amtAdded;
					if (fromItem.Amount <= 0)
					{
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

					// add the Item from the character's bag to the bank.
					fromBankTab[fromTabSlot] = item.Record;

					BankLog.LogEvent(GuildBankLogEntryType.WithdrawItem, chr, fromItem, amount, fromBankTab);
					BankLog.LogEvent(GuildBankLogEntryType.DepositItem, chr, item.Record, fromBankTab);

					chr.Guild.SendGuildBankTabContentUpdateToAll(fromBankTabId, fromTabSlot);
					return;
				}
			}

			BankLog.LogEvent(GuildBankLogEntryType.WithdrawItem, chr, fromItem, amount, fromBankTab);
			chr.Guild.SendGuildBankTabContentUpdateToAll(fromBankTabId, fromTabSlot);
		}

		public void SwapItemsAutoStoreCharToBank(Character chr, GameObject bank, byte toBankTabId, byte bagSlot, byte slot, uint itemEntryId, byte autoStoreCount)
		{
			if (!DoBankChecks(chr, bank)) return;

			//check for valid tab / tab slot
			var toBankTab = this[toBankTabId];
			if (toBankTab == null) return;

			var bankTabRights = chr.GuildMember.Rank.BankTabRights;

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

			chr.Guild.SendGuildBankTabContentUpdateToAll(toBankTabId, freeSlot);
		}

		public void SwapItemsManualCharToBank(Character chr, GameObject bank, byte bagSlot, byte slot, uint itemEntryId, byte toBankTabId, byte toTabSlot, byte amount)
		{
			if (!DoBankChecks(chr, bank)) return;

			//check for valid tab / tab slot
			var toBankTab = this[toBankTabId];
			if (toBankTab == null) return;

			if (toTabSlot >= GuildMgr.MAX_BANK_TAB_SLOTS) return;

			var bankTabRights = chr.GuildMember.Rank.BankTabRights;

			if (!bankTabRights[toBankTabId].Privileges.HasFlag(GuildBankTabPrivileges.DepositItem))
				return;

			var toItem = toBankTab[toTabSlot];

			var bag = chr.Inventory.GetContainer((InventorySlot)bagSlot, false);
			if (bag == null) return;
			if (!bag.IsValidSlot(slot)) return;

			var bagItem = bag[slot];
			if (bagItem == null) return;
			if (bagItem.EntryId != itemEntryId) return; // Cheater!
			if (bagItem.Amount < amount) return; // Cheater!
			if (amount == 0)
			{
				amount = (byte)bagItem.Amount;
			}

			if (toItem == null)
			{
				// unoccupied slot
				bagItem = bag.Remove(slot, true);
				toBankTab[toTabSlot] = bagItem.Record;
			}
			else
			{
				// occupied slot
				if (bagItem.EntryId == toItem.EntryId)
				{
					// Same Item, try to merge the stacks
					// If the whole stack is merged, the bagItem.Amount goes to zero and the bagItem is destroyed
					// If a partial stack is merged, the bagItem.Amount is automatically updated.
					var retItem = toBankTab.StoreItemInSlot(bagItem.Record, amount, toTabSlot, true);
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
					var item = bag.Remove(slot, true);

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

					chr.Guild.SendGuildBankTabContentUpdateToAll(toBankTabId, toTabSlot);
					return;
				}
			}

			BankLog.LogEvent(GuildBankLogEntryType.WithdrawItem, chr, toItem, amount, toBankTab);
			chr.Guild.SendGuildBankTabContentUpdateToAll(toBankTabId, toTabSlot);
		}

		public void BuyTab(Character chr, GameObject bank, byte tabId)
		{
			if (!DoBankChecks(chr, bank)) return;
			if (chr.Guild.Leader.Character != chr) return;
			if (tabId >= GuildMgr.MAX_BANK_TABS) return;
			if (Guild.PurchasedBankTabCount >= GuildMgr.MAX_BANK_TABS) return;
			if (tabId != Guild.PurchasedBankTabCount) return;

			var tabPrice = BankTabPrices[tabId];
			if (chr.Money < tabPrice) return;

			if (AddNewBankTab(tabId))
			{
				GuildHandler.SendGuildBankTabNames(chr, bank);
			}
		}

		public void ModifyTabInfo(Character chr, GameObject bank, byte tabId, string newName, string newIcon)
		{
			if (!DoBankChecks(chr, bank)) return;
			var guild = chr.Guild;
			if (guild.Leader.Character != chr) return;

			if (tabId < 0 || tabId > Guild.PurchasedBankTabCount) return;
            if (tabId > BankTabs.Count - 1) return;

			var tab = BankTabs[tabId];
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
            if (tabId > BankTabs.Count - 1) return;

			var tab = BankTabs[tabId];
			if (tab == null) return;

			GuildHandler.SendGuildBankTabText(chr, tabId, tab.Text);
		}

		public void SetBankTabText(Character chr, byte tabId, string newText)
		{
			if (tabId < 0 || tabId >= GuildMgr.MAX_BANK_TABS) return;
			if (tabId > Guild.PurchasedBankTabCount) return;

			var tab = BankTabs[tabId];
			if (tab == null) return;

			if (!chr.GuildMember.Rank.BankTabRights[tabId].Privileges.HasFlag(GuildBankTabPrivileges.UpdateText))
				return;

			tab.Text = newText.Length < 501 ? newText : newText.Substring(0, 500);
			tab.UpdateLater();

			chr.Guild.Broadcast(GuildHandler.CreateBankTabTextPacket(tabId, newText));
		}

		public void QueryBankLog(Character chr, byte tabId)
		{
			if (tabId < 0 || tabId >= GuildMgr.MAX_BANK_TABS) return;
			if (tabId > Guild.PurchasedBankTabCount) return;
            if (tabId > BankTabs.Count - 1) return;

			var tab = BankTabs[tabId];
			if (tab == null) return;

			GuildHandler.SendGuildBankLog(chr, BankLog, tabId);
		}

		private bool DoBankChecks(Character chr, GameObject bank)
		{
			if (chr == null) return false;
			if (bank == null) return false;
			if (!bank.CanBeUsedBy(chr)) return false;

			return true;
		}

		private bool AddNewBankTab(int tabId)
		{
			if (tabId < 0 || tabId >= GuildMgr.MAX_BANK_TABS) return false;
			if (tabId < BankTabs.Count - 1 && BankTabs[tabId] != null) return false;

			Guild.PurchasedBankTabCount++;
			var tab = new GuildBankTab
			{
				Bank = this,
				BankSlot = tabId,
                Icon = "",
                Name = "Slot " + tabId,
                Text = ""
			};
			BankTabs.Add(tab);

			tab.CreateLater();

			return true;
		}

		public GuildBank(Guild guild, bool isNew)
		{
			Guild = guild;
            BankLog = new GuildBankLog();
			BankTabs = new ImmutableList<GuildBankTab>(5);
			if (isNew)
			{
				var tab = new GuildBankTab(this)
				          	{
				          		BankSlot = 0,
				          		Icon = "",
				          		Name = "Slot 0",
				          		Text = ""
				          	};
				BankTabs.Add(tab);
			}
			else
			{
				var tabs = GuildBankTab.FindAllByProperty("_guildId", (int) guild.Id);
				foreach (var tab in tabs)
				{
					BankTabs.Add(tab);
				}
			}
		}
	}
}