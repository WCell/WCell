using System.Collections.Generic;
using WCell.Constants.Items;
using WCell.Core;

namespace WCell.RealmServer.Database.Entities
{
	/// <summary>
	/// This feature allows players to store sets of equipment, easily swap between saved sets using hotkeys, 
	/// and pull items directly from backpacks or bank slots (must be at the bank to equip inventory from the bank).
	/// </summary>
	public class EquipmentSet
	{
		public static readonly IList<EquipmentSet> EmptyList = new List<EquipmentSet>(1);

		private long entityLowId
		{
			get;
			set;
		}

		public virtual EntityId SetGuid
		{
			get { return EntityId.GetPlayerId((uint)entityLowId); }
		}

		public int Id;
		public string Name;
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

	    public virtual void Fill(int setId, string name, string icon, EquipmentSetItemMapping[] setItemMappings)
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