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
		HitFlag_0x1 = 0x1,
		NormalSwingAnim = 0x02,
		LeftSwing = 0x04,			// actually DualWield?
		HitFlag_0x8 = 0x08,
		Miss = 0x10,
		/// <summary>
		/// plays absorb sound and flash Absorb message
		/// </summary>
		Absorb_1 = 0x20,
		Absorb_2 = 0x40, // resisted at least some damage
		Resist_1 = 0x80,
		Resist_2 = 0x100,
		CriticalStrike = 0x200,
		HitFlag_0x400 = 0x400,
		HitFlag_0x800 = 0x800,
		HitFlag_0x1000 = 0x1000,
		Block = 0x2000,
        HitFlag_0x4000 = 0x4000,
        HitFlag_0x8000 = 0x8000,
		Glancing = 0x10000,
		Crushing = 0x20000,
		HitFlag_0x40000 = 0x40000,
		SwingNoHitSound = 0x80000,

		HitFlag_0x100000 = 0x100000,
		HitFlag_0x200000 = 0x200000,
		HitFlag_0x400000 = 0x400000,
		HitFlag_0x800000 = 0x800000,

		HitFlag_0x1000000 = 0x1000000,
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
		All = MagicSchools | Physical
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