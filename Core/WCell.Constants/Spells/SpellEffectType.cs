using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Spells
{
	public enum SpellEffectType
	{
		None = 0,
		InstantKill = 1,
		SchoolDamage = 2,
		Dummy = 3,
		Unused_PortalTeleport = 4,
		TeleportUnits = 5,
		/// <summary>
		/// MiscValue: AuraId
		/// MiscValueB: StatType
		/// </summary>
		ApplyAura = 6,
		EnvironmentalDamage = 7,
		PowerDrain = 8,
		HealthLeech = 9,
		Heal = 10,
		Bind = 11,
		Portal = 12,
		Unused_RitualBase = 13,
		Unused_RitualSpecialize = 14,
		Unused_RitualActivatePortal = 15,
		QuestComplete = 16,
		WeaponDamageNoSchool = 17,
		Resurrect = 18,
		AddExtraAttacks = 19,
		Dodge = 20,
		Evade = 21,
		Parry = 22,
		Block = 23,
		CreateItem = 24,
		Weapon = 25,
		Defense = 26,
		PersistantAreaAura = 27,
		/// <summary>
		/// MiscValue: Pet entry
		/// MiscValueB: Index in SummonProperties.dbc
		/// </summary>
		Summon = 28,
		Leap = 29,
		Energize = 30,
		WeaponPercentDamage = 31,
		TriggerMissile = 32,
		OpenLock = 33,
		TransformItem = 34,
		/// <summary>
		/// Mobile AreaAura.
		/// Totems and Paladins mostly
		/// </summary>
		ApplyAreaAura = 35,
		LearnSpell = 36,
		SpellDefense = 37,
		Dispel = 38,
		Language = 39,
		DualWeild = 40,
		Unused_SummonWild,
		Unused_SummonGuardian,
		TeleportUnitsFaceCaster,
		SkillStep,
		AddHonor,
		Spawn,
		TradeSkill,
		Stealth,
		Detect,
		SummonObject = 50,
		Unused_ForceCriticalHit,
		Unused_GuaranteeHit,
		EnchantItem,
		EnchantItemTemporary,
		TameCreature,
		SummonPet,
		LearnPetSpell,
		WeaponDamage,
		OpenLockItem,
		Proficiency = 60,
		SendEvent,
		PowerBurn,
		Threat,
		TriggerSpell,
		/// <summary>
		/// Applies to everyone in Group (in radius, if given)
		/// </summary>
		ApplyGroupAura,
		CreateManaGem,
		HealMaxHealth,
		InterruptCast,
		Distract,
		Pull = 70,
		Pickpocket,
		AddFarsight,
		Unused_SummonPossessed,
		/// <summary>
		/// MiscValue = GlyphProperties.dbc
		/// </summary>
		ApplyGlyph = 74,
		HealMechanical,
		SummonObjectWild,
		ScriptEffect,
		Attack,
		Sanctuary,
		AddComboPoints = 80,
		CreateHouse,
		BindSight,
		Duel,
		Stuck,
		SummonPlayer,
		ActivateObject,
		Unused_SummonTotemSlot1,
		Unused_SummonTotemSlot2,
		Unused_SummonTotemSlot3,
		Unused_SummonTotemSlot4 = 90,
		Unused_ThreatAll = 91,
		EnchantHeldItem = 92,
		Unused_SummonPhantasm = 93,
		SelfResurrect = 94,
		Skinning = 95,
		Charge = 96,
		Unused_SummonCritter = 97,
		KnockBack = 98,
		Disenchant = 99,
		Inebriate = 100,
		FeedPet = 101,
		DismissPet = 102,
		Reputation = 103,
		SummonObjectSlot1 = 104,
		SummonObjectSlot2 = 105,
		SummonObjectSlot3 = 106,
		SummonObjectSlot4 = 107,
		DispelMechanic = 108,
		SummonDeadPet = 109,
		DestroyAllTotems = 110,
		DurabilityDamage = 111,
		Unused_SummonDemon = 112,
		ResurrectFlat = 113,
		AttackMe = 114,
		DurabilityDamagePercent = 115,
		SkinPlayerCorpse = 116,
		SpiritHeal = 117,
		Skill = 118,
		ApplyPetAura = 119,
		TeleportGraveyard = 120,
		NormalizedWeaponDamagePlus = 121,
		/// <summary>
		/// Unused
		/// </summary>
		Unused_Effect_122 = 122,
		/// <summary>
		/// Scripted Event?
		/// </summary>
		Video = 123,
		/// <summary>
		/// Pulls the target towards the caster
		/// </summary>
		PlayerPull = 124,
		ReduceThreatPercent = 125,
		/// <summary>
		/// Spellsteal
		/// </summary>
		StealBeneficialBuff = 126,
		Prospecting = 127,
		ApplyStatAura = 128,
		ApplyStatAuraPercent = 129,
		/// <summary>
		/// Effect 3 of spell 34477, Misdirection
		/// </summary>
		RedirectThreat = 130,
		Effect_131 = 131, // 44393, 44393
		/// <summary>
		/// Unused
		/// </summary>
		Effect_132 = 132, //Play Music
		ForgetSpecialization = 133,
		Effect_134 = 134, // Kill Credit
		Effect_135 = 135, //Call-summon pet
		/// <summary>
		/// Heal %?
		/// </summary>
		RestoreHealthPercent = 136,
		RestoreManaPercent = 137,
		/// <summary>
		/// Something about leaping
		/// </summary>
		Effect_138 = 138, //Leap
		/// <summary>
		/// Unused
		/// </summary>
		Unused_Effect_139 = 139, // Clear Quest
		/// <summary>
		/// Weird name, from wowhead
		/// </summary>
		TriggerSpellFromTargetWithCasterAsTarget = 140,
		Effect_141 = 141, // Damage and Reduced Speed (Blood Bolt)
		/// <summary>
		/// Deals with branching targets
		/// </summary>
		Effect_142 = 142, //Prayer of Mending, Spell Aura Jump and Heal - heal after hit?
		/// <summary>
		/// Soul link and Demonic Knowledge
		/// </summary>
		ApplyAuraToMaster = 143,
		Effect_144 = 144, // PushBack
		Effect_145 = 145, // Black Hole Effect, Gravity Well Effect
		ActivateRune = 146, //EmpowerRune
		QuestFail = 147, // Quest Fail
		Unused_Effect_148 = 148,

		SideLeap = 149, //Sliding, Side leap
		Unused_Effect_150 = 150,
		TriggerRitualOfSummoning = 151,
		/// <summary>
		/// Spell 45927, Alows you to summon your Refer-A-Friend.
		/// </summary>
		SummonReferAFriend = 152,
		/// <summary>
		/// Tame Creature
		/// </summary>
		Effect_153 = 153, // Highest as of 2.4.3.8606
		Unused_154 = 154,
		/// <summary>
		/// Allows 2h weapons to be carried in 1h and applied an aura
		/// </summary>
		Allow2HWeaponIn1HAndApplyAura = 155, // Dual wield 1H
		AddPrismaticGem = 156, // Add Socket
		CreateItem2 = 157, // Create Item
		Milling = 158,
		RenamePet = 159, // Highest in 3.0.2.9056
		Effect_160 = 160,
		SetNumberOfTalentGroups = 161,
		ActivateTalentGroup = 162, // Highest as of 0.1.0.9626
		End = 250
	}
}