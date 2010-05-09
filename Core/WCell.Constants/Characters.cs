using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants
{
	[Flags]
	public enum CharacterStatus : byte
	{
		OFFLINE = 0,
		ONLINE = 1,
		AFK = 2,
		DND = 4
	}

	[Flags]
	public enum PlayerFlags : uint
	{
		None = 0,
		GroupLeader = 0x1,//2.4.2
		AFK = 0x2, // 2.4.2
		DND = 0x4, // 2.4.2
		GM = 0x8,
		Ghost = 0x10,
		Resting = 0x20,
		Flag_0x40 = 0x40,
		FreeForAllPVP = 0x80, // 2.4.2
		InPvPSanctuary = 0x100, // 2.4.2
		PVP = 0x200,
		HideHelm = 0x400,
		HideCloak = 0x800,
		PartialPlayTime = 0x1000, //played long time
		NoPlayTime = 0x2000, //played too long time
		OutOfBounds = 0x4000, // Lua_IsOutOfBounds
		Developer = 0x8000,
		Flag_0x10000 = 0x10000,
		Flag_0x20000 = 0x20000, // Taxi Time Test
		PVPTimerActive = 0x40000,
	}


	/// <summary>
	/// The Ids for the different races
	/// </summary>
	/// <remarks>Values come from column 1 of ChrClasses.dbc</remarks>
	public enum ClassId : uint
	{
		PetTalents = 0,
		Warrior = 1,
		Paladin = 2,
		Hunter = 3,
		Rogue = 4,
		Priest = 5,
		DeathKnight = 6,
		Shaman = 7,
		Mage = 8,
		Warlock = 9,
		//??	= 10
		Druid = 11,
		End
	}

	/// <summary>
	/// The mask is the corrosponding ClassId ^2 - 1
	/// </summary>
	[Flags]
	public enum ClassMask : uint
	{
		None = 0,
		Warrior = 0x0001,
		Paladin = 0x0002,
		Hunter = 0x0004,
		Rogue = 0x0008,

		Priest = 0x0010,
		DeathKnight = 0x0020,
		Shaman = 0x0040,
		Mage = 0x0080,

		Warlock = 0x0100,
		//unk = 0x0200,
		Druid = 0x0400,

		AllClasses1 = 0x7FFF,
		AllClasses2 = uint.MaxValue
	}

	/// <summary>
	/// The mask is used when translating PacketIn ClassMask values. 
	/// Similar to ClassMask but its values have twice the original value.
	/// </summary>
	[Flags]
	public enum ClassMask2 : uint
	{
		None = 0x0000,
		Warrior = 0x0002,
		Paladin = 0x0004,
		Hunter = 0x0008,
		Rogue = 0x00010,
		Priest = 0x0020,
		//unk = 0x0040,
		Shaman = 0x0080,
		Mage = 0x0100,
		Warlock = 0x0200,
		//unk = 0x0400,
		Druid = 0x0800,
		All = 0xFFFFFFFF
	}

	/// <summary>
	/// The Ids for the different races
	/// </summary>
	/// <remarks>Values come from column 1 of ChrRaces.dbc</remarks>
	public enum RaceId
	{
		None = 0,
		Human = 1,
		Orc = 2,
		Dwarf = 3,
		NightElf = 4,
		Undead = 5,
		Tauren = 6,
		Gnome = 7,
		Troll = 8,
		Goblin = 9,
		BloodElf = 10,
		Draenei = 11,
		FelOrc = 12,
		Naga = 13,
		Broken = 14,
		Skeleton = 15,
		End
	}

	/// <summary>
	/// The mask is the corrosponding RaceTypes-value ^2 - 1
	/// </summary>
	[Flags]
	public enum RaceMask : uint
	{
		Human = 0x00000001,
		Orc = 0x00000002,
		Dwarf = 0x00000004,
		NightElf = 0x00000008,
		Undead = 0x00000010,
		Tauren = 0x00000020,
		Gnome = 0x00000040,
		Troll = 0x00000080,
		Goblin = 0x00000100,
		BloodElf = 0x00000200,
		Draenei = 0x00000400,
		FelOrc = 0x00000800,
		Naga = 0x00001000,
		Broken = 0x00002000,
		Skeleton = 0x00004000,

		AllRaces1 = uint.MaxValue,
		AllRaces2 = 0x7FFF
	}

	/// <summary>
	/// The mask is used when translating PacketIn RaceMask values.
	///  Similar to RaceMask but its values have twice the original value.
	/// </summary>
	[Flags]
	public enum RaceMask2 : uint
	{
		None = 0x00000000,
		Human = 0x00000002,
		Orc = 0x00000004,
		Dwarf = 0x00000008,
		NightElf = 0x00000010,
		Undead = 0x00000020,
		Tauren = 0x00000040,
		Gnome = 0x00000080,
		Troll = 0x00000100,
		Goblin = 0x00000200,
		BloodElf = 0x00000400,
		Draenei = 0x00000800,
		FelOrc = 0x00001000,
		Naga = 0x00002000,
		Broken = 0x00004000,
		Skeleton = 0x00008000,
		All = 0xFFFFFFFF
	}

	/// <summary>
	/// The gender types
	/// </summary>
	public enum GenderType : int
	{
		Male = 0,
		Female = 1,
		Neutral = 2
	}

	/// <summary>
	/// The power types (Mana, Rage, Energy, Focus, Happiness)
	/// </summary>
	public enum PowerType
	{
		Health = -2,
		Mana = 0,
		Rage = 1,
		Focus = 2,
		Energy = 3,
		Happiness = 4,
		Runes = 5,
		RunicPower = 6,
		Count
	}



	/// <summary>
	/// Mask for
	/// <remarks>Values are from column 20 of CharTitles.dbc. (1 lsh value) </remarks>
	/// </summary>
	public enum CharTitlesMask : ulong
	{
		Disabled = 0x00000000,
		None = 0x00000001,
		Private = 0x00000002,
		Corporal = 0x00000004,
		SergeantAlliance = 0x00000008,
		MasterSergeant = 0x00000010,
		SegeantMajor = 0x00000020,
		Knight = 0x00000040,
		KnightLieutenant = 0x00000080,
		KnightCaptain = 0x00000100,
		KnightChampion = 0x00000200,
		LieutenantCommander = 0x00000400,
		Commander = 0x00000800,
		Marshal = 0x00001000,
		FieldMarshal = 0x00002000,
		GrandMarshal = 0x00004000,
		Scout = 0x00008000,
		Grunt = 0x00010000,
		Sergeant = 0x00020000,
		SeniorSergeant = 0x00040000,
		FirstSergeant = 0x00080000,
		StoneGuard = 0x00100000,
		BloodGuard = 0x00200000,
		Legionnaire = 0x00400000,
		Centurion = 0x00800000,
		Champion = 0x01000000,
		LieutenantGeneral = 0x02000000,
		General = 0x04000000,
		Warlord = 0x08000000,
		HighWarlord = 0x10000000,

		Gladiator = 0x20000000,
		Duelist = 0x40000000,
		Rival = 0x80000000,
		Challenger = 0x100000000,
		ScarabLord = 0x200000000,
		Conqueror = 0x400000000,
		Justicar = 0x800000000,

		ChampionOfTheNaaru = 0x1000000000,
		MercilessGladiator = 0x2000000000,
	}
}
