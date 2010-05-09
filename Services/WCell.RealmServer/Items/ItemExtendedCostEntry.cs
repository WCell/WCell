using System.Collections.Generic;
using Cell.Core;
using WCell.Constants.Items;
using WCell.Core.DBC;

namespace WCell.RealmServer.Items
{
	public class ItemExtendedCostEntry
	{
	    public uint Id;
		public uint HonorCost;
		public uint ArenaPointCost;
        public List<RequiredItem> RequiredItems = new List<RequiredItem>(5);
	    public ItemId ReqItem1
	    {
	        get { return RequiredItems[0].Id; }
	    }
	    public ItemId ReqItem2
	    {
	        get { return RequiredItems[1].Id; }
	    }
        public ItemId ReqItem3
        {
	        get { return RequiredItems[2].Id; }
	    }
        public ItemId ReqItem4
        {
	        get { return RequiredItems[3].Id; }
	    }
        public ItemId ReqItem5
        {
	        get { return RequiredItems[4].Id; }
	    }
	    public int ReqItemAmt1
	    {
	        get { return RequiredItems[0].Cost; }
	    }
	    public int ReqItemAmt2
	    {
	        get { return RequiredItems[1].Cost; }
	    }
	    public int ReqItemAmt3
	    {
	        get { return RequiredItems[2].Cost; }
	    }
	    public int ReqItemAmt4
	    {
	        get { return RequiredItems[3].Cost; }
	    }
	    public int ReqItemAmt5
	    {
	        get { return RequiredItems[4].Cost; }
	    }
	    public uint ReqArenaRating;

        public uint Unk_322;

        public static ItemExtendedCostEntry NullEntry = new ItemExtendedCostEntry() {
	        Id = 0,
	        HonorCost = 0,
	        ArenaPointCost = 0,
	        RequiredItems = new List<RequiredItem>(5) {
                new RequiredItem() {Id = ItemId.None, Cost = 0},
                new RequiredItem() {Id = ItemId.None, Cost = 0},
                new RequiredItem() {Id = ItemId.None, Cost = 0},
                new RequiredItem() {Id = ItemId.None, Cost = 0},
                new RequiredItem() {Id = ItemId.None, Cost = 0}
            },
	        ReqArenaRating = 0
	    };

        public struct RequiredItem
        {
            public ItemId Id;
            public int Cost;
        }
    }

    #region ItemExtendedCostEntry.dbc

    public class DBCItemExtendedCostConverter : AdvancedDBCRecordConverter<ItemExtendedCostEntry>
    {
        public override ItemExtendedCostEntry ConvertTo(byte[] rawData, ref int id)
        {
            var currentIndex = 0u;
            id = (int)rawData.GetUInt32(currentIndex++);
            var honorCost = rawData.GetUInt32(currentIndex++);
            var arenaPointCost = rawData.GetUInt32(currentIndex++);
            currentIndex++; // some unk stuff? not sure
            var unk_322 = rawData.GetUInt32(currentIndex++); // 3.2.2
            var reqItem1 = (ItemId)rawData.GetUInt32(currentIndex++);
            var reqItem2 = (ItemId)rawData.GetUInt32(currentIndex++);
            var reqItem3 = (ItemId)rawData.GetUInt32(currentIndex++);
            var reqItem4 = (ItemId)rawData.GetUInt32(currentIndex++);
            var reqItem5 = (ItemId)rawData.GetUInt32(currentIndex++);
            var reqItemAmt1 = rawData.GetInt32(currentIndex++);
            var reqItemAmt2 = rawData.GetInt32(currentIndex++);
            var reqItemAmt3 = rawData.GetInt32(currentIndex++);
            var reqItemAmt4 = rawData.GetInt32(currentIndex++);
            var reqItemAmt5 = rawData.GetInt32(currentIndex++);
            var reqArenaRating = rawData.GetUInt32(currentIndex++);

            var list = new List<ItemExtendedCostEntry.RequiredItem>(5) {
                new ItemExtendedCostEntry.RequiredItem() {Id = reqItem1, Cost = reqItemAmt1},
                new ItemExtendedCostEntry.RequiredItem() {Id = reqItem2, Cost = reqItemAmt2},
                new ItemExtendedCostEntry.RequiredItem() {Id = reqItem3, Cost = reqItemAmt3},
                new ItemExtendedCostEntry.RequiredItem() {Id = reqItem4, Cost = reqItemAmt4},
                new ItemExtendedCostEntry.RequiredItem() {Id = reqItem5, Cost = reqItemAmt5}
            };

            var entry = new ItemExtendedCostEntry() {
                Id = (uint)id,
                HonorCost = honorCost,
                ArenaPointCost = arenaPointCost,
                Unk_322 = unk_322,
                RequiredItems = list,
                ReqArenaRating = reqArenaRating
            };
            return entry;
        }
    }

    #endregion
}
