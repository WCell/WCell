using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Items
{


	public enum ItemSubClass
	{
		None = -1,
		// Weapon
		WeaponAxe = 0,
		WeaponTwoHandAxe = 1,
		WeaponBow = 2,
		WeaponGun = 3,
		WeaponMace = 4,
		WeaponTwoHandMace = 5,
		WeaponPolearm = 6,
		WeaponSword = 7,
		WeaponTwoHandSword = 8,
		WeaponFist = 13,
		WeaponDagger = 15,
		WeaponStaff = 10,
		WeaponMisc = 14,
		WeaponThrown = 16,
		WeaponCrossbow = 18,
		WeaponWand = 19,
		WeaponFishingPole = 20,

		// Armor
		ArmorMisc = 0,
		ArmorCloth = 1,
		ArmorLeather = 2,
		AmorMail = 3,
		ArmorPlate = 4,
		ArmorShield = 6,

		// Projectile
		ProjectileArrow = 2,
		ProjectileBullet = 3,

		// Trade goods
		ITEM_SUBCLASS_PROJECTILE_TRADE_GOODS = 0,
		ITEM_SUBCLASS_PROJECTILE_PARTS = 1,
		ITEM_SUBCLASS_PROJECTILE_EXPLOSIVES = 2,
		ITEM_SUBCLASS_PROJECTILE_DEVICES = 3,

		// Recipe
		ITEM_SUBCLASS_RECIPE_BOOK = 0,
		ITEM_SUBCLASS_RECIPE_LEATHERWORKING = 1,
		ITEM_SUBCLASS_RECIPE_TAILORING = 2,
		ITEM_SUBCLASS_RECIPE_ENGINEERING = 3,
		ITEM_SUBCLASS_RECIPE_BLACKSMITHING = 4,
		ITEM_SUBCLASS_RECIPE_COOKING = 5,
		ITEM_SUBCLASS_RECIPE_ALCHEMY = 6,
		ITEM_SUBCLASS_RECIPE_FIRST_AID = 7,
		ITEM_SUBCLASS_RECIPE_ENCHANTING = 8,
		ITEM_SUBCLASS_RECIPE_FISHING = 9,

		// Quiver
		AmmoPouch = 3,
		Quiver = 2,

		// Misc
		Junk = 0,
	}

	[Flags]
	public enum ItemSubClassMask : uint
	{
		None = 0,

		AnyMeleeWeapon = 0x2A5F3,
		AnyRangedWeapon = 0x4000C,

		WeaponAxe = 1,
		WeaponTwoHandAxe = 0x000000002,
		WeaponBow = 0x000000004,
		WeaponGun = 0x000000008,
		//WeaponMace = 0x000000008,
		WeaponPolearm = 0x000000010,
		WeaponTwoHandMace = 0x000000020,
		Shield = 0x000000040,
		WeaponOneHandSword = 0x00000080,
		WeaponTwoHandSword = 0x00000100,
		/// <summary>
		/// Only in: NotDisplayed Totem (Id: 27763)
		/// </summary>
		UnknownSubClass1 = 0x00000200,
		WeaponStaff = 0x00000400,
		WeaponFist = 0x00002000,
		WeaponDagger = 0x00008000,
		WeaponThrown = 0x00010000,
		UnknownSubClass2 = 0x00020000,
		WeaponCrossbow = 0x00040000,
		WeaponWand = 0x00080000,
		WeaponFishingPole = 0x00100000,

		ArmorMisc = 1,
		ArmorCloth = 2,
		ArmorLeather = 4,
		ArmorMail = 8,
		ArmorPlate = 0x10,
		ArmorShield = 0x40,
	}
}
