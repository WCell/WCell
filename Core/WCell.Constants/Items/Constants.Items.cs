using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Items
{
	/// <summary>
	/// Loot threshold
	/// </summary>
	public enum ItemQuality : uint
	{
		Poor = 0,
		Common = 1,
		Uncommon = 2,
		Rare = 3,
		Epic = 4,
		Legendary = 5,
		Artifact = 6,
		Heirloom,
		End
	}


	public enum BagSlot : sbyte
	{
		Invalid = -1,
		Bag1 = 19,
		Bag2,
		Bag3,
		LastBag
	}

	public enum ItemBondType
	{
		None = 0,
		OnPickup = 1,
		OnEquip = 2,
		OnUse = 3,
		Quest = 4,
	}

	public enum ItemSpellTrigger
	{
		Use = 0,
		Equip = 1,
		ChanceOnHit = 2,

		/// <summary>
		/// Only used by: Glowing Sanctified Crystal (ID: 23442)
		/// To cast: Unsummon Sanctified Crystal (Id: 29343)
		/// </summary>
		Unsummon = 3,

		Soulstone = 4,

		/// <summary>
		/// Only used by 3 Quest Items; 
		/// each triggering a Dummy spell which seems to check for another requirement and -if given- allow something to happen
		/// </summary>
		Combo = 5,

		/// <summary>
		/// Casted once and then consumes the Item (usually teaching formulars, patterns, designs etc)
		/// </summary>
		Consume = 6
	}

	public enum ItemProjectileType
	{
		None = 0,
		/// <summary>
		/// Obsolete
		/// </summary>
		Bolts = 1,
		Arrows = 2,
		Bullets = 3,
		Thrown = 4
	}

	public enum EnchantSlot : uint
	{
		Permanent = 0,
		Temporary = 1,
		Socket1 = 2,
		Socket2 = 3,
		Socket3 = 4,
		Bonus = 5,
		Prismatic = 6,

		PropSlot0,                        // used with RandomSuffix   (or have HELD enchantments)
		PropSlot1,                        // used with RandomSuffix   (or have HELD enchantments)
		PropSlot2,                        // used with RandomSuffix and RandomProperty
		PropSlot3,                        // used with RandomProperty (or have HELD enchantments)
		PropSlot4,						  // used with RandomProperty (or have HELD enchantments)
		End
	}

	public enum EnchantInfoOffset
	{
		Id = 0,
		Duration = 1,
		Charges = 2
	}

	public enum ItemSuffixCategory : uint
	{
		MainArmor = 0,
		SecondaryArmor = 1,
		Other = 2,
		Weapon = 3,
		Ranged = 4,
		None
	}

	public enum PartialInventoryType
	{
		Equipment = 0,
		BackPack,
		EquippedContainers,
		Bank,
		BankBags,
		BuyBack,
		KeyRing,

		/// <summary>
		/// The amount of different PartialInventories
		/// </summary>
		Count
	}

	public enum SellItemError : byte
	{
		Success = 0x00,
		CantFindItem = 0x01,
		CantSellItem = 0x02,
		CantFindVendor = 0x03,
		PlayerDoesntOwnItem = 0x04,
		Unknown = 0x05,
		OnlyEmptyBag = 0x06
	}

	public enum BuyItemError : byte
	{
		CantFindItem = 0x00,
		ItemAlreadySold = 0x01,
		NotEnoughMoney = 0x02,
		Unknown1 = 0x03,
		SellerDoesntLikeYou = 0x04,
		DistanceTooFar = 0x05,
		Unknown2 = 0x06,
		ItemSoldOut = 0x07,
		CantCarryAnymore = 0x08,
		Unknown3 = 0x09,
		Unknown4 = 0x10,
		RankRequirementNotMet = 0x11,
		ReputationRequirementNotMet = 0x12
	}

	[Flags]
	public enum SocketColor : uint
	{
		None = 0,
		Meta = 1,
		Red = 2,

		Yellow = 4,

		Blue = 8,

	}

    /// <summary>
    /// Used In CMSG_ITEM_PUSH_RESULT
    /// </summary>
    public enum HowObtained : byte
    {
        Looted = 0,
        NPCTransaction = 1
    }

    /// <summary>
    /// Used In CMSG_ITEM_PUSH_RESULT
    /// </summary>
    public enum HowReceived : byte
    {
        Transaction = 0,
        Created = 1
    }
}
