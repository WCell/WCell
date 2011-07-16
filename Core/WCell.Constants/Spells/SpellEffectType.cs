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
		LeapForward = 29,
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
		/// <summary>
		/// Former SummonWild.
		/// Now used in a lot of leaping and pouncing spells
		/// </summary>
		Leap2,
		/// <summary>
		/// Former SummonGuardian, used similarly to Leap2
		/// </summary>
		Leap3,
		TeleportUnitsFaceCaster,
		SkillStep = 44,
		AddHonor,
		Spawn,
		TradeSkill,
		Stealth = 48,
		Detect,
		SummonObject = 50,
		Unused_ForceCriticalHit,
		Unused_GuaranteeHit,
		EnchantItem = 53,
		EnchantItemTemporary = 54,
		TameCreature = 55,
		SummonPet = 56,
		LearnPetSpell,
		WeaponDamage = 58,
		OpenLockItem,
		Proficiency = 60,
		SendEvent = 61,
		PowerBurn,
		Threat = 63,
		TriggerSpell = 64,
		/// <summary>
		/// Applies to everyone in Raid (in radius, if given)
		/// </summary>
		ApplyRaidAura = 65,
		CreateManaGem = 66,
		HealMaxHealth = 67,
		InterruptCast = 68,
		Distract = 69,
		Pull = 70,
		Pickpocket,
		AddFarsight,
		Unused_SummonPossessed,
		/// <summary>
		/// MiscValue = GlyphProperties.dbc
		/// </summary>
		ApplyGlyph = 74,
		HealMechanical,
		SummonObjectWild = 76,
		ScriptEffect,
		Attack,
		Sanctuary = 79,
		AddComboPoints = 80,
		CreateHouse,
		BindSight,
		Duel = 83,
		Stuck,
		SummonPlayer = 85,
		ActivateObject,
		WMODamage = 87,
		WMORepair = 88,
		WMOChange = 89,
		KillCreditPersonal = 90,
		Unused_ThreatAll = 91,
		EnchantHeldItem = 92,
		Unused_SummonPhantasm = 93,
		SelfResurrect = 94,
		Skinning = 95,
		Charge = 96,
		SummonAllTotems = 97,
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
		PlayMusic = 132,
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
		/// Used by spells for the ring in IceCrown
		/// </summary>
		ClearQuest = 139,
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
		Unused_Effect_148 = 148, // Used by only one spell : Orb Of Fire (43509)

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
		TeachFlightPath = 154, // Used only by TeachRiversHeartTaxiPath (64090)
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