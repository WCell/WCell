using System;
using System.Collections;
using System.Collections.Generic;
using NLog;
using WCell.Constants.Items;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.Util.NLog;

namespace WCell.RealmServer.Items
{
	public abstract class BaseInventory : IInventory
	{
		public const int INVALID_SLOT = 0xff;

		protected int m_baseField;
		protected Item[] m_Items;
		protected int m_count;

		/// <summary>
		/// The srcCont or Player that this inventory belongs to.
		/// </summary>
		internal protected IContainer m_container;

		protected BaseInventory(IContainer owner, UpdateFieldId baseField, int invSize) :
			this(owner, baseField, new Item[invSize])
		{
		}

		/// <summary>
		/// Inventory for shared item-array
		/// </summary>
		protected BaseInventory(IContainer owner, UpdateFieldId baseField, Item[] items)
		{
			m_container = owner;
			m_baseField = baseField.RawId;
			m_Items = items;
			m_count = 0;
		}

		#region Properties

		//public Unit Owner
		//{
		//    get
		//    {
		//        Unit owner;
		//        if (m_Container is IInventory)
		//        {
		//            owner = ((IInventory)m_Container).Owner;
		//        }
		//        else
		//        {
		//            owner = m_Container as Unit;
		//        }

		//        return owner;
		//    }
		//}

		/// <summary>
		/// The owning player of this inventory
		/// </summary>
		public Character Owner
		{
			get
			{
				if (m_container is Item)
				{
					return ((Item)m_container).OwningCharacter;
				}
				return m_container as Character;
			}
		}

		/// <summary>
		/// The containing ObjectBase (either Container or Character)
		/// </summary>
		public IContainer Container
		{
			get
			{
				return m_container;
			}
		}

		/// <summary>
		/// The slot of where this Container is located
		/// </summary>
		public byte Slot
		{
			get
			{
				if (m_container is Container)
				{
					return (byte)((Container)m_container).Slot;
				}
				return 0xFF;
			}
		}

		public abstract InventoryError FullError { get; }

		/// <summary>
		/// The underlying arrays of items of this inventory (don't modify from outside)
		/// </summary>
		public Item[] Items
		{
			get
			{
				return m_Items;
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

		/// <summary>
		/// Sets or Gets the Item at the given slot (make sure that slot is valid and unoccupied).
		/// </summary>
		/// <remarks>Cannot set to null - Use <see cref="Remove(int, bool)"/> instead.</remarks>
		/// <param name="slot"></param>
		/// <returns></returns>
		public Item this[int slot]
		{
			get
			{
				if (slot < 0 || slot > m_Items.Length)
					return null;

				return m_Items[slot];
			}
			set
			{
				if (value == null)
				{
					throw new NullReferenceException(string.Format(
						"Cannot set Slot {0} in Inventory \"{1}\" to null - Use the Remove-method instead.", slot, this));
				}

				if (m_Items[slot] != null)
				{
					var msg = string.Format(
						"Cannot add Item \"{0}\" to Slot {1} in Inventory \"{2}\" to - Slot is occupied with Item \"{3}\".",
						value,
						slot,
						this,
						m_Items[slot]);
					LogUtil.ErrorException(msg);
					value.Destroy();
					if (Owner != null)
					{
						Owner.SendSystemMessage(msg);
					}
					return;
				}

				var field = m_baseField + (slot * 2);

				m_count++;
				value.Container = this;
				value.Slot = slot;
				m_container.SetEntityId(field, value.EntityId);
				m_Items[slot] = value;

				var handler = GetHandler(slot);
				if (handler != null)
				{
					handler.Added(value);
				}
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
			return item.Container == this;
		}

		public void CopyTo(Item[] array, int arrayIndex)
		{
			m_Items.CopyTo(array, arrayIndex);
		}

		public bool Remove(Item item)
		{
			throw new InvalidOperationException();
		}

		/// <summary>
		/// The amount of items, currently in this inventory.
		/// </summary>
		public int Count
		{
			get { return m_count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		/// The maximum amount of items, supported by this inventory
		/// </summary>
		public virtual int MaxCount
		{
			get
			{
				return m_Items.Length;
			}
		}

		/// <summary>
		/// whether there are no items in this inventory
		/// </summary>
		public bool IsEmpty
		{
			get { return m_count == 0; }
		}

		/// <summary>
		/// whether there is no space left in this inventory
		/// </summary>
		public bool IsFull
		{
			get { return m_count == m_Items.Length; }
		}
		#endregion

		//private bool TryAdd(Item item, out InventoryError invError)
		//{
		//    invError = InventoryError.OK;

		//    return true;
		//}

		/// <summary>
		/// Swaps the items at the given slots without further checks.
		/// </summary>
		/// <param name="slot1"></param>
		/// <param name="slot2"></param>
		/// <remarks>Make sure the slots are valid before calling.</remarks>
		public void SwapUnchecked(BaseInventory cont1, int slot1, BaseInventory cont2, int slot2)
		{
			var item1 = cont1.m_Items[slot1];
			var item2 = cont2.m_Items[slot2];

			var handler1 = cont1.GetHandler(slot1);
			var handler2 = cont2.GetHandler(slot2);

			if (item2 != null &&
				item1.CanStackWith(item2) &&
				item2.Amount < item2.Template.MaxAmount)
			{
				var diff = item1.Template.MaxAmount - item2.Amount;
				diff = Math.Min(diff, item1.Amount);
				item1.Amount -= diff;
				item2.Amount += diff;
			}
			else
			{
				// remove both Items
				if (item1 != null)
				{
					item1.Slot = slot2;
					if (handler1 != null)
					{
						handler1.Removed(slot1, item1);
					}
				}

				if (item2 != null)
				{
					item2.Slot = slot1;
					if (handler2 != null)
					{
						handler2.Removed(slot2, item2);
					}
				}


				// set new items, fields and tell the container about the change
				cont1.m_Items[slot1] = item2;
				cont2.m_Items[slot2] = item1;

				var field1 = cont2.m_baseField + (slot2 * 2);
				if (item1 != null)
				{
					cont2.Container.SetEntityId(field1, item1.EntityId);
					item1.Container = cont2;
					if (handler2 != null)
					{
						handler2.Added(item1);
					}
				}
				else
				{
					cont2.Container.SetEntityId(field1, EntityId.Zero);
				}

				var field2 = cont1.m_baseField + (slot1 * 2);
				if (item2 != null)
				{
					cont1.Container.SetEntityId(field2, item2.EntityId);
					item2.Container = cont1;
					if (handler1 != null)
					{
						handler1.Added(item2);
					}
				}
				else
				{
					cont1.Container.SetEntityId(field2, EntityId.Zero);
				}
			}
		}

		internal void SwapUnnotified(int slot1, int slot2)
		{
			var item1 = m_Items[slot1];
			var item2 = m_Items[slot2];

			// remove both Items
			if (item1 != null)
			{
				item1.Slot = slot2;
			}

			if (item2 != null)
			{
				item2.Slot = slot1;
			}


			// set new items, fields and tell the container about the change
			m_Items[slot1] = item2;
			m_Items[slot2] = item1;

			var field1 = m_baseField + (slot2 * 2);
			if (item1 != null)
			{
				Container.SetEntityId(field1, item1.EntityId);
			}
			else
			{
				Container.SetEntityId(field1, EntityId.Zero);
			}

			var field2 = m_baseField + (slot1 * 2);
			if (item2 != null)
			{
				Container.SetEntityId(field2, item2.EntityId);
			}
			else
			{
				Container.SetEntityId(field2, EntityId.Zero);
			}
		}

		public bool IsValidSlot(int slot)
		{
			return slot >= 0 && slot < m_Items.Length;
		}

		internal void RemovePlaceHolder(int slot)
		{
			m_Items[slot] = null;
		}

		/// <summary>
		/// Finds a free slot for the given template and occpuies it with a placeholder.
		/// Don't forget to remove it again.
		/// </summary>
		/// <param name="templ"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		internal SimpleSlotId HoldFreeSlot(ItemTemplate templ, int amount)
		{
			var slotId = FindFreeSlot(templ, amount);
			if (slotId.Slot != INVALID_SLOT)
			{
				slotId.Container.m_Items[slotId.Slot] = Item.PlaceHolder;
			}
			return slotId;
		}

		public virtual SimpleSlotId FindFreeSlot(IMountableItem item, int amount)
		{
			return new SimpleSlotId
			{
				Container = this,
				Slot = FindFreeSlot()
			};
		}

		public virtual int FindFreeSlot()
		{
			for (var i = 0; i < m_Items.Length; i++)
			{
				if (m_Items[i] == null)
				{
					return i;
				}
			}
			return INVALID_SLOT;
		}

		/// <summary>
		/// Find a free slot within the given range
		/// </summary>
		public virtual int FindFreeSlot(int offset, int end)
		{
			for (int i = offset; i < end; i++)
			{
				if (m_Items[i] == null)
				{
					return i;
				}
			}
			return INVALID_SLOT;
		}

		/// <summary>
		/// Returns the IItemSlotHandler for the specified spot
		/// </summary>
		public abstract IItemSlotHandler GetHandler(int slot);

		public abstract PlayerInventory OwnerInventory { get; }

		/// <summary>
		/// Is called before adding the given amount of the given Item. 
		/// </summary>
		/// <param name="item"></param>
		/// <param name="amount"></param>
		/// <param name="err"></param>
		internal void CheckUniqueness(IMountableItem item, ref int amount, ref InventoryError err, bool isNew)
		{
			var template = item.Template;
			if (isNew)
			{
				if (template.UniqueCount > 0)
				{
					var count = OwnerInventory.GetUniqueCount(template.ItemId);
					if ((count + (int)amount) > template.UniqueCount)
					{
						amount = amount - template.UniqueCount;
						if (amount < 1)
						{
							err = InventoryError.CANT_CARRY_MORE_OF_THIS;
							return;
						}
					}
				}

				// also check for unique gems
				if (item.Enchantments != null)
				{
					for (var i = EnchantSlot.Socket1; i < EnchantSlot.Socket1 + ItemConstants.MaxSocketCount; i++)
					{
						var enchant = item.Enchantments[(uint)i];
						if (enchant != null &&
							enchant.Entry.GemTemplate != null &&
							enchant.Entry.GemTemplate.UniqueCount > 0 &&
							OwnerInventory.GetUniqueCount(enchant.Entry.GemTemplate.ItemId) >= enchant.Entry.GemTemplate.UniqueCount)
						{
							err = InventoryError.CANT_CARRY_MORE_OF_THIS;
							return;
						}
					}
				}
			}
		}

		#region Add / Ensure
		/// <summary>
		/// Tries to add a new item with the given id to a free slot.
		/// </summary>
		/// <returns>The result (InventoryError.OK in case that it worked)</returns>
		public InventoryError TryAdd(ItemId id)
		{
			return TryAdd(ItemMgr.GetTemplate(id));
		}

		/// <summary>
		/// Tries to add a new item with the given id to a free slot.
		/// </summary>
		/// <returns>The result (InventoryError.OK in case that it worked)</returns>
		public InventoryError TryAdd(ItemTemplate templ)
		{
			if (templ != null)
			{
				var slotId = FindFreeSlot(templ, 1);
				if (slotId.Slot == INVALID_SLOT)
				{
					return FullError;
				}
				return slotId.Container.TryAdd(templ, slotId.Slot);
			}
			return InventoryError.Invalid;
		}

		/// <summary>
		/// Tries to add ONE new item with the given template to the given slot.
		/// Make sure the given targetSlot is valid before calling this method.
		/// </summary>
		/// <returns>The result (InventoryError.OK in case that it worked)</returns>
		public InventoryError TryAdd(ItemId id, int targetSlot)
		{
			var templ = ItemMgr.GetTemplate(id);
			if (templ != null)
			{
				return TryAdd(templ, targetSlot);
			}
			return InventoryError.Invalid;
		}

		/// <summary>
		/// Tries to add ONE new item with the given template to the given slot.
		/// Make sure the given targetSlot is valid before calling this method.
		/// </summary>
		/// <returns>The result (InventoryError.OK in case that it worked)</returns>
		public InventoryError TryAdd(ItemId id, InventorySlot targetSlot)
		{
			var templ = ItemMgr.GetTemplate(id);
			if (templ != null)
			{
				return TryAdd(templ, (int)targetSlot);
			}
			return InventoryError.ITEM_NOT_FOUND;
		}

		/// <summary>
		/// Tries to add a new item with the given template and amount
		/// </summary>
		/// <returns>The result (InventoryError.OK in case that it worked)</returns>
		public InventoryError TryAdd(ItemId templId, ref int amount)
		{
			return TryAdd(templId, ref amount, true);
		}

		/// <summary>
		/// Tries to add an item with the given template and amount
		/// </summary>
		/// <returns>The result (InventoryError.OK in case that it worked)</returns>
		public InventoryError TryAdd(ItemId templId, ref int amount, bool isNew)
		{
			var templ = ItemMgr.GetTemplate(templId);
			if (templ != null)
			{
				return TryAdd(templ, ref amount, isNew);
			}
			return InventoryError.ITEM_NOT_FOUND;
		}

		/// <summary>
		/// Tries to add a new item with the given template and amount
		/// </summary>
		/// <returns>The result (InventoryError.OK in case that it worked)</returns>
		public InventoryError TryAdd(uint templId, ref int amount)
		{
			return TryAdd((ItemId)templId, ref amount);
		}

		/// <summary>
		/// Tries to add ONE new item with the given template to the given slot.
		/// Make sure the given targetSlot is valid before calling this method.
		/// </summary>
		public InventoryError TryAdd(ItemTemplate template, int targetSlot)
		{
			var amount = 1;
			return TryAdd(template, ref amount, targetSlot, true);
		}

		/// <summary>
		/// Tries to add a new item with the given template and amount ot the given slot.
		/// Make sure the given targetSlot is valid before calling this method.
		/// </summary>
		/// <returns>The result (InventoryError.OK in case that it worked)</returns>
		public InventoryError TryAdd(ItemId id, ref int amount, int targetSlot)
		{
			return TryAdd(id, ref amount, targetSlot, true);
		}

		/// <summary>
		/// Tries to add an Item with the given template and amount ot the given slot.
		/// Make sure the given targetSlot is valid before calling this method.
		/// </summary>
		/// <returns>The result (InventoryError.OK in case that it worked)</returns>
		public InventoryError TryAdd(ItemId id, ref int amount, int targetSlot, bool isNew)
		{
			var templ = ItemMgr.GetTemplate(id);
			if (templ != null)
			{
				return TryAdd(templ, ref amount, targetSlot, isNew);
			}
			return InventoryError.ITEM_NOT_FOUND;
		}

		/// <summary>
		/// Tries to add a new item with the given template and amount ot the given slot.
		/// Make sure the given targetSlot is valid before calling this method.
		/// </summary>
		/// <returns>The result (InventoryError.OK in case that it worked)</returns>
		public InventoryError TryAdd(ItemTemplate template, ref int amount, int slot, bool isNew)
		{
			if (m_Items[slot] != null)
			{
				LogManager.GetCurrentClassLogger().Warn("Tried to add Item {0} to {1} in occupied slot {2}", template, this, slot);
				return InventoryError.ITEM_DOESNT_GO_TO_SLOT;
			}

			var err = InventoryError.OK;
			CheckUniqueness(template, ref amount, ref err, isNew);
			if (err == InventoryError.OK)
			{
				var handler = GetHandler(slot);
				if (handler != null)
				{
					err = InventoryError.OK;
					handler.CheckAdd(slot, amount, template, ref err);
					if (err != InventoryError.OK)
					{
						return err;
					}
				}

				AddUnchecked(slot, template, amount, isNew);
			}
			return err;
		}

		/// <summary>
		/// Tries to merge an item with the given template and amount to the stack at the given slot.
		/// If the given slot is empty it adds the item to the slot.
		/// Make sure the given targetSlot is valid before calling this method.
		/// </summary>
		/// <param name="amount">Set to the number of items actually added.</param>
		/// <returns>The result (InventoryError.OK in case that it worked)</returns>
		public InventoryError TryMerge(ItemTemplate template, ref int amount, int slot, bool isNew)
		{
			var err = InventoryError.OK;
			CheckUniqueness(template, ref amount, ref err, isNew);
			if (err == InventoryError.OK)
			{
				var handler = GetHandler(slot);
				if (handler != null)
				{
					err = InventoryError.OK;
					handler.CheckAdd(slot, amount, template, ref err);
					if (err != InventoryError.OK)
					{
						return err;
					}
				}

				MergeUnchecked(slot, template, ref amount, isNew);
			}
			return err;
		}

		/// <summary>
		/// Tries to add a new item with the given template and amount
		/// </summary>
		/// <param name="amount">Amount of items to be added: Will be set to the amount of Items that have actually been added.</param>
		/// <returns>The result (InventoryError.OK in case that it worked)</returns>
		public InventoryError TryAdd(ItemTemplate template, ref int amount)
		{
			return TryAdd(template, ref amount, true);
		}

		/// <summary>
		/// Tries to add an item with the given template and amount
		/// </summary>
		/// <param name="amount">Amount of items to be added: Will be set to the amount of Items that have actually been added.</param>
		/// <returns>The result (InventoryError.OK in case that it worked)</returns>
		public InventoryError TryAdd(ItemTemplate template, ref int amount, bool isNew)
		{
			var distributedAmount = amount;
			var err = InventoryError.OK;
			if (!Distribute(template, ref distributedAmount))
			{
				amount -= distributedAmount;
				CheckUniqueness(template, ref amount, ref err, isNew);
				if (err == InventoryError.OK)
				{
					var slotId = FindFreeSlot(template, amount);
					if (slotId.Slot == INVALID_SLOT)
					{
						return FullError;
					}
					slotId.Container.AddUnchecked(slotId.Slot, template, amount, true);
				}
			}
			// TODO: Send the item update message
			//OnAdded(template, amount);
			return err;
		}

		/// <summary>
		/// Tries to distribute the given item over all available stacks and add the remainder to a free slot.
		/// IMPORTANT:
		/// 1. The Item will be destroyed if it could successfully be distributed over existing stacks of Items.
		/// 2. If item.Container == null, parts of the item-stack might have distributed over other stacks of the same type
		/// but the remainder did not find a free slot or exceeded the max count of the item.
		/// item.Amount will hold the remaining amount.
		/// </summary>
		/// <returns>The result (InventoryError.OK in case that it worked)</returns>
		public InventoryError TryAdd(Item item, bool isNew)
		{
			var amount = item.Amount;
			var err = InventoryError.OK;
			var distributedAmount = amount;
			if (!Distribute(item.Template, ref distributedAmount))
			{
				amount -= distributedAmount;
				item.Amount = amount;
				CheckUniqueness(item, ref amount, ref err, isNew);
				if (err == InventoryError.OK)
				{
					var slotId = FindFreeSlot(item, amount);
					if (slotId.Slot == INVALID_SLOT)
					{
						return FullError;
					}
					slotId.Container.AddUnchecked(slotId.Slot, item, isNew);
				}
			}
			else
			{
				amount -= distributedAmount;
				item.Amount = amount;
			}
			OnAdded(item, amount);
			return err;
		}

		/// <summary>
		/// Tries to distribute the given amount of the given Item over all available stacks and add the remainder to a free slot.
		/// Parts of the stack might have distributed over existing stacks, even if adding the remainder failed.
		/// </summary>
		/// <returns>InventoryError.OK in case that it could be added</returns>
		public InventoryError TryAddAmount(Item item, int amount, bool isNew)
		{
			var distributedAmount = amount;
			var err = InventoryError.OK;
			if (!Distribute(item.Template, ref distributedAmount))
			{
				CheckUniqueness(item, ref amount, ref err, isNew);
				if (err == InventoryError.OK)
				{
					amount -= distributedAmount;
					item.Amount = amount;
					var slotId = FindFreeSlot(item, amount);
					if (slotId.Slot == INVALID_SLOT)
					{
						return InventoryError.BAG_FULL;
					}
					slotId.Container.AddUnchecked(slotId.Slot, item.Template, amount, isNew);
				}
			}
			else
			{
				amount -= distributedAmount;
				item.Amount = amount;
			}
			OnAdded(item, amount);
			return err;
		}

		/// <summary>
		/// Tries to add the given item to the given slot (make sure the slot is valid and not occupied).
		/// Fails if not all items of this stack can be added.
		/// </summary>
		/// <returns>InventoryError.OK in case that it worked</returns>
		public InventoryError TryAdd(int slot, Item item, bool isNew)
		{
			var amount = item.Amount;
			var err = CheckAdd(slot, item, amount);
			if (err == InventoryError.OK)
			{
				CheckUniqueness(item, ref amount, ref err, isNew);
				if (err == InventoryError.OK && amount != item.Amount)
				{
					err = InventoryError.CANT_CARRY_MORE_OF_THIS;
				}
				else
				{
					AddUnchecked(slot, item, isNew);
				}
			}
			OnAdded(item, amount);
			return err;
		}

		void OnAdded(Item item, int amount)
		{
			if (!item.IsBuyback)
			{
				//ItemHandler.SendItemPushResult(Owner, item, true, );
			}
		}
		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="slot"></param>
		/// <param name="item"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public InventoryError CheckAdd(int slot, IMountableItem item, int amount)
		{
			var handler = GetHandler(slot);
			if (handler != null)
			{
				var err = InventoryError.OK;
				handler.CheckAdd(slot, amount, item, ref err);
				return err;
			}
			return InventoryError.OK;
		}

		public Item AddUnchecked(int slot, ItemId id, int amount, bool isNew)
		{
			var templ = ItemMgr.GetTemplate(id);
			return AddUnchecked(slot, templ, amount, isNew);
		}

		/// <summary>
		/// Tries to distribute the given amount of the given template and add the remainder to a free slot.
		/// Does not make any checks (eg for unqueness).
		/// </summary>
		/// <param name="template"></param>
		/// <param name="amount"></param>
		/// <param name="isNew"></param>
		/// <returns>The newly created stack that has not been distributed or null.</returns>
		public Item DistributeUnchecked(ItemTemplate template, int amount, bool isNew)
		{
			var distributedAmount = amount;
			if (!Distribute(template, ref distributedAmount))
			{
				amount -= distributedAmount;
				var slotId = FindFreeSlot(template, amount);
				if (slotId.Slot == INVALID_SLOT)
				{
					return null;
				}
				return slotId.Container.AddUnchecked(slotId.Slot, template, amount, true);
			}
			return null;
		}

		/// <summary>
		/// Adds the Item to the given slot without any further checks.
		/// Make sure all parameters are valid (eg by calling <code>CheckAdd</code> beforehand)
		/// or use <code>TryAdd</code> instead.
		/// </summary>
		/// <param name="slot"></param>
		public Item AddUnchecked(int slot, ItemTemplate template, int amount, bool isNew)
		{
			var item = Item.CreateItem(template, Owner, amount);
			this[slot] = item;
			if (isNew)
			{
				OwnerInventory.OnAdd(item);
			}
			return item;
		}

		/// <summary>
		/// Adds an amount of Items with ItemTemplate to the Item in the given slot, without further checks.
		/// If the given slot is empty, it AddsUnchecked.
		/// </summary>
		/// <param name="amount">Set to the number of Items actually added.</param>
		/// <returns>The Item in the slot you merged to.</returns>
		public Item MergeUnchecked(int slot, ItemTemplate template, ref int amount, bool isNew)
		{
			var item = m_Items[slot];
			if (item == null)
			{
				return AddUnchecked(slot, template, amount, isNew);
			}

			var freeSpace = item.Template.MaxAmount - item.Amount;
			freeSpace = Math.Min(freeSpace, amount);

			amount = freeSpace;
			item.Amount += freeSpace;
			if (isNew)
			{
				OwnerInventory.OnAdd(item);
			}
			return item;
		}

		/// <summary>
		/// Adds the Item to the given slot without any further checks.
		/// Make sure all parameters are valid (eg by calling <code>CheckAdd</code> beforehand)
		/// or use <code>TryAdd</code> instead.
		/// </summary>
		/// <param name="slot"></param>
		/// <param name="item"></param>
		public void AddUnchecked(int slot, Item item, bool isNew)
		{
			this[slot] = item;
			if (isNew)
			{
				OwnerInventory.OnAdd(item);
			}
			else
			{
				var owner = Owner;
				var context = owner.ContextHandler;
				if (context != null)
				{
					context.AddMessage(() =>
					{
						if (owner.IsInWorld)
						{
							owner.AddItemToUpdate(item);
						}
					});
				}
			}
		}

		internal void AddLoadedItem(Item item)
		{
			try
			{
				if (this[item.Slot] != null)
				{
					// no idea why items get saved in the same slot
					LogManager.GetCurrentClassLogger().Warn("Ignoring Item {0} for {1} because slot is already occupied by: {2}", item,
															Owner, this[item.Slot]);
					item.Destroy();
					return;
				}

				var owner = Owner;
				var record = item.Record;
				var inv = OwnerInventory;

				// add enchants
				// item.ApplyEnchant(item.Record.EnchantPerm, EnchantSlot.Permanent, 0, 0);
				// item.ApplyEnchant(item.Record.EnchantTemp, EnchantSlot.Temporary, (uint)item.Record.EnchantTempTime, 0);
				// item.ApplyEnchant(item.Record.EnchantSock1, EnchantSlot.Socket1, 0, 0);
				// item.ApplyEnchant(item.Record.EnchantSock2, EnchantSlot.Socket2, 0, 0);
				// item.ApplyEnchant(item.Record.EnchantSock3, EnchantSlot.Socket3, 0, 0);
				if (record.EnchantIds != null)
				{
					for (var slot = 0; slot < record.EnchantIds.Length; slot++)
					{
						var enchant = record.EnchantIds[slot];
						if (slot == (int)EnchantSlot.Temporary)
						{
							item.ApplyEnchant(enchant, (EnchantSlot)slot, record.EnchantTempTime, 0, false);
						}
						else
						{
							item.ApplyEnchant(enchant, (EnchantSlot)slot, 0, 0, false);
						}
					}
					//item.CheckSocketColors();
				}

				this[item.Slot] = item;

				owner.AddItemToUpdate(item);
				inv.OnAddDontNotify(item);
			}
			catch (Exception e)
			{
				LogUtil.ErrorException(e, "Unable to add Item {0} to Character {1}", item, Owner);
			}
		}

		public Item Remove(InventorySlot slot, bool ownerChange)
		{
			return Remove((int)slot, ownerChange);
		}

		/// <summary>
		/// Make sure that you have a valid slot before calling this method (see IsValidSlot).
		/// </summary>
		/// <param name="ownerChange">whether the owner will change</param>
		public Item Remove(int slot, bool ownerChange)
		{
			if (slot >= m_Items.Length)
			{
				LogUtil.ErrorException(
					new Exception(
						string.Format("Tried to remove Item from invalid Slot {0}/{1} in {2} (belongs to {3})",
						slot, m_Items.Length, this, Owner)));
				return null;
			}

			var item = m_Items[slot];
			if (item != null)
			{
				item.Remove(ownerChange);
			}
			return item;
		}
		/// <summary>
		/// Don't use this method - but use item.Remove instead.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="ownerChange"></param>
		internal void Remove(Item item, bool ownerChange)
		{
			if (ownerChange)
			{
				item.m_unknown = true;
				OwnerInventory.OnRemove(item);
			}

			var slot = item.Slot;
			var handler = GetHandler(slot);
			if (handler != null)
			{
				handler.Removed(slot, item);
			}

			// slot may have changed in the Removed() handler
			slot = item.Slot;
			m_Items[slot] = null;
			var cont = Container;
			if (cont != null)
			{
				cont.SetEntityId(m_baseField + (slot * 2), EntityId.Zero);
			}
			item.Container = null;

			m_count--;
		}

		/// <summary>
		/// Checks for all requirements before destroying the Item in the given slot. 
		/// If it cannot be destroyed, sends error to owner.
		/// </summary>
		public InventoryError TryDestroy(int slot)
		{
			InventoryError err;
			var item = this[slot];
			if (item == null)
			{
				err = InventoryError.ITEM_NOT_FOUND;
			}
			else if (item.IsContainer && ((Container)item).BaseInventory.Count > 0)
			{
				err = InventoryError.CAN_ONLY_DO_WITH_EMPTY_BAGS;
			}
			else if (!item.CanBeTraded)
			{
				err = InventoryError.CANT_DO_RIGHT_NOW;
			}
			else if (!Destroy(slot))
			{
				// Item was not in this Container
				err = InventoryError.DontReport;
			}
			else
			{
				return InventoryError.OK;
			}

			ItemHandler.SendInventoryError(Owner.Client, item, null, err);
			return err;
		}

		/// <summary>
		/// Destroys the item at the given slot
		/// </summary>
		public bool Destroy(int slot)
		{
			var removed = Remove(slot, true);

			if (removed == null)
			{
				return false;
			}
			if (removed is Container)
			{
				var cont = (Container)removed;
				if (!cont.BaseInventory.IsEmpty)
				{
					foreach (var item in cont.BaseInventory)
					{
						item.Destroy();
					}
				}
			}

			removed.DoDestroy();
			return true;
		}

		public InventoryError GetSlots<T>(T[] items, out SimpleSlotId[] slots) where T : IItemStack
		{
			var count = items.Length;
			var err = InventoryError.OK;
			SimpleSlotId slotId;
			var i = 0;
			slots = new SimpleSlotId[count];

			for (; i < count; i++)
			{
				var item = items[i];
				var templ = item.Template;
				var amount = item.Amount;
				CheckUniqueness(templ, ref amount, ref err, true);
				if (err == InventoryError.OK && amount != item.Amount)
				{
					err = InventoryError.CANT_CARRY_MORE_OF_THIS;
					break;
				}
				slotId = FindFreeSlot(templ, amount);
				if (slotId.Slot == INVALID_SLOT)
				{
					// TODO: What if Item can be distributed over existing stacks?
					err = InventoryError.INVENTORY_FULL;
					break;
				}

				// occupy slot with placeholder
				slotId.Container.m_Items[slotId.Slot] = Item.PlaceHolder;
				slots[i] = slotId;
			}

			if (err != InventoryError.OK)
			{
				// remove placeholders
				for (var j = 0; j < i; j++)
				{
					slotId = slots[j];
					slotId.Container.m_Items[slotId.Slot] = null;
				}
				slots = null;
			}
			return err;
		}

		/// <summary>
		/// Adds all given Items to the given slots.
		/// Tries to distribute over existing stacks first.
		/// </summary>
		/// <param name="items"></param>
		/// <returns></returns>
		public void AddAllUnchecked<T>(T[] items, SimpleSlotId[] slots) where T : IItemStack
		{
			var count = items.Length;
			for (var i = 0; i < count; i++)
			{
				var item = items[i];
				var amount = item.Amount;
				if (!Distribute(item.Template, ref amount))
				{
					var slotId = slots[i];
					amount = item.Amount - amount;
					slotId.Container.m_Items[slotId.Slot] = null;
					slotId.Container.AddUnchecked(slotId.Slot, items[i].Template, amount, true);
				}
			}
		}

		/// <summary>
		/// Tries to add all given Items to this Inventory. 
		/// Does not add any if not all could be added.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="items"></param>
		/// <returns></returns>
		public InventoryError TryAddAll<T>(T[] items) where T : IItemStack
		{
			SimpleSlotId[] slots;
			var err = GetSlots(items, out slots);
			if (err == InventoryError.OK)
			{
				AddAllUnchecked(items, slots);
			}
			return err;
		}

		/// <summary>
		/// Tries to distribute Items of the given Template and amount amongst all other stacks of the same Type
		/// </summary>
		/// <param name="amount">Will be set to the amount that has actually been distributed</param>
		/// <returns>whether the complete amount has been fully distributed.</returns>
		public virtual bool Distribute(ItemTemplate template, ref int amount)
		{
			if (template.IsStackable)
			{
				var amountLeft = amount;
				var done = false;
				// add stackable items to existing stacks
				Iterate((invItem) =>
				{
					if (invItem.Template == template)
					{
						var diff = template.MaxAmount - invItem.Amount;
						if (diff > 0)
						{
							if (diff >= amountLeft)
							{
								// we are done
								amountLeft -= invItem.ModAmount(amountLeft);
								done = true;
								return false;
							}

							// we can put a part of the stack here
							amountLeft -= invItem.ModAmount(diff);
						}
					}
					return true;
				});

				amount -= amountLeft;

				return done;
			}

			amount = 0;
			return false;
		}

		/// <summary>
		/// Checks whether Items of the given Template and amount can be distributed 
		/// amongst already existing stacks of the same Type without actually changing anything.
		/// </summary>
		/// <param name="amount">The number of items to try and distribute. 
		/// Will be set to the number of items that would be distributed if a real Distribute is run.</param>
		/// <returns>true if the whole amount can be distributed.</returns>
		public bool CheckDistribute(ItemTemplate template, ref int amount)
		{
			var amountLeft = amount;
			if (template.IsStackable)
			{
				// try to add stackable items to existing stacks
				Iterate(invItem =>
				{
					if (invItem.Template == template)
					{
						if (invItem.Amount < template.MaxAmount)
						{
							var diff = template.MaxAmount - invItem.Amount;
							if (diff < amountLeft)
							{
								// we can put a part of the stack here
								amountLeft -= diff;
							}
							else
							{
								// we are done
								return false;
							}
						}
					}
					return true;
				});
			}

			amount -= amountLeft;
			return (amountLeft == 0);
		}

		/// <summary>
		/// Counts and returns the amount of items in between the given slots.
		/// </summary>
		public int GetCount(int offset, int end)
		{
			var count = 0;
			for (int i = offset; i <= end; i++)
			{
				if (m_Items[i] != null)
					count++;
			}
			return count;
		}

		/// <summary>
		/// Iterates over all Items within this Inventory.
		/// </summary>
		public virtual bool Iterate(Func<Item, bool> iterator)
		{
			Item item;
			for (int i = 0; i < m_Items.Length; i++)
			{
				item = m_Items[i];
				if (item != null)
				{
					if (item is Container)
					{
						var container = (Container)item;
						var contInv = container.BaseInventory;
						if (!contInv.IsEmpty)
						{
							var contItems = contInv.Items;
							for (var j = 0; j < contItems.Length; j++)
							{
								item = contItems[j];
								if (item != null && !iterator(item))
								{
									return false;
								}
							}
						}
					}

					if (!iterator(item))
					{
						return false;
					}
				}
			}
			return true;
		}

		/// <summary>
		/// All items that are currently in this inventory
		/// </summary>
		public IEnumerator<Item> GetEnumerator()
		{
			for (var i = 0; i < m_Items.Length; i++)
			{
				var item = m_Items[i];
				if (item != null)
				{
					yield return item;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}