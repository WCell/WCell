using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Items
{

	public enum InventorySlotType
	{
		None = 0,
		Head = 1,
		Neck = 2,
		Shoulder = 3,
		Body = 4,
		Chest = 5,
		Waist = 6,
		Legs = 7,
		Feet = 8,
		Wrist = 9,
		Hand = 10,
		Finger = 11,
		Trinket = 12,
		Weapon = 13,
		Shield = 14,
		WeaponRanged = 15,
		Cloak = 16,
		TwoHandWeapon = 17,
		Bag = 18,
		Tabard = 19,
		Robe = 20,
		WeaponMainHand = 21,
		WeaponOffHand = 22,
		Holdable = 23,
		Ammo = 24,
		Thrown = 25,
		RangedRight = 26,
		/// <summary>
		/// Ammo pouch
		/// </summary>
		Quiver = 27,
		Relic = 28,
		End
	}

	[Flags]
	public enum InventorySlotTypeMask
	{
		None = 0,
		Head = 0x002,
		Neck = 0x000004,
		Shoulder = 0x000008,
		Body = 0x000010,
		Chest = 0x000020,
		Waist = 0x00040,
		Legs = 0x00080,
		Feet = 0x000100,
		Wrist = 0x00200,
		Hand = 0x00400,
		Finger = 0x00800,
		Trinket = 0x01000,
		Weapon = 0x02000,
		Shield = 0x04000,
		WeaponRanged = 0x08000,
		Cloak = 0x10000,
		TwoHandWeapon = 0x20000,
		Bag = 0x040000,
		Tabard = 0x080000,
		Robe = 0x00100000,
		WeaponMainHand = 0x00200000,
		WeaponOffHand = 0x00400000,
		Holdable = 0x00800000,
		Ammo = 0x01000000,
		Thrown = 0x02000000,
		RangedRight = 0x04000000,
		/// <summary>
		/// Ammo pouch
		/// </summary>
		Quiver = 0x08000000,
		Relic = 0x10000000
	}
}