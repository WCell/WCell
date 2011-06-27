using System;
using System.Collections.Generic;
using NLog;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Looting;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Database;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Items.Enchanting;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.Quests;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.Threading;

namespace WCell.RealmServer.Entities
{
	public partial class Item : ObjectBase, IOwned, IWeapon, INamed, ILockable, IQuestHolder, IMountableItem, IContextHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public static readonly UpdateFieldCollection UpdateFieldInfos = UpdateFieldMgr.Get(ObjectTypeId.Item);

		protected override UpdateFieldCollection _UpdateFieldInfos
		{
			get { return UpdateFieldInfos; }
		}

		public static readonly Item PlaceHolder = new Item();

		protected ItemTemplate m_template;
		protected internal bool m_isInWorld;

		/// <summary>
		/// Items are unknown when a creation update
		/// has not been sent to the Owner yet.
		/// </summary>
		internal bool m_unknown;

		protected internal Character m_owner;
		protected BaseInventory m_container;
		protected ItemEnchantment[] m_enchantments;
		protected IProcHandler m_hitProc;
		protected ItemRecord m_record;

		#region CreateItem
		public static Item CreateItem(uint templateId, Character owner, int amount)
		{
			var template = ItemMgr.GetTemplate(templateId);
			if (template != null)
			{
				return CreateItem(template, owner, amount);
			}
			return null;
		}

		public static Item CreateItem(ItemId templateId, Character owner, int amount)
		{
			var template = ItemMgr.GetTemplate(templateId);
			if (template != null)
			{
				return CreateItem(template, owner, amount);
			}
			return null;
		}

		public static Item CreateItem(ItemTemplate template, Character owner, int amount)
		{
			var item = template.Create();
			item.InitItem(template, owner, amount);
			return item;
		}

		public static Item CreateItem(ItemRecord record, Character owner)
		{
			var template = record.Template;
			if (template == null)
			{
				log.Warn("{0} had an ItemRecord with invalid ItemId: {1}", owner, record);
				return null;
			}

			var item = template.Create();
			item.LoadItem(record, owner, template);
			return item;
		}

		public static Item CreateItem(ItemRecord record, Character owner, ItemTemplate template)
		{
			var item = template.Create();
			item.LoadItem(record, owner, template);
			return item;
		}

		public static Item CreateItem(ItemRecord record, ItemTemplate template)
		{
			var item = template.Create();
			item.LoadItem(record, template);
			return item;
		}
		#endregion

		protected internal Item()
		{
		}

		#region Init & Load
		/// <summary>
		/// Initializes a new Item
		/// </summary>
		internal void InitItem(ItemTemplate template, Character owner, int amount)
		{
			m_record = ItemRecord.CreateRecord();

			EntryId = m_record.EntryId = template.Id;

			Type |= ObjectTypes.Item;

			m_template = template;

			Durability = m_template.MaxDurability;
			MaxDurability = m_template.MaxDurability;
			Flags = m_template.Flags;
			TextId = m_template.PageTextId;
			Amount = amount;
			OwningCharacter = owner;
			EntityId = m_record.EntityId;

			// set charges to max
			if (m_template.UseSpell != null && m_template.UseSpell.HasCharges)
			{
				SpellCharges = (int)m_template.UseSpell.Charges;
			}

			var randomEnchants = m_template.RandomPrefixes;
			if (randomEnchants != null)
			{
				ApplyRandomEnchants(randomEnchants, EnchantSlot.PropSlot2, EnchantSlot.PropSlot4);
			}
			randomEnchants = m_template.RandomSuffixes;
			if (randomEnchants != null)
			{
				if (ApplyRandomEnchants(randomEnchants, EnchantSlot.PropSlot0, EnchantSlot.PropSlot2))
				{
					RandomPropertiesId = m_enchantments[(int)EnchantSlot.PropSlot0].Entry.Id;
				}
			}

			template.NotifyCreated(m_record);
			OnInit();
		}

		/// <summary>
		/// Loads an already created item
		/// </summary>
		internal void LoadItem(ItemRecord record, Character owner, ItemTemplate template)
		{
			m_record = record;
			OwningCharacter = owner;

			LoadItem(record, template);
		}

		/// <summary>
		/// Loads an already created item without owner
		/// </summary>
		/// <param name="record"></param>
		/// <param name="template"></param>
		internal void LoadItem(ItemRecord record, ItemTemplate template)
		{
			m_record = record;
			EntityId = record.EntityId;

			m_template = template;
			EntryId = m_template.Id;

			Type |= ObjectTypes.Item;

			SetUInt32(ItemFields.FLAGS, (uint)record.Flags);
			SetInt32(ItemFields.DURABILITY, record.Durability);
			SetInt32(ItemFields.DURATION, record.Duration);
			SetInt32(ItemFields.STACK_COUNT, record.Amount);
			SetInt32(ItemFields.PROPERTY_SEED, record.RandomSuffix);
			SetInt32(ItemFields.RANDOM_PROPERTIES_ID, record.RandomProperty);
			SetInt64(ItemFields.CREATOR, record.CreatorEntityId);
			SetInt64(ItemFields.GIFTCREATOR, record.GiftCreatorEntityId);

			ItemText = record.ItemText;

			if (m_template.UseSpell != null)
			{
				SetSpellCharges(m_template.UseSpell.Index, (int)record.Charges);
			}
			MaxDurability = m_template.MaxDurability;

			// add enchants
			if (record.EnchantIds != null)
			{
				for (var enchSlot = 0; enchSlot < record.EnchantIds.Length; enchSlot++)
				{
					var enchant = record.EnchantIds[enchSlot];
					if (enchSlot == (int)EnchantSlot.Temporary)
					{
						ApplyEnchant(enchant, (EnchantSlot)enchSlot, record.EnchantTempTime, 0, false);
					}
					else
					{
						ApplyEnchant(enchant, (EnchantSlot)enchSlot, 0, 0, false);
					}
				}
				//item.CheckSocketColors();
			}

			OnLoad();
		}

		/// <summary>
		/// Called after initializing a newly created Item (Owner might be null)
		/// </summary>
		protected virtual void OnInit()
		{
		}

		/// <summary>
		/// Called after loading an Item (Owner might be null)
		/// </summary>
		protected virtual void OnLoad()
		{
		}
		#endregion

		#region Properties
		public ItemTemplate Template
		{
			get { return m_template; }
		}

		public LockEntry Lock
		{
			get
			{
				return m_template.Lock;
			}
		}

		public override bool IsInWorld
		{
			get { return m_isInWorld; }
		}

		/// <summary>
		/// Whether this object has already been deleted.
		/// </summary>
		public bool IsDeleted
		{
			get;
			internal set;
		}

		/// <summary>
		/// Checks whether this Item can currently be used
		/// </summary>
		public bool CanBeUsed
		{
			get { return (MaxDurability == 0 || Durability > 0) && m_loot == null; }
		}

		/// <summary>
		/// The name of this item
		/// </summary>
		public string Name
		{
			get
			{
				if (m_template != null)
				{
					return m_template.DefaultName;
				}
				return "";
			}
		}

		public override ObjectTypeId ObjectTypeId
		{
			get { return ObjectTypeId.Item; }
		}

		public bool IsContainer
		{
			get { return ObjectTypeId == ObjectTypeId.Container; }
		}

		public bool CanBeTraded
		{
			get
			{
				if (IsContainer && !((Container)this).BaseInventory.IsEmpty)
				{
					return false;
				}
				if (Owner != null && Owner is Character)
				{
					var chr = (Character)Owner;
					if (chr.IsLooting && chr.Loot.Lootable.EntityId == EntityId)
					{
						// item is currently being looted (eg a strongbox etc)
						return false;
					}
				}
				return m_template.MaxDurability == 0 || Durability > 0;
			}
		}
		/// <summary>
		/// See IUsable.Owner
		/// </summary>
		public Unit Owner
		{
			get { return m_owner; }
		}

		/// <summary>
		/// Whether this Item is currently equipped.
		/// </summary>
		public bool IsEquipped
		{
			get
			{
				return m_container == m_owner.Inventory && m_record.Slot <= (int)InventorySlot.BagLast;
			}
		}

		/// <summary>
		/// Whether this Item is currently equipped and is not a kind of container.
		/// </summary>
		public bool IsEquippedItem
		{
			get
			{
				if (m_container != null)
					return m_container == m_owner.Inventory && m_record.Slot < (int)InventorySlot.Bag1;
				else
					return false;
			}
		}

		/// <summary>
		/// Whether this is a Container and it is currently
		/// equipped or in a bankbag slot (so Items can be put into it).
		/// </summary>
		public bool IsEquippedContainer
		{
			get { return m_container == m_owner.Inventory && ItemMgr.ContainerSlotsWithBank[Slot]; }
		}

		/// <summary>
		/// Wheter this item's bonuses are applied
		/// </summary>
		public bool IsApplied
		{
			get;
			private set;
		}

		public bool IsBuyback
		{
			get
			{
				return m_record.Slot >= (int)InventorySlot.BuyBack1 &&
					   m_record.Slot <= (int)InventorySlot.BuyBackLast &&
					   m_container == m_owner.Inventory;
			}
		}

		public InventorySlotTypeMask InventorySlotMask
		{
			get { return m_template.InventorySlotMask; }
		}
		#endregion

		/// <summary>
		/// Called when this Item was added to someone's inventory
		/// </summary>
		protected internal void OnAdd()
		{
			if (m_template.BondType == ItemBondType.OnPickup ||
				m_template.BondType == ItemBondType.Quest)
			{
				Flags |= ItemFlags.Soulbound;
			}

			// The container information was updated during the container's add function
			m_owner = m_container.Owner;

			// send enchant information to owner
			for (var slot = (EnchantSlot)0; slot < EnchantSlot.End; slot++)
			{
				var enchant = GetEnchantment(slot);
				if (enchant != null)
				{
					OnOwnerReceivedNewEnchant(enchant);
				}
			}
		}

		/// <summary>
		/// Saves all recent changes that were made to this Item to the DB
		/// </summary>
		public void Save()
		{
			if (IsDeleted)
			{
				LogUtil.ErrorException(new InvalidOperationException("Trying to save deleted Item: " + this));
				return;
			}

			m_record.SaveAndFlush();
		}

		/// <summary>
		/// Subtracts the given amount from this item and creates a new item, with that amount.
		/// WARNING: Make sure that this item is belonging to someone and that amount is valid!
		/// </summary>
		/// <param name="amount">The amount of the newly created item</param>
		public Item Split(int amount)
		{
			Amount -= amount;
			return CreateItem(m_template, OwningCharacter, amount);
		}

		/// <summary>
		/// Creates a new Item of this type with the given amount.
		/// Usually only used on stackable Items that do not have individual
		/// properties (like durability, enchants etc).
		/// WARNING: Make sure that this item is belonging to someone and that amount is valid!
		/// </summary>
		/// <param name="amount">The amount of the newly created item</param>
		public Item CreateNew(int amount)
		{
			return CreateItem(m_template, OwningCharacter, amount);
		}

		/// <summary>
		/// TODO: Random properties
		/// </summary>
		public bool CanStackWith(Item otherItem)
		{
			return m_template.IsStackable && m_template == otherItem.m_template;
		}

		/// <summary>
		/// A chest was looted empty
		/// </summary>
		public override void OnFinishedLooting()
		{
			Destroy();
		}

		public override uint GetLootId(LootEntryType type)
		{
			return m_template.Id;
		}

		#region Enchanting
		/// <summary>
		/// All applied Enchantments.
		/// Could return null if it doesn't have any.
		/// </summary>
		public ItemEnchantment[] Enchantments
		{
			get { return m_enchantments; }
		}

		public bool HasGems
		{
			get
			{
				if (m_enchantments != null && m_template.HasSockets)
				{
					for (var i = 0; i < ItemConstants.MaxSocketCount; i++)
					{
						if (m_enchantments[i + (int)EnchantSlot.Socket1] != null)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public bool HasGem(ItemId id)
		{
			if (m_enchantments != null && m_template.HasSockets)
			{
				for (var i = EnchantSlot.Socket1; i <= EnchantSlot.Socket1 + ItemConstants.MaxSocketCount; i++)
				{
					if (m_enchantments[(int)i] != null &&
						m_enchantments[(int)i].Entry.GemTemplate != null &&
						m_enchantments[(int)i].Entry.GemTemplate.ItemId == id)
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool IsEnchanted
		{
			get
			{
				return m_enchantments[(int)EnchantSlot.Permanent] != null ||
					   m_enchantments[(int)EnchantSlot.Temporary] != null;
			}
		}

		public IEnumerable<ItemEnchantment> GetAllEnchantments()
		{
			for (var slot = (EnchantSlot)0; slot < EnchantSlot.End; slot++)
			{
				var enchant = GetEnchantment(slot);
				if (enchant != null)
				{
					yield return enchant;
				}
			}
		}

		private static int GetEnchantSlot(EnchantSlot slot, EnchantInfoOffset offset)
		{
			return (int)ItemFields.ENCHANTMENT_1_1 + ((int)slot * 3) + (int)offset;
		}

		public void SetEnchantId(EnchantSlot slot, uint value)
		{
			var enchBase = GetEnchantSlot(slot, EnchantInfoOffset.Id);

			SetUInt32(enchBase, value);
		}

		public void SetEnchantDuration(EnchantSlot slot, int value)
		{
			var enchBase = GetEnchantSlot(slot, EnchantInfoOffset.Duration);

			SetInt32(enchBase + 1, value);
		}

		public void SetEnchantCharges(EnchantSlot slot, int value)
		{
			var enchBase = GetEnchantSlot(slot, EnchantInfoOffset.Charges);

			SetInt32(enchBase + 2, value);
		}

		/// <summary>
		/// The time until the given Enchantment expires or <see cref="TimeSpan.Zero"/> if not temporary
		/// </summary>
		/// <param name="enchantSlot"></param>
		/// <returns></returns>
		public TimeSpan GetRemainingEnchantDuration(EnchantSlot enchantSlot)
		{
			return m_enchantments[(uint)enchantSlot].RemainingTime;
		}

		//public void ApplyRandomProps(BaseItemRandomPropertyInfo info)
		//{
		//    if (info.Chance < Utility.Random(0f, 100f))
		//    {
		//        if (info is ItemRandomSuffixInfo)
		//        {
		//            ApplyRandomProps((ItemRandomSuffixInfo)info);
		//        }
		//        else
		//        {
		//            ApplyRandomProps((ItemRandomEnchantEntry)info);
		//        }
		//    }
		//}

		///// <summary>
		///// NYI
		///// </summary>
		///// <param name="info"></param>
		//public void ApplyRandomProps(ItemRandomSuffixInfo info)
		//{

		//}

		///// <summary>
		///// NYI
		///// </summary>
		///// <param name="entry"></param>
		//public void ApplyRandomProps(ItemRandomEnchantEntry entry)
		//{

		//}

		void EnsureEnchantments()
		{
			if (m_enchantments == null)
			{
				m_enchantments = new ItemEnchantment[(int)EnchantSlot.End];
			}
		}

		public ItemEnchantment GetEnchantment(EnchantSlot slot)
		{
			if (m_enchantments == null)
			{
				return null;
			}
			return m_enchantments[(uint)slot];
		}

		public void ApplyEnchant(int enchantEntryId, EnchantSlot enchantSlot, int duration, int charges, bool applyBoni)
		{
			if (enchantEntryId != 0)
			{
				var enchant = EnchantMgr.GetEnchantmentEntry((uint)enchantEntryId);
				if (enchant != null)
				{
					ApplyEnchant(enchant, enchantSlot, duration, charges, applyBoni);
				}
			}
		}

		/// <summary>
		/// Adds a new the <see cref="ItemEnchantment"/> to the given Slot.
		/// Will remove any existing Enchantment in that slot.
		/// </summary>
		/// <param name="enchantSlot"></param>
		public void ApplyEnchant(ItemEnchantmentEntry enchantEntry,
			EnchantSlot enchantSlot,
			int duration,
			int charges,
			bool applyBoni)
		{
			// TODO: Add charges
			if (m_enchantments == null)
			{
				m_enchantments = new ItemEnchantment[(int)EnchantSlot.End];
			}

			if (m_enchantments[(int)enchantSlot] != null)
			{
				RemoveEnchant(enchantSlot);
			}

			var enchant = new ItemEnchantment(enchantEntry, enchantSlot, DateTime.Now, duration);
			m_enchantments[(int)enchantSlot] = enchant;
			m_record.SetEnchant(enchantSlot, (int)enchant.Entry.Id, duration);

			SetEnchantId(enchantSlot, enchantEntry.Id);
			SetEnchantDuration(enchantSlot, duration);
			if (charges > 0)
			{
				SetEnchantCharges(enchantSlot, charges - 1);
			}

			var owner = OwningCharacter;

			if (owner != null)
			{
				EnchantMgr.ApplyEnchantToItem(this, enchant);

				if (enchant.Entry.GemTemplate != null)
				{
					owner.Inventory.ModUniqueCount(enchant.Entry.GemTemplate, 1);
				}
				OnOwnerReceivedNewEnchant(enchant);

				if (applyBoni && IsEquippedItem)
				{
					// render on Character and apply boni
					SetEnchantEquipped(enchant);
				}
			}
		}

		/// <summary>
		/// Called when owner learns about new enchant:
		/// When enchant gets added and when receiving an enchanted item
		/// </summary>
		private void OnOwnerReceivedNewEnchant(ItemEnchantment enchant)
		{
			var owner = OwningCharacter;
			ItemHandler.SendEnchantLog(owner, (ItemId)EntryId, enchant.Entry.Id);

			if (enchant.Duration != 0)
			{
				var timeLeft = (int)enchant.RemainingTime.TotalMilliseconds;
				owner.CallDelayed(timeLeft, obj =>
				{
					if (!IsDeleted && Owner == owner)
					{
						RemoveEnchant(enchant);
					}
				});
				ItemHandler.SendEnchantTimeUpdate(owner, this, enchant.Duration);
			}
		}

		/// <summary>
		/// Removes the <see cref="ItemEnchantment"/> from the given Slot.
		/// </summary>
		/// <param name="enchantSlot"></param>
		public void RemoveEnchant(EnchantSlot enchantSlot)
		{
			ItemEnchantment enchantment;

			if (m_enchantments == null || (enchantment = m_enchantments[(int)enchantSlot]) == null)
			{
				log.Error("Tried to remove Enchantment from unoccupied EnchantmentSlot {0} on Item {1}", enchantSlot, this);
				return;
			}
			RemoveEnchant(enchantment);
		}

		public void RemoveEnchant(ItemEnchantment enchant)
		{
			m_enchantments[(int)enchant.Slot] = null;
			m_record.SetEnchant(enchant.Slot, 0, 0);

			var owner = OwningCharacter;
			if (owner != null)
			{
				EnchantMgr.RemoveEnchantFromItem(this, enchant);
				if (IsEquipped)
				{
					SetEnchantUnequipped(enchant);
				}
				if (enchant.Entry.GemTemplate != null)
				{
					owner.Inventory.ModUniqueCount(enchant.Entry.GemTemplate, -1);
				}
			}
		}

		/// <summary>
		/// Applies the given gems to this Item. 
		/// Each gem will be matched to the socket of the same index.
		/// </summary>
		/// <param name="gems"></param>
		public void ApplyGems<T>(T[] gems)
			where T : class, IMountableItem
		{
			if (CheckGems(gems))
			{
				EnsureEnchantments();
				var matchingColors = true;
				for (var i = 0u; i < gems.Length; i++)
				{
					var gem = gems[i];
					if (gem != null && gem.Template.GemProperties != null)
					{
						if (gem is Item && ((Item)(IMountableItem)gem).IsDeleted)
						{
							return;
						}
						// add new Gem at this index
						var socket = m_template.Sockets.Get(i);
						if (socket.Color != 0)
						{
							ApplyEnchant(gem.Template.GemProperties.Enchantment, EnchantSlot.Socket1 + i, 0, 0, true);
							if (gem is Item)
							{
								// destroy the newly applied Gem
								((Item)(IMountableItem)gem).Destroy();
							}
							matchingColors = matchingColors && gem.Template.GemProperties.Color.HasAnyFlag(socket.Color);
						}
					}
					else
					{
						// check for matching colors with already applied Enchant at this index
						var socket = m_template.Sockets.Get(i);
						matchingColors = matchingColors &&
							(socket.Color == 0 ||
							(m_enchantments[(uint)EnchantSlot.Socket1 + i] != null &&
							(m_enchantments[(uint)EnchantSlot.Socket1 + i].Entry.GemTemplate.GemProperties.Color.HasAnyFlag(socket.Color))));
					}
				}

				if (matchingColors)
				{
					if (GetEnchantment(EnchantSlot.Bonus) == null)
					{
						ApplyEnchant(m_template.SocketBonusEnchant, EnchantSlot.Bonus, 0, 0, true);
					}
				}
				else
				{
					if (GetEnchantment(EnchantSlot.Bonus) != null)
					{
						RemoveEnchant(EnchantSlot.Bonus);
					}
				}
			}
		}

		/// <summary>
		/// Applies a set of random enchants in the prop slots between from and to
		/// </summary>
		public bool ApplyRandomEnchants(List<ItemRandomEnchantEntry> entries, EnchantSlot from, EnchantSlot to)
		{
			var slot = from;
			if (m_enchantments != null)
			{
				while (m_enchantments[(int)slot] != null && m_enchantments.Length > (int)++slot) { }

				if (slot > to)
				{
					// no more free slots
					return false;
				}
			}

			var applied = false;
			foreach (var entry in entries)
			{
				if (Utility.Random(0, 100f) < entry.ChancePercent)
				{
					var enchant = EnchantMgr.GetEnchantmentEntry(entry.EnchantId);
					if (enchant != null)
					{
						ApplyEnchant(enchant, slot, 0, 0, true);
						applied = true;
						// ReSharper disable PossibleNullReferenceException
						while (m_enchantments[(int)slot] != null && ++slot <= to) { }
						// ReSharper restore PossibleNullReferenceException

						if (slot > to)
						{
							// no more free slots
							return true;
						}
					}
				}
			}
			return applied;
		}

		/// <summary>
		/// Activate bonus enchant if all sockets have matching gems.
		/// </summary>
		internal void CheckSocketColors()
		{
			if (m_template.HasSockets && m_template.SocketBonusEnchant != null)
			{
				var matchingColors = true;
				for (var i = 0u; i < ItemConstants.MaxSocketCount; i++)
				{
					var socket = m_template.Sockets.Get(i);
					matchingColors = matchingColors &&
									 (socket.Color == 0 ||
									  (m_enchantments[(uint)EnchantSlot.Socket1 + i] != null &&
									   (m_enchantments[(uint)EnchantSlot.Socket1 + i].Entry.GemTemplate.GemProperties.Color &
										socket.Color) != 0));
				}

				if (matchingColors)
				{
					if (GetEnchantment(EnchantSlot.Bonus) == null)
					{
						// apply bonus Enchant
						ApplyEnchant(m_template.SocketBonusEnchant, EnchantSlot.Bonus, 0, 0, false);
					}
				}
				else
				{
					if (GetEnchantment(EnchantSlot.Bonus) != null)
					{
						// remove bonus Enchant
						RemoveEnchant(EnchantSlot.Bonus);
					}
				}
			}
		}

		/// <summary>
		/// Check whether the given gems match the color of the socket of the corresponding index within
		/// the gems-array.
		/// Check for unique count.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="gems"></param>
		/// <returns></returns>
		private bool CheckGems<T>(T[] gems)
			where T : class, IMountableItem
		{
			for (var i = 0u; i < gems.Length; i++)
			{
				var gem = gems[i];
				if (gem != null)
				{
					var socket = m_template.Sockets.Get(i);
					if (socket.Color != SocketColor.None)
					{
						// only allow color gems in color sockets and meta gems in meta socket
						if ((socket.Color == SocketColor.Meta) != (gem.Template.GemProperties.Color == SocketColor.Meta))
						{
							return false;
						}

						if (IsEquipped)
						{
							// check uniqueness
							if (!m_owner.Inventory.CheckEquippedGems(gem.Template))
							{
								ItemHandler.SendInventoryError(m_owner, this, null, InventoryError.ITEM_MAX_COUNT_EQUIPPED_SOCKETED);
								return false;
							}
						}
					}
				}
			}
			return true;
		}

		void SetEnchantEquipped(ItemEnchantment enchant)
		{
			if (enchant.Slot == EnchantSlot.Permanent)
			{
				Owner.SetUInt16Low(
					PlayerFields.VISIBLE_ITEM_1_ENCHANTMENT + (Slot * ItemConstants.PlayerFieldVisibleItemSize), (ushort)enchant.Entry.Id);
			}
			else if (enchant.Slot == EnchantSlot.Temporary)
			{
				Owner.SetUInt16High(
						PlayerFields.VISIBLE_ITEM_1_ENCHANTMENT + (Slot * ItemConstants.PlayerFieldVisibleItemSize), (ushort)enchant.Entry.Id);
			}

			for (var i = 0; i < enchant.Entry.Effects.Length; i++)
			{
				EnchantMgr.ApplyEquippedEffect(this, enchant.Entry.Effects[i]);
			}
		}

		void SetEnchantUnequipped(ItemEnchantment enchant)
		{
			if (enchant.Slot == EnchantSlot.Permanent)
			{
				Owner.SetUInt16Low(
					PlayerFields.VISIBLE_ITEM_1_ENCHANTMENT + (Slot * ItemConstants.PlayerFieldVisibleItemSize), 0);
			}
			else if (enchant.Slot == EnchantSlot.Temporary)
			{
				Owner.SetUInt16High(
						PlayerFields.VISIBLE_ITEM_1_ENCHANTMENT + (Slot * ItemConstants.PlayerFieldVisibleItemSize), 0);
			}

			for (var i = 0; i < enchant.Entry.Effects.Length; i++)
			{
				EnchantMgr.RemoveEffect(this, enchant.Entry.Effects[i]);
			}
		}
		#endregion

		#region Equipping
		/// <summary>
		/// Tries to equip this Item
		/// </summary>
		/// <returns></returns>
		public InventoryError Equip()
		{
			return m_owner.Inventory.TryEquip(m_container, Slot);
		}

		public bool Unequip()
		{
			var inv = m_owner.Inventory;
			var slotId = inv.FindFreeSlot(this, Amount);
			if (slotId.Slot == BaseInventory.INVALID_SLOT)
			{
				return false;
			}

			inv.SwapUnchecked(m_container, Slot, slotId.Container, slotId.Slot);
			return true;
		}

		public InventoryError CheckEquip(Character user)
		{
			var err = m_template.CheckEquip(user);
			//if (err == InventoryError.OK)
			//{
			//    if (m_enchantments != null)
			//    {
			//        for (var i = 0u; i < ItemConstants.MaxSocketCount; i++)
			//        {
			//            var enchant = m_enchantments[i + (uint)EnchantSlot.Socket1];
			//            if (enchant != null)
			//            {
			//                err = enchant.Entry.GemTemplate.CheckEquip(user);
			//                if (err != InventoryError.OK)
			//                {
			//                    return err;
			//                }
			//            }
			//        }
			//    }
			//}
			return err;
		}

		internal void OnEquipDecision()
		{
			// weapon handling
			if (m_template.IsWeapon)
			{
				var slot = (InventorySlot)Slot;

				switch (slot)
				{
					case InventorySlot.MainHand:
						m_owner.MainWeapon = this;
						return;
					case InventorySlot.OffHand:
						m_owner.OffHandWeapon = this;
						return;
					case InventorySlot.ExtraWeapon:
						m_owner.RangedWeapon = this;
						return;
				}
			}
			OnEquip();
		}

		internal void OnUnequipDecision(InventorySlot slot)
		{
			// weapon handling
			if (m_template.IsWeapon)
			{
				switch (slot)
				{
					case InventorySlot.MainHand:
						m_owner.MainWeapon = null;
						return;
					case InventorySlot.OffHand:
						m_owner.OffHandWeapon = null;
						return;
					case InventorySlot.ExtraWeapon:
						m_owner.RangedWeapon = null;
						return;
				}
			}
			OnUnEquip(slot);
		}

		/// <summary>
		/// Called when this Item gets equipped.
		/// Requires map context.
		/// </summary>
		public void OnEquip()
		{
			if (IsApplied) return;
			IsApplied = true;

			var slot = (InventorySlot)Slot;
			var chr = OwningCharacter;
			if (slot < InventorySlot.Bag1 && !m_template.IsAmmo)
			{
				chr.SetVisibleItem(slot, this);
			}
			m_template.ApplyStatMods(chr);

			// binding
			if (m_template.BondType == ItemBondType.OnEquip)
			{
				Flags |= ItemFlags.Soulbound;
			}

			// spell casting
			if (chr.IsUsingSpell)
			{
				// equipping items cancels SpellCasts
				chr.SpellCast.Cancel();
			}

			// Apply resistance boni
			for (var i = 0; i < m_template.Resistances.Length; i++)
			{
				var res = m_template.Resistances[i];
				if (res > 0)
				{
					chr.ModBaseResistance((DamageSchool)i, res);
				}
			}
			foreach (var enchant in GetAllEnchantments())
			{
				foreach (var effect in enchant.Entry.Effects)
				{
					if (effect.Type == ItemEnchantmentType.Resistance)
					{
						// add resistances
						chr.ModBaseResistance((DamageSchool)effect.Misc, effect.MaxAmount);
					}
				}
			}

			// ammo
			if (slot == InventorySlot.Invalid)
			{
				// ammo
				chr.UpdateRangedDamage();
			}

			// block rating
			else if (m_template.InventorySlotType == InventorySlotType.Shield)
			{
				chr.UpdateBlockChance();
			}

			// cast spells
			if (m_template.EquipSpells != null)
			{
				chr.SpellCast.TriggerAll(chr, m_template.EquipSpells);
			}

			// add procs
			if (m_template.HitSpells != null)
			{
				foreach (var spell in m_template.HitSpells)
				{
					chr.AddProcHandler(m_hitProc = new ItemHitProcHandler(this, spell));
				}
			}

			// set boni
			if (m_template.Set != null)
			{
				var setCount = chr.Inventory.GetSetCount(m_template.Set);
				var boni = m_template.Set.Boni.Get(setCount - 1);
				if (boni != null)
				{
					chr.SpellCast.TriggerAll(chr, boni);
				}
			}

			// enchants
			if (m_enchantments != null)
			{
				foreach (var enchant in m_enchantments)
				{
					if (enchant != null)
					{
						SetEnchantEquipped(enchant);
					}
				}
			}

			m_owner.PlayerAuras.OnEquip(this);
			if (m_owner.Inventory.m_ItemEquipmentEventHandlers != null)
			{
				foreach (var handler in m_owner.Inventory.m_ItemEquipmentEventHandlers)
				{
					handler.OnEquip(this);
				}
			}

			m_template.NotifyEquip(this);
		}

		/// <summary>
		/// Called when this Item gets unequipped.
		/// Requires map context.
		/// </summary>
		public void OnUnEquip(InventorySlot slot)
		{
			if (!IsApplied) return;
			IsApplied = false;

			if (!m_template.IsAmmo)
			{
				m_owner.SetVisibleItem(slot, null);
			}
			m_template.RemoveStatMods(m_owner);

			// remove triggered buffs
			if (m_template.EquipSpells != null)
			{
				foreach (var spell in m_template.EquipSpells)
				{
					if (spell.IsAura)
					{
						m_owner.Auras.Remove(spell);
					}
				}
			}

			// remove procs
			if (m_template.HitSpells != null)
			{
				foreach (var spell in m_template.HitSpells)
				{
					m_owner.RemoveProcHandler(spell.SpellId);
				}
			}

			// resistances
			for (var i = 0; i < m_template.Resistances.Length; i++)
			{
				var res = m_template.Resistances[i];
				if (res > 0)
				{
					m_owner.ModBaseResistance((DamageSchool)i, -res);
				}
			}

			// ammo
			if (slot == InventorySlot.Invalid)
			{
				// ammo
				m_owner.UpdateRangedDamage();
			}

			// block rating
			else if (m_template.InventorySlotType == InventorySlotType.Shield)
			{
				m_owner.UpdateBlockChance();
			}

			// set boni
			if (m_template.Set != null)
			{
				var setCount = m_owner.Inventory.GetSetCount(m_template.Set);
				var boni = m_template.Set.Boni.Get(setCount - 1);
				if (boni != null)
				{
					foreach (var bonus in boni)
					{
						var aura = m_owner.Auras[bonus, true];
						if (aura != null)
						{
							aura.Remove(false);
						}
					}
				}
			}

			// enchants
			if (m_enchantments != null)
			{
				foreach (var enchant in m_enchantments)
				{
					if (enchant != null)
					{
						SetEnchantUnequipped(enchant);
					}
				}
			}

			// hit proc
			if (m_hitProc != null)
			{
				m_owner.RemoveProcHandler(m_hitProc);
				m_hitProc = null;
			}

			m_owner.PlayerAuras.OnBeforeUnEquip(this);
			if (m_owner.Inventory.m_ItemEquipmentEventHandlers != null)
			{
				foreach (var handler in m_owner.Inventory.m_ItemEquipmentEventHandlers)
				{
					handler.OnBeforeUnEquip(this);
				}
			}

			m_template.NotifyUnequip(this);
		}
		#endregion

		#region Using
		/// <summary>
		/// Called whenever an item is used.
		/// Make sure to only call on Items whose Template has a UseSpell.
		/// </summary>
		internal void OnUse()
		{
			if (m_template.BondType == ItemBondType.OnUse)
			{
				Flags |= ItemFlags.Soulbound;
			}

			if (m_template.UseSpell != null)
			{
				// consume a charge
				if (m_template.Class == ItemClass.Consumable || m_template.Class == ItemClass.Miscellaneous
					|| m_template.Class == ItemClass.Glyph || m_template.Class == ItemClass.Recipe
					|| m_template.Class == ItemClass.TradeGoods)
				{
					SpellCharges = SpellCharges < 0 ? SpellCharges++ : SpellCharges--;
				}
			}

			m_template.NotifyUsed(this);
		}

		#endregion

		#region Destroy / Remove
		/// <summary>
		/// Destroys the Item without further checks.
		/// Also destroys all contained Items if this is a Container.
		/// </summary>
		public void Destroy()
		{
			if (m_container != null && m_container.IsValidSlot(Slot))
			{
				m_container.Destroy(Slot);
			}
			else
			{
				DoDestroy();
			}
		}

		/// <summary>
		/// Called by the container to 
		/// </summary>
		protected internal virtual void DoDestroy()
		{
			var record = m_record;
			m_owner.Inventory.OnAmountChanged(this, -Amount);
			if (record != null)
			{
				record.OwnerId = 0;
				record.DeleteLater();
				m_record = null;

				Dispose();
			}
		}

		/// <summary>
		/// Removes this Item from its old Container (if it was added to any).
		/// After calling this method,
		/// make sure to either Dispose the item after removing (in this case you can also simply use <see cref="Destroy"/>
		/// or re-add it somewhere else.
		/// </summary>
		public void Remove(bool ownerChange)
		{
			if (m_container != null)
			{
				m_container.Remove(this, ownerChange);
			}
		}
		#endregion

		#region QuestHolder


		public QuestHolderInfo QuestHolderInfo
		{
			get { return m_template.QuestHolderInfo; }
		}

		public bool CanGiveQuestTo(Character chr)
		{
			return m_owner == chr;
		}

		public void OnQuestGiverStatusQuery(Character chr)
		{
			// do nothing
		}
		#endregion

		public override void Dispose(bool disposing)
		{
			m_owner = null;
			m_isInWorld = false;
			IsDeleted = true;
		}

		public override string ToString()
		{
			return string.Format("{0}{1} in Slot {4} (Templ: {2}, Id: {3})",
				Amount != 1 ? Amount + "x " : "", Template.DefaultName, m_template.Id, EntityId, Slot);
		}

		public bool IsInContext
		{
			get
			{
				var owner = Owner;
				if (owner != null)
				{
					var context = owner.ContextHandler;
					if (context != null)
					{
						return context.IsInContext;
					}
				}
				return false;
			}
		}

		public void AddMessage(IMessage message)
		{
			var owner = Owner;
			if (owner != null)
			{
				owner.AddMessage(message);
			}
		}

		public void AddMessage(Action action)
		{
			var owner = Owner;
			if (owner != null)
			{
				owner.AddMessage(action);
			}
		}

		public bool ExecuteInContext(Action action)
		{
			var owner = Owner;
			if (owner != null)
			{
				return owner.ExecuteInContext(action);
			}
			return false;
		}

		public void EnsureContext()
		{
			var owner = Owner;
			if (owner != null)
			{
				owner.EnsureContext();
			}
		}
	}
}