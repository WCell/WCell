using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Items;
using WCell.Constants.Looting;
using WCell.RealmServer.Items;
using WCell.Util;
using WCell.Util.Data;

namespace WCell.RealmServer.Looting
{
	class LootEntryStorage : IValueSetter
	{
		public object Get(object key)
		{
			return ItemMgr.Templates.Get((uint)key);
		}

		public void Set(object value)
		{
			var entry = (LootEntry)value;
			var entries = LootMgr.LootEntries[(uint)entry.LootType];
			if (entry.EntryId >= entries.Length)
			{
				ArrayUtil.EnsureSize(ref entries, (int)(entry.EntryId * ArrayUtil.LoadConstant) + 1);
				LootMgr.LootEntries[(uint)entry.LootType] = entries;
			}

			var list = entries[entry.EntryId];
			if (list == null)
			{
				entries[entry.EntryId] = list = new List<LootEntry>();
			}
			list.Add(entry);
		}
	}

	[DataHolder(typeof(LootEntryStorage))]
	public class LootEntry : IDataHolder
	{
		public LootEntryType LootType;
		public uint EntryId;
		public ItemId ItemId;
		public uint GroupId;
		public uint MinAmount, MaxAmount;
		public bool IsFFA;
		public float DropChance;
		public float HeroicDropChance;
		
		public ItemTemplate ItemTemplate
		{
			get
			{
				return ItemMgr.GetTemplate(ItemId);
			}
		}

		public object GetId()
		{
			return EntryId;
		}

		public void FinalizeAfterLoad()
		{
			
		}
	}
}
