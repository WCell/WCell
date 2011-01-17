using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Items
{


	/// <summary>
	/// Used in the ITEMFLAGS updatefield
	/// </summary>
	[Flags]
	public enum ItemFlags : uint
	{
		None = 0,

		Soulbound = 0x1, // PER ITEM, NOT PROTO

		/// <summary>
		/// Warlock/Mage stones, water etc
		/// </summary>
		Conjured = 0x2,

		/// <summary>
		/// Lockboxes, clams etc
		/// </summary>
		Openable = 0x4,

		GiftWrapped = 0x8, // PER ITEM, NOT PROTO

		/// <summary>
		/// Shaman totem tools
		/// </summary>
		Totem = 0x20,

		/// <summary>
		/// Items bearing a spell (Equip/Use/Proc etc) 
		/// </summary>
		TriggersSpell = 0x40,

		/// <summary>
		/// CHECK THIS
		/// </summary>
		NoEquipCooldown = 0x80,

		/// <summary>
		/// These items would appear to do ranged magical damage
		/// ITEM_FLAG_INTBONUSINSTEAD = 0x100
		/// </summary>
		Wand = 0x100,

		/// <summary>
		/// usable..?
		/// </summary>
		Usable = 0x00000040,

		/// <summary>
		/// These items can wrap other items (wrapping paper)
		/// </summary>
		WrappingPaper = 0x200,

		/// <summary>
		/// These items produce other items when right clicked (motes, enchanting essences, darkmoon cards...)
		/// </summary>
		Producer = 0x400,

		/// <summary>
		/// Everyone in the group/raid can loot a copy of the item.
		/// </summary>
		MultiLoot = 0x800,

		/// <summary>
		/// ITEM_FLAG_BRIEFSPELLEFFECTS = 0x1000
		/// </summary>
		BriefSpellEffect = 0x1000,

		/// <summary>
		/// item can be refunded within 2 hours of purchase
		/// </summary>
		Refundable = 0x00001000,

		/// <summary>
		/// Guild charters
		/// </summary>
		Charter = 0x2000,

		/// <summary>
		/// ??? see Refundable
		/// </summary>
		Refundable2 = 0x00008000,

		/// <summary>
		/// "Right Click to Read"
		/// </summary>
		Readable = 0x4000,

		/// <summary>
		/// 
		/// </summary>
		PVPItem = 0x8000,

		/// <summary>
		/// Item expires after a certain amount of time
		/// </summary>
		Expires = 0x10000,

		/// <summary>
		/// Items a jewel crafter can prospect
		/// </summary>
		Prospectable = 0x40000,

		/// <summary>
		/// Items you can only have one of equipped, but multiple in your inventory
		/// </summary>
		UniqueEquipped = 0x80000,

		/// <summary>
		/// Lowers durability on cast (also contains deprecated throwing weapons)
		/// </summary>
		ThrownWeapon = 0x400000,

		AccountBound = 0x8000000,

		EnchantScroll = 0x10000000,

		Millable = 0x20000000,
	}

    [Flags]
    public enum ItemFlags2 : uint
    {
        /// <summary>
        /// Item can only be equipped by horde
        /// </summary>
        HordeOnly = 0x00000001,

        /// <summary>
        /// Item can only be equipped by alliance
        /// </summary>
        AllianceOnly = 0x00000002,

        /// <summary>
        /// This item must be acquired with an extended cost plus gold
        /// </summary>
        ExtendedCostRequiresGold = 0x00000004,

        Unknown4 = 0x00000008,
        Unknown5 = 0x00000010,
        Unknown6 = 0x00000020,
        Unknown7 = 0x00000040,
        Unknown8 = 0x00000080,

        /// <summary>
        /// It is not possible to use a need roll for this item
        /// </summary>
        NeedRollDisabled = 0x00000100,

        /// <summary>
        /// Item uses caster specific dbc file for DPS calculations
        /// </summary>
        CasterWeapon = 0x00000200
    }
}