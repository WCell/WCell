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
        Vrykul = 16,
        Tuskarr = 17,
        ForestTroll = 18,
        Taunka = 19,
        NorthrendSkeleton = 20,
        IceTroll = 21,
        Worgen = 22,
        Gilnean = 23,
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
		End
	}



	/// <summary>
	/// Mask for
	/// <remarks>Values are from column 36 of CharTitles.dbc. (1 lsh value) </remarks>
	/// </summary>
	[Flags]
	public enum CharTitlesMask : ulong
	{
        Disabled = 0x0000000000000000,
        None = 0x0000000000000001,
        Private = 0x0000000000000002, // 1
        Corporal = 0x0000000000000004, // 2
        SergeantA = 0x0000000000000008, // 3
        MasterSergeant = 0x0000000000000010, // 4
        SergeantMajor = 0x0000000000000020, // 5
        Knight = 0x0000000000000040, // 6
        KnightLieutenant = 0x0000000000000080, // 7
        KnightCaptain = 0x0000000000000100, // 8
        KnightChampion = 0x0000000000000200, // 9
        LieutenantCommander = 0x0000000000000400, // 10
        Commander = 0x0000000000000800, // 11
        Marshal = 0x0000000000001000, // 12
        FieldMarshal = 0x0000000000002000, // 13
        GrandMarshal = 0x0000000000004000, // 14
        Scout = 0x0000000000008000, // 15
        Grunt = 0x0000000000010000, // 16
        SergeantH = 0x0000000000020000, // 17
        SeniorSergeant = 0x0000000000040000, // 18
        FirstSergeant = 0x0000000000080000, // 19
        StoneGuard = 0x0000000000100000, // 20
        BloodGuard = 0x0000000000200000, // 21
        Legionnaire = 0x0000000000400000, // 22
        Centurion = 0x0000000000800000, // 23
        Champion = 0x0000000001000000, // 24
        LieutenantGeneral = 0x0000000002000000, // 25
        General = 0x0000000004000000, // 26
        Warlord = 0x0000000008000000, // 27
        HighWarlord = 0x0000000010000000, // 28
        Gladiator = 0x0000000020000000, // 29
        Duelist = 0x0000000040000000, // 30
        Rival = 0x0000000080000000, // 31
        Challenger = 0x0000000100000000, // 32
        ScarabLord = 0x0000000200000000, // 33
        Conqueror = 0x0000000400000000, // 34
        Justicar = 0x0000000800000000, // 35
        ChampionOfTheNaaru = 0x0000001000000000, // 36
        MercilessGladiator = 0x0000002000000000, // 37
        OfTheShatteredSun = 0x0000004000000000, // 38
        HandOfAdal = 0x0000008000000000, // 39
        VengefulGladiator = 0x0000010000000000, // 40
	}
}