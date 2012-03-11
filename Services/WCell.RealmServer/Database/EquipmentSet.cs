using System.Collections.Generic;
using Castle.ActiveRecord;
using WCell.Constants.Items;
using WCell.Core;

namespace WCell.RealmServer.Database
{
    /// <summary>
    /// This feature allows players to store sets of equipment, easily swap between saved sets using hotkeys,
    /// and pull items directly from backpacks or bank slots (must be at the bank to equip inventory from the bank).
    /// </summary>
    [ActiveRecord("EquipmentSets", Access = PropertyAccess.Property)]
    public class EquipmentSet : ActiveRecordBase<EquipmentSet>
    {
        public static readonly IList<EquipmentSet> EmptyList = new List<EquipmentSet>(1);

        private static readonly NHIdGenerator m_idGenerator =
            new NHIdGenerator(typeof(EquipmentSet), "EquipmentSets", "EntityLowId");

        [PrimaryKey(PrimaryKeyType.Assigned, "EntityLowId")]
        private long lowId
        {
            get;
            set;
        }

        public EntityId SetGuid
        {
            get { return EntityId.GetPlayerId((uint)lowId); }
        }

        [Field]
        public int Id;

        [Field]
        public string Name;

        [Field]
        public string Icon;

        public IList<EquipmentSetItemMapping> Items
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the next unique Id for a new Set
        /// </summary>
        public static long NextId()
        {
            return m_idGenerator.Next();
        }

        //[Initialization(InitializationPass.Fifth, "Initializing EquipmentSetRecords")]
        //public static void InitializeEquipmentSetIds()
        //{
        //    try
        //    {
        //        const string hql = "SELECT max(r.EntityLowId) FROM EquipmentSets r";
        //        var query = new ScalarQuery<long>(typeof(EquipmentSet), hql);
        //        var val = query.Execute();
        //        highestLowId = val > 0 ? val : 1;
        //    }
        //    catch (Exception)
        //    {
        //        highestLowId = 0;
        //    }
        //}

        public static EquipmentSet CreateSet()
        {
            EquipmentSet newSet;

            newSet = new EquipmentSet
            {
                lowId = NextId()
            };

            return newSet;
        }

        public void Fill(int setId, string name, string icon, EquipmentSetItemMapping[] setItemMappings)
        {
            Id = setId;
            Name = name;
            Icon = icon;
            Items = setItemMappings;
        }

        private EquipmentSet()
        {
        }
    }

    public struct EquipmentSwapHolder
    {
        public EntityId ItemGuid;
        public InventorySlot SrcContainer;
        public int SrcSlot;
    }
}