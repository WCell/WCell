/*************************************************************************
 *
 *   file		: Owner.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-30 10:02:00 +0100 (lø, 30 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1234 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using WCell.Constants.Items;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.Util;
using Castle.ActiveRecord;
using WCell.Util.NLog;

namespace WCell.RealmServer.Items
{
	// TODO: Don't allow to use/delete/move Items under certain circumstances (stunned, disarmed etc)
	public class PlayerInventory : BaseInventory
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Durability loss on all Items upon Death in percent
		/// </summary>
		public static int DeathDurabilityLossPct = 10;

		/// <summary>
		/// Durability loss of *all* Items when reviving at the SpiritHealer
		/// </summary>
		public static int SHResDurabilityLossPct = 25;

		public const int MaxBankBagCount = (InventorySlot.BankBagLast - InventorySlot.BankBag1);

		internal Character m_owner;
		internal Item m_ammo;
		internal WorldObject m_currentBanker;
		private int m_totalCount;
		public readonly Dictionary<ItemId, int> UniqueCounts = new Dictionary<ItemId, int>(3);

		/// <summary>
		/// Every PartialInventory is also ensured to be IISlotHandler
		/// </summary>
		protected internal PartialInventory[] m_partialInventories;

		public static bool AutoEquipNewItems;


		/// <summary>
		/// 
		/// </summary>
		public PlayerInventory(Character character)
			: base(character, PlayerFields.INV_SLOT_HEAD, (int)InventorySlot.Count)
		{
			m_owner = character;

			m_partialInventories = new PartialInventory[(int)PartialInventoryType.Count];
			m_partialInventories[(int)PartialInventoryType.Equipment] = new EquipmentInventory(this);
			m_partialInventories[(int)PartialInventoryType.BackPack] = new BackPackInventory(this);
			m_partialInventories[(int)PartialInventoryType.EquippedContainers] = new EquippedContainerInventory(this);
			m_partialInventories[(int)PartialInventoryType.Bank] = new BankInventory(this);
			m_partialInventories[(int)PartialInventoryType.BankBags] = new BankBagInventory(this);
			m_partialInventories[(int)PartialInventoryType.BuyBack] = new BuyBackInventory(this);
			m_partialInventories[(int)PartialInventoryType.KeyRing] = new KeyRingInventory(this);
		}

		#region Properties

		/// <summary>
		/// The amount of items, currently in this inventory.
		/// </summary>
		public int TotalCount
		{
			get { return m_totalCount; }
		}

		public override PlayerInventory OwnerInventory
		{
			get { return this; }
		}

		//Item GetItemOrAmmo(EquipmentSlot slot)
		//{
		//    if (slot == EquipmentSlot.Invalid) return Ammo;
		//    return this[slot];
		//}

		/// <summary>
		/// Gets the item at the given InventorySlot (or null)
		/// </summary>
		public Item this[InventorySlot slot]
		{
			get
			{
				return m_Items[(int)slot];
			}
			set
			{
				this[(int)slot] = value;
			}
		}
		/// <summary>
		/// Gets the item at the given InventorySlot (or null)
		/// </summary>
		public Item this[EquipmentSlot slot]
		{
			get
			{
				return m_Items[(int)slot];
			}
			set
			{
				this[(int)slot] = value;
			}
		}

		/// <summary>
		/// whether the client currently is allowed to/tries to access his/her bank (through a banker usually)
		/// </summary>
		public bool IsBankOpen
		{
			get
			{
				if (m_currentBanker != null && !m_owner.IsInRadius(m_currentBanker, 5f))
				{
					m_currentBanker = null;
				}
				return m_currentBanker != null;
			}
		}

		/// <summary>
		/// The banker at which we currently opened the BankBox (if any)
		/// </summary>
		public WorldObject CurrentBanker
		{
			get
			{
				return m_currentBanker;
			}
			set
			{
				m_currentBanker = value;
			}
		}

		/// <summary>
		/// The currently used Ammo
		/// </summary>
		public Item Ammo
		{
			get
			{
				return m_ammo;
			}
			set
			{
				if (value != m_ammo)
				{
					if (m_ammo != null)
					{
						m_ammo.OnUnEquip(InventorySlot.Invalid);
						m_owner.SetUInt32(PlayerFields.AMMO_ID, 0);
					}

					if (value != null)
					{
						m_owner.SetUInt32(PlayerFields.AMMO_ID, value.Template.Id);
						value.Slot = (int)InventorySlot.Invalid;
						value.OnEquip();
					}
					m_ammo = value;
				}
			}
		}

		/// <summary>
		/// The id of the currently used Ammo
		/// </summary>
		public uint AmmoId
		{
			get
			{
				return m_owner.GetUInt32(PlayerFields.AMMO_ID);
			}
		}

		/// <summary>
		/// All equipped items
		/// </summary>
		public EquipmentInventory Equipment
		{
			get
			{
				return m_partialInventories[(int)PartialInventoryType.Equipment] as EquipmentInventory;
			}
		}

		/// <summary>
		/// The 4 equippable srcCont slots, next to the BackPack
		/// </summary>
		public EquippedContainerInventory EquippedContainers
		{
			get
			{
				return m_partialInventories[(int)PartialInventoryType.EquippedContainers] as EquippedContainerInventory;
			}
		}

		/// <summary>
		/// The contents of the backpack
		/// </summary>
		public BackPackInventory BackPack
		{
			get
			{
				if (m_partialInventories[(int)PartialInventoryType.BackPack] == null)
				{
					m_partialInventories[(int)PartialInventoryType.BackPack] = new BackPackInventory(this);
				}
				return m_partialInventories[(int)PartialInventoryType.BackPack] as BackPackInventory;
			}
		}

		/// <summary>
		/// The contents of the bankbox (excluding bags)
		/// </summary>
		public BankInventory Bank
		{
			get
			{
				return m_partialInventories[(int)PartialInventoryType.Bank] as BankInventory;
			}
		}

		/// <summary>
		/// All available containers in the bank
		/// </summary>
		public BankBagInventory BankBags
		{
			get
			{
				return m_partialInventories[(int)PartialInventoryType.BankBags] as BankBagInventory;
			}
		}

		/// <summary>
		/// Items that have been sold and can be re-purchased by the player
		/// </summary>
		public BuyBackInventory BuyBack
		{
			get
			{
				return m_partialInventories[(int)PartialInventoryType.BuyBack] as BuyBackInventory;
			}
		}

		/// <summary>
		/// The keyring
		/// </summary>
		public KeyRingInventory KeyRing
		{
			get
			{
				return m_partialInventories[(int)PartialInventoryType.KeyRing] as KeyRingInventory;
			}
		}

		public override InventoryError FullError
		{
			get { return InventoryError.INVENTORY_FULL; }
		}
		#endregion

		#region Unique Count
		internal void ModUniqueCount(ItemTemplate templ, int delta)
		{
			if (templ.UniqueCount > 0)
			{
				int count;
				UniqueCounts.TryGetValue(templ.ItemId, out count);
				UniqueCounts[templ.ItemId] = count + delta;
			}
		}

		internal void AddItemUniqueCount(Item item)
		{
			if (!item.IsBuyback)
			{
				ModUniqueCount(item.Template, (int)item.Amount);

				// unique gems
				if (item.Enchantments != null && item.Template.HasSockets)
				{
					for (var i = 0; i < ItemConstants.MaxSocketCount; i++)
					{
						var enchant = item.Enchantments[i + (int)EnchantSlot.Socket1];
						if (enchant != null)
						{
							ModUniqueCount(enchant.Entry.GemTemplate, 1);
						}
					}
				}
			}
		}

		internal void RemoveItemUniqueCount(Item item)
		{
			if (item.IsBuyback) return;
			ModUniqueCount(item.Template, -item.Amount);

			// unique gems
			if (item.Enchantments != null && item.Template.HasSockets)
			{
				for (var i = 0; i < ItemConstants.MaxSocketCount; i++)
				{
					var enchant = item.Enchantments[i + (int)EnchantSlot.Socket1];
					if (enchant != null)
					{
						ModUniqueCount(enchant.Entry.GemTemplate, -1);
					}
				}
			}
		}

		public int GetUniqueCount(ItemId id)
		{
			int count;
			UniqueCounts.TryGetValue(id, out count);
			return count;
		}
		#endregion

		IItemSlotHandler GetHandlerForItemOrAmmo(int slot)
		{
			return slot == -1 ? Equipment : GetHandler(slot);
		}

		/// <summary>
		/// Returns the IItemSlotHandler for the specified InventorySlot
		/// </summary>
		public override IItemSlotHandler GetHandler(int slot)
		{
			if (!IsValidSlot(slot))
				return null;

			return m_partialInventories[(int)ItemMgr.PartialInventoryTypes[slot]] as IItemSlotHandler;
		}

		#region Containers
		/// <summary>
		/// Returns the inventory of the corresponding cont (or null).
		/// Only works for bags in the equipment's or bank's cont slots (only these bags may contain items).
		/// </summary>
		public BaseInventory GetContainer(InventorySlot slot, bool inclBank)
		{
			if (slot == InventorySlot.Invalid || (int)slot >= ItemMgr.ContainerSlotsWithBank.Length)
			{
				return this;
			}

			if ((inclBank && ItemMgr.ContainerSlotsWithBank[(int)slot]) || (!inclBank && ItemMgr.ContainerSlotsWithoutBank[(int)slot]))
			{
				var bag = this[slot] as Container;
				if (bag != null)
				{
					return bag.BaseInventory;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns the Inventory of the Container with the given id or this Character.
		/// </summary>
		/// <returns>Never null.</returns>
		public BaseInventory GetContainer(EntityId containerId, bool inclBank)
		{
			if (containerId == EntityId.Zero || containerId == m_owner.EntityId)
			{
				return this;
			}

			var conts = EquippedContainers;
			for (var i = 0; i < conts.Count; i++)
			{
				var bag = conts[i];
				if (bag.EntityId == containerId)
				{
					return ((Container)bag).BaseInventory;
				}
			}

			if (inclBank)
			{
				for (var i = 0; i < BankBags.Count; i++)
				{
					var bag = BankBags[i];
					if (bag.EntityId == containerId)
					{
						return ((Container)bag).BaseInventory;
					}
				}
			}

			return this;
		}

		/// <summary>
		/// Returns the inventory of the corresponding bank-container (or null)
		/// </summary>
		public BaseInventory GetBankContainer(InventorySlot slot)
		{
			if (slot == InventorySlot.Invalid || (int)slot >= ItemMgr.ContainerSlotsWithBank.Length)
			{
				return this;
			}

			if (ItemMgr.ContainerBankSlots[(int)slot])
			{
				var bag = this[slot] as Container;
				if (bag != null)
				{
					return bag.BaseInventory;
				}
			}
			return null;
		}
		#endregion

		/// <summary>
		/// Checks some basic parameters for whether the character may interact with (Equip or Use) items and
		/// sends an error if the character cannot interact. Also cancels the current spellcast (if any)
		/// </summary>
		public InventoryError CheckInteract()
		{
			if (m_owner.GodMode)
			{
				return InventoryError.OK;
			}

			InventoryError err;
			var chr = Owner;
			if (!chr.IsAlive)
			{
				err = InventoryError.YOU_ARE_DEAD;
			}
			else if (chr.IsUnderInfluenceOf(SpellMechanic.Disarmed))
			{
				err = InventoryError.CANT_DO_WHILE_DISARMED;
			}
			else if (!chr.CanInteract)
			{
				err = InventoryError.CANT_DO_RIGHT_NOW;
			}
			//else if (chr.IsMechanic(SpellMechanic.Disarmed))
			//{
			//	err = InventoryError.CANT_DO_IN_COMBAT;
			//}
			else
			{
				// moving items cancels spellcasting?
				// m_owner.SpellCast.Cancel();

				return InventoryError.OK;
			}

			ItemHandler.SendInventoryError(chr.Client, null, null, err);
			return err;
		}

		#region Adding
		/// <summary>
		/// Tries to add a new item with the given template and amount ot the given slot.
		/// Make sure the given targetSlot is valid before calling this method.
		/// If slot is occupied, method will find another unoccupied slot.
		/// </summary>
		/// <returns>The result (InventoryError.OK in case that it worked)</returns>
		public InventoryError TryAdd(ItemId id, ref int amount, InventorySlot targetSlot)
		{
			var templ = ItemMgr.GetTemplate(id);
			if (templ != null)
			{
				return TryAdd(templ, ref amount, (int)targetSlot, true);
			}
			return InventoryError.Invalid;
		}

		/// <summary>
		/// Tries to add ONE new item with the given template to the given slot.
		/// Make sure the given targetSlot is valid before calling this method.
		/// </summary>
		public InventoryError TryAdd(ItemTemplate template, InventorySlot targetSlot)
		{
			var amount = 1;
			return TryAdd(template, ref amount, (int)targetSlot, true);
		}

		/// <summary>
		/// Tries to add a single new item with the given template to the given slot.
		/// Make sure the given targetSlot is valid before calling this method.
		/// </summary>
		public InventoryError TryAdd(ItemTemplate template, EquipmentSlot targetSlot)
		{
			var amount = 1;
			return TryAdd(template, ref amount, (int)targetSlot, true);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>The result (InventoryError.OK in case that it worked)</returns>
		public InventoryError Ensure(ItemId templId, int amount, bool equip)
		{
			var templ = ItemMgr.GetTemplate(templId);
			if (templ != null)
			{
				return Ensure(templ, amount);
			}
			return InventoryError.ITEM_NOT_FOUND;
		}

		public InventoryError Ensure(ItemTemplate templ, int amount)
		{
			return Ensure(templ, amount, AutoEquipNewItems);
		}

		public InventoryError Ensure(ItemTemplate templ, int amount, bool equip)
		{
			if (templ.EquipmentSlots == null)
			{
				if (equip)
				{
					return InventoryError.ITEM_CANT_BE_EQUIPPED;
				}
			}
			else
			{
				for (var i = 0; i < templ.EquipmentSlots.Length; i++)
				{
					var slot = templ.EquipmentSlots[i];
					var item = this[slot];
					if (item != null && item.Template.Id == templ.Id)
					{
						// done
						return InventoryError.OK;
					}
				}
			}

			var found = 0;
			if (Iterate(item =>
			{
				if (item.Template == templ)
				{
					found += item.Amount;
					if (equip && !item.IsEquipped)
					{
						TryEquip(this, item.Slot);
						return false;
					}
					else if (found >= amount)
					{
						return false;
					}
				}
				return true;
			}))
			{
				// didn't add everything yet
				amount -= found;
				if (!equip)
				{
					return TryAdd(templ, ref amount);
				}

				var slot = GetEquipSlot(templ, true);
				if (slot == InventorySlot.Invalid)
				{
					return InventoryError.INVENTORY_FULL;
				}
				return TryAdd(templ, slot);
			}
			return InventoryError.OK;
		}


		/// <summary>
		/// Called whenever the Player receives a new Item
		/// </summary>
		internal void OnAdd(Item item)
		{
			item.m_owner = m_owner;
			OnAddDontNotify(item);
			// notify about new item

		}

		internal void OnAddDontNotify(Item item)
		{
			if (!item.IsBuyback)
			{
				m_totalCount++;
				//OnAmountChanged(item, (int)item.Amount);
				AddItemUniqueCount(item);
				m_owner.QuestLog.OnItemAmountChanged(item, item.Amount);
				item.OnAdd();
			}

			var context = m_owner.ContextHandler;
			if (context != null)
			{
				context.AddMessage(() =>
				{
					if (m_owner != null)
					{
						m_owner.AddItemToUpdate(item);
					}
				});
			}
		}

		/// <summary>
		/// Called when the given Item is removed for good.
		/// Don't use this method - but use item.Remove instead.
		/// </summary>
		internal void OnRemove(Item item)
		{
			if (item.IsBuyback) return;
			m_totalCount--;
			if (item == m_ammo)
			{
				// look for more ammo if the old stack is done:
				SetAmmo(m_ammo.Template.Id);
			}

			m_owner.RemoveOwnedItem(item);
			m_owner.QuestLog.OnItemAmountChanged(item, -item.Amount);
			RemoveItemUniqueCount(item);
		}

		/// <summary>
		/// Called when the given Item's amount changes by the given difference
		/// </summary>
		public void OnAmountChanged(Item item, int diff)
		{
			if (!item.IsBuyback)
			{
				m_owner.QuestLog.OnItemAmountChanged(item, diff);
				ModUniqueCount(item.Template, diff);
			}
		}
		#endregion

		#region Searching

		/// <summary>
		/// Gets a free slot in the backpack (use FindFreeSlot(IMountableItem, uint) to also look through equipped bags and optionally the bank)
		/// </summary>
		public override int FindFreeSlot(int offset, int end)
		{
			var slot = BackPack.FindFreeSlot();
			return slot;
		}

		/// <summary>
		/// Finds a free slot after checking for uniqueness
		/// </summary>
		/// <param name="templ"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public SimpleSlotId FindFreeSlotCheck(ItemTemplate templ, int amount, out InventoryError err)
		{
			err = InventoryError.OK;
			var possibleAmount = amount;
			CheckUniqueness(templ, ref possibleAmount, ref err, true);
			if (possibleAmount != amount)
			{
				return SimpleSlotId.Default;
			}

			var slot = FindFreeSlot(templ, amount);
			if (slot.Slot == INVALID_SLOT)
			{
				err = InventoryError.INVENTORY_FULL;
			}
			return slot;
		}

		/// <summary>
		/// Gets a free slot in a preferred equipped bag (eg Herb bag for Herbs) or backpack
		/// </summary>
		public override SimpleSlotId FindFreeSlot(IMountableItem mountItem, int amount)
		{
			return FindFreeSlot(mountItem, amount, AutoEquipNewItems);
		}

		/// <summary>
		/// Gets a free slot in a preferred equipped bag (eg Herb bag for Herbs) or backpack.
		/// Looks for a suitable equipment slot first, if tryEquip is true
		/// </summary>
		public SimpleSlotId FindFreeSlot(IMountableItem mountItem, int amount, bool tryEquip)
		{
			var templ = mountItem.Template;

			if (tryEquip && templ.EquipmentSlots != null)
			{
				// try to equip
				for (var i = 0; i < templ.EquipmentSlots.Length; i++)
				{
					var slot = (int)templ.EquipmentSlots[i];
					var item = this[slot];
					if (item == null)
					{
						var handler = GetHandlerForItemOrAmmo(slot);
						var err = InventoryError.OK;
						handler.CheckAdd(slot, amount, templ, ref err);
						if (err == InventoryError.OK)
						{
							return new SimpleSlotId { Container = this, Slot = slot };
						}
						break;
					}
				}
			}

			var slotId = SimpleSlotId.Default;

			// check for preferred bag (ore goes to mining sacks, keys go to keychain etc)
			GetPreferredSlot(templ, amount, ref slotId);
			if (slotId.Slot == INVALID_SLOT)
			{
				// add it to any free spot if there isn't any preferred bag
				var slots = ItemMgr.StorageSlotsWithoutBank;
				var contLookup = ItemMgr.ContainerSlotsWithBank;

				for (var j = 0; j < slots.Length; j++)
				{
					var avlblSlot = (int)slots[j];
					if (contLookup[avlblSlot])
					{
						var container = m_Items[avlblSlot] as Container;
						if (container != null && container.Template.MayAddToContainer(templ))
						{
							var contInv = container.BaseInventory;
							var contItems = contInv.Items;
							for (var i = 0; i < contItems.Length; i++)
							{
								if (contItems[i] == null)
								{
									slotId.Container = contInv;
									slotId.Slot = i;
									return slotId;
								}
							}
						}
					}
					else if (m_Items[avlblSlot] == null)
					{
						slotId.Container = this;
						slotId.Slot = avlblSlot;
						return slotId;
					}
				}
				slotId.Slot = INVALID_SLOT;
			}
			return slotId;
		}

		/// <summary>
		/// Sets slotId to the slot that the given templ would prefer (if it has any bag preference).
		/// </summary>
		/// <param name="templ"></param>
		/// <param name="slotId"></param>
		public void GetPreferredSlot(ItemTemplate templ, int amount, ref SimpleSlotId slotId)
		{
			if (templ.IsKey)
			{
				slotId.Container = this;
				slotId.Slot = KeyRing.FindFreeSlot();
			}
			else if (!templ.IsContainer && templ.BagFamily != ItemBagFamilyMask.None)
			{
				for (var slot = InventorySlot.Bag1; slot <= InventorySlot.BagLast; slot++)
				{
					var bag = this[slot] as Container;
					if (bag != null && bag.Template.BagFamily != 0)
					{
						var inv = bag.BaseInventory;
						if (bag.Template.MayAddToContainer(templ))
						{
							slotId.Slot = inv.FindFreeSlot();
							if (slotId.Slot != INVALID_SLOT)
							{
								slotId.Container = inv;
								break;
							}
						}
					}
				}
			}
		}

		public override bool Distribute(ItemTemplate template, ref int amount)
		{
			// distribute to ammo
			if (m_ammo != null && m_ammo.Template == template)
			{
				var diff = template.MaxAmount - m_ammo.Amount;
				if (diff > 0)
				{
					if (amount <= diff)
					{
						m_ammo.Amount += amount;
						return true;		// done
					}

					m_ammo.Amount += diff;
					amount -= diff;
				}
			}
			return base.Distribute(template, ref amount);
		}

		/// <summary>
		/// Gets a free slot in the bank or one of the bankbags
		/// </summary>
		public SimpleSlotId FindFreeBankSlot(IMountableItem item, int amount)
		{
			var slotId = new SimpleSlotId();

			for (var i = 0; i < ItemMgr.BankSlots.Length; i++)
			{
				var avlblSlot = (int)ItemMgr.BankSlots[i];
				if (m_Items[avlblSlot] == null)
				{
					slotId.Container = this;
					slotId.Slot = avlblSlot;
					return slotId;
				}
			}

			for (var i1 = 0; i1 < ItemMgr.BankBagSlots.Length; i1++)
			{
				var avlblSlot = (int)ItemMgr.BankBagSlots[i1];
				var container = m_Items[avlblSlot] as Container;
				if (container != null)
				{
					var contInv = container.BaseInventory;
					if (contInv.CheckAdd(0, item, amount) == InventoryError.OK)
					{
						var contItems = contInv.Items;
						for (var i = 0; i < contItems.Length; i++)
						{
							if (contItems[i] == null)
							{
								slotId.Container = contInv;
								slotId.Slot = i;
								return slotId;
							}
						}
					}
				}
			}
			slotId.Slot = INVALID_SLOT;
			return slotId;
		}


		/// <summary>
		/// Gets 0 to max free slots in the backpack or one of the equipped bags and optionally the bank (+ its bags)
		/// </summary>
		/// <param name="inclBank">whether to also look in the bank for free slots</param>
		/// <param name="max">The max of free inventory slots to be returned</param>
		public IList<SimpleSlotId> FindFreeSlots(bool inclBank, int max)
		{
			var slotList = new List<SimpleSlotId>();
			var slots = inclBank ? ItemMgr.InvSlotsWithBank : ItemMgr.StorageSlotsWithoutBank;

			var contLookup = ItemMgr.ContainerSlotsWithBank;

			for (var i1 = 0; i1 < slots.Length; i1++)
			{
				var avlblSlot = (int)slots[i1];
				if (contLookup[avlblSlot])
				{
					var container = m_Items[avlblSlot] as Container;
					if (container != null)
					{
						var contInv = container.BaseInventory;
						var contItems = contInv.Items;
						for (var i = 0; i < contItems.Length; i++)
						{
							if (contItems[i] == null)
							{
								var slotId = new SimpleSlotId { Container = contInv, Slot = i };
								slotList.Add(slotId);
								if (slotList.Count == max)
									return slotList;
							}
						}
					}
				}
				else if (m_Items[avlblSlot] == null)
				{
					var slotId = new SimpleSlotId { Container = this, Slot = avlblSlot };
					slotList.Add(slotId);
					if (slotList.Count == max)
					{
						return slotList;
					}
				}
			}
			return slotList;
		}

		/// <summary>
		/// Iterates over all existing items that this Character carries in backpack + bags (if specified, also Bank + BankBags)
		/// </summary>
		/// <param name="validator">Returns whether to continue iteration</param>
		/// <returns>whether iteration was not cancelled (usually indicating we didn't find what we were looking for)</returns>
		public override bool Iterate(Func<Item, bool> validator)
		{
			return Iterate(false, validator);
		}

		/// <summary>
		/// Iterates over all existing items that this Character carries in backpack + bags (if specified, also Bank + BankBags)
		/// </summary>
		/// <param name="inclBank">whether to also look through the bank</param>
		/// <param name="validator">Returns whether to continue iteration</param>
		/// <returns>whether iteration was not cancelled (usually indicating we didn't find what we were looking for)</returns>
		public bool Iterate(bool inclBank, Func<Item, bool> validator)
		{
			return Iterate(inclBank ? ItemMgr.InvSlotsWithBank : ItemMgr.InvSlots, validator);
		}

		/// <summary>
		/// Iterates over the Backpack and Bags
		/// </summary>
		/// <param name="validator"></param>
		/// <returns></returns>
		public bool IterateStorage(Func<Item, bool> validator)
		{
			return Iterate(ItemMgr.StorageSlotsWithoutBank, validator);
		}

		/// <summary>
		/// Iterates over all equipped items
		/// </summary>
		/// <param name="validator"></param>
		/// <returns></returns>
		public bool IterateEquipment(Func<Item, bool> validator)
		{
			return Iterate(ItemMgr.EquipmentSlots, validator);
		}

		public bool Iterate(InventorySlot[] slots, Func<Item, bool> validator)
		{
			var contLookup = ItemMgr.ContainerSlotsWithBank;

			for (var i1 = 0; i1 < slots.Length; i1++)
			{
				var avlblSlot = (int)slots[i1];
				var item = m_Items[avlblSlot];
				if (contLookup[avlblSlot])
				{
					// container
					var container = item as Container;
					if (container != null)
					{
						var contInv = container.BaseInventory;
						var contItems = contInv.Items;
						for (var i = 0; i < contItems.Length; i++)
						{
							item = contItems[i];
							if (item != null && !validator(item))
							{
								return false;
							}
						}
					}
				}
				else if (item != null && !validator(item))
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// whether the character has at least the given amount of Items with the given templateId
		/// </summary>
		public bool Contains(uint templateId, int amount, bool inclBank)
		{
			var found = 0;

			Iterate(inclBank, item =>
			{
				if (item.Template.Id == templateId)
				{
					found += item.Amount;
					return found < amount;		// continue if we didn't find enough yet
				}
				return true;
			});

			return found >= amount;
		}

		/// <summary>
		/// whether the character has  at least the given amount of Items with the given templateId
		/// </summary>
		public bool Contains(ItemId templateId, int amount, bool inclBank)
		{
			var found = 0;

			Iterate(inclBank, item =>
			{
				if (item.Template.ItemId == templateId)
				{
					found += item.Amount;
					return found < amount;		// continue if we didn't find enough yet
				}
				return true;
			});

			return found >= amount;
		}

		/// <summary>
		/// whether the character has the Item with the given ItemId
		/// </summary>
		public bool Contains(uint itemId)
		{
			return GetItemByItemId(itemId) != null;
		}

		/// <summary>
		/// whether the character has the Item with the given ItemId
		/// </summary>
		public bool Contains(ItemId itemId)
		{
			return GetItemByItemId(itemId) != null;
		}

		/// <summary>
		/// whether the character has the Item with the given unique id
		/// </summary>
		public bool ContainsLowId(uint lowId)
		{
			return GetItemByLowId(lowId) != null;
		}

		/// <summary>
		/// whether the character has all Items with the given unique id
		/// </summary>
		public bool ContainsAll(uint[] entryIds)
		{
			for (var i = 0; i < entryIds.Length; i++)
			{
				var id = entryIds[i];
				if (id == 0)
				{
					continue;
				}
				if (Iterate(true, item => item.EntryId != id))
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// whether the character has all Items with the given unique id
		/// </summary>
		public bool ContainsAll(ItemId[] itemIds)
		{
			for (var i = 0; i < itemIds.Length; i++)
			{
				var id = itemIds[i];
				if (id == 0)
				{
					continue;
				}
				if (Iterate(true, item => item.EntryId != (uint)id))
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Consumes the given amount of items with the given templateId.
		/// Also looks through the bank when inclBank is set.
		/// </summary>
		/// <param name="inclBank">whether to also search in the bank and its bags (if not enough was found in inventory and bags)</param>
		/// <param name="templateId"></param>
		/// <param name="amount">If 0, consumes all Items that were found</param>
		/// <param name="force">Whether only to remove if there are at least the given amount of items</param>
		/// <returns>whether the required amount of items was found (and thus consumed).</returns>
		public bool Consume(ItemId templateId, bool inclBank = false, int amount = 1, bool force = true)
		{
			return Consume((uint)templateId, inclBank, amount, force);
		}

		/// <summary>
		/// Consumes the given amount of items with the given templateId.
		/// Also looks through the bank when inclBank is set.
		/// </summary>
		/// <param name="inclBank">whether to also search in the bank and its bags (if not enough was found in inventory and bags)</param>
		/// <param name="templateId"></param>
		/// <param name="amount">If 0, consumes all Items that were found</param>
		/// <param name="force">Whether only to remove if there are at least the given amount of items</param>
		/// <returns>whether the required amount of items was found (and thus consumed).</returns>
		public bool Consume(uint templateId, bool inclBank = false, int amount = 1, bool force = true)
		{
			var slotIds = new List<SimpleSlotId>();						// the locations of the found items
			var found = Find(inclBank, amount, slotIds, item => item.Template.Id == templateId);

			if (force || found >= amount)
			{
				var count = slotIds.Count;
				var lastDelPos = found == amount ? 0 : 1;

				if (count > lastDelPos)
				{
					for (var i = count - 1; i >= lastDelPos; i--)
					{
						slotIds[i].Container.Destroy(slotIds[i].Slot);
					}
				}

				if (found > amount)
				{
					// we didn't delete the first item, instead we have to remove the remaining amount from it
					var firstSlotId = slotIds[0];
					var firstItem = firstSlotId.Item;
					firstItem.Amount = found - amount;
				}

				return true;
			}
			return false;
		}

		/// <summary>
		/// Finds and consumes all of the given items. 
		/// Does not consume anything and returns false if not all items were found.
		/// </summary>
		public bool Consume(ItemStackDescription[] items, bool inclBank)
		{
			var slotIdLists = new List<SimpleSlotId>[items.Length];				// the slots of the found items
			var foundAmounts = new int[items.Length];
			List<SimpleSlotId> slotIds;
			for (var i = 0; i < items.Length; i++)
			{
				var template = items[i];
				slotIdLists[i] = slotIds = new List<SimpleSlotId>();

				foundAmounts[i] = Find(inclBank, template.Amount, slotIds, item => item.Template.ItemId == template.ItemId);

				if (foundAmounts[i] < template.Amount)
				{
					return false;
				}
			}

			for (var i = 0; i < items.Length; i++)
			{
				var template = items[i];
				slotIds = slotIdLists[i];
				var found = foundAmounts[i];

				var count = slotIds.Count;
				var lastDelPos = found == template.Amount ? 0 : 1;

				if (count > lastDelPos)
				{
					for (var j = count - 1; j >= lastDelPos; j--)
					{
						slotIds[j].Container.Destroy(slotIds[j].Slot);
					}
				}

				if (found > template.Amount)
				{
					// we didn't delete the first item, instead we have to remove the remaining amount from it
					var firstSlotId = slotIds[0];
					firstSlotId.Item.Amount = found - template.Amount;
				}
			}
			return true;
		}

		/// <summary>
		/// Finds up to max items that are validated by the given validator and puts them in the given list.
		/// </summary>
		/// <param name="inclBank">whether to also search in the bank and its bags (if not enough was found in inventory and bags)</param>
		/// <param name="max">Maximum amount of items to be looked through</param>
		/// <param name="list">List of slot-identifiers for the items to be added to</param>
		/// <returns>The amount of found items</returns>
		public int Find(bool inclBank, int max, IList<SimpleSlotId> list, Func<Item, bool> validator)
		{
			var slots = inclBank ? ItemMgr.InvSlotsWithBank : ItemMgr.StorageSlotsWithoutBank;
			var found = 0;

			var contLookup = ItemMgr.ContainerSlotsWithBank;

			Item item;
			foreach (int avlblSlot in slots)
			{
				item = m_Items[avlblSlot];
				if (contLookup[avlblSlot])
				{
					var container = item as Container;
					if (container != null)
					{
						var contInv = container.BaseInventory;
						var contItems = contInv.Items;
						for (var i = 0; i < contItems.Length; i++)
						{
							item = contItems[i];
							if (item != null && validator(item))
							{
								found += item.Amount;
								var slotId = new SimpleSlotId { Container = contInv, Slot = i };
								list.Add(slotId);
								if (found >= max)
								{
									return found;
								}
							}
						}
					}
				}
				else if (item != null && validator(item))
				{
					found += item.Amount;
					var slotId = new SimpleSlotId
					{
						Container = this,
						Slot = avlblSlot
					};
					list.Add(slotId);
					if (found >= max)
					{
						return found;
					}
				}
			}
			return found;
		}

		/// <summary>
		/// Returns the total amount of Items within this Inventory of the given ItemId
		/// </summary>
		/// <returns></returns>
		public int GetAmount(ItemId id)
		{
			var amount = 0;
			Iterate(nexItem =>
			{
				if (nexItem.Template.ItemId == id)
				{
					amount += nexItem.Amount;
				}
				return true;
			});
			return amount;
		}
		#endregion

		#region Move/Equip Items

		/// <summary>
		/// Moves an item from one slot to another
		/// </summary>
		public InventoryError TrySwap(InventorySlot srcBagSlot, int srcSlot, InventorySlot destBagSlot, int destSlot)
		{
			var srcCont = GetContainer(srcBagSlot, IsBankOpen);
			var destCont = GetContainer(destBagSlot, IsBankOpen);

			InventoryError err;
			if (srcCont == null || destCont == null)
			{
				err = InventoryError.ITEMS_CANT_BE_SWAPPED;
				ItemHandler.SendInventoryError(Owner.Client, null, null, err);
			}
			else
			{
				err = TrySwap(srcCont, srcSlot, destCont, destSlot);
			}
			return err;
		}

		/// <summary>
		/// Moves an item from one slot to another.
		/// Core method for moving items around.
		/// </summary>
		public InventoryError TrySwap(BaseInventory srcCont, int srcSlot, BaseInventory destCont, int destSlot)
		{
			var err = InventoryError.OK;
			Item srcItem = null;
			Item destItem = null;
			Item occupyingWeapon = null;

			if (!srcCont.IsValidSlot(srcSlot))
			{
				err = InventoryError.ITEM_NOT_FOUND;
			}
			else if (!destCont.IsValidSlot(destSlot))
			{
				err = InventoryError.ITEM_NOT_FOUND2;
			}
			else
			{
				srcItem = srcCont[srcSlot];
				if (srcItem == null)
				{
					err = InventoryError.SLOT_IS_EMPTY;
				}
				else if (!srcItem.CanBeUsed)
				{
					err = InventoryError.CANT_DO_RIGHT_NOW;
				}
				else if (!m_owner.CanInteract)
				{
					err = InventoryError.CANT_DO_RIGHT_NOW;
				}
				else if (srcItem.IsEquippedContainer &&
					!((Container)srcItem).BaseInventory.IsEmpty &&
					!ItemMgr.IsContainerEquipmentSlot(destSlot) &&
					!Owner.GodMode)
				{
					err = InventoryError.CAN_ONLY_DO_WITH_EMPTY_BAGS;
				}
				else
				{
					// check whether the src item may be added to the dest spot
					if (destCont == this)
					{
						if (destSlot == (int)InventorySlot.OffHand)
						{
							// unequip 2h weapon when trying to equip offhand weapon (later move to available space if possible)
							occupyingWeapon = this[EquipmentSlot.MainHand];
							if (occupyingWeapon != null && !occupyingWeapon.Template.IsTwoHandWeapon)
							{
								occupyingWeapon = null;
							}
						}
						else if (destSlot == (int)InventorySlot.MainHand && srcItem.Template.IsTwoHandWeapon)
						{
							// unequip offhand weapon when trying to equip 2h weapon (later move to available space if possible)
							occupyingWeapon = this[EquipmentSlot.OffHand];
						}
					}

					destItem = destCont[destSlot];
					if (destItem != null)
					{
						if (!destItem.CanBeUsed)
						{
							err = InventoryError.CANT_DO_RIGHT_NOW;
						}
						else
						{
							if (occupyingWeapon != null)
							{
								// need extra space for occupyingWeapon
								if (!occupyingWeapon.Unequip())
								{
									err = InventoryError.INVENTORY_FULL;
									ItemHandler.SendInventoryError(Owner.Client, srcItem, destItem, err);
									return err;
								}
								occupyingWeapon = null;
							}

							if (destItem.IsEquippedContainer)
							{
								// move into container
								var cont = ((Container)destItem).BaseInventory;
								err = cont.CheckAdd(0, destItem, destItem.Amount);
								if (err != InventoryError.OK)
								{
									ItemHandler.SendInventoryError(Owner.Client, srcItem, destItem, err);
									return err;
								}

								var slot = cont.FindFreeSlot();
								if (slot == INVALID_SLOT)
								{
									// no free slot -> don't do anything
									return InventoryError.OK;
								}
								cont.AddUnchecked(slot, destItem, false);
							}
							else
							{
								// move to occupied slot
								var destHandler = destCont.GetHandler(destSlot);
								var amount = srcItem.Amount;
								destHandler.CheckAdd(destSlot, amount, srcItem, ref err);
								if (err == InventoryError.OK)
								{
									if (!srcItem.CanStackWith(destItem))
									{
										// check whether the dest item may be added to the src spot
										var srcHandler = srcCont.GetHandler(srcSlot);
										srcHandler.CheckAdd(srcSlot, destItem.Amount, destItem, ref err);
									}
								}
							}
						}
					}
					else
					{
						// move to empty slot

						var occupyingWeaponSlot = 0;
						if (occupyingWeapon != null)
						{
							// check whether we can put the occupyingWeapon into srcSlot
							var srcHandler = srcCont.GetHandler(srcSlot);
							srcHandler.CheckAdd(srcSlot, occupyingWeapon.Amount, occupyingWeapon, ref err);
							if (err != InventoryError.OK)
							{
								ItemHandler.SendInventoryError(Owner.Client, srcItem, destItem, err);
								return err;
							}
							occupyingWeaponSlot = occupyingWeapon.Slot;
							occupyingWeapon.Remove(false);
						}

						// check whether the src item may be added to the dest spot
						var destHandler = destCont.GetHandler(destSlot);
						destHandler.CheckAdd(destSlot, srcItem.Amount, srcItem, ref err);

						if (err != InventoryError.OK && occupyingWeapon != null)
						{
							// put occupying weapon back
							AddUnchecked(occupyingWeaponSlot, occupyingWeapon, false);
						}
					}
				}
			}

			if (err != InventoryError.OK)
			{
				ItemHandler.SendInventoryError(Owner.Client, srcItem, destItem, err);
			}
			else
			{
				SwapUnchecked(srcCont, srcSlot, destCont, destSlot);
				if (occupyingWeapon != null)
				{
					// occupying weapon was removed but not added again
					srcCont.AddUnchecked(srcSlot, occupyingWeapon, false);
				}
			}

			return err;
		}

		/// <summary>
		/// Tries to move an Item from one container into another
		/// </summary>
		public InventoryError TryMove(InventorySlot srcBagSlot, int srcSlot, InventorySlot destBagSlot)
		{
			var srcCont = GetContainer(srcBagSlot, IsBankOpen);
			var destCont = GetContainer(destBagSlot, IsBankOpen);

			InventoryError err;
			if (srcCont == null || destCont == null)
			{
				err = InventoryError.ITEMS_CANT_BE_SWAPPED;
				ItemHandler.SendInventoryError(Owner.Client, null, null, err);
			}
			else
			{
				err = TryMove(srcCont, srcSlot, destCont);
			}
			return err;
		}

		/// <summary>
		/// Tries to auto-equip an item from the given slot (from within the corresponding cont)
		/// </summary>
		public InventoryError TryMove(BaseInventory srcCont, int srcSlot, BaseInventory destCont)
		{
			var err = InventoryError.ITEM_NOT_FOUND;
			Item item = null;
			if (srcCont.IsValidSlot(srcSlot))
			{
				item = srcCont[srcSlot];

				if (item != null)
				{
					var slot = destCont.FindFreeSlot();
					if (slot != INVALID_SLOT)
					{
						return TrySwap(srcCont, srcSlot, destCont, slot);
					}
					err = destCont.FullError;
				}
				else
				{
					err = InventoryError.ITEM_NOT_FOUND;
				}
			}
			ItemHandler.SendInventoryError(Owner.Client, item, null, err);
			return err;
		}

		/// <summary>
		/// Tries to auti-equip an item from the given slot (from within the corresponding cont)
		/// </summary>
		public InventoryError TryEquip(InventorySlot contSlot, int slot)
		{
			var err = InventoryError.ITEM_NOT_FOUND;
			var cont = GetContainer(contSlot, IsBankOpen);
			if (cont != null)
			{
				err = TryEquip(cont, slot);
			}
			else
			{
				ItemHandler.SendInventoryError(m_owner.Client, null, null, err);
			}
			return err;
		}

		/// <summary>
		/// Tries to auti-equip an item from the given slot (from within the corresponding cont)
		/// </summary>
		public InventoryError TryEquip(BaseInventory cont, int slot)
		{
			InventoryError err;
			Item item = null;
			if (cont.IsValidSlot(slot))
			{
				item = cont[slot];
				if (item != null)
				{
					if (!item.IsEquipped)
					{
						if (item.Template.EquipmentSlots == null)
						{
							err = InventoryError.ITEM_CANT_BE_EQUIPPED;
						}
						else
						{
							var equipSlot = GetEquipSlot(item.Template, false);
							if (equipSlot != InventorySlot.Invalid)
							{
								return TrySwap(cont, slot, this, (int)equipSlot);
							}
							err = InventoryError.INVENTORY_FULL;
						}
					}
					else
					{
						err = InventoryError.OK;
					}
				}
				else
				{
					err = InventoryError.ITEM_NOT_FOUND;
				}
			}
			else
			{
				err = InventoryError.ITEM_NOT_FOUND;
			}
			ItemHandler.SendInventoryError(Owner.Client, item, null, err);
			return err;
		}

		public InventoryError Split(InventorySlot srcBagSlot, int srcSlot, InventorySlot destBagSlot, int destSlot, int amount)
		{
			var m_isBankOpen = IsBankOpen;
			var srcCont = GetContainer(srcBagSlot, m_isBankOpen);
			var destCont = GetContainer(destBagSlot, m_isBankOpen);

			InventoryError err;
			if (srcCont == null || destCont == null)
			{
				err = InventoryError.ITEM_NOT_FOUND;
				ItemHandler.SendInventoryError(Owner.Client, null, null, err);
			}
			else
			{
				err = Split(srcCont, srcSlot, destCont, destSlot, amount);
			}
			return err;
		}

		public InventoryError Split(BaseInventory srcCont, int srcSlot, BaseInventory destCont, int destSlot, int amount)
		{
			var err = InventoryError.OK;

			Item srcItem = null;
			Item destItem = null;

			if (!m_owner.CanInteract)
			{
				err = InventoryError.ITEMS_CANT_BE_SWAPPED;
			}
			else if (!srcCont.IsValidSlot(srcSlot))
			{
				err = InventoryError.ITEM_NOT_FOUND;
			}
			else if (!destCont.IsValidSlot(destSlot))
			{
				err = InventoryError.ITEM_NOT_FOUND2;
			}
			else
			{
				srcItem = srcCont[srcSlot];
				if (srcItem == null)
				{
					err = InventoryError.ITEM_NOT_FOUND;
				}
				else
				{
					var template = srcItem.Template;
					if (!srcItem.CanBeUsed)
					{
						err = InventoryError.CANT_DO_RIGHT_NOW;
					}
					if (!template.IsStackable)
					{
						err = InventoryError.COULDNT_SPLIT_ITEMS;
					}
					else if (amount > srcItem.Amount)
					{
						err = InventoryError.TRIED_TO_SPLIT_MORE_THAN_COUNT;
					}
					else if (amount > 0)
					{
						destItem = destCont[destSlot];
						if (destItem == null)
						{
							// empty target spot
							var destHandler = destCont.GetHandler(destSlot);
							destHandler.CheckAdd(destSlot, amount, srcItem, ref err);
							if (err == InventoryError.OK)
							{
								destCont[destSlot] = srcItem.Split(amount);
							}
						}
						else
						{
							// occupied target spot
							if (destItem.IsContainer && destCont == this)
							{
								// move to a container
								var cont = ((Container)destItem).BaseInventory;
								//var srcAmount = srcItem.Amount;
								err = cont.TryAddAmount(srcItem, amount, false);
							}
							else if (!srcItem.CanStackWith(destItem))
							{
								// move to a different type of Item
								err = InventoryError.COULDNT_SPLIT_ITEMS;
							}
							else
							{
								// move ontop of the same type of Item
								amount = Math.Min(amount, template.MaxAmount - destItem.Amount);

								srcItem.Amount -= amount;
								destItem.Amount += amount;
							}
						}
					}
				}
			}

			if (err != InventoryError.OK)
			{
				ItemHandler.SendInventoryError(Owner.Client, srcItem, destItem, err);
			}
			return err;
		}

		/// <summary>
		/// Moves the item from the given slot in the bank to the first free slot in the inventory or one of the equipped bags
		/// </summary>
		public InventoryError Withdraw(InventorySlot bagSlot, int slot)
		{
			InventoryError err;

			var item = GetBankItem(bagSlot, slot);

			if (item == null)
			{
				err = InventoryError.ITEM_NOT_FOUND;
			}
			else
			{
				var freeSlot = FindFreeSlot(item, item.Amount);
				if (freeSlot.Slot == INVALID_SLOT)
				{
					err = InventoryError.INVENTORY_FULL;
					ItemHandler.SendInventoryError(m_owner.Client, item, null, err);
				}
				else
				{
					err = InventoryError.OK;
					SwapUnchecked(item.Container, slot, freeSlot.Container, freeSlot.Slot);
				}
			}
			return err;
		}

		/// <summary>
		/// Moves the item from the given slot to the given slot in the Bank or one of its bags
		/// </summary>
		public InventoryError Deposit(InventorySlot bagSlot, int slot)
		{
			InventoryError err;

			var item = GetItem(bagSlot, slot, false);

			if (item == null)
			{
				err = InventoryError.ITEM_NOT_FOUND;
			}
			else
			{
				var freeBankSlot = FindFreeBankSlot(item, item.Amount);
				if (freeBankSlot.Slot == INVALID_SLOT)
				{
					err = InventoryError.BANK_FULL;
				}
				else
				{
					err = TrySwap(item.Container, slot, freeBankSlot.Container, freeBankSlot.Slot);
				}
			}

			if (err != InventoryError.OK)
			{
				ItemHandler.SendInventoryError(m_owner.Client, item, null, err);
			}
			return err;
		}
		#endregion

		#region Get Items
		/// <summary>
		/// Returns the first stack of Items with ItemId encountered in the Character's backpack
		/// and bags and (if open) in the Bank and Bankbags.
		/// </summary>
		/// <param name="id">ItemId of the Item to find.</param>
		/// <returns>The first stack encountered or null.</returns>
		public Item GetItemByItemId(ItemId id)
		{
			return GetItemByItemId((uint)id, IsBankOpen);
		}

		/// <summary>
		/// Returns the first stack of Items with ItemId encountered in the Character's backpack
		/// and bags and (if open) in the Bank and Bankbags.
		/// </summary>
		/// <param name="id">ItemId of the Item to find.</param>
		/// <returns>The first stack encountered or null.</returns>
		public Item GetItemByItemId(uint id)
		{
			return GetItemByItemId(id, IsBankOpen);
		}

		/// <summary>
		/// Returns the first stack of Items with ItemId encountered in the Character's backpack
		/// and bags and (optionally) in the Bank and Bankbags.
		/// </summary>
		/// <param name="id">ItemId of the Item to find.</param>
		/// <param name="includeBank">Whether to search the Bank.</param>
		/// <returns>The first stack encountered or null.</returns>
		public Item GetItemByItemId(ItemId id, bool includeBank)
		{
			return GetItemByItemId((uint)id, includeBank);
		}

		/// <summary>
		/// Returns the first stack of Items with ItemId encountered in the Character's backpack
		/// and bags and (optionally) in the Bank and Bankbags.
		/// </summary>
		/// <param name="templateId">ItemId of the Item to find.</param>
		/// <param name="includeBank">Whether to search the Bank.</param>
		/// <returns>The first stack encountered or null.</returns>
		public Item GetItemByItemId(uint templateId, bool includeBank)
		{
			Item foundItem = null;
			Iterate(includeBank, item =>
			{
				if (item.Template.Id == templateId)
				{
					foundItem = item;
					return false;
				}
				return true;
			});
			return foundItem;
		}

		/// <summary>
		/// Searches through a Character's Backpack and Bags and (if open) Bank and Bank Bags
		/// for Items with the given Id and adds up their amounts.
		/// </summary>
		/// <param name="id">ItemId of the Item to find.</param>
		/// <returns>The sum of the amounts of the Items encountered.</returns>
		public int GetItemAmountByItemId(ItemId id)
		{
			return GetItemAmountByItemId((uint)id, IsBankOpen);
		}

		/// <summary>
		/// Searches through a Character's Backpack and Bags and (if open) Bank and Bank Bags
		/// for Items with the given Id and adds up their amounts.
		/// </summary>
		/// <param name="id">ItemId of the Item to find.</param>
		/// <returns>The sum of the amounts of the Items encountered.</returns>
		public int GetItemAmountByItemId(uint id)
		{
			return GetItemAmountByItemId(id, IsBankOpen);
		}

		/// <summary>
		/// Searches through a Character's Backpack and Bags and (if open) Bank and Bank Bags
		/// for Items with the given Id and adds up their amounts.
		/// </summary>
		/// <param name="id">ItemId of the Item to find.</param>
		/// <param name="includeBank">Whether to search the Bank and Bank Bags.</param>
		/// <returns>The sum of the amounts of the Items encountered.</returns>
		public int GetItemAmountByItemId(ItemId id, bool includeBank)
		{
			return GetItemAmountByItemId((uint)id, includeBank);
		}

		/// <summary>
		/// Searches through a Character's Backpack and Bags and (if open) Bank and Bank Bags
		/// for Items with the given Id and adds up their amounts.
		/// </summary>
		/// <param name="templateId">ItemId of the Item to find.</param>
		/// <param name="includeBank">Whether to search the Bank and Bank Bags.</param>
		/// <returns>The sum of the amounts of the Items encountered.</returns>
		public int GetItemAmountByItemId(uint templateId, bool includeBank)
		{
			var total = 0;
			Iterate(includeBank, (item) =>
			{
				if (item.Template.Id == templateId)
				{
					total += item.Amount;
				}
				return true;
			});
			return total;
		}

		/// <summary>
		/// Returns the Item with the given EntityId if it exists in the player's inventory (or bank, if it is open).
		/// </summary>
		public Item GetItem(EntityId id)
		{
			return GetItem(id, IsBankOpen);
		}

		public Item GetItem(EntityId id, bool includeBank)
		{
			Item item = null;
			Iterate(includeBank, nextItem =>
			{
				if (nextItem.EntityId == id)
				{
					item = nextItem;
					return false;
				}
				return true;
			});
			return item;
		}

		public Item GetItemByLowId(uint entityLowId)
		{
			return GetItemByLowId(entityLowId, false);
		}

		public Item GetItemByLowId(uint entityLowId, bool includeBank)
		{
			Item item = null;
			Iterate(includeBank, nextItem =>
			{
				if (nextItem.EntityId.Low == entityLowId)
				{
					item = nextItem;
					return false;
				}
				return true;
			});
			return item;
		}

		/// <summary>
		/// Attempts to retreive an item in the character's available inventory.
		/// </summary>
		/// <param name="contSlot">The container's slot (Bag1 ... BagLast, BankBag1 ... BankBagLast), 
		/// use InventorySlot.Invalid for the Backpack slots.</param>
		/// <param name="slot">The slot within the container where the item resides.</param>
		/// <param name="inclBank">Whether to check the bank as well.</param>
		/// <returns>The item or null.</returns>
		public Item GetItem(InventorySlot contSlot, int slot, bool inclBank)
		{
			var cont = GetContainer(contSlot, inclBank) ?? this;

			if (!cont.IsValidSlot(slot))
				return null;

			var item = cont[slot];
			return item;
		}

		/// <summary>
		/// Returns an item from the bank in the given slot
		/// </summary>
		public Item GetBankItem(InventorySlot contSlot, int slot)
		{
			var cont = GetBankContainer(contSlot);

			if (cont != null && cont.IsValidSlot(slot))
			{
				var item = cont[slot];
				return item;
			}
			return null;
		}

		/// <summary>
		/// Enumerates all of the Character's owned Items (optionally with or without BuyBack)
		/// </summary>
		/// <returns></returns>
		public IEnumerable<Item> GetAllItems(bool includeBuyBack)
		{
			foreach (var slot in includeBuyBack ? ItemMgr.AllSlots : ItemMgr.OwnedSlots)
			{
				var item = this[slot];
				if (item != null)
				{
					yield return item;
					if (item is Container && ItemMgr.ContainerSlotsWithBank[item.Slot])
					{
						foreach (var contItem in ((Container)item).BaseInventory)
						{
							yield return contItem;
						}
					}
				}
			}
		}
		#endregion

		#region Remove
		/// <summary>
		/// Removes Items from the Backpack, Bags and (if indicated) from the Bank and BankBags.
		/// </summary>
		/// <param name="id">The ItemId of the Items to remove.</param>
		/// <param name="amount">The number of Items to remove.</param>
		/// <param name="includeBank">Whether to include the Bank and BankBags.</param>
		/// <returns>Whether amount of items was removed.</returns>
		public bool RemoveByItemId(ItemId id, int amount, bool includeBank)
		{
			var invAmt = GetItemAmountByItemId(id, includeBank);
			if (invAmt < amount)
			{
				return false;
			}

			Iterate(includeBank, (item) =>
			{
				if (item.Template.ItemId != id)
				{
					return true;
				}

				if (item.Amount >= amount)
				{
					item.Amount -= amount;
					amount = 0;
					return false;
				}

				amount -= item.Amount;
				item.Amount = 0;
				return true;
			});

			return (amount <= 0);
		}
		#endregion

		#region Equipment
		/// <summary>
		/// </summary>
		/// <returns>Whether a match was found</returns>
		public bool Iterate(InventorySlotTypeMask slots, Func<Item, bool> callback)
		{
			for (var sType = InventorySlotType.Head; sType < InventorySlotType.End; sType++)
			{
				if (!slots.HasAnyFlag(sType))
				{
					continue;
				}
				foreach (var slot in ItemMgr.GetEquipmentSlots(sType))
				{
					var item = this[slot];
					if (item != null && !callback(item))
					{
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Checks if the given slot is occupied and -if so- puts the item from that slot into a free
		/// storage slot (within the backpack or any equipped bags).
		/// </summary>
		/// <returns>whether the given slot is now empty</returns>
		public bool EnsureEmpty(EquipmentSlot slot)
		{
			var item = this[slot];
			if (item == null)
			{
				return true;
			}

			var newSlotId = FindFreeSlot(item, item.Amount);
			if (newSlotId.Slot == INVALID_SLOT)
			{
				// no space left
				return false;
			}

			SwapUnchecked(this, (int)slot, newSlotId.Container, newSlotId.Slot);
			return true;
		}

		/// <summary>
		/// Tries to unequip the item at the given InventorySlot into a free available slot
		/// </summary>
		public bool Unequip(InventorySlot slot)
		{
			return Unequip(this, (int)slot);
		}

		/// <summary>
		/// Tries to unequip the item at the given slot and put it into the first available slot in the backpack or an equipped cont.
		/// </summary>
		public bool Unequip(BaseInventory container, int slot)
		{
			var item = container[slot];
			if (item == null)
			{
				return false;
			}

			return item.Unequip();
		}

		/// <summary>
		/// Returns a free EquipmentSlot (or the first allowed slot). 
		/// If force is set and there is no free slot and the backpack still has space, it will unequip the
		/// item in the first possible slot to the backpack or an equipped bag.
		/// </summary>
		public InventorySlot GetEquipSlot(ItemTemplate templ, bool force)
		{
			var possibleSlots = templ.EquipmentSlots;

			if (templ.IsMeleeWeapon)
			{
				// special case: Trying to equip 1h weapon when 2h weapon is equipped
				var mhWeapon = this[EquipmentSlot.MainHand];
				if (mhWeapon != null && mhWeapon.Template.IsTwoHandWeapon)
				{
					return InventorySlot.MainHand;
				}
			}

			// see if we got a free slot
			for (var i = 0; i < possibleSlots.Length; i++)
			{
				var slot = possibleSlots[i];
				if (slot == EquipmentSlot.OffHand && !m_owner.Skills.CanDualWield)
				{
					continue;
				}

				var occupyingItem = this[slot];
				if (occupyingItem == null)
				{
					return (InventorySlot)slot;
				}
			}

			// no free slot, use the first one
			var firstSlot = possibleSlots[0];
			if (force)
			{
				// var lastSlot = possibleSlots[possibleSlots.Length - 1];
				if (!EnsureEmpty(firstSlot))
				{
					return InventorySlot.Invalid;
				}
			}

			return (InventorySlot)firstSlot;
		}


		/// <summary>
		/// Sets the ammo to the given template-id. 
		/// </summary>
		/// <returns>whether the ammo with the given id was found</returns>
		public bool SetAmmo(uint templId)
		{
			if (templId == 0)
			{
				Ammo = null;
				return true;
			}

			var found = false;
			Iterate(IsBankOpen,
					item =>
					{
						if (item.Template.Id == templId && item.Amount > 0 && item != m_ammo)
						{
							var err = item.Template.CheckEquip(Owner);
							if (err == InventoryError.OK)
							{
								if (!item.CanBeUsed)
								{
									err = InventoryError.CANT_DO_RIGHT_NOW;
								}
								else if (item.Template.IsAmmo)
								{
									Ammo = item;
									found = true;
								}
								else
								{
									err = InventoryError.ONLY_AMMO_CAN_GO_HERE;
								}
							}
							if (err != InventoryError.OK)
							{
								ItemHandler.SendInventoryError(Owner.Client, item, null, err);
							}
							return false;
						}
						return true;
					});
			return found;
		}

		/// <summary>
		/// Tries to consume 1 piece of ammo and returns whether there was any left to be consumed
		/// </summary>
		public bool ConsumeAmmo()
		{
			if (m_ammo != null)
			{
				m_ammo.Amount--;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Returns the amount of Items of the given Set that the owner currently has equipped.
		/// </summary>
		/// <param name="set"></param>
		/// <returns></returns>
		public uint GetSetCount(ItemSet set)
		{
			uint setCount = 0;
			foreach (var template in set.Templates)
			{
				foreach (var setSlot in template.EquipmentSlots)
				{
					var item = Equipment[setSlot];
					if (item != null && item.Template.Set == set)
					{
						setCount++;
					}
				}
			}
			return setCount;
		}
		#endregion

		#region Equipment Sets

		public IList<EquipmentSet> EquipmentSets
		{
			get
			{
				return Owner == null ? EquipmentSet.EmptyList : Owner.Record.EquipmentSets;
			}
		}

		public void SetEquipmentSet(EntityId setEntityId, int setId, string name, string icon, IList<EntityId> itemList)
		{
			//var setItemMappings = new EquipmentSetItemMapping[19];
			//for (var i = 0; i < 19; i++)
			//{
			//    var id = itemList[i];
			//    if (id != EntityId.Zero)
			//    {
			//        if (GetItem(id) != null)
			//        {
			//            setItemMappings[i] = new EquipmentSetItemMapping()
			//            {
			//                ItemEntityId = id
			//            };
			//            continue;
			//        }
			//    }

			//    setItemMappings[i] = null;
			//}

			//EquipmentSet set;
			//if (setEntityId == EntityId.Zero)
			//{
			//    // This is a new set.
			//    set = EquipmentSet.CreateSet();
			//    if (set == null) return;

			//    set.Fill(setId, name, icon, setItemMappings);
			//    ItemHandler.SendEquipmentSetSaved(Owner.Client, set);
			//    // TODO: Save to DB
			//    return;
			//}

			//set = GetEquipmentSet(setEntityId);
			//if (set == null) return;

			//set.Id = setId;
			//set.Name = name;
			//set.Icon = icon;
			//set.Items = setItemMappings;
		}

		public void UseEquipmentSet(EquipmentSwapHolder[] swaps)
		{
			if (swaps == null) return;
			for (var i = 0; i < 19; i++)
			{
				var holder = swaps[i];
				if (holder.ItemGuid == EntityId.Zero) continue;
				var destItem = GetItem(holder.ItemGuid);
				var srcItem = GetItem(holder.SrcContainer, holder.SrcSlot, IsBankOpen);
				if (destItem == null) continue;
				if (destItem == srcItem) continue;

				var msg = TrySwap(holder.SrcContainer, holder.SrcSlot, InventorySlot.Invalid, i);
				if (msg != InventoryError.OK)
				{
					ItemHandler.SendInventoryError(Owner.Client, msg);
					return;
				}
				ItemHandler.SendUseEquipmentSetResult(Owner.Client, UseEquipmentSetError.Success);
			}
		}

		public void SendEquipmentSetList()
		{
			ItemHandler.SendEquipmentSetList(Owner, EquipmentSets);
		}

		public void DeleteEquipmentSet(EntityId setGuid)
		{
			foreach (var set in EquipmentSets)
			{
				if (set.SetGuid != setGuid) continue;

				set.Items = null;
				EquipmentSets.Remove(set);
				break;
			}
		}

		private EquipmentSet GetEquipmentSet(EntityId setEntityId)
		{
			foreach (var set in EquipmentSets)
			{
				if (set.SetGuid != setEntityId) continue;
				return set;
			}
			return null;
		}

		#endregion

		#region Item Equipment Handlers
		internal List<IItemEquipmentEventHandler> m_ItemEquipmentEventHandlers;

		/// <summary>
		/// Adds a handler to be notified upon equipment changes
		/// </summary>
		public void AddEquipmentHandler(IItemEquipmentEventHandler handler)
		{
			if (m_ItemEquipmentEventHandlers == null)
			{
				m_ItemEquipmentEventHandlers = new List<IItemEquipmentEventHandler>(3);
			}
			m_ItemEquipmentEventHandlers.Add(handler);
		}

		/// <summary>
		/// Removes the given handler
		/// </summary>
		public void RemoveEquipmentHandler(IItemEquipmentEventHandler handler)
		{
			if (m_ItemEquipmentEventHandlers != null)
			{
				m_ItemEquipmentEventHandlers.Remove(handler);
			}
		}
		#endregion

		#region Checks
		/// <summary>
		/// Checks for whether the given amount of that Item can still be added 
		/// (due to max unique count).
		/// </summary>
		/// <param name="mountItem"></param>
		/// <returns></returns>
		internal InventoryError CheckEquipCount(IMountableItem mountItem)
		{
			var templ = mountItem.Template;
			if (templ.Flags.HasFlag(ItemFlags.UniqueEquipped))
			{
				// may only equip a certain maximum of this item
				foreach (var slot in templ.EquipmentSlots)
				{
					var item = this[slot];
					if (item != null && item.Template.Id == templ.Id)
					{
						return InventoryError.ITEM_UNIQUE_EQUIPABLE;
					}
				}
			}

			// also check for unique gems
			if (mountItem.Enchantments != null)
			{
				for (var i = EnchantSlot.Socket1; i < EnchantSlot.Socket1 + ItemConstants.MaxSocketCount; i++)
				{
					var enchant = mountItem.Enchantments[(uint)i];
					if (enchant != null && !CheckEquippedGems(enchant.Entry.GemTemplate))
					{
						return InventoryError.ITEM_UNIQUE_EQUIPPABLE_SOCKETED;
					}
				}
			}
			return InventoryError.OK;
		}

		internal bool CheckEquippedGems(ItemTemplate gemTempl)
		{
			if (gemTempl != null && gemTempl.Flags.HasFlag(ItemFlags.UniqueEquipped))
			{
				// may only equip a certain maximum of this kind of gem
				for (var slot = EquipmentSlot.Head; slot < EquipmentSlot.Bag1; slot++)
				{
					var item = this[slot];
					if (item != null && item.HasGem(gemTempl.ItemId))
					{
						return false;
					}
				}
			}
			return true;
		}
		#endregion

		#region Misc
		/// <summary>
		/// Removes the given percentage of durability from all Items of this Inventory.
		/// </summary>
		public void ApplyDurabilityLoss(int durLossPct)
		{
			IterateEquipment(item =>
			{
				if (item.MaxDurability > 0)
				{
					item.Durability = Math.Max(0, item.Durability - (((item.Durability * durLossPct) + 50) / 100));
				}
				return true;
			});
			ItemHandler.SendDurabilityDamageDeath(Owner);
		}

		/// <summary>
		/// Unequips all Items (if there is enough space left)
		/// </summary>
		public void Strip()
		{
			foreach (var item in Equipment)
			{
				item.Unequip();
			}
		}

		/// <summary>
		/// Destroys all Items
		/// </summary>
		public void Purge()
		{
			foreach (var item in GetAllItems(false))
			{
				item.Destroy();
			}
		}

		/// <summary>
		/// Check for whether there is at least one Item of each Category in this Inventory.
		/// </summary>
		/// <param name="cats"></param>
		/// <returns></returns>
		public bool CheckTotemCategories(TotemCategory[] cats)
		{
			for (var i = 0; i < cats.Length; i++)
			{
				var cat = cats[i];
				var slots = ItemMgr.GetTotemCatSlots(cat);
				var found = false;
				if (slots != null)
				{
					// tools that must be equipped
					for (var j = 0; j < slots.Length; j++)
					{
						var slot = slots[j];
						var item = this[slot];
						if (item != null && item.Template.TotemCategory == cat)
						{
							found = true;
							break;
						}
					}
				}
				else
				{
					// unequippable tools
					if (this[EquipmentSlot.MainHand] == null || this[EquipmentSlot.MainHand].Template.TotemCategory != cat)
					{
						found = !Iterate(item =>
						{
							if (item.Template.TotemCategory == cat)
							{
								return false;
							}
							return true;
						});
					}
					else
					{
						found = true;
					}
				}
				if (!found)
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Ensures that there is at least one Item that
		/// works as HearthStone in this inventory (if missing, tries to add one).
		/// </summary>
		/// <returns>False only if Hearthstone is missing and could not be added, otherwise true.</returns>
		public bool EnsureHearthStone()
		{
			if (Iterate(item => !item.Template.IsHearthStone))
			{
				// iterated over all items without finding it
				var err = TryAdd(ItemId.Hearthstone);
				if (err != InventoryError.OK)
				{
					//ItemHandler.SendInventoryError(m_owner, err);
					return false;
				}
				return true;
			}
			return true;
		}
		#endregion

		#region Load / Save
		public void AddDefaultItems()
		{
			var archetype = m_owner.Archetype;
			var items = archetype.GetInitialItems(m_owner.Gender);
			if (items != null)
			{
				for (var i = 0; i < items.Count; i++)
				{
					var itemDesc = items[i];
					var item = Item.CreateItem(itemDesc.Template, m_owner, itemDesc.Amount);
					var slotId = FindFreeSlot(item, 1, true);
					if (slotId.Slot == INVALID_SLOT)
					{
						log.Warn("{0} could not equip initial Item \"{1}\": No available slot.", m_owner, item);
						continue;
					}
					slotId.Container[slotId.Slot] = item;
					OnAddDontNotify(item);
					if (item.Template.IsAmmo)
					{
						Ammo = item;
					}
				}
			}
		}

		/// <summary>
		/// Adds the Items of this Character
		/// </summary>
		internal void AddOwnedItems()
		{
			// The following line will only load from DB if
			// ItemMgr was initialized after login 
			// which can only happen once and usually only on developing machines
			var records = m_owner.Record.LoadedItems;
			if (records == null) return;
			var containers = new List<BaseInventory>(7) { this };
			var items = new List<Item>(records.Count);

			foreach (var record in records)
			{
				//skip load if this itemrecord has been auctioned
				if (record.IsAuctioned)
					continue;

				var template = ItemMgr.Templates.Get(record.EntryId);
				if (template == null)
				{
					log.Warn("Item #{0} on {1} could not be loaded because it had an invalid ItemId: {2} ({3})",
						record.Guid, this, record.EntryId, (ItemId)record.EntryId);
					continue;
				}

				var item = Item.CreateItem(record, m_owner, template);
				if (record.IsEquippedContainer)
				{
					AddLoadedItem(item);
					if (item.IsInWorld)
					{
						containers.Add(((Container)item).BaseInventory);
					}
				}
				else
				{
					items.Add(item);
				}
			}

			foreach (var item in items)
			{
				// add items to their slots
				var record = item.Record;
				var cont = containers.FirstOrDefault(bag => bag.Slot == record.ContainerSlot);
				if (cont == null)
				{
					log.Error("Error when loading Item for Character: {0} - Could not find Container for {1} at slot {2} ({3})",
						this, item.Record, (InventorySlot)record.ContainerSlot, record.ContainerSlot);
				}
				else
				{
					cont.AddLoadedItem(item);
					if (item.IsInWorld && item.Template.IsAmmo && m_ammo == null)
					{
						Ammo = item;
					}
				}
			}
		}

		/// <summary>
		/// Saves all items and adds their records to the given list.
		/// </summary>
		/// <param name="records"></param>
		public void SaveAll(List<ItemRecord> records)
		{
			foreach (var item in GetAllItems(false))
			{
				item.Save();
				records.Add(item.Record);
			}
		}
		#endregion

		public override string ToString()
		{
			return "Inventory of " + Owner + ": " + this.ToArray().ToString(" / ");
		}
	}
}