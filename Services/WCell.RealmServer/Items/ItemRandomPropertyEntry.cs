using System.Collections.Generic;
using WCell.Constants.Items;
using WCell.RealmServer.Items.Enchanting;
using WCell.Util;
using WCell.Util.Data;
using WCell.RealmServer.Content;

namespace WCell.RealmServer.Items
{
	public class ItemRandomPropertyEntry
	{
		public uint Id;
		public ItemEnchantmentEntry[] Enchants = new ItemEnchantmentEntry[ItemConstants.MaxEnchantsPerEntry];
	}

	public class ItemRandomSuffixEntry
	{
		public int Id;// 0

		public ItemEnchantmentEntry[] Enchants; // 19, 20, 21
		public int[] Values; // 22, 23, 24
	}

	public abstract class BaseItemRandomPropertyInfo : IDataHolder
	{
		public uint EnchantId;
		public uint PropertiesId;
		/// <summary>
		/// 
		/// </summary>
		public float ChancePercent;

		public abstract void FinalizeDataHolder();
	}

	[DataHolder]
	public class ItemRandomEnchantEntry : BaseItemRandomPropertyInfo
	{
		//[NotPersistent]
		//public ItemRandomPropertyEntry RandomPropEntry;

		public override void FinalizeDataHolder()
		{
			//RandomPropEntry = ItemMgr.GetRandomPropertyEntry(EntryId);
			if (PropertiesId > 30000)
			{
				ContentHandler.OnInvalidDBData("RandomEnchantEntry has invalid PropertiesId: {0} (Enchant: {2})", PropertiesId, EnchantId);
			}
			else
			{
				var list = EnchantMgr.RandomEnchantEntries.Get(PropertiesId);
				if (list == null)
				{
					ArrayUtil.Set(ref EnchantMgr.RandomEnchantEntries, PropertiesId, list = new List<ItemRandomEnchantEntry>());
				}
				list.Add(this);
			}
		}
	}

	//[DataHolder]
	//public class ItemRandomSuffixInfo : BaseItemRandomPropertyInfo
	//{
	//    [NotPersistent]
	//    public ItemRandomSuffixEntry Entry;

	//    public override void FinalizeAfterLoad()
	//    {
	//        Entry = ItemMgr.GetRandomSuffixEntry(EntryId);

	//        var list = ArrayUtil.GetOrCreate(ref ItemMgr.RandomSuffixes, Id);
	//        list.Add(this);
	//    }
	//}
}
