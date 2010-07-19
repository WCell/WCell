using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.Misc
{
	public class Trade
	{
		public int TradeId;
		public TradeInfo Trader1, Trader2;


	}

	/// <summary>
	/// Represents the progress of trading between two characters
	/// Each trading party has its own instance of Trade
	/// </summary>
	public class TradeInfo
	{
		private const int TRADE_SLOT_COUNT = 7;
		private const int TRADE_SLOT_NONTRADED = 6;
		private const int TRADE_SLOT_TRADED_COUNT = 6;

		private readonly Item[] m_items;
		private bool m_accepted;
		private Character m_chr;
		private uint m_money;

		private TradeStatus m_status;

		private TradeInfo(Trade trade)
		{
			Trade = trade;
			m_items = new Item[TRADE_SLOT_COUNT];
		}

		public Trade Trade { get; set; }

		/// <summary>
		/// Status of the trading progress
		/// </summary>
		public TradeStatus Status
		{
			get { return m_status; }
			set { m_status = value; }
		}

		/// <summary>
		/// Other party of the trading progress
		/// </summary>
		public TradeInfo Other
		{
			get { return Trade.Trader1 == this ? Trade.Trader2 : Trade.Trader1; }
		}

		/// <summary>
		/// Makes initChr propose trading to targetChr
		/// Call CheckRequirements first
		/// </summary>
		/// <param name="initChr">initiator of trading</param>
		/// <param name="targetChr">traget of trading</param>
		public static void Propose(Character initChr, Character targetChr)
		{
			var trade = new Trade();
			trade.Trader1 = new TradeInfo(trade)
			{
				m_chr = initChr,
				m_status = TradeStatus.Proposed
			};

			trade.Trader2 = new TradeInfo(trade)
			{
				m_chr = targetChr,
				m_status = TradeStatus.Proposed
			};

			initChr.TradeInfo = trade.Trader1;
			targetChr.TradeInfo = trade.Trader2;

			TradeHandler.SendTradeProposal(targetChr.Client, initChr);
		}

		/// <summary>
		/// Checks requirements for trading between two characters
		/// </summary>
		/// <param name="initChr">possible initiator of trading</param>
		/// <param name="targetChr">possible target of trading</param>
		/// <returns></returns>
		public static bool CheckRequirements(Character initChr, Character targetChr)
		{
			var tradeStatus = TradeStatus.Proposed;

			if (initChr.IsLoggingOut)
			{
				tradeStatus = TradeStatus.LoggingOut;
			}
			else if (!initChr.IsAlive)
			{
				tradeStatus = TradeStatus.PlayerDead;
			}

			if (tradeStatus != TradeStatus.Proposed)
			{
				TradeHandler.SendTradeStatus(initChr, tradeStatus);
				return false;
			}

			if (targetChr == null)
			{
				TradeHandler.SendTradeStatus(initChr, TradeStatus.PlayerNotFound);
				return false;
			}

			if (!targetChr.IsInRadius(initChr, 10.0f))
			{
				tradeStatus = TradeStatus.TooFarAway;
			}
			else if (!targetChr.IsAlive)
			{
				tradeStatus = TradeStatus.TargetDead;
			}
			else if (targetChr.IsStunned)
			{
				tradeStatus = TradeStatus.TargetStunned;
			}
			else if (Singleton<RelationMgr>.Instance.HasRelation(
				targetChr.EntityId.Low, initChr.EntityId.Low, CharacterRelationType.Ignored))
			{
				tradeStatus = TradeStatus.PlayerIgnored;
			}
			else if (targetChr.TradeInfo != null)
			{
				tradeStatus = TradeStatus.AlreadyTrading;
			}
			else if (targetChr.Faction.Group != initChr.Faction.Group && !initChr.Role.IsStaff)
			{
				tradeStatus = TradeStatus.WrongFaction;
			}
			else if (targetChr.IsLoggingOut)
			{
				tradeStatus = TradeStatus.TargetLoggingOut;
			}

			if (tradeStatus != TradeStatus.Proposed)
			{
				TradeHandler.SendTradeStatus(initChr.Client, tradeStatus);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Accepts the proposition of trade
		/// </summary>
		public void AcceptTradeProposal()
		{
			SetTradeStatus(TradeStatus.Initiated);
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
		public void Cancel()
		{
			StopTrade(TradeStatus.Cancelled, true);
		}

		/// <summary>
		/// Puts an item into the trading window
		/// </summary>
		/// <param name="tradeSlot">slot in the trading window</param>
		/// <param name="bag">inventory bag number</param>
		/// <param name="slot">inventory slot number</param>
		public void SetTradeItem(byte tradeSlot, byte bag, byte slot)
		{
			if (tradeSlot >= TRADE_SLOT_COUNT)
				return;

			var inv = m_chr.Inventory;

			var item = inv.GetItem((InventorySlot)bag, slot, inv.IsBankOpen);

			if (item == null || (tradeSlot != TRADE_SLOT_NONTRADED && !item.CanBeTraded))
				return; // possible cheating

			for (var i = 0; i < TRADE_SLOT_COUNT; i++)
				if (item == m_items[i])
					return;

			m_items[tradeSlot] = item;

			UpdateTrade();
		}

		/// <summary>
		/// Removes an item from the trading window
		/// </summary>
		/// <param name="tradeSlot">slot in the trading window</param>
		public void ClearTradeItem(byte tradeSlot)
		{
			if (tradeSlot >= TRADE_SLOT_COUNT)
				return;

			m_items[tradeSlot] = null;

			UpdateTrade();
		}

		/// <summary>
		/// Changes the amount of money to trade
		/// </summary>
		/// <param name="money">new amount of coins</param>
		public void SetMoney(uint money)
		{
			// Amount checked at CommitTrade
			m_money = money;

			UpdateTrade();
		}

		/// <summary>
		/// Accepts the trade
		/// If both parties have accepted, commits the trade
		/// </summary>
		public void AcceptTrade()
		{
			m_accepted = true;

			if (!CheckMoney() || !CheckItems())
				return;

			if (Other.m_accepted)
				CommitTrade();
			else
				TradeHandler.SendTradeStatus(Other.m_chr.Client, TradeStatus.Accepted);
		}

		/// <summary>
		/// Unaccepts the trade (usually due to change of traded items or amount of money)
		/// </summary>
		public void UnacceptTrade()
		{
			if (Other.m_chr != null)
				TradeHandler.SendTradeStatus(Other.m_chr.Client, TradeStatus.StateChanged);

			m_accepted = false;
		}

		/// <summary>
		/// Sends new information about the trading process to other party
		/// </summary>
		private void UpdateTrade()
		{
			if (Other.m_chr != null)
				TradeHandler.SendTradeUpdate(Other.m_chr.Client, m_money, m_items);
		}

		/// <summary>
		/// Sends the notification about the change of trade status and stops the trade
		/// </summary>
		/// <param name="status">new status</param>
		/// <param name="notifySelf">whether to notify the caller himself</param>
		private void StopTrade(TradeStatus status, bool notifySelf)
		{
			Other.Status = m_status = status;

			if (m_chr != null)
			{
				if (notifySelf)
					TradeHandler.SendTradeStatus(m_chr.Client, m_status);

				m_chr.TradeInfo = null;
			}

			if (Other.m_chr != null)
			{
				TradeHandler.SendTradeStatus(Other.m_chr.Client, m_status);
				Other.m_chr.TradeInfo = null;
			}
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
			for (var i = 0; i < TRADE_SLOT_COUNT; i++)
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
			TradeHandler.SendTradeStatus(Other.m_chr.Client, TradeStatus.Accepted);

			var myFreeSlots = GetSlotsForTradedItems();
			var othersFreeSlots = Other.GetSlotsForTradedItems();

			if (!CheckFreeSlots(myFreeSlots, Other.m_items) || !Other.CheckFreeSlots(othersFreeSlots, m_items))
			{
				m_accepted = false;
				return;
			}

			RemoveTradedItems();
			Other.RemoveTradedItems();

			AddItems(myFreeSlots, Other.m_items);
			Other.AddItems(othersFreeSlots, m_items);

			GiveMoney();
			Other.GiveMoney();

			//m_chr.SaveLater();
			//Other.m_chr.SaveLater();

			StopTrade(TradeStatus.Complete, true);
		}

		private bool CheckFreeSlots(IList<SimpleSlotId> myFreeSlots, Item[] items)
		{
			var hasSlots = true;
			var currentSlot = 0;

			for (var i = 0; i < TRADE_SLOT_TRADED_COUNT; i++)
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
				Other.m_chr.SendSystemMessage("Other party doesn't have enough free slots");

				SetTradeStatus(TradeStatus.StateChanged);
			}

			return hasSlots;
		}

		private IList<SimpleSlotId> GetSlotsForTradedItems()
		{
			var slots = m_chr.Inventory.FindFreeSlots(false, TRADE_SLOT_TRADED_COUNT);

			if (slots.Count >= TRADE_SLOT_TRADED_COUNT)
				return slots;

			// We didn't get enough free slots, so check if we would have enough free slots
			// after the trade takes place
			for (var i = 0; i < TRADE_SLOT_TRADED_COUNT; i++)
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
				Other.m_chr.AddMoney(m_money);
		}

		private void RemoveTradedItems()
		{
			for (var i = 0; i < TRADE_SLOT_TRADED_COUNT; i++)
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
		/// Sets new status of trade and sends notification about the change to both parties
		/// </summary>
		/// <param name="status">new status</param>
		private void SetTradeStatus(TradeStatus status)
		{
			Other.m_status = m_status = status;

			if (m_chr != null)
				TradeHandler.SendTradeStatus(m_chr.Client, m_status);

			if (Other.m_chr != null)
				TradeHandler.SendTradeStatus(Other.m_chr.Client, m_status);
		}
	}
}