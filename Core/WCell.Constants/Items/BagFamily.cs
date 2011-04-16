using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Items
{
	public enum ItemBagFamily
	{
		None = 0,
		Arrows = 1,
		Bullets = 2,
		SoulShards = 3,
		Leatherworking = 4,
		Unused = 5,
		Herbs = 6,
		Enchanting = 7,
		Engineering = 8,
		Keys = 9,
		Gems = 10,
		Mining = 11,
        SoulboundEquipment = 12,
        VanityPets = 13,
        CurrencyTokens = 14,
        QuestItems = 15
	}

	[Flags]
	public enum ItemBagFamilyMask
	{
		None = 0,
		Arrows = 1,
		Bullets = 2,
		SoulShards = 4,
		Leatherworking = 8,
		Unused = 0x0010,
		Herbs = 0x0020,
		Enchanting = 0x0040,
		Engineering = 0x0080,
		Keys = 0x0100,
		Gems = 0x0200,
		Mining = 0x0400,
		Soulbound = 0x0800,
		VanityPets = 0x1000,
        CurrencyTokens = 0x2000,
        QuestItems = 0x4000,
	}
}