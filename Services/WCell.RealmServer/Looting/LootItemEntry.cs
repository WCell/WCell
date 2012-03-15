using System.Collections.Generic;
using WCell.Constants.Items;
using WCell.Constants.Looting;
using WCell.RealmServer.Items;
using WCell.Util.Data;

namespace WCell.RealmServer.Looting
{
    [DataHolder(Inherit = true)]
    public class GOLootItemEntry : LootItemEntry
    {
        public static IEnumerable<GOLootItemEntry> GetAllDataHolders()
        {
            var all = new List<GOLootItemEntry>(20000);
            AddItems(LootEntryType.GameObject, all);
            return all;
        }
    }

    [DataHolder(Inherit = true)]
    public class ItemLootItemEntry : LootItemEntry
    {
        public static IEnumerable<ItemLootItemEntry> GetAllDataHolders()
        {
            var all = new List<ItemLootItemEntry>(20000);
            AddItems(LootEntryType.Item, all);
            AddItems(LootEntryType.Disenchanting, all);
            AddItems(LootEntryType.Prospecting, all);
            AddItems(LootEntryType.Milling, all);
            return all;
        }
    }

    [DataHolder(Inherit = true)]
    public class NPCLootItemEntry : LootItemEntry
    {
        public static IEnumerable<NPCLootItemEntry> GetAllDataHolders()
        {
            var all = new List<NPCLootItemEntry>(500000);
            AddItems(LootEntryType.NPCCorpse, all);
            AddItems(LootEntryType.Skinning, all);
            AddItems(LootEntryType.PickPocketing, all);
            return all;
        }
    }

    [DataHolder(Inherit = true)]
    public class FishingLootItemEntry : LootItemEntry
    {
        public static IEnumerable<FishingLootItemEntry> GetAllDataHolders()
        {
            var all = new List<FishingLootItemEntry>(400);
            AddItems(LootEntryType.Fishing, all);
            return all;
        }
    }

    [DataHolder(Inherit = true)]
    public class MillingLootItemEntry : LootItemEntry
    {
        public static IEnumerable<MillingLootItemEntry> GetAllDataHolders()
        {
            var all = new List<MillingLootItemEntry>(10000);
            AddItems(LootEntryType.Milling, all);
            return all;
        }
    }

    [DataHolder(Inherit = true)]
    public class ProspectingLootItemEntry : LootItemEntry
    {
        public static IEnumerable<ProspectingLootItemEntry> GetAllDataHolders()
        {
            var all = new List<ProspectingLootItemEntry>(10000);
            AddItems(LootEntryType.Prospecting, all);
            return all;
        }
    }

    [DataHolder(Inherit = true)]
    public class PickPocketLootItemEntry : LootItemEntry
    {
        public static IEnumerable<PickPocketLootItemEntry> GetAllDataHolders()
        {
            var all = new List<PickPocketLootItemEntry>(10000);
            AddItems(LootEntryType.PickPocketing, all);
            return all;
        }
    }

    [DataHolder(Inherit = true)]
    public class SkinningLootItemEntry : LootItemEntry
    {
        public static IEnumerable<SkinningLootItemEntry> GetAllDataHolders()
        {
            var all = new List<SkinningLootItemEntry>(10000);
            AddItems(LootEntryType.Skinning, all);
            return all;
        }
    }

    [DataHolder(Inherit = true)]
    public class DisenchantingLootItemEntry : LootItemEntry
    {
        public static IEnumerable<DisenchantingLootItemEntry> GetAllDataHolders()
        {
            var all = new List<DisenchantingLootItemEntry>(10000);
            AddItems(LootEntryType.Disenchanting, all);
            return all;
        }
    }

    [DataHolder(Inherit = true)]
    public class ReferenceLootItemEntry : LootItemEntry
    {
        public static IEnumerable<ReferenceLootItemEntry> GetAllDataHolders()
        {
            var all = new List<ReferenceLootItemEntry>(10000);
            AddItems(LootEntryType.Reference, all);
            return all;
        }
    }

    public class LootEntity
    {
        public uint EntryId;
        public int MinAmount, MaxAmount;
        public ItemId ItemId;

        /// <summary>
        /// A value between 0 and 100 to indicate the chance of this Entry to drop
        /// </summary>
        public float DropChance;

        public ItemTemplate ItemTemplate
        {
            get { return ItemMgr.GetTemplate(ItemId); }
        }
    }

    public abstract class LootItemEntry : LootEntity, IDataHolder
    {
        public LootEntryType LootType;
        public uint GroupId;
        public int MinAmountOrRef;
        public uint ReferencedEntryId;

        public object GetId()
        {
            return EntryId;
        }

        public void FinalizeDataHolder()
        {
            if (MinAmountOrRef < 0)
            {
                ReferencedEntryId = (uint)(-MinAmountOrRef);
            }
            else
            {
                MinAmount = MinAmountOrRef;
            }

            if (MinAmount < 1)
            {
                MinAmount = 1;
            }

            if (MinAmount > MaxAmount)
            {
                MaxAmount = MinAmount;
            }

            if (DropChance < 0)
            {
                DropChance = -DropChance;
            }

            LootMgr.AddEntry(this);
        }

        protected static void AddItems<T>(LootEntryType t, List<T> all)
            where T : LootItemEntry
        {
            var entries = LootMgr.GetEntries(t);
            foreach (var list in entries)
            {
                if (list != null)
                {
                    foreach (var entry in list)
                    {
                        all.Add((T)entry);
                    }
                }
            }
        }

        public override string ToString()
        {
            return ItemTemplate + " (" + DropChance + "%)";
        }
    }
}