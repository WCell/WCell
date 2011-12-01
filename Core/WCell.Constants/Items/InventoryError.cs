namespace WCell.Constants.Items
{
	public enum InventoryError
	{
		/// <summary>
		/// If nothing else helps
		/// </summary>
		Invalid = -1,
		OK,
		/// <summary>
		/// Done
		/// </summary>
		YOU_MUST_REACH_LEVEL_N = 1,
		/// <summary>
		/// Done
		/// </summary>
		SKILL_ISNT_HIGH_ENOUGH,
		/// <summary>
		/// Done
		/// </summary>
		ITEM_DOESNT_GO_TO_SLOT,
		/// <summary>
		/// Done
		/// </summary>
		BAG_FULL,
		/// <summary>
		/// Done
		/// </summary>
		NONEMPTY_BAG_OVER_OTHER_BAG,
		/// <summary>
		/// TODO: Missing
		/// </summary>
		CANT_TRADE_EQUIP_BAGS,
		/// <summary>
		/// Done
		/// </summary>
		ONLY_AMMO_CAN_GO_HERE,
		/// <summary>
		/// Done
		/// </summary>
		NO_REQUIRED_PROFICIENCY,
		/// <summary>
		/// Done
		/// </summary>
		NO_EQUIPMENT_SLOT_AVAILABLE,
		/// <summary>
		/// Done
		/// </summary>
		YOU_CAN_NEVER_USE_THAT_ITEM = 10,
		/// <summary>
		/// Uncertain
		/// </summary>
		YOU_CAN_NEVER_USE_THAT_ITEM2,
		/// <summary>
		/// Uncertain
		/// </summary>
		NO_EQUIPMENT_SLOT_AVAILABLE2,
		/// <summary>
		/// Done
		/// </summary>
		CANT_EQUIP_WITH_TWOHANDED,
		/// <summary>
		/// Done
		/// </summary>
		CANT_DUAL_WIELD,
		/// <summary>
		/// Done
		/// </summary>
		ITEM_DOESNT_GO_INTO_BAG = 15,
		/// <summary>
		/// Uncertain
		/// </summary>
		ITEM_DOESNT_GO_INTO_BAG2 = 16,
		/// <summary>
		/// Considers Unique count
		/// </summary>
		CANT_CARRY_MORE_OF_THIS,
		/// <summary>
		/// Uncertain
		/// </summary>
		NO_EQUIPMENT_SLOT_AVAILABLE3,
		/// <summary>
		/// Uncertain
		/// </summary>
		ITEM_CANT_STACK,
		/// <summary>
		/// Done
		/// </summary>
		ITEM_CANT_BE_EQUIPPED,
		/// <summary>
		/// Done
		/// </summary>
		ITEMS_CANT_BE_SWAPPED,
		/// <summary>
		/// Uncertain
		/// </summary>
		SLOT_IS_EMPTY,
		/// <summary>
		/// Done
		/// </summary>
		ITEM_NOT_FOUND,
		/// <summary>
		/// Some Quest items cannot be dropped during the Quest itself
		/// 
		/// Done
		/// </summary>
		CANT_DROP_SOULBOUND,
		/// <summary>
		/// Uncertain
		/// </summary>
		OUT_OF_RANGE,
		/// <summary>
		/// Done
		/// </summary>
		TRIED_TO_SPLIT_MORE_THAN_COUNT,
		/// <summary>
		/// Done
		/// </summary>
		COULDNT_SPLIT_ITEMS,
		/// <summary>
		/// Uncertain
		/// </summary>
		MISSING_REAGENT,
		/// <summary>
		/// Done (Vendors)
		/// </summary>
		NOT_ENOUGH_MONEY,
		/// <summary>
		/// Done
		/// </summary>
		NOT_A_BAG,
		/// <summary>
		/// Done
		/// </summary>
		CAN_ONLY_DO_WITH_EMPTY_BAGS,
		/// <summary>
		/// Probably cannot take/use Items of Trade partners etc
		/// 
		/// TODO: Missing
		/// </summary>
		DONT_OWN_THAT_ITEM,
		/// <summary>
		/// TODO: Missing
		/// </summary>
		CAN_EQUIP_ONLY1_QUIVER,
		/// <summary>
		/// Done
		/// </summary>
		MUST_PURCHASE_THAT_BAG_SLOT,
		/// <summary>
		/// Done
		/// </summary>
		TOO_FAR_AWAY_FROM_BANK,
		/// <summary>
		/// Probably when trying to move an item while being disarmed
		/// 
		/// TODO: Missing
		/// </summary>
		ITEM_LOCKED,
		/// <summary>
		/// Done
		/// </summary>
		YOU_ARE_STUNNED,
		/// <summary>
		/// Done
		/// </summary>
		YOU_ARE_DEAD,
		/// <summary>
		/// Uncertain
		/// </summary>
		CANT_DO_RIGHT_NOW,
		/// <summary>
		/// Uncertain
		/// </summary>
		BAG_FULL2 = 40,
		/// <summary>
		/// Uncertain
		/// </summary>
		CAN_EQUIP_ONLY1_QUIVER2,
		/// <summary>
		/// TODO: Missing
		/// </summary>
		CAN_EQUIP_ONLY1_AMMOPOUCH,

		// TODO: wrappings
		STACKABLE_CANT_BE_WRAPPED,
		EQUIPPED_CANT_BE_WRAPPED,
		WRAPPED_CANT_BE_WRAPPED,
		BOUND_CANT_BE_WRAPPED,
		UNIQUE_CANT_BE_WRAPPED,
		BAGS_CANT_BE_WRAPPED,

		/// <summary>
		/// Done
		/// </summary>
		ALREADY_LOOTED,
		/// <summary>
		/// Done
		/// </summary>
		INVENTORY_FULL = 50,
		/// <summary>
		/// Done
		/// </summary>
		BANK_FULL,
		/// <summary>
		/// See Vendors
		/// 
		/// TODO: Missing
		/// </summary>
		ITEM_IS_CURRENTLY_SOLD_OUT,
		/// <summary>
		/// Uncertain
		/// </summary>
		BAG_FULL3,
		/// <summary>
		/// Uncertain
		/// </summary>
		ITEM_NOT_FOUND2,
		/// <summary>
		/// Uncertain
		/// </summary>
		ITEM_CANT_STACK2,
		/// <summary>
		/// Uncertain
		/// </summary>
		BAG_FULL4,
		/// <summary>
		/// See Vendors
		/// 
		/// TODO: Missing
		/// </summary>
		ITEM_SOLD_OUT,
		/// <summary>
		/// Uncertain
		/// </summary>
		OBJECT_IS_BUSY,
		DontReport,
		/// <summary>
		/// Done
		/// </summary>
		CANT_DO_IN_COMBAT = 60,
		/// <summary>
		/// Done
		/// </summary>
		CANT_DO_WHILE_DISARMED,
		/// <summary>
		/// Uncertain
		/// </summary>
		BAG_FULL6,
		/// <summary>
		/// Uncertain
		/// </summary>
		ITEM_RANK_NOT_ENOUGH,
		/// <summary>
		/// Probably Vendor-related
		/// 
		/// TODO: Missing
		/// </summary>
		ITEM_REPUTATION_NOT_ENOUGH,
		/// <summary>
		/// TODO: Missing
		/// </summary>
		MORE_THAN1_SPECIAL_BAG = 65,

		LOOT_CANT_LOOT_THAT_NOW = 66,
		ITEM_UNIQUE_EQUIPABLE = 67,
		VENDOR_MISSING_TURNINS = 68,
		NOT_ENOUGH_HONOR_POINTS = 69,
		NOT_ENOUGH_ARENA_POINTS = 70,
		ITEM_MAX_COUNT_SOCKETED = 71,
		MAIL_BOUND_ITEM = 72,
		NO_SPLIT_WHILE_PROSPECTING = 73,
		ITEM_MAX_COUNT_EQUIPPED_SOCKETED = 75,
		ITEM_UNIQUE_EQUIPPABLE_SOCKETED = 76,
		TOO_MUCH_GOLD = 77,
		NOT_DURING_ARENA_MATCH = 78,
		CANNOT_TRADE_THAT = 79,
		PERSONAL_ARENA_RATING_TOO_LOW = 80
	}

    public enum UseEquipmentSetError : byte
    {
        Success = 0,
        BagsFull = 4
    }
}