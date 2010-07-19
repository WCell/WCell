/*************************************************************************
 *
 *   file		: SpellEnums.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-05-11 22:39:47 +0800 (Sun, 11 May 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 335 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;

namespace WCell.Constants.Spells
{

    [Flags]
    public enum CastFlags : uint
    {
        None = 0,
        Flag_0x1 = 0x1,
        Flag_0x2 = 0x2,
        Flag_0x4 = 0x4,
        Flag_0x8 = 0x8,
        Flag_0x10 = 0x10,
        Ranged = 0x20,
        Flag_0x40 = 0x40,
        Flag_0x80 = 0x80,
        Flag_0x100 = 0x100,
        Flag_0x200 = 0x200,
        Flag_0x400 = 0x400,
        Flag_0x800 = 0x800,
        Flag_0x10000 = 0x10000,
        Flag_0x20000 = 0x20000,
        Flag_0x40000 = 0x40000,
        Flag_0x80000 = 0x80000,
        Flag_0x100000 = 0x100000,
        Flag_0x200000 = 0x200000,
        Flag_0x4000000 = 0x04000000,
    }

    [Flags]
    public enum SpellLogFlags
    {
        None = 0,
		SpellLogFlag_0x1 = 0x1,
        Critical = 0x2,
		SpellLogFlag_0x4 = 0x4,
		SpellLogFlag_0x8 = 0x8,
		SpellLogFlag_0x10 = 0x10,
		SpellLogFlag_0x20 = 0x20,
    }

	public enum CastMissReason : byte
	{
		None = 0,
		Miss = 1,
		Resist = 2,
		Dodge = 3,
		Parry = 4,
		Block = 5,
		Evade = 6,
		Immune = 7,
        Immune_2 = 8,
		Deflect = 9,
		Absorb = 10,
		Reflect = 11,
	}

	/// <summary>
	/// Used in MiscValueB of SpellEffect.Summon
	/// </summary>
	public enum SummonType
	{
		Pet = 0,
		Critter = 41,
		Guardian = 61,
		/// <summary>
		/// Fire Totems
		/// </summary>
		TotemSlot1 = 63,
		Wild = 64,
		Possessed = 65,
		Demon = 66,
		Summon = 67,
		TotemSlot2 = 81,
		TotemSlot3 = 82,
		/// <summary>
		/// Air Totems
		/// </summary>
		TotemSlot4 = 83,
		Totem = 121,
		Type_181 = 181,
		Type_187 = 187,
		Type_247 = 247,
		Critter2 = 307,
		Critter3 = 407,
		Type_409 = 409,
		Type_427 = 427,
		/// <summary>
		/// Only used in steam tonk summon
		/// </summary>
		SummonAndPossess = 428,
		Guardian2 = 713,
		/// <summary>
		/// Only used for the Priest's LightWell summon
		/// </summary>
		Lightwell = 1141,
		Guardian3 = 1161,

        /// <summary>
        /// Summons the NPC exactly as in the entry, only used for ClassSkillCurseOfDoomEffect
        /// </summary>
        DoomGuard = 1221,
		Elemental = 1561,
		End
	};

    

	[Flags]
	public enum AuraStateMask : uint
	{
		None = 0x0000,
		/// <summary>
		/// Used for Revenge spell
		/// </summary>
		DodgeOrBlockOrParry = 0x0001,

		/// <summary>
		/// Very low on Health
		/// </summary>
		Health20Percent = 0x0002,

		/// <summary>
		/// Going Berserk
		/// Only used by: Berserking Racial (Id: 26635)
		/// </summary>
		Berserk = 0x0004,

		/// <summary>
		/// Frozen
		/// </summary>
		Frozen = 0x0008,

		/// <summary>
		/// Used for judgemental Paladins
		/// </summary>
		Judgement = 0x0010,
		AuraState0x0020 = 0x0020,

		/// <summary>
		/// Parried the last blow
		/// </summary>
		Parry = 0x0040,
		State0x0080 = 0x0080,
		State0x0100 = 0x0100,

		/// <summary>
		/// Just killed someone who yielded honor or xp
		/// Used by Victory Rush amongst others
		/// Stays on for 20 secs
		/// </summary>
		KillYieldedHonorOrXp = 0x0200,

		/// <summary>
		/// Only used by some Test spells
		/// </summary>
		ScoredCriticalHit = 0x0400,
		State0x0800 = 0x0800,

		/// <summary>
		/// Low on Health
		/// </summary>
		Health35Percent = 0x1000,
		
		/// <summary>
		/// Immolated (Damn Pyros!)
		/// </summary>
		Immolate = 0x2000,

		/// <summary>
		/// Only used with: Druid Restoration Swiftmend (Id: 18562) [DruidRestorationSwiftmend]
		/// </summary>
		RejuvenationOrRegrowth = 0x4000,
		DeadlyPoison = 0x8000,
		Enraged = 0x10000,
		WeakenedSoul = 0x20000,
		Hypothermia = 0x40000,
		AuraState0x0080000 = 0x0080000,
		AuraState0x0100000 = 0x0100000,
		AuraState0x0200000 = 0x0200000,

		/// <summary>
		/// In good shape
		/// </summary>
		HealthAbove75Pct = 0x0400000
	}

	public enum AuraState : uint
	{
		None = 0,
		DodgeOrBlockOrParry = 1,
		Health20Percent = 2,
		Berserk = 3,
		Frozen = 4,
		Judgement = 5,
		AuraState6 = 6,
		HunterParryRogueStealthAttack = 7,
		/// <summary>
		/// Unused
		/// </summary>
		State0x0080 = 8,
		/// <summary>
		/// Unused
		/// </summary>
		State0x0100 = 9,
		/// <summary>
		/// Only in ClassSkill Victory Rush (Id: 34428) as CasterAuraState
		/// </summary>
		KillYieldedHonorOrXp = 10,
		ScoredCriticalHit = 11,
		/// <summary>
		/// Unused
		/// </summary>
		StealthInvis = 12,
		Health35Percent = 13,
		/// <summary>
		/// Must be immolated; consumes Immolate after use
		/// </summary>
		Immolate = 14,
		/// <summary>
		/// Only used for ClassSkill Swiftmend (Id: 18562)
		/// </summary>
		RejuvenationOrRegrowth = 15,
		DeadlyPoison = 16,
        /// <summary>
		/// Required by caster for Enraged Regeneration (Id: 55694)
        /// </summary>
        Enraged = 17,

        //WeakenedSoul = 18,
        ///// <summary>
        ///// Results into: Hypothermia (Dummy Aura) - ARGHS
        ///// Only in: ClassSkill Ice Block (Id: 45438)
        ///// </summary>
        //Hypothermia = 19,
		HealthAbove75Pct = 23
	}

    public enum GenericFlags : uint
    {
        Flag_0_0x1 = 0x1,//0
        Flag_1_0x2 = 0x2,//1
        Flag_2_0x4 = 0x4,//2
        Flag_3_0x8 = 0x8,//3
        Flag_4_0x10 = 0x10,//4
        Flag_5_0x20 = 0x20,//5
        Flag_6_0x40 = 0x40,//6
        Flag_7_0x80 = 0x80,//7
        Flag_8_0x100 = 0x100,//8
        Flag_9_0x200 = 0x200,//9
        Flag_10_0x400 = 0x400,//10
        Flag_11_0x800 = 0x800,//11
        Flag_12_0x1000 = 0x1000,//12
        Flag_13_0x2000 = 0x2000,//13
        Flag_14_0x4000 = 0x4000,//14
        Flag_15_0x8000 = 0x8000,//15
        Flag_16_0x10000 = 0x10000,//16
        Flag_17_0x20000 = 0x20000,//17
        Flag_18_0x40000 = 0x40000,//18
        Flag_19_0x80000 = 0x80000,//19
        Flag_20_0x100000 = 0x100000,//20
        Flag_21_0x200000 = 0x200000,//21
        Flag_22_0x400000 = 0x400000,//22
        Flag_23_0x800000 = 0x800000,//23
        Flag_24_0x1000000 = 0x1000000,//24
        Flag_25_0x2000000 = 0x2000000,//25
        Flag_26_0x4000000 = 0x4000000,//26
        Flag_27_0x8000000 = 0x8000000,//27
        Flag_28_0x10000000 = 0x10000000,//28
        Flag_29_0x20000000 = 0x20000000,//29
        Flag_30_0x40000000 = 0x40000000,//30
        Flag_31_0x80000000 = 0x80000000,//31
    }

	[Flags]
	public enum SpellAttributes : uint
	{
		None = 0,

		Attr_0_0x1 = 0x1,//0

        /// <summary>
        /// 0x2
        /// </summary>
		Ranged = 0x2,//1

        /// <summary>
        /// 0x4
        /// </summary>
        OnNextMelee = 0x4,//2
		/// <summary>
		/// 0x8
		/// </summary>
		Unused_AttrFlag0x8 = 0x8,//3

		/// <summary>
		/// 0x10 - Means this is not a "magic spell"
		/// </summary>
		IsAbility = 0x10,//4

        /// <summary>
        /// 0x20
        /// </summary>
		IsTradeSkill = 0x20,//5

        /// <summary>
        /// 0x40
        /// </summary>
        Passive = 0x40,//6

        /// <summary>
        /// Does not show in buff/debuff pane. normally passive buffs
        /// 0x80
        /// </summary>
        NoVisibleAura = 0x80,//7

        /// <summary>
        /// Every learn-spell has it, but also many other spells
        /// 0x100
        /// </summary>
        Attr_8_0x100 = 0x100,//8

        /// <summary>
        /// 0x200
        /// </summary>
        TempWeaponEnchant = 0x200,//9

		OnNextMelee_2 = 0x400,//10

		Attr_11_0x800 = 0x800,//11

		OnlyUsableInDaytime = 0x1000,//12

		OnlyUsableAtNight = 0x2000,//13

		OnlyUsableIndoors = 0x4000,//14

        OnlyUsableOutdoors = 0x8000,//15

        /// <summary>
        /// Not while shapeshifted
        /// </summary>
		NotWhileShapeshifted = 0x10000,//16

        RequiresStealth = 0x20000,//17

		Attr_18_0x40000 = 0x40000,//18

        /// <summary>
        /// Scale the damage with the caster's level
        /// </summary>
		ScaleDamageWithCasterLevel = 0x80000,//19

        /// <summary>
        /// Stop attack after use this spell (and not begin attack if use)
        /// </summary>
		StopsAutoAttack = 0x100000,//20

        /// <summary>
        /// This attack cannot be dodged, blocked, or parried
        /// </summary>
		CannotDodgeBlockParry = 0x200000,//21

		Attr_22_0x400000 = 0x400000,//22

		CastableWhileDead = 0x800000,//23

        /// <summary>
        /// Can be casted while mounted
        /// </summary>
        CastableWhileMounted = 0x1000000,//24

        /// <summary>
        /// Needs cooldown to be sent as extra packet
        /// Activate and start cooldown after aura fade or remove summoned creature or go
        /// </summary>
        StartCooldownAfterEffectFade = 0x2000000,//25

        Attr_26_0x4000000 = 0x4000000,//26

        /// <summary>
        /// All kinds of beneficial Auras (but also some summons?)
        /// </summary>
        CastableWhileSitting = 0x8000000,//27

        /// <summary>
		/// Set for all spells that are not allowed during combat, including all kinds of:
		/// Stealth, summoning of items or creatures, Mounts etc
		/// </summary>
		CannotBeCastInCombat = 0x10000000,

		UnaffectedByInvulnerability = 0x20000000,

        /// <summary>
        /// Effect movement in a negative way with:
        /// Root, stun, charm, possess, fear, confuse, decrease speed etc.
        /// Probably chance to break on damage
        /// </summary>
        MovementImpairing = 0x40000000,

        /// <summary>
        /// Positive Aura but cannot right click to remove
        /// </summary>
        CannotRemove = 0x80000000,//31
	}

	[Flags]
	public enum SpellAttributesEx : uint
	{
		None = 0,

        /// <summary>
        /// Something with summoning pet
        /// BringsUpPetBar maybe?
        /// This means the spell can always target a corpse
        /// </summary>
		AttrEx_0_0x1 = 0x1,

        DrainEntireManaPool = 0x2,//1

        /// <summary>
        /// Maybe CanCancel?
        /// Used in checks to see if client can cancel a buff, Lua_CancelPlayerBuff
        /// </summary>
        Channeled_1 = 0x4,//2

		AttrEx_3_0x8 = 0x8,

		AttrEx_4_0x10 = 0x10,

        RemainStealthed = 0x20,//5


		Channeled_2 = 0x40,//6

        /// <summary>
        /// TODO: Check validity
        /// </summary>
		Negative = 0x80,//7

		TargetNotInCombat = 0x100,//8

		AttrEx_9_0x200 = 0x200,//9

		AttrEx_10_0x400 = 0x400,//10

		AttrEx_11_0x800 = 0x800,//11

        PickPocket = 0x1000,//12

        ChangeSight = 0x2000,//13

		AttrEx_14_0x4000 = 0x4000,//14

		DispelAurasOnImmunity = 0x8000,//15

		UnaffectedBySchoolImmunity = 0x10000,//16

        RemainOutOfCombat = 0x20000,//17

		AttrEx_18_0x40000 = 0x40000,//18

		CannotTargetSelf = 0x80000,//19
        MustBeBehindTarget = 0x100000,//20

		AttrEx_21_0x200000 = 0x200000,//21

        /// <summary>
        /// Requires Combo Points
        /// </summary>
        FinishingMove = 0x400000,//22

		AttrEx_23_0x800000 = 0x800000,//23

		AttrEx_24_0x1000000 = 0x1000000,//24

		AttrEx_25_0x2000000 = 0x2000000,//25

		AttrEx_26_0x4000000 = 0x4000000,//26

        /// <summary>
        /// Only for attack moves (usually requiring weapons)
        /// </summary>
        AttrEx_27_0x8000000 = 0x8000000,//27

		AttrEx_28_0x10000000 = 0x10000000,//28

		AttrEx_29_0x20000000 = 0x20000000,//29

		Overpower = 0x40000000,//30

		AttrEx_31_0x80000000 = 0x80000000,//31
	}

	[Flags]
	public enum SpellAttributesExB : uint
	{
		None = 0,
        AttrExB_0_0x1 = 0x1,//0
        AttrExB_1_0x2 = 0x2,//1
        AttrExB_2_0x4 = 0x4,//2
        AttrExB_3_0x8 = 0x8,//3

		/// <summary>
		/// Does not actually make this an Aura but indicates that effects dont stack with other modifiers?
		/// </summary>
        PaladinAura = 0x10,

        AutoRepeat = 0x20, // auto shot

        Polymorph = 0x40,

        AttrExB_7_0x80 = 0x80,//7
        AttrExB_8_0x100 = 0x100,//8
        AttrExB_9_0x200 = 0x200,//9

        TamePet = 0x400,

        TargetPeriodicHeal = 0x800,

        AttrExB_12_0x1000 = 0x1000,//12
        AttrExB_13_0x2000 = 0x2000,//13
        AttrExB_14_0x4000 = 0x4000,//14
        AttrExB_15_0x8000 = 0x8000,//15
        AttrExB_16_0x10000 = 0x10000,//16

        RequiresRangedWeapon = 0x20000,
        /// <summary>
        /// Revive Pet (982)
        /// </summary>
        RevivePet = 0x40000,

        AttrExB_19_0x80000 = 0x80000,//19

        RequiresBehindTarget = 0x100000,

        AttrExB_21_0x200000 = 0x200000,//21
        AttrExB_22_0x400000 = 0x400000,//22
        AttrExB_23_0x800000 = 0x800000,//23
        AttrExB_24_0x1000000 = 0x1000000,//24
        AttrExB_25_0x2000000 = 0x2000000,//25

        /// <summary>
        /// In ClassSkillChaosBoltRank1 and all taunts (can't be resisted)
        /// </summary>
        CannotBeResisted = 0x4000000,
        AttrExB_27_0x8000000 = 0x8000000,//27
        AttrExB_28_0x10000000 = 0x10000000,//28
        CannotCrit = 0x20000000,//29
        AttrExB_30_0x40000000 = 0x40000000,//30
        AttrExB_31_0x80000000 = 0x80000000,//31		
	}

	[Flags]
	public enum SpellAttributesExC : uint
	{
		None = 0x0,
        AttrExC_0_0x1 = 0x1,//0
        AttrExC_1_0x2 = 0x2,//1
        AttrExC_2_0x4 = 0x4,//2
        AttrExC_3_0x8 = 0x8,//3

		Rebirth = 0x10,

        AttrExC_5_0x20 = 0x20,//5
        AttrExC_6_0x40 = 0x40,//6
        AttrExC_7_0x80 = 0x80,//7
        AttrExC_8_0x100 = 0x100,//8
        AttrExC_9_0x200 = 0x200,//9
        RequiresMainHandWeapon = 0x400,//10

		BattleGroundOnly = 0x800,

        AttrExC_12_0x1000 = 0x1000,//12
        AttrExC_13_0x2000 = 0x2000,//13

		HonorlessTarget = 0x4000,

		ShootRangedWeapon = 0x8000,

        AttrExC_16_0x10000 = 0x10000,//16
        /// <summary>
        /// Does not cause initial aggro (think hunter's mark)
        /// </summary>
        NoInitialAggro = 0x20000,//17
        AttrExC_18_0x40000 = 0x40000,//18
        AttrExC_19_0x80000 = 0x80000,//19

		PersistsThroughDeath = 0x100000,

		NaturesGrasp = 0x00200000,

		RequiresWand = 0x00400000,

        AttrExC_23_0x800000 = 0x800000,//23
        RequiresOffHandWeapon = 0x1000000,//24
        AttrExC_25_0x2000000 = 0x2000000,//25

		/// <summary>
		/// Not in the old land or instances (probably labeled incorrectly)
		/// </summary>
		OldOnlyInOutlands = 0x4000000,

        AttrExC_27_0x8000000 = 0x8000000,//27
        AttrExC_28_0x10000000 = 0x10000000,//28
        AttrExC_29_0x20000000 = 0x20000000,//29
        AttrExC_30_0x40000000 = 0x40000000,//30
        AttrExC_31_0x80000000 = 0x80000000,//31
		RequiresTwoWeapons = RequiresMainHandWeapon | RequiresOffHandWeapon
	}

	[Flags]
	public enum SpellAttributesExD : uint
	{
		None = 0,

        AttrExD_0_0x1 = 0x1,//0

        NoReagentsInPrep = 0x2,//1

        AttrExD_2_0x4 = 0x4,//2

        UsableWhileStunned = 0x8,//3

        AttrExD_4_0x10 = 0x10,//4

        /// <summary>
        /// Can only be applied to a single target at a time
        /// </summary>
        SingleTargetOnly = 0x20,//5

        AttrExD_6_0x40 = 0x40,//6

        AttrExD_7_0x80 = 0x80,//7

        AttrExD_8_0x100 = 0x100,//8

        AttrExD_9_0x200 = 0x200,//9

        AttrExD_10_0x400 = 0x400,//10

        /// <summary>
        /// This spell can't be absorbed
        /// ClassSkillChaosBoltRank1 has this, is it correct though?
        /// However, does this mean Entangling Roots DoT can't be absorbed?
        /// </summary>
        CannotBeAbsorbed = 0x800,//11

        AttrExD_12_0x1000 = 0x1000,//12

        AttrExD_13_0x2000 = 0x2000,//13

        AttrExD_14_0x4000 = 0x4000,//14

        AttrExD_15_0x8000 = 0x8000,//15

        AttrExD_16_0x10000 = 0x10000,//16

        UsableWhileFeared = 0x20000,//17

        UsableWhileConfused = 0x40000,//18

        AttrExD_19_0x80000 = 0x80000,//19

        AttrExD_20_0x100000 = 0x100000,//20

        AttrExD_21_0x200000 = 0x200000,//21

        AttrExD_22_0x400000 = 0x400000,//22

        AttrExD_23_0x800000 = 0x800000,//23

        AttrExD_24_0x1000000 = 0x1000000,//24

        AttrExD_25_0x2000000 = 0x2000000,//25

        AttrExD_26_0x4000000 = 0x4000000,//26

        AttrExD_27_0x8000000 = 0x8000000,//27

        AttrExD_28_0x10000000 = 0x10000000,//28

        AttrExD_29_0x20000000 = 0x20000000,//29

        AttrExD_30_0x40000000 = 0x40000000,//30

        AttrExD_31_0x80000000 = 0x80000000,//31
	}

	[Flags]
	public enum SpellAttributesExE : uint
	{
        None = 0,

        AttrExE_0_0x1 = 0x1,//0

        AttrExE_1_0x2 = 0x2,//1

        AttrExE_2_0x4 = 0x4,//2

        AttrExE_3_0x8 = 0x8,//3

        AttrExE_4_0x10 = 0x10,//4

        AttrExE_5_0x20 = 0x20,//5

        AttrExE_6_0x40 = 0x40,//6

        AttrExE_7_0x80 = 0x80,//7

        AttrExE_8_0x100 = 0x100,//8

        AttrExE_9_0x200 = 0x200,//9

        AttrExE_10_0x400 = 0x400,//10

        AttrExE_11_0x800 = 0x800,//11

        AttrExE_12_0x1000 = 0x1000,//12

        AttrExE_13_0x2000 = 0x2000,//13

        AttrExE_14_0x4000 = 0x4000,//14

        AttrExE_15_0x8000 = 0x8000,//15

        AttrExE_16_0x10000 = 0x10000,//16

        AttrExE_17_0x20000 = 0x20000,//17

        AttrExE_18_0x40000 = 0x40000,//18

        AttrExE_19_0x80000 = 0x80000,//19

        AttrExE_20_0x100000 = 0x100000,//20

        AttrExE_21_0x200000 = 0x200000,//21

        AttrExE_22_0x400000 = 0x400000,//22

        AttrExE_23_0x800000 = 0x800000,//23

        AttrExE_24_0x1000000 = 0x1000000,//24

        AttrExE_25_0x2000000 = 0x2000000,//25

        AttrExE_26_0x4000000 = 0x4000000,//26

        AttrExE_27_0x8000000 = 0x8000000,//27

        AttrExE_28_0x10000000 = 0x10000000,//28

        AttrExE_29_0x20000000 = 0x20000000,//29

        AttrExE_30_0x40000000 = 0x40000000,//30

        AttrExE_31_0x80000000 = 0x80000000,//31

		//AttributesexUnk22 = 0x100000, // related to "Finishing move" and "Instantly overpowers"
		//AttributesexUnk23 = 0x200000,
		//AttributesexUnk24 = 0x400000, // only related to "Finishing move"
		//AttributesexUnk25 = 0x800000, // related to spells like "ClearAllBuffs"
		//AttributesexUnk26 = 0x1000000, // Fishing Spells
		//AttributesexUnk27 = 0x2000000, // related to "Detect" spell
		//AttributesexUnk28 = 0x4000000,
		//AttributesexUnk29 = 0x8000000,
		//AttributesexUnk30 = 0x10000000,
		//AttributesexUnk31 = 0x20000000,
		//AttributesexUnk32 = 0x40000000, // Overpower
	}

	[Flags]
	public enum SpellAttributesExF : uint
	{
        None = 0,

        AttrExF_0_0x1 = 0x1,//0

        AttrExF_1_0x2 = 0x2,//1

        AttrExF_2_0x4 = 0x4,//2

        AttrExF_3_0x8 = 0x8,//3

        AttrExF_4_0x10 = 0x10,//4

        AttrExF_5_0x20 = 0x20,//5

        AttrExF_6_0x40 = 0x40,//6

        AttrExF_7_0x80 = 0x80,//7

        AttrExF_8_0x100 = 0x100,//8

        AttrExF_9_0x200 = 0x200,//9

        AttrExF_10_0x400 = 0x400,//10

        AttrExF_11_0x800 = 0x800,//11

        AttrExF_12_0x1000 = 0x1000,//12

        AttrExF_13_0x2000 = 0x2000,//13

        AttrExF_14_0x4000 = 0x4000,//14

        AttrExF_15_0x8000 = 0x8000,//15

        AttrExF_16_0x10000 = 0x10000,//16

        AttrExF_17_0x20000 = 0x20000,//17

        AttrExF_18_0x40000 = 0x40000,//18

        AttrExF_19_0x80000 = 0x80000,//19

        AttrExF_20_0x100000 = 0x100000,//20

        AttrExF_21_0x200000 = 0x200000,//21

        AttrExF_22_0x400000 = 0x400000,//22

        AttrExF_23_0x800000 = 0x800000,//23

        AttrExF_24_0x1000000 = 0x1000000,//24

        AttrExF_25_0x2000000 = 0x2000000,//25

        AttrExF_26_0x4000000 = 0x4000000,//26

        AttrExF_27_0x8000000 = 0x8000000,//27

        AttrExF_28_0x10000000 = 0x10000000,//28

        AttrExF_29_0x20000000 = 0x20000000,//29

        AttrExF_30_0x40000000 = 0x40000000,//30

        AttrExF_31_0x80000000 = 0x80000000,//31
	}

    public enum SpellClassSet
    {
        Generic = 0,
        Name_1 = 1,
        Unused_Name_2 = 2,
        Mage = 3,
        Warrior = 4,
        Warlock = 5,
        Priest = 6,
        Druid = 7,
        Rogue = 8,
        Hunter = 9,
        Paladin = 10,
        Shaman = 11,
        Unused_Name_12 = 12,
        Unused_Name_13 = 13,
        DeathKnight = 15,
        HunterPets = 17,
    }

	/// <summary>
	/// Events that can interrupt casting
	/// </summary>
	[Flags]
	public enum InterruptFlags
	{
		None = 0x0,
		OnSilence = 0x1,
		OnSleep = 0x2,
		OnStunned = 0x4,
		OnMovement = 0x8,
		OnTakeDamage = 0x10
	}

	/// <summary>
	/// Events that can interrupt Auras
	/// </summary>
	[Flags]
	public enum AuraInterruptFlags : uint
	{
		None = 0x0,
		OnHostileSpellInflicted = 0x1,
		OnDamage = 0x2,

		Flag_0x4 = 0x4,

		OnMovement = 0x8,

		OnTurn = 0x10,
		OnEnterCombat = 0x20,
		OnDismount = 0x40,

		OnEnterWater = 0x80,
		OnLeaveWater = 0x100,

		Flag_0x200 = 0x200,
		Flag_0x400 = 0x400,
		Flag_0x800 = 0x800,

		OnStartAttack = 0x1000,

		Flag_0x2000 = 0x2000,
		Flag_0x4000 = 0x4000,

		OnCast = 0x8000,

		Flag_0x10000 = 0x10000,

		OnMount = 0x20000,
		OnStandUp = 0x40000,
		OnLeaveArea = 0x80000,
		OnInvincible = 0x100000,
		OnStealth = 0x200000,

		Flag_0x400000 = 0x400000,

		OnEnterPvP = 0x800000,
        /// <summary>
        /// removed when hit by direct damage (no dots)
        /// </summary>
		OnDirectDamage = 0x1000000,
		InterruptFlag0x2000000 = 0x2000000,
		InterruptFlag0x4000000 = 0x4000000,
		InterruptFlag0x8000000 = 0x8000000,
		InterruptFlag0x10000000 = 0x10000000,
		InterruptFlag0x20000000 = 0x20000000,
		InterruptFlag0x40000000 = 0x40000000,
		InterruptFlag0x80000000 = 0x80000000
	}

	[Flags]
	public enum ChannelInterruptFlags
	{
		None = 0x0,
		ChannelInterruptOn1 = 0x1,
		ChannelInterruptOn2 = 0x2,
		ChannelInterruptOn3 = 0x4,
		ChannelInterruptOn4 = 0x8,
		ChannelInterruptOn5 = 0x10,
		ChannelInterruptOn6 = 0x20,
		ChannelInterruptOn7 = 0x40,
		ChannelInterruptOn8 = 0x80,
		ChannelInterruptOn9 = 0x100,
		ChannelInterruptOn10 = 0x200,
		ChannelInterruptOn11 = 0x400,
		ChannelInterruptOn12 = 0x800,
		ChannelInterruptOn13 = 0x1000,
		ChannelInterruptOn14 = 0x2000,
		ChannelInterruptOn15 = 0x4000,
		ChannelInterruptOn16 = 0x8000,
		ChannelInterruptOn17 = 0x10000,
		ChannelInterruptOn18 = 0x20000
	}

    [Flags]
    public enum SpellFacingFlags
    {
        RequiresInFront = 0x1,
        Flag_1_0x2 = 0x2,
        Flag_2_0x4 = 0x4,
        Flag_3_0x8 = 0x8,
    }

	public enum SpellDefenseType
	{
		None = 0,
		Magic = 1,
		Melee = 2,
		Ranged = 3
	}

    public enum SpellPreventionType
    {
        None = 0,
        /// <summary>
        /// Cannot cast spells of this type when Silenced
        /// </summary>
        Magic = 1,
        /// <summary>
        /// Cannot cast spells of this type when Pacified
        /// </summary>
        Melee = 2,
    }

	[Flags]
	public enum SpellFamilyFlags : ulong
	{
		#region Generic
		_0x0000000000000001 = 0x0000000000000001,
		_0x0000000000000002 = 0x0000000000000002,
		_0x0000000000000004 = 0x0000000000000004,
		_0x0000000000000008 = 0x0000000000000008,

		_0x0000000000000010 = 0x0000000000000010,
		_0x0000000000000020 = 0x0000000000000020,
		_0x0000000000000040 = 0x0000000000000040,
		_0x0000000000000080 = 0x0000000000000080,

		_0x0000000000000100 = 0x0000000000000100,
		_0x0000000000000200 = 0x0000000000000200,
		_0x0000000000000400 = 0x0000000000000400,
		_0x0000000000000800 = 0x0000000000000800,

		_0x0000000000001000 = 0x0000000000001000,
		_0x0000000000002000 = 0x0000000000002000,
		_0x0000000000004000 = 0x0000000000004000,
		_0x0000000000008000 = 0x0000000000008000,

		_0x0000000000010000 = 0x0000000000010000,
		_0x0000000000020000 = 0x0000000000020000,
		_0x0000000000040000 = 0x0000000000040000,
		_0x0000000000080000 = 0x0000000000080000,

		_0x0000000000100000 = 0x0000000000100000,
		_0x0000000000200000 = 0x0000000000200000,
		_0x0000000000400000 = 0x0000000000400000,
		_0x0000000000800000 = 0x0000000000800000,

		_0x0000000001000000 = 0x0000000001000000,
		_0x0000000002000000 = 0x0000000002000000,
		_0x0000000004000000 = 0x0000000004000000,
		_0x0000000008000000 = 0x0000000008000000,

		_0x0000000010000000 = 0x0000000010000000,
		_0x0000000020000000 = 0x0000000020000000,
		_0x0000000040000000 = 0x0000000040000000,
		_0x0000000080000000 = 0x0000000080000000,

		_0x0000000100000000 = 0x0000000100000000,
		_0x0000000200000000 = 0x0000000200000000,
		_0x0000000400000000 = 0x0000000400000000,
		_0x0000000800000000 = 0x0000000800000000,

		_0x0000001000000000 = 0x0000001000000000,
		_0x0000002000000000 = 0x0000002000000000,
		_0x0000004000000000 = 0x0000004000000000,
		_0x0000008000000000 = 0x0000008000000000,

		_0x0000010000000000 = 0x0000010000000000,
		_0x0000020000000000 = 0x0000020000000000,
		_0x0000040000000000 = 0x0000040000000000,
		_0x0000080000000000 = 0x0000080000000000,

		_0x0000100000000000 = 0x0000100000000000,
		_0x0000200000000000 = 0x0000200000000000,
		_0x0000400000000000 = 0x0000400000000000,
		_0x0000800000000000 = 0x0000800000000000,

		_0x0001000000000000 = 0x0001000000000000,
		_0x0002000000000000 = 0x0002000000000000,
		_0x0004000000000000 = 0x0004000000000000,
		_0x0008000000000000 = 0x0008000000000000,

		_0x0010000000000000 = 0x0010000000000000,
		_0x0020000000000000 = 0x0020000000000000,
		_0x0040000000000000 = 0x0040000000000000,
		_0x0080000000000000 = 0x0080000000000000,

		_0x0100000000000000 = 0x0100000000000000,
		_0x0200000000000000 = 0x0200000000000000,
		_0x0400000000000000 = 0x0400000000000000,
		_0x0800000000000000 = 0x0800000000000000,

		_0x1000000000000000 = 0x1000000000000000,
		_0x2000000000000000 = 0x2000000000000000,
		_0x4000000000000000 = 0x4000000000000000,
		_0x8000000000000000 = 0x8000000000000000,
		#endregion

		/*// Paladin
        Blessing = 0x10000000,
        Seal = 0xA000200,

        // Mage
        MoltenArmor = 0x40000,
        FrostIceArmor = 0x2000000,
        MageArmor = 0x10000000,
        Blizzard = 0x80,

        // Shaman
        LightningShield = 0x400,
        EarthShield = 0x40000000000,


        // Warrior
        ThunderClap = 0x80,

        // Rogue
        Sap = 0x00000080,
        Vanish = 0x00000800,
        Stealth = 0x00400000,
        Backstab = 0x00800004,
        Feint = 0x08000000,
        KidneyShot = 0x00200000,*/
	}

	/// <summary>
	/// Mask from CreatureType.dbc
	/// </summary>
	[Flags]
	public enum TargetCreatureMask
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

	[Flags]
	public enum SpellTargetFlags : uint
	{
		Self = 0,
		SpellTargetFlag_Dynamic_0x1 = 0x1,
		Unit = 0x0002,
		SpellTargetFlag_Dynamic_0x4 = 0x4,
		SpellTargetFlag_Dynamic_0x8 = 0x8,
		Item = 0x10,
		SourceLocation = 0x20,
		DestinationLocation = 0x40,
		UnkObject_0x80 = 0x80,
		UnkUnit_0x100 = 0x100,
		PvPCorpse = 0x200,
		UnitCorpse = 0x400,
		Object = 0x800,
		TradeItem = 0x1000,
		String = 0x2000,
		/// <summary>
		/// For spells that open an object
		/// </summary>
		OpenObject = 0x4000,
		Corpse = 0x8000,
		SpellTargetFlag_Dynamic_0x10000 = 0x10000,
		Glyph = 0x20000,

        Flag_0x200000 = 0x200000,
	}

    public enum ImplicitTargetType
    {
        /// <summary>
        /// Default
        /// </summary>
        None = 0,
        Self = 1,
        /// <summary>
        /// 30235
        /// This is cast by the Astral Flares summoned by The Curator in Karazhan.
        /// It chains to nearby characters, hitting up to three of them.
        /// </summary>
        Type_2 = 2,
        InvisibleOrHiddenEnemiesAtLocationRadius = 3,
        /// <summary>
        /// Single target, can spread to nearby. Ie 3439, 7103
        /// </summary>
        SpreadableDesease = 4,
        /// <summary>
        /// Own pet or summon
        /// </summary>
        Pet = 5,
        /// <summary>
        /// Single enemy, might be chained
        /// </summary>
        SingleEnemy = 6,
        ScriptedTarget = 7,
        AllAroundLocation = 8,
        /// <summary>
        /// Teleport back home
        /// </summary>
        HeartstoneLocation = 9,
        //Unused_Type_10 = 10,
        /// <summary>
        /// Spell 4, Word of Recall Other
        /// </summary>
        Type_11 = 11,
        //Unused_Type_12 = 12,
        //Unused_Type_13 = 13,
        //Unused_Type_14 = 14,
        AllEnemiesInArea = 15,
        AllEnemiesInAreaInstant = 16,
        TeleportLocation = 17,
        LocationToSummon = 18,
        //Unused_Type_19 = 19,
        AllPartyAroundCaster = 20,
        SingleFriend = 21,
        AllEnemiesAroundCaster = 22,
        /// <summary>
        /// All kinds of GameObject interactions (mostly for quests)
        /// </summary>
        GameObject = 23,
		/// <summary>
		/// Is only used for negative effects - for positive ones, use: LocationInFrontCaster, LocationInFrontCasterAtRange
		/// </summary>
        InFrontOfCaster = 24,
        /// <summary>
        /// Enemy target apparently
        /// </summary>
        Duel = 25,
        /// <summary>
        /// Needed for all open lock spells
        /// </summary>
        GameObjectOrItem = 26,
        PetMaster = 27,
        AllEnemiesInAreaChanneled = 28,
        /// <summary>
        /// Tranquility and some Dummies (visual only)
        /// </summary>
        AllPartyInAreaChanneled = 29,
        AllFriendlyInAura = 30,
        AllTargetableAroundLocationInRadiusOverTime = 31,
        Minion = 32,
        AllPartyInArea = 33,
        /// <summary>
        /// Only used for Tranquilities' Trigger spell
        /// </summary>
        Tranquility = 34,
        SingleParty = 35,
        PetSummonLocation = 36,
        AllParty = 37,
        ScriptedOrSingleTarget = 38,
        SelfFishing = 39,
        ScriptedGameObject = 40,
        TotemEarth = 41,
        TotemWater = 42,
        TotemAir = 43,
        TotemFire = 44,
        /// <summary>
        /// Any kind of chain effect:
        /// Chain Lightning etc, as well as Multishot 
        /// </summary>
        Chain = 45,
        ScriptedObjectLocation = 46,
        /// <summary>
        /// Spell-targettype always = self
        /// Examples: Mortar, all kinds of Picnics etc
        /// </summary>
        DynamicObject = 47,
        MultipleSummonLocation = 48,
        MultipleSummonPetLocation = 49,
        SummonLocation = 50,
        /// <summary>
        /// Odd stuff
        /// </summary>
        CaliriEggs = 51,
        LocationNearCaster = 52,
        CurrentSelection = 53,
        TargetAtOrientationOfCaster = 54,
        /// <summary>
        /// Teleport location
        /// </summary>
        LocationInFrontCaster = 55,
        PartyAroundCaster = 56,
        PartyMember = 57,
        Type_58 = 58,
        TargetForVisualEffect = 59,
        ScriptedTarget2 = 60,
        AreaEffectPartyAndClass = 61,
        //Unused_PriestChampion = 62,
        NatureSummonLocation = 63,
        Type_64 = 64,
        /// <summary>
        /// Eg Shadowstep, Warp etc
        /// </summary>
        BehindTargetLocation = 65,
        Type_66 = 66,
        Type_67 = 67,
        Type_68 = 68,
        Type_69 = 69,
        Type_70 = 70,
        //Unused_Type_71 = 71,
        MultipleGuardianSummonLocation = 72,
        NetherDrakeSummonLocation = 73,
        ScriptedLocation = 74,
        /// <summary>
        /// Teleport location
        /// </summary>
        LocationInFrontCasterAtRange = 75,
        //Unused_EnemiesInAreaChanneledWithExceptions = 76,
        SelectedEnemyChanneled = 77,
        Type_78 = 78,
        Type_79 = 79,
        Type_80 = 80,
        Type_81 = 81,
        //Unused_Type_82 = 82,
        //Unused_Type_83 = 83,
        //Unused_Type_84 = 84,
        Type_85 = 85,
        SelectedEnemyDeadlyPoison = 86,
        Type_87 = 87,
        Type_88 = 88,
        Type_89 = 89,
        Type_90 = 90,
        Type_91 = 91,
        //Unused_Type_92 = 92,
        Type_93 = 93,					// Highest as of 2.4.3.8606
		ConeInFrontOfCaster = 104,
		Target_105						// Highest as of 3.2.2
    }

	public enum DispelType
	{
		Trinkets = -1,
		None = 0,
		Magic = 1,
		Curse = 2,
		Disease = 3,
		Poison = 4,
		Stealth = 5,
		Invisibility = 6,
		All = 7,

		Frenzy = 9
	};

	public enum HarmType
	{
		/// <summary>
		/// Neutral spell/effect or spell that has both, beneficial and harmful effects
		/// </summary>
		Neutral = 0,
		Beneficial,
		Harmful
	}

	// still usable?
	public enum AIGroupAction
	{
		Spawn = 0x0,
		Idle = 0x1,
		MoveTo = 0x2,
		Teleport = 0x3,
		Wander = 0x4,
		WanderSpawnRelative_Obsolete = 0x5,
		WanderArea = 0x6,
		FollowGuid = 0x7,
		FollowPath = 0x8,
		PatrolLine = 0x9,
		PatrolCircle = 0xA,
		GuardGuid = 0xB,
		GuardArea = 0xC,
		SetFormation = 0xD,
		UnitChangeModeObsolete = 0xE,
		UnitSay = 0xF,
		UnitCast = 0x10,
		UnitActivateObject = 0x11,
		GenerateEvent = 0x12,
		Despawn = 0x13,
		SetRadiusObsolete = 0x14,
		SetFactionObsolete = 0x15,
		UnitSetFacing = 0x16,
		UnitFaceGuid = 0x17,
		UnitEmote = 0x18,
		MovetoGuid = 0x19,
		AttackGuid = 0x1A,
		UnitMount = 0x1B,
		UnitDismount = 0x1C,
		BeastmasterOn = 0x1D,
		BeastmasterOff = 0x1E,
		UnitMode = 0x1F,
		UnitModeReset = 0x20,
		UnitFaction = 0x21,
		UnitFactionReset = 0x22,
		UnitRadius = 0x23,
		UnitRadiusReset = 0x24,
		QuestComplete = 0x25,
		UnitQuestgiver = 0x26,
		UnitTrainer = 0x27,
		SplinePath = 0x28,
		PlayerAction = 0x29,
		ReturnHome = 0x2A,
		UnitSayRandom = 0x2B,
		UnitYell = 0x2C,
		UnitYellRandom = 0x2D,
		UnitSetItemMainhand = 0x2E,
		UnitResetItemMainhand = 0x2F,
		UnitChatEmote = 0x30,
		UnitChatEmoteRandom = 0x31,
		UnitGenerateEvent = 0x32,
		VendorIdleObsolete = 0x33,
		QuestFailed = 0x34,
		UnitTriggers = 0x35,
		UnitTriggersReset = 0x36,
		UnitLeaveCombat = 0x37,
		IdleCombatStart = 0x38,
		IdleCombatStop = 0x39,
		UnitImmunepc = 0x3A,
		UnitImmunepcReset = 0x3B,
		UnitImmunenpc = 0x3C,
		UnitImmunenpcReset = 0x3D,
		UnitUnkillable = 0x3E,
		UnitUnkillableReset = 0x3F,
		UnitSpells = 0x40,
		UnitSpellsReset = 0x41,
		Reuseme = 0x42,
		UnitSendLocalEvent = 0x43,
		UnitBroadcastLocalEvent = 0x44,
		UnitFlee = 0x45,
		UnitRetreat = 0x46,
		ObjectChatEmote = 0x47,
		ObjectChatEmoteRandom = 0x48,
		Avoid = 0x49,
		AvoidGuid = 0x4A,
		ObjectActivate = 0x4B,
		UnitActivateObjects = 0x4C,
		UnitStringid = 0x4D,
		UnitStringidReset = 0x4E,
		PeriodicEvent = 0x4F,
		UnitSetItemOffhand = 0x50,
		UnitResetItemOffhand = 0x51,
		UnitSetItemRanged = 0x52,
		UnitResetItemRanged = 0x53,
		UnitSheathe = 0x54,
		UnitUnsheathe = 0x55,
		UnitCancelCast = 0x56,
		UnitCancelAura = 0x57,
		UnitFinishCast = 0x58,
		EmoteState = 0x59,
		UnitCallForHelp = 0x5A,
		FlightPath = 0x5B,
		UnitCombatTrigger = 0x5C
	}

	public enum InvisType : byte
	{
		Normal = 0,
		/// <summary>
		/// Totem quest for Shamans (Sapta Sight etc)
		/// </summary>
		ElementalSpirit = 1,
		/// <summary>
		/// Some Quests seem to use this
		/// </summary>
		Quests1 = 2,
		/// <summary>
		/// Used for Trap detection
		/// </summary>
		Traps = 3,
		/// <summary>
		/// Mostly only used for Rogue's and Spellcasters' Vanish
		/// - Can be "smelled" by nearby NPCs
		/// - Resets aggro
		/// - Boosts stealth
		/// - Breaks Movement impairing effects
		/// </summary>
		Vanish = 4,
		/// <summary>
		/// Dead players mostly
		/// </summary>
		Ghosts = 5,
		/// <summary>
		/// Only used for: Drunk Invisibility (Pink) (Id: 36440)
		/// </summary>
		Drunk = 6,
		/// <summary>
		/// Only used for some Shadowmoon-related quests
		/// </summary>
		Shadowmoon = 7,
		/// <summary>
		/// Similar to the other one (Triangulation Point?)
		/// </summary>
		Shadowmoon2 = 8,
		/// <summary>
		/// More quests
		/// </summary>
		Quests2 = 9
	}

	#region Auras
	/// <summary>
	/// 8 Bit flags
	/// </summary>
	[Flags]
	public enum AuraFlags : byte
	{
		None = 0,
		Effect1AppliesAura = 0x1, // 001
        Effect2AppliesAura = 0x2, // 010
        Effect3AppliesAura = 0x4, // 100
		TargetIsCaster = 0x8,
		Positive = 0x10,
		HasDuration = 0x20,
        Flag_0x40 = 0x40,
        Negative = 0x80,
	}

	[Flags]
	public enum AuraTickFlags
	{
		None = 0x00,
		PeriodicDamage = 0x02,
		PeriodicTriggerSpell = 0x04,
		PeriodicHeal = 0x08,
		PeriodicLeech = 0x10,
		PeriodicEnergize = 0x20
	}
	#endregion

	public enum SkinningType
	{
		Skinning = 0,
		Herbalism = 1,
		Mining = 2,
		Engineering =3
	}
}