using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Queries;
using NHibernate.Criterion;
using WCell.Constants.Items;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.RealmServer.Items;

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

		[PrimaryKey(PrimaryKeyType.Increment, "EntityLowId")]
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

		public static EquipmentSet CreateSet()
		{
		    var newSet = new EquipmentSet();

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