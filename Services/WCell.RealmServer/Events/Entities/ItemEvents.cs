using System;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Items
{
	public partial class ItemTemplate
	{
		/// <summary>
		/// Called when an ItemRecord of this ItemTemplate has been created (if newly created or loaded from DB).
		/// That is before the actual Item object has been created.
		/// Called from the IO context if loaded from DB.
		/// </summary>
		public event Action<ItemRecord> Created;

		/// <summary>
		/// Called whenever an Item of this ItemTemplate is equipped
		/// </summary>
		public event Action<Item> Equipped;

		/// <summary>
		/// Called whenever an Item of this ItemTemplate is unequipped
		/// </summary>
		public event Action<Item> Unequipped;

		/// <summary>
		/// Called whenever an item of this ItemTemplate has been used
		/// </summary>
		public event Action<Item> Used;

		internal void NotifyCreated(ItemRecord record)
		{
			OnRecordCreated(record);
			var evt = Created;
			if (evt != null)
			{
				evt(record);
			}
		}

		internal void NotifyEquip(Item item)
		{
			var evt = Equipped;
			if (evt != null)
			{
				evt(item);
			}
		}

		internal void NotifyUnequip(Item item)
		{
			var evt = Unequipped;
			if (evt != null)
			{
				evt(item);
			}
		}

		internal void NotifyUsed(Item item)
		{
			var evt = Used;
			if (evt != null)
			{
				evt(item);
			}
		}
	}
}