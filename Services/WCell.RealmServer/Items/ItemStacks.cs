using System;
using WCell.Constants.Items;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items.Enchanting;

namespace WCell.RealmServer.Items
{
	public interface IItemStack
	{
		int Amount { get; }

		ItemTemplate Template { get; }
	}

	public struct ItemStackDescription : IItemStack
	{
		public static readonly ItemStackDescription Empty = default(ItemStackDescription);
		public static readonly ItemStackDescription[] EmptyArray = new ItemStackDescription[0];

		public ItemId ItemId;
		private int m_Amount;

		public int Amount
		{
			get { return m_Amount; }
			set { m_Amount = value; }
		}

		public ItemStackDescription(ItemId id, int amount)
		{
			ItemId = id;
			m_Amount = amount;
		}

		public ItemTemplate Template
		{
			get { return ItemMgr.GetTemplate(ItemId); }
		}


		public override string ToString()
		{
			if (Template != null)
			{
				return (Template.IsStackable ? Amount + "x " : "") + Template;
			}
			return Amount + "x " + ItemId + " (" + (int)ItemId + ")";
		}
	}

	public struct ItemStackTemplate : IItemStack
	{
		public static readonly ItemStackTemplate[] EmptyArray = new ItemStackTemplate[0];

		private ItemTemplate m_Template;

		private int m_Amount;

		public ItemStackTemplate(ItemId id)
			: this(ItemMgr.GetTemplate(id), 1)
		{
			if (m_Template == null)
			{
				throw new ArgumentException("ItemId " + id + " is invalid.");
			}
		}

		public ItemStackTemplate(ItemId id, int amount)
			: this(ItemMgr.GetTemplate(id), amount)
		{
			if (m_Template == null)
			{
				throw new ArgumentException("id " + id + " is invalid.");
			}
		}

		public ItemStackTemplate(ItemTemplate templ)
			: this(templ, templ.MaxAmount)
		{
		}

		public ItemStackTemplate(ItemTemplate templ, int amount)
		{
			m_Template = templ;
			m_Amount = amount;
		}

		public ItemTemplate Template
		{
			get { return m_Template; }
			set { m_Template = value; }
		}

		public int Amount
		{
			get { return m_Amount; }
			set { m_Amount = value; }
		}

		public override string ToString()
		{
			if (Template != null)
			{
				return (Template.IsStackable ? Amount + "x " : "") + Template;
			}
			return Amount + "x [Unknown]";
		}
	}

	/// <summary>
	/// ItemTemplate or Item
	/// </summary>
	public interface IMountableItem
	{
		ItemTemplate Template { get; }

		ItemEnchantment[] Enchantments { get; }

		bool IsEquipped { get; }

		InventoryError CheckEquip(Character owner);
	}

	/*
		public struct IndexedItemStackTemplate
		{
			public uint Index;
			public uint ItemId;
			public uint Amount;

			public ItemTemplate Template
			{
				get
				{
					return ItemMgr.Templates.Get(ItemId);
				}
			}
		}
	 */
}
