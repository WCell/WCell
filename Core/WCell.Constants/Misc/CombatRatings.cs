using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Misc
{
	public enum CombatRating : uint
	{
		WeaponSkill = 1,
		DefenseSkill = 2,
		Dodge = 3,
		Parry = 4,
		Block = 5,
		MeleeHitChance = 6,
		RangedHitChance = 7,
		SpellHitChance = 8,
		MeleeCritChance = 9,
		RangedCritChance = 10,
		SpellCritChance = 11,
		/// <summary>
		/// Chance of an attacker to hit the char with a melee attack
		/// </summary>
		MeleeAttackerHit = 12,
		/// <summary>
		/// Chance of an attacker to hit the char with a ranged attack
		/// </summary>
		RangedAttackerHit = 13,
		/// <summary>
		/// Chance of an attacker to hit the char with spells
		/// </summary>
		SpellAttackerHit = 14,
		/// <summary>
		/// Reduction of chance of an attacker to make a melee crit
		/// </summary>
		MeleeResilience = 15,
		/// <summary>
		/// Reduction of chance of an attacker to make a ranged crit
		/// </summary>
		RangedResilience = 16,
		/// <summary>
		/// Reduction of chance of an attacker to make a spell crit
		/// </summary>
		SpellResilience = 17,

		MeleeHaste = 18,
		RangedHaste = 19,
		SpellHaste = 20,

		WeaponSkillMainhand = 21,
		WeaponSkillOffhand = 22,
		WeaponSkillRanged = 23,
		Expertise = 24
	}

	/// <summary>
	/// Used for AuraType: ModRating
	/// </summary>
	[Flags]
	public enum CombatRatingMask : uint
	{
		/// <summary>
		/// Different kinds of weapons - The actual weapon has to be figured out (but, depending on what?)
		/// </summary>
		Weapon = 0x00000001,
		Defence = 0x00000002,
		/// <summary>
		/// Modifies Dodge rating
		/// Most Rejuvenation Spells also have this one (but with EffectValue = 0)
		/// </summary>
		Dodge = 0x00000004,
		Parry = 0x00000008,
		Block = 0x00000010,
		MeleeHitChance = 0x00000020,
		RangedHitChance = 0x00000040,
		SpellHitChance = 0x00000080,
		MeleeCritical = 0x00000100,
		RangedCritical = 0x00000200,
		SpellCritical = 0x00000400,
		MeleeResilience = 0x00004000,
		RangedResilience = 0x00008000,
		SpellResilience = 0x00010000,
		MeleeHaste = 0x00020000,
		RangedHaste = 0x00040000,
		SpellHaste = 0x00080000,
		Resilience = 0x1C000
	}
}