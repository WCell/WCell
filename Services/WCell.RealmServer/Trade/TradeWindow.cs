using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.Trade
{

	/// <summary>
	/// Represents the progress of trading between two characters
	/// Each trading party has its own instance of a TradeWindow
	/// </summary>
	public class TradeWindow
	{
		private readonly Item[] m_items;
		private bool m_accepted;
		private Character m_chr;
		private uint m_money;

		internal TradeWindow m_otherWindow;

		internal TradeWindow(Character owner)
		{
			m_chr = owner;
			m_items = new Item[TradeMgr.MaxSlotCount];
		}

		public Character Owner
		{
			get { return m_chr; }
		}

		public bool Accepted
		{
			get { return m_accepted; }
		}

		/// <summary>
		/// Other party of the trading progress
		/// </summary>
		public TradeWindow OtherWindow
		{
			get { return m_otherWindow; }
		}

		/// <summary>
		/// Accepts the proposition of trade
		/// </summary>
		public void AcceptTradeProposal()
		{
			SendStatus(TradeStatus.Initiated);
			//Other.m_status = m_status = TradeStatus.Initiated;

			//TradeHandler.SendTradeProposalAccepted(m_chr.Client);
			//TradeHandler.SendTradeProposalAccepted(Other.m_chr.Client);
		}

		/// <summary>
		/// Declines the trade request because one party is busy
		/// </summary>
		public void DeclineBusy()
		{
			StopTrade(TradeStatus.PlayerBusy, false);
		}

		/// <summary>
		/// Declines the trade request because one party ignores another
		/// </summary>
		public void DeclineIgnore()
		{
			StopTrade(TradeStatus.PlayerIgnored, false);
		}

		/// <summary>
		/// Cancels the trade
		/// </summary>
		public void Cancel(TradeStatus status = TradeStatus.Cancelled)
		{
			StopTrade(status, true);
		}

		/// <summary>
		/// Puts an item into the trading window
		/// </summary>
		/// <param name="tradeSlot">slot in the trading window</param>
		/// <param name="bag">inventory bag number</param>
		/// <param name="slot">inventory slot number</param>
		public void SetTradeItem(byte tradeSlot, byte bag, byte slot, bool updateSelf = true)
		{
			if (tradeSlot >= TradeMgr.MaxSlotCount)
				return;

			var inv = m_chr.Inventory;
			var item = inv.GetItem((InventorySlot)bag, slot, inv.IsBankOpen);

			if (item == null || tradeSlot == TradeMgr.NontradeSlot || !item.CanBeTraded)
				return; // possible cheating

			for (var i = 0; i < TradeMgr.MaxSlotCount; i++)
				if (item == m_items[i])
					return;

			m_items[tradeSlot] = item;

			SendTradeInfo(updateSelf);
		}

		/// <summary>
		/// Removes an item from the trading window
		/// </summary>
		/// <param name="tradeSlot">slot in the trading window</param>
		public void ClearTradeItem(byte tradeSlot, bool updateSelf = true)
		{
			if (tradeSlot >= TradeMgr.MaxSlotCount)
				return;

			m_items[tradeSlot] = null;

			SendTradeInfo(updateSelf);
		}

		/// <summary>
		/// Changes the amount of money to trade
		/// </summary>
		/// <param name="money">new amount of coins</param>
		public void SetMoney(uint money, bool updateSelf = true)
		{
			// Amount checked at CommitTrade
			m_money = money;

			SendTradeInfo(updateSelf);
		}

		/// <summary>
		/// Accepts the trade
		/// If both parties have accepted, commits the trade
		/// </summary>
		public void AcceptTrade(bool updateSelf = true)
		{
			m_accepted = true;

			if (!CheckMoney() || !CheckItems())
				return;

			if (OtherWindow.m_accepted)
			{
				CommitTrade();
			}
			else
			{
				SendStatus(TradeStatus.Accepted, updateSelf);
			}
		}

		/// <summary>
		/// Unaccepts the trade (usually due to change of traded items or amount of money)
		/// </summary>
		public void UnacceptTrade(bool updateSelf = true)
		{
			m_accepted = false;

			SendStatus(TradeStatus.StateChanged, updateSelf);
		}

		/// <summary>
		/// Sends the notification about the change of trade status and stops the trade
		/// </summary>
		/// <param name="status">new status</param>
		/// <param name="notifySelf">whether to notify the caller himself</param>
		private void StopTrade(TradeStatus status, bool notifySelf)
		{
			SendStatus(status, notifySelf);

			m_chr.TradeWindow = null;
			m_chr = null;

			OtherWindow.m_chr.TradeWindow = null;
			OtherWindow.m_chr = null;
		}

		/// <summary>
		/// Checks if the party has enough money for trade
		/// If not, unaccepts the trade
		/// </summary>
		/// <returns></returns>
		private bool CheckMoney()
		{
			if (m_money <= m_chr.Money)
				return true;

			m_chr.SendSystemMessage("Not enough gold");
			m_accepted = false;
			TradeHandler.SendTradeStatus(m_chr.Client, TradeStatus.StateChanged);

			return false;
		}

		/// <summary>
		/// Checks if the party can trade selected items
		/// If not, unaccepts the trade
		/// </summary>
		/// <returns></returns>
		private bool CheckItems()
		{
			for (var i = 0; i < TradeMgr.MaxSlotCount; i++)
			{
				if (m_items[i] == null || m_items[i].CanBeTraded) continue;
				m_accepted = false;
				TradeHandler.SendTradeStatus(m_chr.Client, TradeStatus.StateChanged);

				return false;
			}

			return true;
		}

		/// <summary>
		/// Commits the trade between two parties
		/// </summary>
		private void CommitTrade()
		{
			TradeHandler.SendTradeStatus(OtherWindow.m_chr.Client, TradeStatus.Accepted);

			var myFreeSlots = GetSlotsForTradedItems();
			var othersFreeSlots = m_otherWindow.GetSlotsForTradedItems();

			if (!CheckFreeSlots(myFreeSlots, OtherWindow.m_items) || !m_otherWindow.CheckFreeSlots(othersFreeSlots, m_items))
			{
				m_accepted = false;
				return;
			}

			RemoveTradedItems();
			m_otherWindow.RemoveTradedItems();

			AddItems(myFreeSlots, m_otherWindow.m_items);
			m_otherWindow.AddItems(othersFreeSlots, m_items);

			GiveMoney();
			m_otherWindow.GiveMoney();

			//m_chr.SaveLater();
			//Other.m_chr.SaveLater();

			StopTrade(TradeStatus.Complete, true);
		}

		private bool CheckFreeSlots(IList<SimpleSlotId> myFreeSlots, Item[] items)
		{
			var hasSlots = true;
			var currentSlot = 0;

			for (var i = 0; i < TradeMgr.TradeSlotCount; i++)
			{
				if (items[i] == null)
					continue;

				if (myFreeSlots.Count <= currentSlot)
				{
					hasSlots = false;
					break;
				}

				var slot = myFreeSlots[currentSlot].Slot;
				var container = myFreeSlots[currentSlot].Container;
				var handler = container.GetHandler(slot);

				var err = InventoryError.OK;

				// Does not check if the slot is unoccupied, which is good
				handler.CheckAdd(slot, items[i].Amount, items[i], ref err);

				if (err != InventoryError.OK)
				{
					hasSlots = false;
					break;
				}

				var amount = items[i].Amount;

				container.CheckUniqueness(items[i], ref amount, ref err, true);

				if (err != InventoryError.OK)
				{
					hasSlots = false;
					break;
				}

				currentSlot++;
			}

			if (!hasSlots)
			{
				m_chr.SendSystemMessage("You don't have enough free slots");
				m_otherWindow.m_chr.SendSystemMessage("Other party doesn't have enough free slots");

				SendStatus(TradeStatus.StateChanged);
			}

			return hasSlots;
		}

		private IList<SimpleSlotId> GetSlotsForTradedItems()
		{
			var slots = m_chr.Inventory.FindFreeSlots(false, TradeMgr.TradeSlotCount);

			if (slots.Count >= TradeMgr.TradeSlotCount)
				return slots;

			// We didn't get enough free slots, so check if we would have enough free slots
			// after the trade takes place
			for (var i = 0; i < TradeMgr.TradeSlotCount; i++)
			{
				if (m_items[i] == null) continue;
				slots.Add(new SimpleSlotId
				{
					Container = m_items[i].Container,
					Slot = m_items[i].Slot
				});
			}

			return slots;
		}

		private void GiveMoney()
		{
			if (m_chr.SubtractMoney(m_money))
				m_otherWindow.m_chr.AddMoney(m_money);
		}

		private void RemoveTradedItems()
		{
			for (var i = 0; i < TradeMgr.TradeSlotCount; i++)
			{
				if (m_items[i] != null)
				{
					//m_items[i].GiftCreator = m_chr;
					m_items[i].Remove(true); // ownerChange = true
				}
			}
		}

		private void AddItems(IList<SimpleSlotId> simpleSlots, Item[] items)
		{
			var slotId = 0;

			for (var i = 0; i < items.Length; i++)
			{
				var item = items[i];

				if (item == null) continue;

				var simpleSlot = simpleSlots[slotId];
				var amount = item.Amount;

				simpleSlot.Container.Distribute(item.Template, ref amount);
				if (amount < item.Amount)
				{
					item.Amount -= amount;
					simpleSlot.Container.AddUnchecked(simpleSlot.Slot, item, true);
				}
				else
				{
					// The whole stack was added to the other stacks, we can dispose of the item.
					item.Amount = 0;  // destroys the item.
				}

				slotId++;
			}
		}

		/// <summary>
		/// Sends new information about the trading process to other party
		/// </summary>
		private void SendTradeInfo(bool updateSelf)
		{
			if (updateSelf)
			{
				TradeHandler.SendTradeUpdate(m_chr.Client, OtherWindow.m_money, OtherWindow.m_items);
				// TODO: Send own data to self again
			}
			TradeHandler.SendTradeUpdate(OtherWindow.m_chr.Client, m_money, m_items);
		}

		/// <summary>
		/// Sets new status of trade and sends notification about the change to both parties
		/// </summary>
		/// <param name="status">new status</param>
		private void SendStatus(TradeStatus status, bool notifySelf = true)
		{
			if (notifySelf)
			{
				TradeHandler.SendTradeStatus(m_chr.Client, status);
			}
			TradeHandler.SendTradeStatus(OtherWindow.m_chr.Client, status);
		}
	}
}
