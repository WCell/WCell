using System;

namespace WCell.Constants
{
	public enum ClientId
	{
		/// <summary>
		/// Classic WoW
		/// </summary>
		Original = 0,
		/// <summary>
		/// The Burning Crusade
		/// </summary>
		TBC = 1,
		/// <summary>
		/// Wrath of the Lich King
		/// </summary>
		Wotlk = 2
	}

	public enum SkillCategory
	{
		/// <summary>
		/// Only "Pet - Raveger" and "Pet Event - Remote Control"
		/// </summary>
		Invalid = -1,
		/// <summary>
		/// Strength, Agility, etc.
		/// </summary>
		Attribute = 5,
		/// <summary>
		/// Bow, Gun, Sword, etc.
		/// </summary>
		WeaponProficiency = 6,
		/// <summary>
		/// Any kind of ability + spell that are obtainable through vendors and talents
		/// </summary>
		ClassSkill = 7,
		/// <summary>
		/// Cloth, Leather, Mail, Plate
		/// </summary>
		ArmorProficiency = 8,
		/// <summary>
		/// Cooking, First Aid, Fishing, Riding
		/// </summary>
		SecondarySkill = 9,
		/// <summary>
		/// Common, Orcish, Darnassian, Gutterspeak, etc.
		/// </summary>
		Language = 10,
		/// <summary>
		/// Alchemy, Blacksmithing, Engineering, Leatherworking, Tailoring, Jewelcrafting,
		/// Herbalism, Mining, Skinning, Enchanting, Inscription
		/// </summary>
		Profession = 11,
		NotDisplayed = 12
	}

    public enum SkillAcquireMethod
    {
        OnLearningSkill = 1,
        InitialClassOrRaceSkill = 2,
    }

	public enum ChatTag : byte
	{
		None = 0,
		AFK = 1,
		//QA = 2,
		DND = 3,
		GM = 4
	}

	// May be only used in client
	public enum UnitNameType
	{
		UNITNAME_TITLE_PET = 1,
		UNITNAME_TITLE_MINION,
		UNITNAME_TITLE_GUARDIAN,
		UNITNAME_TITLE_CREATION,
		UNITNAME_TITLE_COMPANION,
	}

    public enum TradeStatus
    {
        PlayerBusy = 0,
        /// <summary>
        /// Client will send CMSG_BUSY_TRADE if already in a trade
        /// </summary>
        Proposed = 1,
        Initiated = 2,
        Cancelled = 3,
        Accepted = 4,
        AlreadyTrading = 5,
        PlayerNotFound = 6,
        StateChanged = 7,
        Complete = 8,
        Denied = 9,
        /// <summary>
        /// "Trade target is too far away."
        /// </summary>
        TooFarAway = 10,
        /// <summary>
        /// "Target is unfriendly."
        /// </summary>
        WrongFaction = 11,
        Failed = 12,
        TargetDead = 13,
        Petition = 14,
        PlayerIgnored = 15,
        /// <summary>
        /// "Target is stunned"; -- Trade failure
        /// </summary>
        TargetStunned = 16,
        /// <summary>
        /// "You can't do that when you're dead."
        /// </summary>
        PlayerDead = 17,
        /// <summary>
        /// "You can't trade with dead players."
        /// </summary>
        TradeTargetDead = 18,
        /// <summary>
        /// "You are logging out"
        /// </summary>
        LoggingOut = 19,
        /// <summary>
        /// "That player is logging out"
        /// </summary>
        TargetLoggingOut = 20,
        /// <summary>
        /// "Trial accounts cannot perform that action"
        /// </summary>
        TrialAccountRestriction = 21,
        /// <summary>
        /// "You may only trade conjured items to players from other realms"
        /// </summary>
        WrongRealm = 22,
    }

	public enum TradeData
	{
		Give = 0x00,
		Receive = 0x01,
	}

	public enum ChairHeight
	{
		Low = 0,
		Medium = 1,
		High = 2
	}

	/// <summary>
	/// Results of char relation related commands
	/// </summary>
	public enum RelationResult
	{
		FRIEND_DB_ERROR = 0x00,
		FRIEND_LIST_FULL = 0x01,
		FRIEND_ONLINE = 0x02,
		FRIEND_OFFLINE = 0x03,
		FRIEND_NOT_FOUND = 0x04,
		FRIEND_REMOVED = 0x05,
		FRIEND_ADDED_ONLINE = 0x06,
		FRIEND_ADDED_OFFLINE = 0x07,
		FRIEND_ALREADY = 0x08,
		FRIEND_SELF = 0x09,
		FRIEND_ENEMY = 0x0A,
		IGNORE_LIST_FULL = 0x0B,
		IGNORE_SELF = 0x0C,
		IGNORE_NOT_FOUND = 0x0D,
		IGNORE_ALREADY = 0x0E,
		IGNORE_ADDED = 0x0F,
		IGNORE_REMOVED = 0x10,
		IGNORE_AMBIGUOUS = 0x11,
		MUTED_LIST_FULL = 0x12,
		MUTED_SELF = 0x13,
		MUTED_NOT_FOUND = 0x14,
		MUTED_ALREADY = 0x15,
		MUTED_ADDED = 0x16,
		MUTED_REMOVED = 0x17,
		MUTED_AMBIGUOUS = 0x18,
		UNKNOWN_1 = 0x19
	}

	[Flags]
	public enum CorpseReleaseFlags : byte
	{
		None = 0,
		TrackStealthed = 0x02,
		ShowCorpseAutoReleaseTimer = 0x8,
		HideReleaseWindow = 0x10
	}

	public enum MapTransferError
	{
        TRANSFER_ABORT_NONE = 0x00,
        TRANSFER_ABORT_ERROR = 0x01,
        TRANSFER_ABORT_MAX_PLAYERS = 0x02,                // Transfer Aborted: instance is full
        TRANSFER_ABORT_NOT_FOUND = 0x03,                  // Transfer Aborted: instance not found
        TRANSFER_ABORT_TOO_MANY_INSTANCES = 0x04,         // You have entered too many instances recently.
        TRANSFER_ABORT_ZONE_IN_COMBAT = 0x06,             // Unable to zone in while an encounter is in progress.
        TRANSFER_ABORT_INSUF_EXPAN_LVL = 0x07,            // You must have <TBC,WotLK> expansion installed to access this area.
        TRANSFER_ABORT_DIFFICULTY = 0x08,                 // <Normal,Heroic,Epic> difficulty mode is not available for %s.
        TRANSFER_ABORT_UNIQUE_MESSAGE = 0x09,             // Until you've escaped TLK's grasp, you cannot leave this place!
        TRANSFER_ABORT_TOO_MANY_REALM_INSTANCES = 0x0A,   // Additional instances cannot be launched, please try again later.
        TRANSFER_ABORT_NEED_GROUP = 0x0B,                 // 3.1
        TRANSFER_ABORT_UNK_1 = 0x0C,                      // 3.1
        TRANSFER_ABORT_UNK_2 = 0x0D,                      // 3.1
        TRANSFER_ABORT_UNK_3 = 0x0E,                      // 3.2
        TRANSFER_ABORT_REALM_ONLY = 0x0F,                 // All players on party must be from the same realm.
        TRANSFER_ABORT_MAP_NOT_ALLOWED = 0x10,            // Map can't be entered at this time.
	}

	public enum Material
	{
		None = 0,
		None2 = -1,
		Metal = 1,
		Wood = 2,
		Gem = 3,
		Cloth = 7
	}

	public enum PageMaterial
	{
		None2 = -1,
		None = 0,
		Parchment = 1,
		Stone = 2,
		Marble = 3,
		Silver = 4,
		Bronze = 5,
		Valentine = 6
	}

	/// <summary>
	/// Used in PLAYER_BYTES_2, 4th Byte
	/// </summary>
	public enum RestState : byte
	{
		Normal = 0x01,
		Resting = 0x02,
	}
    
	/// <summary>
	/// 
	/// </summary>
	public enum LockInteractionGroup
	{
		None = 0,
		Key,
		/// <summary>
		/// See LockType.DBC
		/// </summary>
		Profession
	}

	/// <summary>
	/// Mask of LockInteractionType
	/// </summary>
	[Flags]
	public enum LockMask : uint
	{
		None = 0,
	}

	/// <summary>
	/// See LockType.dbc
	/// </summary>
	public enum LockInteractionType
	{
		None = 0,
		PickLock = 1,
		Herbalism = 2,
		Mining = 3,
		DisarmTrap = 4,
		Open = 5,
		Treasure = 6,
		ElvenGems = 7,
		Close = 8,
		ArmTrap = 9,
		QuickOpen = 10,
		QuickClose = 11,
		OpenTinkering = 12,
		OpenKneeling = 13,
		OpenAttacking = 14,

		/// <summary>
		/// For tracking Gahz'ridian and in GO:
		/// 
		/// Name: Gahz'ridian
		/// Id: 140971
		/// </summary>
		GahzRidian = 15,
		Blasting = 16,
		PvPOpen = 17,
		PvPClose = 18,
		Fishing = 19,
		Inscription = 20,
		OpenFromVehicle = 21,
	}

	[Flags]
	public enum ZoneFlags : uint
	{
		None = 0,
		Unk_0x1 = 0x1,
		Unk_0x2 = 0x2,		// Only used by Naxxramas (3456) and Razorfen Downs (722)
		Unk_0x4 = 0x4,		// Only used by Dragonblight (65), Zul'Drak (66), Icecrown (210), Crystalsong Forest (2817) 
							//	and Hrothgar's Landing (4742)
		Capital = 0x8,		// Used by capitals (Ironforge, Darnassus, Orgrimmar, Thunder Bluff, Stormwind, Silvermoon City, etc...)
		Unk_0x10 = 0x10,	// Used by playable faction capitals and Strand Of The Ancients (4384)
		Capital2 = 0x20,	// Used by capitals and its subzones (like Hall Of Legends (2917) and Champions' Hall (2918))

		Duel = 0x40,		// Duels are allowed

		/// <summary>
		/// Let's fight!
		/// </summary>
		Arena = 0x80,

		/// <summary>
		/// 
		/// </summary>
		CapitalCity = 0x100, // Used by capitals
		Unk_0x200 = 0x200, // Used by only one zone named City (3459). It doesn't have a map so where is it ?

		CanFly = 0x400, // Used by Outland and Northrend zones

		/// <summary>
		/// No harm in this zone
		/// </summary>
		Sanctuary = 0x800,

		/// <summary>
		/// Areas with this flag are up on in the air. when you die you respawn alive at the graveyard
		/// </summary>
		RespawnNoCorpse = 0x1000,
		Unk_0x4000 = 0x4000,

		/// <summary>
		/// Unused in 3.3.5a (was PVPObjectiveArea before)
		/// </summary>
		Unused_0x8000 = 0x8000,

		InstancedArena = 0x10000, // Used only by instanced arenas
		/// <summary>
		/// Unused in 3.5.5a (was AlwaysContested)
		/// </summary>
		Unused_0x40000 = 0x40000,
		PlayableFactionCapital = 0x200000, // Used by playable faction capitals (Orgrimmar, Stormwind, etc)
		OutdoorPvP = 0x1000000, // Used only by Wintergrasp (4917) in 3.3.5a
		Unk_0x2000000 = 0x2000000,
		Unk_0x4000000 = 0x4000000, // Used by four maps (Ahn'Qiraj, Dalaran Arena, The Ring Of Valor and Undercity)

		CanHearthAndResurrectFromArea = 0x8000000, // Used only by Wintergrasp (4917)
		CannotFly = 0x20000000 // Used by Dalaran (4395)
	}

	public enum EnviromentalDamageType : uint
	{
		Exhausted = 0,
		Drowning = 1,
		Fall = 2,
		Lava = 3,
		Slime = 4,
		Fire = 5
	};

	public enum MapType
	{
		Normal = 0,
		Dungeon = 1,
		Raid = 2,
		Battleground = 3,
		Arena = 4
	}

	#region Dueling
	public enum DuelTeam : uint
	{
		Challenger = 1,
		Rival = 2
	}

	public enum DuelWin : byte
	{
		Knockout = 0,
		OutOfRange = 1
	}
	#endregion

	#region Corpses
	[Flags]
	public enum CorpseFlags
	{
		None = 0,
		IsClaimed = 0x1,
		CorpseFlag_0x0002 = 0x2,
		Bones = 0x4,
		CorpseFlag_0x0008 = 0x8,
		CorpseFlag_0x0010 = 0x10,
		CorpseFlag_0x0020 = 0x20,
		CorpseFlag_0x0040 = 0x40,
	}

	[Flags]
	public enum CorpseDynamicFlags
	{
		None = 0,
		/// <summary>
		/// "Sparks" emerging from the Corpse
		/// </summary>
		PlayerLootable = 0x1
	}
	#endregion

	public enum StatType
	{
		None = -1,
		Strength = 0,
		Agility = 1,
		Stamina = 2,
		Intellect = 3,
		Spirit = 4,
		End
	}

	public enum StatModifierFloat
	{
		BlockValue,
		MeleeAttackTime,
		RangedAttackTime,
		HealthRegen,
	}

	public enum StatModifierInt
	{
		Power = 1,

		PowerPct,

		/// <summary>
		/// Overall HealthRegen
		/// </summary>
		HealthRegen,
		/// <summary>
		/// HealthRegen during Combat
		/// </summary>
		HealthRegenInCombat,
		/// <summary>
		/// HealthRegen while not in Combat
		/// </summary>
		HealthRegenNoCombat,
		
		PowerRegen,
		PowerRegenPercent,
		BlockChance,
		BlockValue,
        ParryChance,
        RangedCritChance,
		HitChance,

		DodgeChance,

		CritDamageBonusPct,

		AttackerMeleeHitChance,
		AttackerRangedHitChance,
		/// <summary>
		/// A negative chance that reduces target's dodge chance
		/// </summary>
		TargetDodgesAttackChance,

		CriticalHealValuePct,
		ManaRegenInterruptPct,

		Expertise
	}

	public enum AreaTriggerType : uint
	{
		None = 0,
		Instance = 1,
		QuestTrigger = 2,
		Rest = 3,
		Teleport = 4,
		Spell,
		Battleground,
		End
	}
    
	public enum AccountDataType
	{
		GlobalConfigCache = 0,
		PerCharacterConfigCache = 1,
		GlobalBindingsCache = 2,
		PerCharacterBindingsCache = 3,
		GlobalMacrosCache = 4,
		PerCharacterMacrosCache = 5,
		Layout = 6,
		ChatCache = 7,
	}

	public enum PetitionType
	{
		None = 0,
		Arena2vs2 = 2,
		Arena3vs3 = 3,
		Arena5vs5 = 5,
        Guild = 9
	}

    public enum PetitionTurns : uint
    {
        OK = 0,
        ALREADY_IN_GUILD = 2,
        NEED_MORE_SIGNATURES = 4,
    }

    public enum PetitionSigns : uint
    {
        OK = 0,
        ALREADY_SIGNED = 1,
        ALREADY_IN_GUILD = 2,
        CANT_SIGN_OWN = 3,
        NOT_SERVER = 4,
    }

	// extract from ServerMessages.dbc
	public enum ServerMessagesType
	{
		ServerShutdownStart = 1,
		ServerRestartStart = 2,
		Custom = 3,
		ServerShutdownCancelled = 4,
		ServerRestartCancelled = 5,
		BattlegroundShutdown = 6,
		BattlegroundRestart = 7,
		InstanceShutdown = 8,
		InstanceRestart = 9
	}
}