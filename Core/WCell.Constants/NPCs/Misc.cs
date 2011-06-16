using System;

namespace WCell.Constants.NPCs
{
	public enum AIType
	{
		Loner,
		Aggro,
		Social,
		Pet,
		Totem,
		Guardian
	}

	public enum AIMovementType
	{
		ForwardThenBack,
		ForwardThenStop,
		ForwardThenFirst
	}

	public enum AIMovementState
	{
		Move,
		Follow,
		Stop,
		ForcedStop,
		FollowOwner
	}

	public enum AICreatureState
	{
		Stopped,
		Moving,
		Attacking
	}

	[Flags]
	public enum AIMoveType
	{
		Walk,
		Run,
		Sprint,
		Fly
	}

	[Flags]
	public enum MonsterMoveFlags : uint
	{
		None = 0,
		Flag_0x1 = 0x1,
		Flag_0x2 = 0x2,
		Flag_0x4 = 0x4,
		Flag_0x8 = 0x8,
		Flag_0x10 = 0x10,
		Flag_0x20 = 0x20,
		Flag_0x40 = 0x40,
		Flag_0x80 = 0x80,
		Flag_0x100 = 0x100,
		Flag_0x200 = 0x200,
		Flag_0x400 = 0x400,

		/// <summary>
		/// Adds a float and uint to the packet
		/// </summary>
		Flag_0x800 = 0x800,

		/// <summary>
		/// The monster will run unless this is set
		/// </summary>
		Walk = 0x1000,
		/// <summary>
		/// Client expects points in full vector3 format
		/// </summary>
		Flag_0x2000_FullPoints_1 = 0x2000,

		Flag_0x4000 = 0x4000,
		Flag_0x8000 = 0x8000,
		Flag_0x10000 = 0x10000,
		Flag_0x20000 = 0x20000,

		/// <summary>
		/// Client expects points in full vector3 format
		/// </summary>
		Flag_0x40000_FullPoints_2 = 0x40000,
		Flag_0x80000 = 0x80000,
		Flag_0x100000 = 0x100000,
		/// <summary>
		/// Adds a byte and uint to the packet
		/// </summary>
		Flag_0x200000 = 0x200000,

		Flag_0x400000 = 0x400000,
		Flag_0x800000 = 0x800000,
		Flag_0x1000000 = 0x1000000,
		Flag_0x2000000 = 0x2000000,
		Flag_0x4000000 = 0x4000000,
		Flag_0x8000000 = 0x8000000,
		Flag_0x10000000 = 0x10000000,
		Flag_0x20000000 = 0x20000000,
		FullPoints = 0x40000000,
		Flag_0x80000000 = 0x80000000,

		/// <summary>
		/// The default mask sets the Run flag and the flag that makes the client expect full vector3's for the intermediate points
		/// </summary>
		//DefaultMask = Sprint | Flag_0x2000_FullPoints_1,
        DefaultMask = FullPoints,

        Fly = Flag_0x2000_FullPoints_1 | FullPoints,
	}

	public enum MonsterMoveType
	{
		Normal = 0,
		Stop = 1,
		FinalFacingPoint = 2,
		FinalFacingGuid = 3,
		FinalFacingAngle = 4,
	}

	public enum CreatureType
	{
		None = 0,
		Beast = 1,
		Dragonkin = 2,
		Demon = 3,
		Elemental = 4,
		Giant = 5,
		Undead = 6,
		Humanoid = 7,
		Critter = 8,
		Mechanical = 9,
		NotSpecified = 10,
		Totem = 11,
		NonCombatPet = 12,
		GasCloud = 13,
		End
	}

	/// <summary>
	/// Mask from CreatureType.dbc
	/// </summary>
	[Flags]
	public enum CreatureMask
	{
		None = 0x0,
		Beast = 0x1,
		Dragonkin = 0x2,
		Demon = 0x4,
		Elemental = 0x8,
		Giant = 0x10,
		Undead = 0x20,
		Humanoid = 0x40,
		Critter = 0x80,
		Mechanical = 0x100,
		NotSpecified = 0x200,
		Totem = 0x400,
		NonCombatPet = 0x800,
		GasCloud = 0x1000,
	}

	public enum CreatureRank
	{
		Normal = 0,
		Elite = 1,
		RareElite = 2,
		WorldBoss = 3,
		Rare = 4,
	}

	/// <summary>
	/// Same a NPCFlags, but as a simple (not flag-)field
	/// </summary>
	public enum NPCEntryType
	{
		None = 0,
		Gossip = 1,
		QuestGiver = 2,
		Trainer = 4,
		ClassTrainer = 5,
		ProfessionTrainer = 6,
		Vendor = 7,
		GeneralGoodsVendor = 8,
		FoodVendor = 9,
		PoisonVendor = 10,
		ReagentVender = 11,
		Armorer = 12,
		TaxiVendor = 13,
		SpiritHealer = 14,
		SpiritGuide = 15,
		InnKeeper = 16,
		Banker = 17,
		Petitioner = 18,
		TabardVendor = 19,
		BattleMaster = 20,
		Auctioneer = 21,
		Stable = 22,
		GuildBanker = 23
	}

	/// <summary>
	/// NPC Type Flags
	/// </summary>
	[Flags]
	public enum NPCFlags
	{
		None = 0,
		Gossip = 0x1,
		QuestGiver = 0x2,
		Flag_0x4 = 0x4,
		Flag_0x8 = 0x8,
		UnkTrainer = 0x10,       // 100%
		ClassTrainer = 0x20,       // 100%
		ProfessionTrainer = 0x40,       // 100%
		AnyTrainer = ClassTrainer | ProfessionTrainer | UnkTrainer,
		Vendor = 0x80,       // 100%
		GeneralGoodsVendor = 0x100,       // 100%, general goods vendor
		FoodVendor = 0x200,       // 100%
		PoisonVendor = 0x400,       // guessed
		ReagentVendor = 0x800,       // 100%
		Armorer = 0x1000,       // 100%
		AnyVendor = FoodVendor | GeneralGoodsVendor | PoisonVendor | Armorer | Vendor,
		FlightMaster = 0x2000,       // 100%
		/// <summary>
		/// Makes the unit invisible when player is alive
		/// </summary>
		SpiritHealer = 0x4000,       // guessed
		/// <summary>
		/// Makes the unit invisible when player is alive
		/// </summary>
		SpiritGuide = 0x8000,       // guessed
		InnKeeper = 0x10000,       // 100%
		Banker = 0x20000,       // 100%
		Petitioner = 0x40000,       // 100% 0xC0000 = guild petitions, 0x40000 = arena team petitions
		TabardDesigner = 0x80000,       // 100%
		BattleMaster = 0x100000,       // 100%
		Auctioneer = 0x200000,       // 100%
		StableMaster = 0x400000,       // 100%
		GuildBanker = 0x800000,       // cause client to send 997 opcode
		/// <summary>
		/// Makes the client send CMSG_SPELLCLICK
		/// </summary>
		SpellClick = 0x1000000,       // cause client to send 1015 opcode
	}

	public enum BarberShopPurchaseResult
	{
		Success = 0,
		LowMoney = 1,
		Unknown = 2,
		LowMoney2 = 3,
	}
}