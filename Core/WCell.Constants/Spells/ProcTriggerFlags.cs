using System;

namespace WCell.Constants.Spells
{
	/// <summary>
	/// Indicates events that let this Spell proc (if it is a proc spell)
	/// </summary>
	/// <remarks>
	/// Spells with ProcTriggerFlags have ProcTriggerSpell aura effects that are usually
	/// missing the id of the Spell to be casted.
	/// </remarks>
	[Flags]
	public enum ProcTriggerFlags : uint
	{
		None = 0x0,

		[Obsolete("Not used")]
		ScriptedAction = 0x1,

		KilledTargetThatYieldsExperienceOrHonor = 0x2,

		DoneMeleeAutoAttack = 0x4,
		ReceivedMeleeAutoAttack = 0x8,

		/// <summary>
		/// Done attack from spell with <see cref="DamageType.Melee"/>
		/// </summary>
		DoneMeleeSpell = 0x10,

		/// <summary>
		/// Received attack from spell with <see cref="DamageType.Melee"/>
		/// </summary>
		ReceivedMeleeSpell = 0x20,

		DoneRangedAutoAttack = 0x40,
		ReceivedRangedAutoAttack = 0x80,

		/// <summary>
		/// Done attack from spell with <see cref="DamageType.Ranged"/>
		/// </summary>
		DoneRangedSpell = 0x100,

		/// <summary>
		/// Received attack from spell with <see cref="DamageType.Ranged"/>
		/// </summary>
		ReceivedRangedSpell = 0x200,

		/// <summary>
		/// Done <see cref="HarmType.Beneficial"/> spell with <see cref="DamageType.None"/>
		/// </summary>
		DoneBeneficialSpell = 0x400,

		/// <summary>
		/// Received <see cref="HarmType.Beneficial"/> spell with <see cref="DamageType.None"/>
		/// </summary>
		ReceivedBeneficialSpell = 0x800,

		/// <summary>
		/// Done <see cref="HarmType.Harmful"/> spell with <see cref="DamageType.None"/>
		/// </summary>
		DoneHarmfulSpell = 0x1000,

		/// <summary>
		/// Received <see cref="HarmType.Harmful"/> spell with <see cref="DamageType.None"/>
		/// </summary>
		ReceivedHarmfulSpell = 0x2000,

		/// <summary>
		/// Done <see cref="HarmType.Beneficial"/> spell with <see cref="DamageType.Magic"/>
		/// </summary>
		DoneBeneficialMagicSpell = 0x4000,

		/// <summary>
		/// Received <see cref="HarmType.Beneficial"/> spell with <see cref="DamageType.Magic"/>
		/// </summary>
		ReceivedBeneficialMagicSpell = 0x8000,

		/// <summary>
		/// Done <see cref="HarmType.Harmful"/> spell with <see cref="DamageType.Magic"/>
		/// </summary>
		DoneHarmfulMagicSpell = 0x10000,

		/// <summary>
		/// Received <see cref="HarmType.Harmful"/> spell with <see cref="DamageType.Magic"/>
		/// </summary>
		ReceivedHarmfulMagicSpell = 0x20000,

		DonePeriodicDamageOrHeal = 0x40000,
		ReceivedPeriodicDamageOrHeal = 0x80000,

		ReceivedAnyDamage = 0x100000,

		/// <summary>
		/// Someone stepped in our trap
		/// </summary>
		TrapTriggered = 0x200000,

		[Obsolete("Not used")]
		DoneMeleeAttackWithMainHandWeapon = 0x400000,

		[Obsolete("Not used")]
		DoneMeleeAttackWithOffHandWeapon = 0x800000,

		/// <summary>
		/// We have died
		/// </summary>
		Death = 0x01000000,

		RequiringHitFlags = DoneMeleeAutoAttack | DoneMeleeSpell |
									DoneRangedAutoAttack | DoneRangedSpell |
									DoneHarmfulSpell | DoneHarmfulMagicSpell |
									DoneBeneficialSpell | DoneBeneficialMagicSpell |
									DonePeriodicDamageOrHeal | ReceivedPeriodicDamageOrHeal |
									ReceivedMeleeAutoAttack | ReceivedMeleeSpell |
									ReceivedRangedAutoAttack | ReceivedRangedSpell |
									ReceivedHarmfulSpell | ReceivedHarmfulMagicSpell |
									ReceivedBeneficialSpell | ReceivedBeneficialMagicSpell
	}

	/// <summary>
	/// Contains information needed for ProcTriggerFlags depending on hit result
	/// </summary>
	[Flags]
	public enum ProcHitFlags : uint
	{
		None = 0x00000000,
		NormalHit = 0x00000001,
		CriticalHit = 0x00000002,
		Hit = NormalHit | CriticalHit,
		Miss = 0x00000004,
		Resist = 0x00000008,
		Dodge = 0x00000010,
		Parry = 0x00000020,
		Block = 0x00000040,
		Evade = 0x00000080,
		Immune = 0x00000100,
		Deflect = 0x00000200,
		Absorb = 0x00000400,
		Reflect = 0x00000800,
		Interrupt = 0x00001000,
		FullBlock = 0x00002000,
		All = Hit | Miss | Resist | Dodge | Parry | Block | Evade | Immune | Deflect | Absorb | Reflect | Interrupt | FullBlock
	}

	/// <summary>
	/// procEx flags from UDB's spell_proc_event database table
	/// </summary>
	public enum ProcFlagsExLegacy : uint
	{
		None = 0x00000000,
		NormalHit = 0x00000001,
		CriticalHit = 0x00000002,
		Miss = 0x00000004,
		Resist = 0x00000008,
		Dodge = 0x00000010,
		Parry = 0x00000020,
		Block = 0x00000040,
		Evade = 0x00000080,
		Immune = 0x00000100,
		Deflect = 0x00000200,
		Absorb = 0x00000400,
		Reflect = 0x00000800,
		Interrupt = 0x00001000,
		FullBlock = 0x00002000,
		Reserved = 0x00004000,
		NotActiveSpell = 0x00008000,
		TriggerAlways = 0x00010000,
		OneTimeTrigger = 0x00020000,
		OnlyActiveSpell = 0x00040000,
	}
}