using System;

namespace WCell.Constants
{
	public enum VictimState : uint
	{
		Miss = 0,
		Wound = 1,
		Dodge = 2,
		Parry = 3,
		Interrupt = 4,
		Block = 5,
		Evade = 6,
		Immune = 7,
		Deflect = 8
	}

	[Flags]
	public enum HitFlags : uint
	{
		NormalSwing = 0x00,
        /// <summary>
        /// This flag means a bunch of floats appear in the packet. Haven't seen them used anywhere though
        /// </summary>

		HitFlag_0x1 = 0x1,
        /// <summary>
        /// This flag causes the wound animation to be played and a sound that is dependent on other flags
        /// If 0x20000 (Crushing) is set, UnitSound 9 will be played
        /// If 0x200 (Critical) is set, UnitSound 3 will be played
        /// If 0x10 (Miss) is not present, UnitSound 2 will be played
        /// </summary>
		PlayWoundAnimation = 0x02,

		OffHand = 0x04,

		HitFlag_0x8 = 0x08,

		Miss = 0x10,
		/// <summary>
		/// Plays absorb sound and flash Absorb message.
        /// Absorbed part/all of damage type 1
        /// </summary>
		AbsorbType1 = 0x20,
        /// <summary>
        /// Plays absorb sound and flash Absorb message.
        /// Absorbed part/all of damage type 2
        /// </summary>
		AbsorbType2 = 0x40,
        /// <summary>
        /// Resisted part/all of damage type 1
        /// </summary>
		ResistType1 = 0x80,
        /// <summary>
        /// Resisted part/all of damage type 2
        /// </summary>
		ResistType2 = 0x100,
        /// <summary>
        /// Plays UnitSound 3
        /// </summary>
		CriticalStrike = 0x200,

		HitFlag_0x400 = 0x400,
		HitFlag_0x800 = 0x800,
		HitFlag_0x1000 = 0x1000,

		Block = 0x2000,
        /// <summary>
        /// Hides MissType text from being displayed on the victim if damage is 0, as long as flag 0x1000000 is not present
        /// </summary>
        HideWorldTextForNoDamage = 0x4000,
        /// <summary>
        /// Changes blood to spurt from the back instead of the front
        /// </summary>
        BloodSpurtInBack = 0x8000,

		Glancing = 0x10000,
        /// <summary>
        /// Plays UnitSound 9
        /// </summary>
		Crushing = 0x20000,
        /// <summary>
        /// Ignore's this attack round
        /// </summary>
		Ignore = 0x40000,

		SwingNoHitSound = 0x80000,

		HitFlag_0x100000 = 0x100000,
		HitFlag_0x200000 = 0x200000,
		HitFlag_0x400000 = 0x400000,
        /// <summary>
        /// Modifies packet structure. The Int dependent on this flag is the delta to add to the current predicted power
        /// </summary>
        ModifyPredictedPower = 0x800000,
        /// <summary>
        /// Show blood spurt even if damage is 0
        /// </summary>
        ForceShowBloodSpurt = 0x1000000,

		HitFlag_0x2000000 = 0x2000000,
		HitFlag_0x4000000 = 0x4000000,
		HitFlag_0x8000000 = 0x8000000,
	}

	public enum WeaponAttackType
	{
		BaseAttack = 0,
		OffhandAttack = 1,
		RangedAttack = 2
	}

	public enum DamageEffectType
	{
		DirectDamage = 0,
		SpellDirectDamage = 1,
		DamageOverTime = 2,
		Heal = 3,
		NoDamage = 4,
		SelfDamage = 5
	}

	[Flags]
	public enum DamageSchoolMask : uint
	{
		None = 0,
		Physical = 1,
		Holy = 2,
		Fire = 4,
		Nature = 8,
		Frost = 0x10,
		Shadow = 0x20,
		Arcane = 0x40,

		MagicSchools = Holy | Fire | Nature | Frost | Shadow | Arcane,
		AllSchools = MagicSchools | Physical
	}

	public enum DamageSchool : uint
	{
		Physical = 0,
		Holy = 1,
		Fire = 2,
		Nature = 3,
		Frost = 4,
		Shadow = 5,
		Arcane = 6,
		Count
	}

    // SMSG_AI_REACTION
    public enum AIReaction
    {
        Alert = 0x0,
        Friendly = 0x1,
        Hostile = 0x2,
        Afraid = 0x3,
        Destroy = 0x4
    };
}