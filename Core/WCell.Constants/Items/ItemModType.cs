namespace WCell.Constants.Items
{
	/// <summary>
	/// Item modifiers
	/// </summary>
	public enum ItemModType
	{
		None = -1,
		Power = 0,
		Health = 1,
		/// <summary>
		/// Unused
		/// </summary>
		Unused = 2,
		Agility = 3,
		Strength = 4,
		Intellect = 5,
		Spirit = 6,
		Stamina = 7,

		WeaponSkillRating = 11,
		DefenseRating = 12,
		DodgeRating = 13,
		ParryRating = 14,
		BlockRating = 15,
		/// <summary>
		/// Unused
		/// </summary>
		MeleeHitRating = 16,
		/// <summary>
		/// Unused
		/// </summary>
		RangedHitRating = 17,
		SpellHitRating = 18,
		MeleeCriticalStrikeRating = 19,
		RangedCriticalStrikeRating = 20,
		SpellCriticalStrikeRating = 21,
		/// <summary>
		/// Unused
		/// </summary>
		MeleeHitAvoidanceRating = 22,
		/// <summary>
		/// Unused
		/// </summary>
		RangedHitAvoidanceRating = 23,
		/// <summary>
		/// Unused
		/// </summary>
		SpellHitAvoidanceRating = 24,
		/// <summary>
		/// Unused (see Resilience)
		/// </summary>
		MeleeCriticalAvoidanceRating = 25,
		/// <summary>
		/// Unused (see Resilience)
		/// </summary>
		RangedCriticalAvoidanceRating = 26,
		/// <summary>
		/// Unused (see Resilience)
		/// </summary>
		SpellCriticalAvoidanceRating = 27,
		MeleeHasteRating = 28,
		RangedHasteRating = 29,
		SpellHasteRating = 30,
		/// <summary>
		/// Melee and Ranged HitRating (no SpellHitRating)
		/// </summary>
		HitRating = 31,

		CriticalStrikeRating = 32,
		/// <summary>
		/// Unused
		/// </summary>
		HitAvoidanceRating = 33,
		/// <summary>
		/// Unused (see Resilience)
		/// </summary>
		CriticalAvoidanceRating = 34,
		ResilienceRating = 35,
		HasteRating = 36,
		ExpertiseRating = 37,

		// 3.x
		AttackPower = 38,
		RangedAttackPower = 39,
		FeralAttackPower = 40,
		SpellHealingDone = 41,
		SpellDamageDone = 42,
		ManaRegeneration = 43,
		ArmorRegenRating = 44,
		SpellPower = 45,

		// 3.2.2
		HealthRegenration = 46,
		SpellPenetration = 47,
		BlockValue = 48,

        // 4.0.0
        MasteryRating = 49,
        ExtraArmor = 50,
        FireResistance = 51,
        FrostResistance = 52,
        HolyResistance = 53,
        ShadowResistance = 54,
        NatureResistence = 55,
        ArcaneResistance = 56,

		End
	}
}
