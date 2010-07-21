using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Items;
using WCell.Constants.Skills;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;
using WCell.Util;

namespace WCell.RealmServer.Items
{
	/// <summary>
	/// A part of another inventory (eg backpack, bank, equipment, buyback etc are all parts of the PlayerInventory)
	/// </summary>
	public abstract class PartialInventory : IInventory
	{
		protected PlayerInventory m_inventory;

		protected PartialInventory(PlayerInventory baseInventory)
		{
			m_inventory = baseInventory;
		}

		#region Properties
		/// <summary>
		/// The offset within the underlying inventory of what this part occupies.
		/// </summary>
		public abstract int Offset { get; }

		/// <summary>
		/// The end within the underlying inventory of what this part occupies
		/// </summary>
		public abstract int End { get; }

		/// <summary>
		/// The actual owner of this srcCont.
		/// </summary>
		public Character Owner
		{
			get
			{
				return m_inventory.Owner;
			}
		}

		public int IndexOf(Item item)
		{
			return item.Slot;
		}

		public void Insert(int index, Item item)
		{
			throw new InvalidOperationException();
		}

		public void RemoveAt(int index)
		{
			throw new InvalidOperationException();
		}

		Item IList<Item>.this[int index]
		{
			get { return this[index]; }
			set { this[index] = value; }
		}

		/// <summary>
		/// Gets the corresponding slot without further checks
		/// </summary>
		public Item this[int slot]
		{
			get{return m_inventory[slot];}
			internal set{m_inventory.Items[slot] = value;}
		}

		public Container GetBag(int slot)
		{
			return m_inventory[Offset + slot] as Container;
		}

		/// <summary>
		/// A copy of the items of this PartialInventory.
		/// </summary>
		public Item[] Items
		{
			get
			{
				var items = new Item[MaxCount];
				Array.Copy(m_inventory.Items, Offset, items, 0, items.Length);
				return items;
			}
		}

		public void Add(Item item)
		{
			throw new InvalidOperationException();
		}

		public void Clear()
		{
			throw new InvalidOperationException();
		}

		public bool Contains(Item item)
		{
			throw new InvalidOperationException();
		}

		public void CopyTo(Item[] array, int arrayIndex)
		{
			throw new InvalidOperationException();
		}

		public bool Remove(Item item)
		{
			throw new InvalidOperationException();
		}

		/// <summary>
		/// The amount of items, currently in this part of the inventory.
		/// Recounts everytime.
		/// </summary>
		public int Count
		{
			get
			{
				return m_inventory.GetCount(Offset, End);
			}
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		/// The maximum amount of items, supported by this inventory
		/// </summary>
		public int MaxCount
		{
			get
			{
				return End - Offset + 1;
			}
		}

		/// <summary>
		/// whether there are no items in this inventory
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				var items = m_inventory.Items;
				foreach (var item in items)
				{
					if (item != null)
					{
						return false;
					}
				}
				return true;
			}
		}

		/// <summary>
		/// whether there is no space left in this inventory
		/// </summary>
		public bool IsFull
		{
			get { return Count >= MaxCount; }
		}
		#endregion

		/// <summary>
		/// Returns the next free slot in this part of the inventory, else BaseInventory.INVALID_SLOT
		/// </summary>
		public virtual int FindFreeSlot()
		{
			var offset = Offset;
			for (var i = offset; i <= End; i++)
			{
				if (m_inventory.Items[i] == null)
					return i;
			}
			return BaseInventory.INVALID_SLOT;
		}

		/// <summary>
		/// Returns whether the given slot is valid to access items of this inventory
		/// </summary>
		public bool IsValidSlot(int slot)
		{
			return slot >= 0 && slot <= MaxCount;
		}

		/// <summary>
		/// Tries to add the given item to the corresponding slot: The offset of this inventory + the given slot
		/// </summary>
		/// <returns>whether the item could be added</returns>
		public virtual InventoryError TryAdd(int slot, Item item, bool isNew)
		{
			return m_inventory.TryAdd(slot, item, isNew);
		}

		/// <summary>
		/// Tries to add the item to a free slot in this srcCont
		/// </summary>
		/// <returns>whether the item could be added</returns>
		public virtual InventoryError TryAdd(Item item, bool isNew)
		{
			return m_inventory.TryAdd(FindFreeSlot(), item, isNew);
		}

		/// <summary>
		/// Removes the item at the given slot. If you intend to enable the user continuing to use that item, do not use this method
		/// but use PlayerInventory.TrySwap instead.
		/// </summary>
		/// <returns>whether there was an item to be removed and removal was successful</returns>
		public Item Remove(int slot, bool ownerChange)
		{
			return m_inventory.Remove(slot, ownerChange);
		}

		public Container RemoveBag(int slot, bool fullRemove)
		{
			return m_inventory.Remove(slot + Offset, fullRemove) as Container;
		}

		/// <summary>
		/// Deletes the item in the given slot (item cannot be re-used afterwards).
		/// </summary>
		/// <returns>whether the given item could be deleted</returns>
		public bool Destroy(int slot)
		{
			return m_inventory.Destroy(slot);
		}

		/// <summary>
		/// Returns all existing inventories (only containers have them)
		/// </summary>
		/// <returns></returns>
		public IEnumerable<BaseInventory> GetInventories()
		{
			foreach (var item in m_inventory.Items)
			{
				if (item is Container)
				{
					var inv = ((Container)item).BaseInventory;
					if (inv != null)
						yield return inv;
				}
			}
		}

		public bool Contains(ItemId id)
		{
			foreach (var item in m_inventory.Items)
			{
				if (item != null && item.EntryId == (uint)id)
				{
					return true;
				}
			}
			return false;
		}

		public IEnumerator<Item> GetEnumerator()
		{
			foreach (var item in m_inventory.Items)
			{
				if (item != null)
					yield return item;
			}
		}

		public override string ToString()
		{
			return GetType().Name + " of " + Owner;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}


	#region PartialInventory subclasses
	/// <summary>
	/// Represents the Equipment
	/// </summary>
	public class EquipmentInventory : PartialInventory, IItemSlotHandler
	{
		public EquipmentInventory(PlayerInventory baseInventory)
			: base(baseInventory)
		{
		}

		public override int Offset
		{
			get { return (int)InventorySlot.Head; }
		}

		public override int End
		{
			get { return (int)InventorySlot.Tabard; }
		}

		public Item this[EquipmentSlot slot]
		{
			get { return m_inventory.Items[(int)slot]; }
		}

		public Item Weapon
		{
			get { return m_inventory.Items[(int)EquipmentSlot.MainHand]; }
		}

		#region IItemSlotHandler
		/// <summary>
		/// Is called before adding to check whether the item may be added to the corresponding slot 
		/// (given the case that the corresponding slot is valid and unoccupied)
		/// </summary>
		public void CheckAdd(int slot, int amount, IMountableItem item, ref InventoryError err)
		{
			var templ = item.Template;
			err = templ.CheckEquip(m_inventory.Owner);
			if (err == InventoryError.OK)
			{
				if (templ.EquipmentSlots == null)
				{
					err = InventoryError.ITEM_CANT_BE_EQUIPPED;
				}
				else if (!templ.EquipmentSlots.Contains((EquipmentSlot)slot))
				{
					// client won't ever let you equip an item in a slot that it doesn't go to anyway
					err = InventoryError.ITEM_DOESNT_GO_TO_SLOT;
				}
				else if (slot == (int)InventorySlot.OffHand)
				{
					var mainHandWeapon = m_inventory[InventorySlot.MainHand];
					if (mainHandWeapon != null && mainHandWeapon.Template.IsTwoHandWeapon)
					{
						err = InventoryError.CANT_EQUIP_WITH_TWOHANDED;
					}
					else if (templ.IsWeapon && !m_inventory.Owner.Skills.Contains(SkillId.DualWield))
					{
						err = InventoryError.CANT_DUAL_WIELD;
					}
				}
				else if (templ.IsTwoHandWeapon && m_inventory[EquipmentSlot.OffHand] != null)
				//|| ((slot == (int)InventorySlot.OffHand && m_inventory[EquipmentSlot.MainHand] != null))
				{
					err = InventoryError.CANT_EQUIP_WITH_TWOHANDED;
				}
				else if (!item.IsEquipped)
				{
					err = m_inventory.CheckEquipCount(item);
				}
			}
		}

		/// <summary>
		/// Is called before removing the given item to check whether it may actually be removed
		/// </summary>
		public void CheckRemove(int slot, IMountableItem templ, ref InventoryError err)
		{
			// Disarmed
			if (templ is IWeapon && templ.Template.IsWeapon && !Owner.MayCarry(templ.Template.InventorySlotMask))
			{
				err = InventoryError.CANT_DO_WHILE_DISARMED;
			}
		}

		/// <summary>
		/// Is called after the given item is added to the given slot
		/// </summary>
		public void Added(Item item)
		{
			item.OnEquipDecision();
		}

		/// <summary>
		/// Is called after the given item is removed from the given slot
		/// </summary>
		public void Removed(int slot, Item item)
		{
			item.OnUnequipDecision((InventorySlot)slot);
		}
		#endregion
	}

	/// <summary>
	/// Represents all items within the BackPack
	/// </summary>
	public class BackPackInventory : PartialInventory, IItemSlotHandler
	{
		public BackPackInventory(PlayerInventory baseInventory)
			: base(baseInventory)
		{
		}

		public override int Offset
		{
			get { return (int)InventorySlot.BackPack1; }
		}

		public override int End
		{
			get { return (int)InventorySlot.BackPackLast; }
		}

		/// <returns>whether the given InventorySlot is a backpack slot</returns>
		public static bool IsBackpackSlot(InventorySlot slot)
		{
			return slot >= InventorySlot.BackPack1 && slot <= InventorySlot.BackPackLast;
		}

		#region IItemSlotHandler
		/// <summary>
		/// Is called before adding to check whether the item may be added to the corresponding slot 
		/// (given the case that the corresponding slot is valid and unoccupied)
		/// </summary>
		public void CheckAdd(int slot, int amount, IMountableItem item, ref InventoryError err)
		{
			if (item.Template.IsKey)
			{
				err = InventoryError.ITEM_DOESNT_GO_INTO_BAG;
			}
		}

		/// <summary>
		/// Is called before removing the given item to check whether it may actually be removed
		/// </summary>
		public void CheckRemove(int slot, IMountableItem item, ref InventoryError err)
		{
		}

		/// <summary>
		/// Is called after the given item is added to the given slot
		/// </summary>
		public void Added(Item item)
		{
		}

		/// <summary>
		/// Is called after the given item is removed from the given slot
		/// </summary>
		public void Removed(int slot, Item item)
		{
		}
		#endregion
	}

	/// <summary>
	/// Represents all equippable bags
	/// </summary>
	public class EquippedContainerInventory : PartialInventory, IItemSlotHandler
	{
		public EquippedContainerInventory(PlayerInventory baseInventory)
			: base(baseInventory)
		{
		}

		public override int Offset
		{
			get { return (int)InventorySlot.Bag1; }
		}

		public override int End
		{
			get { return (int)InventorySlot.BagLast; }
		}

		#region IItemSlotHandler
		/// <summary>
		/// Is called before adding to check whether the item may be added to the corresponding slot 
		/// (given the case that the corresponding slot is valid and unoccupied)
		/// </summary>
		public virtual void CheckAdd(int slot, int amount, IMountableItem item, ref InventoryError err)
		{
			var templ = item.Template;
			err = templ.CheckEquip(Owner);
			if (err == InventoryError.OK)
			{
				if (!templ.IsBag)
				{
					err = InventoryError.NOT_A_BAG;
				}
				else
				{
					var oldBag = m_inventory[slot];
					if (oldBag != null)
					{
						err = InventoryError.NONEMPTY_BAG_OVER_OTHER_BAG;
					}
					else if (!item.IsEquipped)
					{
						err = m_inventory.CheckEquipCount(item);
					}
				}
			}
		}

		/// <summary>
		/// Is called before a bag is removed
		/// </summary>
		public void CheckRemove(int slot, IMountableItem item, ref InventoryError err)
		{
			var cont = item as Container;
			if (cont != null)
			{
				if (!cont.BaseInventory.IsEmpty)
				{
					err = InventoryError.CAN_ONLY_DO_WITH_EMPTY_BAGS;
				}
			}
		}

		/// <summary>
		/// Is called after the given item is added to the given slot
		/// </summary>
		public void Added(Item item)
		{
			item.OnEquipDecision();
		}

		/// <summary>
		/// Is called after the given item is removed from the given slot
		/// </summary>
		public void Removed(int slot, Item item)
		{
			item.OnUnequipDecision((InventorySlot)slot);
		}
		#endregion
	}

	/// <summary>
	/// Represents all items within the bank (excluding bags in srcCont slots)
	/// </summary>
	public class BankInventory : PartialInventory, IItemSlotHandler
	{
		public BankInventory(PlayerInventory baseInventory)
			: base(baseInventory)
		{
		}

		public override int Offset
		{
			get { return (int)InventorySlot.Bank1; }
		}

		public override int End
		{
			get { return (int)InventorySlot.BankLast; }
		}

		#region IItemSlotHandler
		/// <summary>
		/// Is called before adding to check whether the item may be added to the corresponding slot 
		/// (given the case that the corresponding slot is valid and unoccupied)
		/// </summary>
		public void CheckAdd(int slot, int amount, IMountableItem item, ref InventoryError err)
		{
			if (!m_inventory.IsBankOpen)
			{
				err = InventoryError.TOO_FAR_AWAY_FROM_BANK;
			}
		}

		/// <summary>
		/// Is called before removing the given item to check whether it may actually be removed
		/// </summary>
		public void CheckRemove(int slot, IMountableItem item, ref InventoryError err)
		{
		}

		/// <summary>
		/// Is called after the given item is added to the given slot
		/// </summary>
		public void Added(Item item)
		{
		}

		/// <summary>
		/// Is called after the given item is removed from the given slot
		/// </summary>
		public void Removed(int slot, Item item)
		{
		}
		#endregion
	}

	/// <summary>
	/// Represents all bank bags
	/// </summary>
	public class BankBagInventory : EquippedContainerInventory
	{
		public BankBagInventory(PlayerInventory baseInventory)
			: base(baseInventory)
		{
		}

		public override int Offset
		{
			get { return (int)InventorySlot.BankBag1; }
		}

		public override int End
		{
			get { return (int)InventorySlot.BankBagLast; }
		}

		/// <summary>
		/// Tries to increase the amount of slots for bank bags by one.
		/// </summary>
		/// <param name="takeMoney">whether to also check (and subtract) the amount of money for the slot</param>
		/// <returns>whether there was still space left to add (and -if takeMoney is true- also checks whether the funds were sufficient)</returns>
		public BuyBankBagResponse IncBankBagSlotCount(bool takeMoney)
		{
			var chr = m_inventory.Owner;

			var count = chr.BankBagSlots;

			if (count == MaxCount)
			{
				return BuyBankBagResponse.LimitReached;
			}

			if (takeMoney)
			{
				var costs = NPCMgr.BankBagSlotPrices.Get((uint)count);
				if (costs == 0 || chr.Money < costs)
				{
					return BuyBankBagResponse.CantAfford;
				}
				chr.Money -= costs;
			}

			chr.BankBagSlots = (byte)(count + 1);
			return BuyBankBagResponse.Ok;
		}

		/// <summary>
		/// Tries to reduce the amount of slots for bank bags by one.
		/// Returns the cont, or null if the amount is already at 0 or if the last cont still contains items.
		/// </summary>
		/// <returns></returns>
		public Container DecBankBagSlotCount()
		{
			var chr = m_inventory.Owner;
			if (chr == null)
				return null;

			byte count = chr.BankBagSlots;

			if (count == 0)
			{
				return null;
			}

			// get the last cont
			var bag = m_inventory.Items[(int)(InventorySlot.BankBag1 + count)] as Container;
			if (bag == null)
				// shouldn't happen
				return null;

			if (!bag.BaseInventory.IsEmpty)
				return null;

			chr.BankBagSlots = (byte)(count - 1);
			return bag;
		}

		#region IItemSlotHandler
		public override void CheckAdd(int slot, int amount, IMountableItem item, ref InventoryError err)
		{
			if (!m_inventory.IsBankOpen)
			{
				err = InventoryError.TOO_FAR_AWAY_FROM_BANK;
			}
			else if (slot < m_inventory.Owner.BankBagSlots)
			{
				err = InventoryError.MUST_PURCHASE_THAT_BAG_SLOT;
			}
		}
		#endregion
	}

	/// <summary>
	/// Represents all BuyBack items (that can be re-purchased at a vendor)
	/// </summary>
	public class BuyBackInventory : PartialInventory, IItemSlotHandler
	{
		public BuyBackInventory(PlayerInventory baseInventory)
			: base(baseInventory)
		{
		}

		public override int Offset
		{
			get { return (int)InventorySlot.BuyBack1; }
		}

		public override int End
		{
			get { return (int)InventorySlot.BuyBackLast; }
		}

		public override InventoryError TryAdd(int slot, Item item, bool isNew)
		{
			return AddBuyBackItem(slot, item, isNew);
		}

		public override InventoryError TryAdd(Item item, bool isNew)
		{
			return AddBuyBackItem(item, isNew);
		}

		/// <summary>
		/// Adds an Item to the BuyBack inventory. If the BuyBack is full, the item in BuyBackSlot1 is destroyed
		/// and the items in the other slots are shifted up one to free-up the BuyBackLast slot.
		/// This method should only be called by the CMSG_SELL_ITEM handler.
		/// </summary>
		/// <param name="item">The item to Add to the BuyBack PartialInventory</param>
		public InventoryError AddBuyBackItem(Item item, bool isNew)
		{
			var slot = FindFreeSlot();
			if (slot == BaseInventory.INVALID_SLOT)
			{
				slot = PushBack();
			}

			return AddBuyBackItem(slot, item, isNew);
		}

		public InventoryError AddBuyBackItem(int slot, Item item, bool isNew)
		{
			if (!isNew)
			{
				item.Remove(true);
			}

			m_inventory.AddUnchecked(slot, item, true);
			return InventoryError.OK;
		}

		/// <summary>
		/// The BuyBack inventory is full. In order to make room for further items, push the top item off the list
		/// and then move every other item up one slot, thereby freeing the last slot.
		/// </summary>
		/// <returns></returns>
		private int PushBack()
		{
			Destroy(Offset);
			return End;
		}

		#region BuyBack Item Fields
		private uint GetBuyBackPriceField(int buyBackSlot)
		{
			var offset = (buyBackSlot - (int)InventorySlot.BuyBack1);
			return m_inventory.Owner.GetUInt32(PlayerFields.BUYBACK_PRICE_1 + offset);
		}

		private void SetBuyBackPriceField(int buyBackSlot, uint price)
		{
			var offset = (buyBackSlot - (int)InventorySlot.BuyBack1);
			m_inventory.Owner.SetUInt32(PlayerFields.BUYBACK_PRICE_1 + offset, price);
		}

		private DateTime GetBuyBackTimeStampField(int buyBackSlot)
		{
			var offset = (buyBackSlot - (int)InventorySlot.BuyBack1);
			return m_inventory.Owner.LastLogin.AddSeconds(m_inventory.Owner.GetUInt32(PlayerFields.BUYBACK_TIMESTAMP_1 + offset));
		}

		private void SetBuyBackTimeStampField(int buyBackSlot)
		{
			var baseTime = (uint)((DateTime.Now - m_inventory.Owner.LastLogin).Seconds + (30 * 3600));
			SetBuyBackTimeStampField(buyBackSlot, baseTime);
		}

		private void SetBuyBackTimeStampField(int buyBackSlot, DateTime stamp)
		{
			var baseTime = (uint)((stamp - m_inventory.Owner.LastLogin).Seconds);
			SetBuyBackTimeStampField(buyBackSlot, baseTime);
		}

		private void SetBuyBackTimeStampField(int buyBackSlot, uint secondsFromLogin)
		{
			var offset = (buyBackSlot - (int)InventorySlot.BuyBack1);
			m_inventory.Owner.SetUInt32(PlayerFields.BUYBACK_TIMESTAMP_1 + offset, secondsFromLogin);
		}

		void RemoveItemFromBuyBackField(int buyBackSlot)
		{
			SetBuyBackPriceField(buyBackSlot, 0u);
			SetBuyBackTimeStampField(buyBackSlot, 0u);
		}

		public void MoveItem(int from, int to)
		{
			var price = GetBuyBackPriceField(from);
			var timeStamp = GetBuyBackTimeStampField(from);

			RemoveItemFromBuyBackField(from);

			SetBuyBackPriceField(to, price);
			SetBuyBackTimeStampField(to, timeStamp);
		}
		#endregion

		#region IItemSlotHandler
		/// <summary>
		/// Is called before adding to check whether the item may be added to the corresponding slot 
		/// (given the case that the corresponding slot is valid and unoccupied)
		/// </summary>
		public void CheckAdd(int slot, int amount, IMountableItem item, ref InventoryError err)
		{
		}

		/// <summary>
		/// Is called before removing the given item to check whether it may actually be removed
		/// </summary>
		public void CheckRemove(int slot, IMountableItem item, ref InventoryError err)
		{
		}

		/// <summary>
		/// Is called after the given item is added to the given slot
		/// </summary>
		public void Added(Item item)
		{
			var slot = item.Slot;
			if (slot < Offset || slot > End)
			{
				return;
			}

			//Handled by m_inventory[]
			//SetBuyBackItemField(buyBackSlot, item.EntityId);
			SetBuyBackPriceField(slot, (item.Template.SellPrice * (uint)item.Amount));
			SetBuyBackTimeStampField(slot);
		}

		/// <summary>
		/// Is called after the given item is removed from the given slot
		/// </summary>
		public void Removed(int slot, Item item)
		{
			RemoveItemFromBuyBackField(slot);

			// Shift the rest of the items up one slot.
			for (var i = slot; i < End; ++i)
			{
				if (m_inventory[i + 1] == null)
					continue;

				MoveItem((i + 1), i);
				m_inventory.SwapUnnotified(i + 1, i);
			}
		}
		#endregion
	}

	/// <summary>
	/// Represents the keyring
	/// </summary>
	public class KeyRingInventory : PartialInventory, IItemSlotHandler
	{
		public KeyRingInventory(PlayerInventory baseInventory)
			: base(baseInventory)
		{
		}

		public override int Offset
		{
			get { return (int)InventorySlot.Key1; }
		}

		public override int End
		{
			get { return (int)InventorySlot.KeyLast; }
		}


		#region IItemSlotHandler
		/// <summary>
		/// Is called before adding to check whether the item may be added to the corresponding slot 
		/// (given the case that the corresponding slot is valid and unoccupied)
		/// </summary>
		public void CheckAdd(int slot, int amount, IMountableItem item, ref InventoryError err)
		{
			if (item.Template.Class != ItemClass.Key)
			{
				err = InventoryError.ITEM_DOESNT_GO_TO_SLOT;
			}
		}

		/// <summary>
		/// Is called before removing the given item to check whether it may actually be removed
		/// </summary>
		public void CheckRemove(int slot, IMountableItem item, ref InventoryError err)
		{
		}

		/// <summary>
		/// Is called after the given item is added to the given slot
		/// </summary>
		public void Added(Item item)
		{
		}

		/// <summary>
		/// Is called after the given item is removed from the given slot
		/// </summary>
		public void Removed(int slot, Item item)
		{
		}
		#endregion
	}
	#endregion
}