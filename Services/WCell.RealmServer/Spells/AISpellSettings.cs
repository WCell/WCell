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

		public AISpellSettings(Spell spell)
		{
			Spell = spell;
		}

		public Spell Spell
		{
			get;
			private set;
		}

		public void SetValues(int cdMin, int cdMax)
		{
			SetCooldown(cdMin, cdMax);
		}

		public void SetValues(int cd)
		{
			SetCooldown(cd);
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

			// select a default cooldown time
			var def = GetDefaultCategoryCooldown(category);

			if (Cooldown.MinDelay < 0)
			{
				Cooldown.MinDelay = def.MinDelay;
			}

			if (Cooldown.MaxDelay < 0)
			{
				Cooldown.MinDelay = def.MaxDelay;
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

		public static TargetDefinition GetDefaultAITargetHandlerDefintion(SpellEffect effect)
		{
			
		}
	}
	#endregion
}