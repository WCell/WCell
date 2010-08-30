using System;
using System.Collections.Generic;
using WCell.Constants.Items;
using WCell.Constants.NPCs;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Items;
using WCell.Util.Data;
using WCell.Constants.Factions;

namespace WCell.RealmServer.NPCs.Vendors
{
	[DataHolder]
	public class VendorItemEntry : IDataHolder
	{
		public static readonly List<VendorItemEntry> EmptyList = new List<VendorItemEntry>(1);

		private int remainingStackAmount;

		public NPCId VendorId;
		public ItemId ItemId;

		/// <summary>
		/// The amount of available stacks
		/// </summary>
		public int StockAmount;

		/// <summary>
		/// The size of one stack
		/// </summary>
		public int BuyStackSize;

		/// <summary>
		/// The time until the vendor restocks, after he/she ran out of this Item
		/// </summary>
		public uint StockRefillDelay;

		public uint ExtendedCostId;

		[NotPersistent]
		public ItemExtendedCostEntry ExtendedCostEntry;

		[NotPersistent]
		private DateTime lastUpdate;

		[NotPersistent]
		public ItemTemplate Template;

		/// <summary>
		/// If this item has a limited supply available, this returns a number smaller than uint.MaxValue
		/// </summary>
		[NotPersistent]
		public int RemainingStockAmount
		{
			get
			{
				if (StockAmount > 0 && StockRefillDelay > 0)
				{
					var numRegenedStacks = (int)((DateTime.Now - lastUpdate).Milliseconds / StockRefillDelay);
					if ((remainingStackAmount + numRegenedStacks) > StockAmount)
						remainingStackAmount = StockAmount;
					else
						remainingStackAmount += numRegenedStacks;

					lastUpdate = DateTime.Now;
					return remainingStackAmount;
				}
				return -1;
			}
			set
			{
				remainingStackAmount = value;
			}
		}

		public void FinalizeDataHolder()
		{
			Template = ItemMgr.GetTemplate(ItemId);
			if (Template == null)
			{
				ContentMgr.OnInvalidDBData("{0} has invalid ItemId: {1} ({2})", this, ItemId, (int)ItemId);
			}
			else
			{
				var list = NPCMgr.GetOrCreateVendorList(VendorId);

				// set defaults
				if (StockAmount < 0)
				{
					StockAmount = Template.StockAmount;
				}
				if (StockRefillDelay < 0)
				{
					StockRefillDelay = Template.StockRefillDelay;
				}

				remainingStackAmount = StockAmount;
				list.Add(this);
			}
		}

		public override string ToString()
		{
			return GetType().Name + " " + VendorId + " (" + (int)VendorId + ")";
		}

		public static IEnumerable<VendorItemEntry> GetAllDataHolders()
		{
			var list = new List<VendorItemEntry>(20000);
			foreach (var vendor in NPCMgr.GetAllEntries())
			{
				if (vendor == null || vendor.VendorItems == null)
				{
					continue;
				}
				list.AddRange(vendor.VendorItems);
			}
			return list;
		}
	}
}