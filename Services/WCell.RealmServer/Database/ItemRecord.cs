using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Queries;
using NHibernate.Criterion;
using WCell.Util.Logging;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.Database;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.RealmServer.Items.Enchanting;
using WCell.Util;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Database
{
	/// <summary>
	/// The DB-representation of an Item
	/// TODO: Charges
	/// </summary>
	[ActiveRecord(Access = PropertyAccess.Property)]
	public class ItemRecord : WCellRecord<ItemRecord>
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		static readonly Order ContOrder = new Order("ContainerSlots", false);
		private static readonly NHIdGenerator _idGenerator =
			new NHIdGenerator(typeof(ItemRecord), "Guid");

		/// <summary>
		/// Returns the next unique Id for a new Item
		/// </summary>
		public static long NextId()
		{
			return _idGenerator.Next();
		}

		internal static ItemRecord CreateRecord()
		{
			try
			{
				var itemRecord = new ItemRecord
				{
					Guid = (uint)_idGenerator.Next(),
					State = RecordState.New
				};

				//s_log.Debug("creating new item with EntityId {0}", itemRecord.EntityId);
				//itemRecord.ItemRecordGuid = Guid.NewGuid();
				//itemRecord.EntityId = EntityIdSetter.GetItemEntityId();

				return itemRecord;
			}
			catch (Exception ex)
			{
				throw new WCellException(ex, "Unable to create new ItemRecord.");
			}
		}

		void InitItemRecord()
		{
			var cfg = ActiveRecordMediator.GetSessionFactoryHolder().GetConfiguration(typeof(ActiveRecordBase));
			// cfg.SetListener(MyIPostLoadEventListener);
		}

		[Field("EntryId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _entryId;
		[Field("DisplayId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _displayId;
		[Field("ContainerSlot", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private byte _containerSlot;
		[Field("ItemFlags", NotNull = true)]
		private int flags;

		[Property(NotNull = true)]
		public int OwnerId
		{
			get;
			set;
		}

		[PrimaryKey(PrimaryKeyType.Assigned, "EntityLowId")]
		public long Guid
		{
			get;
			set;
		}

		public uint EntityLowId
		{
			get
			{
				return (uint)Guid;
			}
			set
			{
				Guid = value;
			}
		}

		public uint EntryId
		{
			get
			{
				return (uint)_entryId;
			}
			set
			{
				_entryId = (int)value;
			}
		}

		public EntityId EntityId
		{
			get
			{
				//return EntityId.GetItemId(EntityLowId, (ItemId)EntryId);
				return EntityId.GetItemId(EntityLowId);
			}
		}

		public uint DisplayId
		{
			get
			{
				return (uint)_displayId;
			}
			set
			{
				_displayId = (int)value;
			}
		}

		/// <summary>
		/// The slot of the container, holding this Item
		/// </summary>
		public byte ContainerSlot
		{
			get
			{
				return _containerSlot;
			}
			set
			{
				_containerSlot = value;
			}
		}

		[Property(NotNull = true)]
		public int Slot
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public long CreatorEntityId
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public long GiftCreatorEntityId
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public int Durability
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public int Duration
		{
			get;
			set;
		}

		public ItemFlags Flags
		{
			get { return (ItemFlags)flags; }
			set { flags = (int)value; }
		}

		[Field("ItemTextId", Access = PropertyAccess.FieldCamelcase)]
		private int m_ItemTextId;

		public uint ItemTextId
		{
			get { return (uint)m_ItemTextId; }
			set { m_ItemTextId = (int)value; }
		}

		[Property]
		public string ItemText
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public int RandomProperty
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public int RandomSuffix
		{
			get;
			set;
		}

		/// <summary>
		/// Charges of the Use-spell
		/// </summary>
		[Property(NotNull = true)]
		public short Charges
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public int Amount
		{
			get;
			set;
		}

		public bool IsContainer
		{
			get
			{
				return ContSlots > 0;
			}
		}

		public bool IsEquippedContainer
		{
			get
			{
				return ContSlots > 0 &&
					_containerSlot == 0xFF &&
					ItemMgr.ContainerSlotsWithBank[Slot];
			}
		}

		public bool IsSoulbound
		{
			get { return Flags.HasFlag(ItemFlags.Soulbound); }
		}

		/// <summary>
		/// If this > 0, we have a container with this amount of slots.
		/// </summary>
		[Property(NotNull = true)]
		public int ContSlots
		{
			get;
			set;
		}

		[Property]
		public int[] EnchantIds
		{
			get;
			set;
		}

		[Property]
		public int EnchantTempTime
		{
			get;
			set;
		}

		public ItemEnchantmentEntry GetEnchant(EnchantSlot slot)
		{
			if (EnchantIds != null)
			{
				return EnchantMgr.GetEnchantmentEntry((uint)EnchantIds[(int)slot]);
			}
			return null;
		}

		internal void SetEnchant(EnchantSlot slot, int id, int timeLeft)
		{
			if (EnchantIds == null)
			{
				EnchantIds = new int[(int)EnchantSlot.End];
			}
			EnchantIds[(int)slot] = id;
			if (slot == EnchantSlot.Temporary)
			{
				EnchantTempTime = timeLeft;
			}
			//switch (slot)
			//{
			//    case EnchantSlot.Permanent:
			//        EnchantPerm = id;
			//        break;
			//    case EnchantSlot.Temporary:
			//        EnchantTemp = id;
			//        EnchantTempTime = timeLeft;
			//        break;
			//    case EnchantSlot.Socket1:
			//        EnchantSock1 = id;
			//        break;
			//    case EnchantSlot.Socket2:
			//        EnchantSock2 = id;
			//        break;
			//    case EnchantSlot.Socket3:
			//        EnchantSock3 = id;
			//        break;
			//    case EnchantSlot.PropSlot0:
			//        EnchantProp0 = id;
			//        break;
			//    case EnchantSlot.PropSlot1:
			//        EnchantProp1 = id;
			//        break;
			//    case EnchantSlot.PropSlot2:
			//        EnchantProp2 = id;
			//        break;
			//    case EnchantSlot.PropSlot3:
			//        EnchantProp3 = id;
			//        break;
			//    case EnchantSlot.PropSlot4:
			//        EnchantProp4 = id;
			//        break;

			//}
		}

		//[Property]
		//public int EnchantPerm
		//{
		//    get;
		//    set;
		//}

		//[Property]
		//public int EnchantTemp
		//{
		//    get;
		//    set;
		//}

		//[Property]
		//public int EnchantSock1
		//{
		//    get;
		//    set;
		//}

		//[Property]
		//public int EnchantSock2
		//{
		//    get;
		//    set;
		//}

		//[Property]
		//public int EnchantSock3
		//{
		//    get;
		//    set;
		//}

		//[Property]
		//public int EnchantProp0
		//{
		//    get;
		//    set;
		//}

		//[Property]
		//public int EnchantProp1
		//{
		//    get;
		//    set;
		//}
		//[Property]
		//public int EnchantProp2
		//{
		//    get;
		//    set;
		//}

		//[Property]
		//public int EnchantProp3
		//{
		//    get;
		//    set;
		//}

		//[Property]
		//public int EnchantProp4
		//{
		//    get;
		//    set;
		//}

		/// <summary>
		/// The life-time of the Item in seconds
		/// </summary>
		//[Property(NotNull = true)]
		//public uint ExistingDuration
		//{
		//    get { return m_ExistingDuration; }
		//    set
		//    {
		//        m_ExistingDuration = value;
		//        m_lastLifetimeUpdate = DateTime.Now; ;
		//    }
		//}

		public ItemTemplate Template
		{
			get { return ItemMgr.GetTemplate(EntryId); }
		}

		public bool IsInventory
		{
			get
			{
				var template = Template;
				if (template == null)
				{
					// should never happen
					return false;
				}
				return template.IsInventory;
			}
		}

		#region Non-Inventory Items
		public bool IsOwned
		{
			get { return !IsAuctioned && MailId == 0; }
		}

		/// <summary>
		/// if this is true, the item actually is auctioned
		/// </summary>
		[Property]
		public bool IsAuctioned
		{
			get;
			set;
		}

		/// <summary>
		/// The id of the mail that this Item is attached to (if any)
		/// </summary>
		[Property]
		public long MailId
		{
			get;
			set;
		}
		#endregion

		#region Loading
		public static ItemRecord[] LoadItems(uint lowCharId)
		{
			return FindAll(Restrictions.Eq("OwnerId", (int)lowCharId));
		}

		public static ItemRecord[] LoadItemsContainersFirst(uint lowChrId)
		{
			// containers first
			return FindAll(ContOrder, Restrictions.Eq("OwnerId", (int)lowChrId));
		}

		public static ItemRecord[] LoadAuctionedItems()
		{
			return FindAll(Restrictions.Eq("IsAuctioned", true));
		}

		public static ItemRecord GetRecordByID(long id)
		{
			return FindOne(Restrictions.Eq("Guid", id));
		}

		public static ItemRecord GetRecordByID(uint itemLowId)
		{
			return GetRecordByID((long)itemLowId);
		}

		protected void OnLoad()
		{
			var templ = Template;
			if (templ != null)
			{
				templ.NotifyCreated(this);
			}
			else
			{
				log.Warn("ItemRecord has invalid EntryId: " + this);
			}
		}
		#endregion

		public static ItemRecord CreateRecord(ItemTemplate templ)
		{
			var item = CreateRecord();
			item.EntryId = templ.Id;

			item.Amount = templ.MaxAmount;
			item.Durability = templ.MaxDurability;
			item.Flags = templ.Flags;
			item.ItemTextId = templ.PageTextId;
			item.RandomProperty = (int)(templ.RandomPropertiesId != 0 ? templ.RandomPropertiesId : templ.RandomSuffixId);
			item.RandomSuffix = (int) templ.RandomSuffixId;
			item.Duration = templ.Duration;

			if (templ.UseSpell != null)
			{
				item.Charges = (short)templ.UseSpell.Charges;
			}
			return item;
		}

		public override string ToString()
		{
			return string.Format("ItemRecord \"{0}\" ({1}) #{2}", EntityId, _entryId, EntityLowId);
		}
	}
}