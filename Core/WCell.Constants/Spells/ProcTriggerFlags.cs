using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Spells
{
	/// <summary>
	/// Spells with ProcTriggerFlags have ProcTriggerSpell aura effects that are usually
	/// missing the id of the Spell to be casted.
	/// </summary>
	[Flags]
	public enum ProcTriggerFlags : uint
	{
		#region Generic
		Flag0x1000000 = 0x1000000,
		Flag0x2000000 = 0x2000000,
		Flag0x4000000 = 0x4000000,
		Flag0x8000000 = 0x8000000,

		Flag0x10000000 = 0x10000000,
		Flag0x20000000 = 0x20000000,
		Flag0x40000000 = 0x40000000,
		Flag0x80000000 = 0x80000000,
		#endregion

		None = 0x0,
		/// <summary>
		/// Seems useless
		/// </summary>
		AnyHostileAction = 0x1,

		/// <summary>
		/// Triggered when killing a target that yields experience or honor
		/// </summary>
		GainExperience = 0x2,
		/// <summary>
		/// We attack
		/// </summary>
		MeleeHitOther = 0x4,
		/// <summary>
		/// We are critically hit
		/// </summary>
		MeleeCriticalHit = 0x8,

		/// <summary>
		/// We cast a damage spell.
		/// If you want this to proc on non-damaging spells,
		/// make sure to use Spell.AddCasterProcSpells.
		/// </summary>
		SpellCast = 0x10,

		/// <summary>
		/// We are attacked physically
		/// </summary>
		PhysicalAttack = 0x20,
		/// <summary>
		/// We hit someone with a ranged weapon's ammo
		/// </summary>
		RangedHitOther = 0x40,
		/// <summary>
		/// We are critcally hit with a ranged weapon
		/// </summary>
		RangedCriticalHit = 0x80,

		/// <summary>
		/// We physically attack someone else
		/// </summary>
		PhysicalAttackOther = 0x100,
		/// <summary>
		/// We are struck by a melee weapon
		/// </summary>
		MeleeHit = 0x200,
		/// <summary>
		/// We do something with someone else
		/// </summary>
		ActionOther = 0x400,
		/// <summary>
		/// Unused
		/// </summary>
		ProcTrigger0x800 = 0x800,

		/// <summary>
		/// We critically hit someone
		/// </summary>
		MeleeCriticalHitOther = 0x1000,

		/// <summary>
		/// We are hit by a ranged weapon
		/// </summary>
		RangedHit = 0x2000,

		/// <summary>
		/// We heal sb else
		/// </summary>
		HealOther = 0x4000,
		/// <summary>
		/// We get healed
		/// </summary>
		Heal = 0x8000,

		/// <summary>
		/// We cast a critical damage spell.
		/// See SpellCast for reference.
		/// </summary>
		SpellCastCritical = 0x10000,

		/// <summary>
		/// We get hit by a damage spell
		/// </summary>
		SpellHit = 0x20000,

		/// <summary>
		/// We get critically hit by a damage spell
		/// </summary>
		SpellHitCritical = 0x40000,
		ProcFlag0x80000 = 0x80000,

		AnyDamage = 0x100000,

		/// <summary>
		/// Someone stepped in our trap
		/// </summary>
		TrapTriggered = 0x200000,

		/// <summary>
		/// Seems useless
		/// </summary>
		AutoShotHit = 0x400000,
		/// <summary>
		/// Seems useless, its set usually together with AutoShotHit on any kind of Proc
		/// </summary>
		Absorb = 0x800000,

		/*
		 * Custom Flags
		 * The Following effects were added to provide additional functionality:
		 */
		/// <summary>
		/// Triggered for the caster, when an Aura is started on a target
		/// </summary>
		AuraStarted = 0x20000000,

		/// <summary>
		/// Triggered for the caster, when an Aura gets removed
		/// </summary>
		AuraRemoved = 0x40000000,

		/// <summary>
		/// Triggered when blocking damage
		/// </summary>
		Block = 0x80000000,


		All = 0xFFFFFFFF
	}

	/**
	 
	 * 	enum SpellProcFlags
	{
		SCRIPT_ACTION									= 0x0000001,
		KILLING_TARGET_THAT_GIVES_XP					= 0x0000002,
		BEGIN_HIT_BY_MELEE_ATTACK_WHITE_DAMAGE_ONLY		= 0x0000008,
		MELEE_ATTACK_YELLOW_DAMAGE_ONLY					= 0x0000010,
		BEING_HIT_BY_MELEE_ATTACK_YELLOW_DAMAGE_ONLY	= 0x0000020,
		RANGED_ATTACK_WHITE_DAMAGE_ONLY					= 0x0000040,
		BEING_HIT_BY_RANGED_ATTACK_WHITE_DAMAGE_ONLY	= 0x0000080,
		RANGED_ATTACK_YELLOW_DAMAGE_ONLY				= 0x0000100,
		BEING_HIT_BY_RANGED_ATTACK_YELLOW_DAMAGE_ONLY	= 0x0000200,
		CASTING_ANY_SPELL								= 0x0000400,
		BEING_CAST_ANY_SPELL							= 0x0000800,
		DEALING_DAMAGE									= 0x0001000,
		TAKINNG_ANY_DAMAGE								= 0x0002000,
		HEALING_ANY_TARGET								= 0x0004000,
		BEING_HEALED									= 0x0008000,
		DEALING_DIRECT_MAGIC_DAMAGE						= 0x0010000,	// mimo DOT
		TAKING_DIRECT_MAGIC_DAMAGE						= 0x0020000,	// mimo DOT
		DEALING_PERIODIC_MAGIC_DAMAGE					= 0x0040000,	// DOT
		TAKING_PERIODIC_MAGIC_DAMAGE					= 0x0080000,	// DOT
		LOSING_HEALTH									= 0x0100000,
		TRAPING_TARGET									= 0x0200000,
		MELEE_ATTACK_WITH_MH_WEAPON						= 0x0400000,
		MELEE_ATTACK_WITH_OH_WEAPON						= 0x0800000,
		DEATH											= 0x1000000,
	};
	 */
}