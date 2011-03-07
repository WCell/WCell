using System;
using System.Collections.Generic;
using WCell.Constants.Items;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.Util;

namespace WCell.RealmServer.NPCs.Vendors
{
	public class VendorEntry
	{
		public const int UnlimitedSupply = -1;

		/// <summary>
		/// A list of VendorItemEtnries that can be bought at this Vendor.
		/// </summary>
		public readonly List<VendorItemEntry> ItemsForSale = new List<VendorItemEntry>(10);

		public readonly NPC NPC;

		public VendorEntry(NPC npc, List<VendorItemEntry> items)
		{
			NPC = npc;

			if (items != null)
			{
				foreach (var item in items)
				{
					ItemsForSale.Add(new VendorItemEntry
					{
						BuyStackSize = item.BuyStackSize,
						RemainingStockAmount = item.RemainingStockAmount,
						ExtendedCostEntry = NPCMgr.ItemExtendedCostEntries[(int)item.ExtendedCostId],
						StockAmount = item.StockAmount,
						StockRefillDelay = item.StockRefillDelay,
						Template = item.Template
					});
				}
			}
		}

		/// <summary>
		/// Returns the VendorItemEntry with the given entryId if contained in the Vendor's ItemsForSale, else null.
		/// </summary>
		public VendorItemEntry GetVendorItem(uint entryId)
		{
			// Does this vendor have this item for sale?
			return ItemsForSale.Find(item => (item.Template.Id == entryId));
		}

		/// <summary>
		/// Character starts a trade-session with this Vendor
		/// </summary>
		/// <param name="chr"></param>
		public void UseVendor(Character chr)
		{
			if (!CheckVendorInteraction(chr))
				return;

			chr.OnInteract(NPC);
			NPCHandler.SendVendorInventoryList(chr, NPC, ItemsForSale);
		}

		/// <summary>
		/// Tries to sell the given Item of the given Character
		/// </summary>
		/// <param name="chr">The seller.</param>
		/// <param name="item">May be null (will result into error message for chr)</param>
		/// <param name="amount">The amount of Item to sell.</param>
		public void SellItem(Character chr, Item item, int amount)
		{
			if (!CheckVendorInteraction(chr))
				return;

			var error = SellItemError.Success;
			if (!CheckCanSellItem(chr, item, ref error))
			{
				NPCHandler.SendSellError(chr.Client, NPC.EntityId, item.EntityId, error);
				return;
			}

			if (amount <= 0 || amount > item.Amount)
			{
				amount = item.Amount;
			}

			var money = (uint)(amount * item.Template.SellPrice);
			var maxDur = item.MaxDurability;
			if (maxDur != 0)
			{
				money = (money * (uint)item.Durability) / (uint)maxDur;
			}

			chr.Money += money;

			var invError = InventoryError.OK;
			if (amount < item.Amount)
			{
				// split the stack

				// create a new stack of items with the number sold
				var newBuyBackItem = item.CreateNew(amount);

				// subtract the sold items from the original stack
				// if item.Amount reaches 0, the item is destroyed
				item.Amount -= amount;

				// Move the new stack to the buyback partial inventory
				invError = chr.Inventory.BuyBack.AddBuyBackItem(newBuyBackItem, true);
			}
			else if (item.Slot != (int)InventorySlot.Invalid)
			{
				// Move the item from the Player's inventory to the buyback inventory
				invError = chr.Inventory.BuyBack.AddBuyBackItem(item, false);
			}

			if (invError != InventoryError.OK)
			{
				ItemHandler.SendInventoryError(chr.Client, invError);
				return;
			}

			NPCHandler.SendSellError(chr.Client, NPC.EntityId, item.EntityId, error);
		}

		public void BuyBackItem(Character chr, int slot)
		{
			var item = TryGetBuyBackItem(chr, slot);
			if (item == null)
			{
				return;
			}

			BuyBackItem(chr, item);
		}

		public void BuyBackItem(Character chr, Item item)
		{
			var inv = chr.Inventory;
			var newSlotId = inv.FindFreeSlot(item, item.Amount, false);

			var err = newSlotId.Container.CheckAdd(newSlotId.Slot, item, item.Amount);

			if (err == InventoryError.OK)
			{
				var amount = item.Amount;
				inv.CheckUniqueness(item, ref amount, ref err, true);
				if (err == InventoryError.OK && amount != item.Amount)
				{
					err = InventoryError.CANT_CARRY_MORE_OF_THIS;
				}
				else
				{
					if (newSlotId.Slot != BaseInventory.INVALID_SLOT)
					{
						var price = ((uint)item.Amount * item.Template.SellPrice);
						if (chr.Money < price)
						{
							NPCHandler.SendBuyError(chr.Client, NPC, (ItemId)item.EntryId, BuyItemError.NotEnoughMoney);
							return;
						}
						else
						{
							item.Remove(false);
							newSlotId.Container.AddUnchecked(newSlotId.Slot, item, true);
							chr.Money -= price;

							NPCHandler.SendBuyItem(chr.Client, NPC, item.Template.ItemId, amount);
							return;
						}
					}
					else
					{
						err = InventoryError.INVENTORY_FULL;
					}
				}
			}

			ItemHandler.SendInventoryError(chr.Client, err);
		}

		public void BuyItem(Character chr, uint itemEntryId, BaseInventory bag, int amount, int slot)
		{
			if (!CheckVendorInteraction(chr))
				return;

			var item = GetVendorItem(itemEntryId);
			if (item == null)
				return;

			amount = Math.Max(amount, 1);
			if (item.BuyStackSize > 0)
			{
				amount = amount * item.BuyStackSize;
			}

			if (!CheckBuyItem(chr, item, amount))
				return;

			// Does the player have enough money?
			var price = chr.Reputations.GetDiscountedCost(NPC.Faction.ReputationIndex, item.Template.BuyPrice);
			if (chr.Money < price)
			{
				NPCHandler.SendBuyError(chr, NPC, item.Template.ItemId, BuyItemError.NotEnoughMoney);
				return;
			}
			BaseInventory inv = chr.Inventory;
			InventoryError err;
			if (inv.IsValidSlot(slot))
			{
				err = inv.TryAdd(item.Template, ref amount, slot);
			}
			else
			{
				// count will be set to the actual amount of items that found space in the inventory
				// if not all could be added, err contains the reason why
				err = inv.TryAdd(item.Template, ref amount);
			}

			if (err != InventoryError.OK)
			{
				ItemHandler.SendInventoryError(chr.Client, null, null, err);
			}

			if (amount <= 0)
			{
				// Nothing was purchased
				// Should usually never happen, but just to make sure
				ItemHandler.SendInventoryError(chr.Client, null, null, InventoryError.INVENTORY_FULL);
				return;
			}

			chr.Money -= (price * (uint)amount); // we already checked that our money is sufficient
			if (item.ExtendedCostEntry != null)
			{
				var exCost = item.ExtendedCostEntry;

				chr.HonorPoints -= exCost.HonorCost;
				chr.ArenaPoints -= exCost.ArenaPointCost;

				foreach (var reqItem in exCost.RequiredItems)
				{
					if (reqItem.Id == ItemId.None)
					{
						break;
					}

					if (!chr.Inventory.RemoveByItemId(reqItem.Id, reqItem.Cost, false))
					{
						NPCHandler.SendBuyError(chr, NPC, item.Template.ItemId, BuyItemError.NotEnoughMoney);
						return;
					}
				}
			}

			int remainingAmount;
			if (item.RemainingStockAmount != UnlimitedSupply)
			{
				// The vendor had a limited supply of this item, update the chr.Client with the new inventory
				remainingAmount = item.RemainingStockAmount - amount;
				//NPCHandler.SendVendorInventoryList(chr.Client, NPC.EntityId, ConstructVendorItemList(chr));
			}
			else
			{
				remainingAmount = UnlimitedSupply;
			}

			NPCHandler.SendBuyItem(chr.Client, NPC, item.Template.ItemId, amount, remainingAmount);
		}

		private bool CheckVendorInteraction(Character chr)
		{
			if (!NPC.CheckVendorInteraction(chr))
				return false;

			// Remove illegal Auras for vendor interaction
			chr.Auras.RemoveByFlag(AuraInterruptFlags.OnStartAttack);

			return true;
		}

		/// <summary>
		/// Checks whether the given Item may be sold by the given
		/// Character and sends an Error reply if not
		/// </summary>
		/// <param name="chr"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool CheckCanSellItem(Character chr, Item item, ref SellItemError error)
		{
			// Can't sell items that don't belong to you.
			if (chr != item.OwningCharacter)
			{
				error = SellItemError.PlayerDoesntOwnItem;
				return false;
			}

			var template = item.Template;

			// Can't sell non-empty bags
			if (template.IsBag && !((Container)item).BaseInventory.IsEmpty)
			{
				error = SellItemError.OnlyEmptyBag;
				return false;
			}

			// Item is in invalid state or simply not for sale
			if (!item.CanBeTraded || template.SellPrice == 0)
			{
				error = SellItemError.CantSellItem;
				return false;
			}

			return true;
		}

		private Item TryGetBuyBackItem(Character chr, int slot)
		{
			if (!CheckVendorInteraction(chr))
				return null;

			var curChar = chr.Client.ActiveCharacter;

			// Invalid slot number
			if (curChar.Inventory.BuyBack.IsValidSlot(slot))
			{
				NPCHandler.SendBuyError(chr.Client, NPC, 0, BuyItemError.CantFindItem);
				return null;
			}

			var item = curChar.Inventory.BuyBack[slot];
			// non-existent item
			if (item == null)
			{
				NPCHandler.SendBuyError(chr.Client, NPC, 0, BuyItemError.CantFindItem);
				return null;
			}

			// Not enough cash-money
			if ((item.Amount * item.Template.SellPrice) > curChar.Money)
			{
				NPCHandler.SendBuyError(chr.Client, NPC, (ItemId)item.EntryId, BuyItemError.NotEnoughMoney);
				return null;
			}

			return item;
		}

		private bool CheckBuyItem(Character chr, VendorItemEntry vendorItem, int count)
		{
			// Can the player use this item?
			//if (vendorItem.Template.CheckRequirements(curChar) != InventoryError.OK)
			//{
			//	NPCHandler.SendBuyError(chr.Client, NPC, vendorItem.Template.Id, BuyItemError.CantFindItem);
			//	return false;
			//}

			// Does the vendor have enough inventory for this trade?
			if (vendorItem.RemainingStockAmount != UnlimitedSupply && vendorItem.RemainingStockAmount < count)
			{
				NPCHandler.SendBuyError(chr, NPC, vendorItem.Template.ItemId, BuyItemError.ItemAlreadySold);
				return false;
			}

			// Is the player's faction standing high enough with this vendor?
			if (vendorItem.Template.RequiredFaction != null)
			{
				if (chr.Reputations.GetStandingLevel(vendorItem.Template.RequiredFaction.ReputationIndex) < vendorItem.Template.RequiredFactionStanding)
				{
					NPCHandler.SendBuyError(chr, NPC, vendorItem.Template.ItemId, BuyItemError.ReputationRequirementNotMet);
					return false;
				}
			}

			if (vendorItem.ExtendedCostEntry != null)
			{
				var exCost = vendorItem.ExtendedCostEntry;
				if (chr.HonorPoints < exCost.HonorCost)
				{
					NPCHandler.SendBuyError(chr, NPC, vendorItem.Template.ItemId, BuyItemError.NotEnoughMoney);
					return false;
				}

				if (chr.ArenaPoints < exCost.ArenaPointCost)
				{
					NPCHandler.SendBuyError(chr, NPC, vendorItem.Template.ItemId, BuyItemError.NotEnoughMoney);
					return false;
				}

				if (chr.MaxPersonalArenaRating < exCost.ReqArenaRating)
				{
					NPCHandler.SendBuyError(chr, NPC, vendorItem.Template.ItemId, BuyItemError.RankRequirementNotMet);
					return false;
				}

				foreach (var reqItem in exCost.RequiredItems)
				{
					if (reqItem.Id == ItemId.None) break;
					var amt = chr.Inventory.GetItemAmountByItemId(reqItem.Id);
					if (amt < reqItem.Cost)
					{
						NPCHandler.SendBuyError(chr, NPC, vendorItem.Template.ItemId, BuyItemError.NotEnoughMoney);
						return false;
					}
				}
			}
			return true;
		}

		public List<VendorItemEntry> ConstructVendorItemList(Character curChar)
		{
			//var tempList = new List<VendorItemEntry>(100);
			//foreach (var item in vendor.ItemsForSale)
			//{

			//    // Not needed: You can buy items if you cannot use them no?
			//    // Some items you might not be allowed to buy but you can still see them?

			//    //if (item == null || (item.Template.CheckRequirements(curChar) != InventoryError.OK))
			//    //    continue;

			//    tempList.Add(item);
			//}
			//return tempList.ToArray();
			return ItemsForSale;
		}

		//private Dictionary<uint, List<VendorItemEntry>> LoadVendorItemLists(string filename)
		//{
		//    var itemDict = LoadItemEntries("NPCs/items_amounts.xml");
		//    var listDict = new Dictionary<uint, List<VendorItemEntry>>(2500);
		//    var filePath = Path.Combine(RealmServer.Instance.Configuration.ContentDir, filename);
		//    var vendorReader = XmlReader.Create(filePath);

		//    while (vendorReader.Read())
		//    {
		//        if (!vendorReader.IsStartElement() ||
		//            vendorReader.Name.ToLower() != "row")
		//        {
		//            continue;
		//        }

		//        vendorReader.ReadToDescendant("entry");
		//        var temp = vendorReader.ReadString().Trim();
		//        var vendorId = uint.Parse(temp);

		//        vendorReader.ReadToNextSibling("item");
		//        temp = vendorReader.ReadString().Trim();
		//        var itemId = uint.Parse(temp);

		//        VendorItemEntry vendorItemEntry;
		//        if (!itemDict.TryGetValue(itemId, out vendorItemEntry))
		//        {
		//            Console.WriteLine("Vendor with Id: {0} has an invalid item entry: {1} in {2}", vendorId, itemId, filename);
		//            continue;
		//        }

		//        List<VendorItemEntry> itemList;
		//        if (listDict.TryGetValue(vendorId, out itemList))
		//        {
		//            itemList.Add(vendorItemEntry);
		//        }
		//        else
		//        {
		//            itemList = new List<VendorItemEntry>(256);
		//            itemList.Add(vendorItemEntry);
		//            listDict.Add(vendorId, itemList);
		//        }
		//    }
		//    return listDict;
		//}

		public override string ToString()
		{
			return Utility.GetStringRepresentation(ItemsForSale);
		}
	}
}