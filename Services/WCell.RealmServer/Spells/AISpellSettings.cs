using System;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras;
using WCell.Util;

namespace WCell.RealmServer.Spells
{
	/// <summary>
	/// Defines parameters of AI when casting this spell
	/// </summary>
	public class AISpellSettings
	{
		public static readonly CooldownRange[] DefaultCooldownsByCategory = new CooldownRange[(int)AISpellCooldownCategory.End];

		public static CooldownRange GetDefaultCategoryCooldown(AISpellCooldownCategory cat)
		{
			return DefaultCooldownsByCategory[(int)cat];
		}

		public static void SetDefaultCategoryCooldown(AISpellCooldownCategory cat, int min, int max)
		{
			DefaultCooldownsByCategory[(int)cat] = new CooldownRange(min, max);
		}

		static AISpellSettings()
		{
			// initialize with default values - can be changed using the static SetDefaultCategoryCooldown method
			DefaultCooldownsByCategory[(int)AISpellCooldownCategory.AuraBeneficial] = new CooldownRange(30000, 60000);
			DefaultCooldownsByCategory[(int)AISpellCooldownCategory.AuraHarmful] = new CooldownRange(30000, 60000);
			DefaultCooldownsByCategory[(int)AISpellCooldownCategory.DirectBeneficial] = new CooldownRange(30000, 60000);
			DefaultCooldownsByCategory[(int)AISpellCooldownCategory.DirectHarmful] = new CooldownRange(5000, 10000);
		}

		public readonly CooldownRange Cooldown = new CooldownRange(-1, -1);

		/// <summary>
		/// Amount of time to idle after casting the spell
		/// </summary>
		public int IdleTimeAfterCastMillis = 500;

		public AISpellCastTargetType TargetType;

		public AISpellSettings(Spell spell)
		{
			Spell = spell;
		}

		public Spell Spell
		{
			get;
			private set;
		}

		public void SetValues(int cdMin, int cdMax, AISpellCastTargetType targetType)
		{
			SetCooldown(cdMin, cdMax);
			TargetType = targetType;
		}

		public void SetValues(int cd, AISpellCastTargetType targetType)
		{
			SetCooldown(cd);
			TargetType = targetType;
		}

		public void SetTarget(AISpellCastTargetType targetType)
		{
			TargetType = targetType;
		}

		public void SetCooldown(int cd)
		{
			SetCooldown(cd, cd);
		}

		public void SetCooldown(int cdMin, int cdMax)
		{
			Cooldown.MinDelay = cdMin;
			Cooldown.MaxDelay = cdMax;
		}

		#region Initialization & Loading
		internal void InitializeAfterLoad()
		{
			// set all values that were not overridden
			var category = Spell.GetAISpellCooldownCategory();
			var def = GetDefaultCategoryCooldown(category);

			if (Cooldown.MinDelay < 0)
			{
				Cooldown.MinDelay = def.MinDelay;
			}

			if (Cooldown.MaxDelay < 0)
			{
				Cooldown.MinDelay = def.MaxDelay;
			}

			if (TargetType == AISpellCastTargetType.Default)
			{
				// figure out what kind of targeting method each effect requires
				foreach (var effect in Spell.Effects)
				{
					// only assign target type, if not overridden
					if (effect.AISpellCastTargetType != AISpellCastTargetType.Default) continue;

					if (effect.IsHealEffect)
					{
						effect.AISpellCastTargetType = AISpellCastTargetType.WoundedAlly;
					}
					else if (effect.IsDamageEffect)
					{
						effect.AISpellCastTargetType = AISpellCastTargetType.Hostile;
					}
					else if (effect.IsAuraEffect)
					{
						// avoid targets that already have that Aura
						effect.AISpellCastTargetType = effect.HarmType == HarmType.Beneficial ? 
							AISpellCastTargetType.ExclusiveBuff : AISpellCastTargetType.ExclusiveDebuff;
					}
					else
					{
						// no other category
						effect.AISpellCastTargetType = effect.HarmType == HarmType.Beneficial ?
							AISpellCastTargetType.Allied : AISpellCastTargetType.Hostile;
					}
				}
			}
		}
		#endregion
	}

	#region CooldownRange
	/// <summary>
	/// Defines minimum and maximum of possible Spell cooldown.
	/// </summary>
	public class CooldownRange
	{
		public int MinDelay, MaxDelay;

		public CooldownRange()
		{
		}

		public CooldownRange(int min, int max)
		{
			MinDelay = min;
			MaxDelay = max;
		}

		public int GetRandomCooldown()
		{
			return Utility.Random(MinDelay, MaxDelay);
		}
	}
	#endregion

	#region AISpellCooldownCategory
	/// <summary>
	/// Categories of default cooldowns for AI casters
	/// </summary>
	public enum AISpellCooldownCategory
	{
		/// <summary>
		/// Buff or positive aura (eg. to improve stats, speed etc)
		/// </summary>
		AuraBeneficial,

		/// <summary>
		/// Debuff or negative aura (eg. to reduce stats, speed etc)
		/// </summary>
		AuraHarmful,

		/// <summary>
		/// Positive non-aura spell (healing etc)
		/// </summary>
		DirectBeneficial,

		/// <summary>
		/// Negative non-aura spell (damage spells)
		/// </summary>
		DirectHarmful,

		End
	}
	#endregion

	#region AISpellCastTarget
	/// <summary>
	/// Custom target types that are not covered by <see cref="ImplicitSpellTargetType"/>
	/// </summary>
	public enum AISpellCastTargetType
	{
		Default,

		// standard targets (used for any spell, by default)
		
		// hostile

		/// <summary>
		/// Negative auras
		/// </summary>
		ExclusiveDebuff,

		/// <summary>
		/// Damage spells only require hostile targets
		/// </summary>
		Hostile,


		// allied
		
		/// <summary>
		/// Any positive non-categorized spell
		/// </summary>
		Allied,

		/// <summary>
		/// Positive auras
		/// </summary>
		ExclusiveBuff,

		/// <summary>
		/// Heal spells
		/// </summary>
		WoundedAlly,



		// special targets (used for boss spells)

		// hostile
		NearestHostilePlayer,
		RandomHostilePlayer,
		SecondHighestThreatTarget,


		// allied
		RandomAlliedUnit
	}
	#endregion

	#region SpellAIUtil
	public static class AISpellUtil
	{
		public static AISpellCooldownCategory GetAISpellCooldownCategory(this Spell spell)
		{
			var beneficial = spell.HarmType == HarmType.Beneficial;
			if (spell.IsAura)
			{
				return beneficial ? AISpellCooldownCategory.AuraBeneficial : AISpellCooldownCategory.AuraHarmful;
			}
			else
			{
				return beneficial ? AISpellCooldownCategory.DirectBeneficial : AISpellCooldownCategory.DirectHarmful;
			}
		}
	}
	#endregion
}