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
		public static readonly CooldownRange[] DefaultCooldownsByCategory = new CooldownRange[(int)AISpellCategory.End];

		public static CooldownRange GetDefaultCategoryCooldown(AISpellCategory cat)
		{
			return DefaultCooldownsByCategory[(int)cat];
		}

		public static void SetDefaultCategoryCooldown(AISpellCategory cat, int min, int max)
		{
			DefaultCooldownsByCategory[(int)cat] = new CooldownRange(min, max);
		}

		static AISpellSettings()
		{
			// initialize with default values - can be changed using the static SetDefaultCategoryCooldown method
			DefaultCooldownsByCategory[(int)AISpellCategory.AuraBeneficial] = new CooldownRange(30000, 60000);
			DefaultCooldownsByCategory[(int)AISpellCategory.AuraHarmful] = new CooldownRange(30000, 60000);
			DefaultCooldownsByCategory[(int)AISpellCategory.DirectBeneficial] = new CooldownRange(30000, 60000);
			DefaultCooldownsByCategory[(int)AISpellCategory.DirectHarmful] = new CooldownRange(5000, 10000);
		}

		public readonly CooldownRange Cooldown = new CooldownRange(-1, -1);

		/// <summary>
		/// Amount of time to idle after casting the spell
		/// </summary>
		public int IdleTimeAfterCastMillis = 500;

		public AISpellCastTarget TargetType;

		public AISpellSettings(Spell spell)
		{
			Spell = spell;
		}

		public Spell Spell
		{
			get;
			private set;
		}

		public void SetValues(int cdMin, int cdMax, AISpellCastTarget target)
		{
			SetCooldown(cdMin, cdMax);
			TargetType = target;
		}

		public void SetValues(int cd, AISpellCastTarget target)
		{
			SetCooldown(cd);
			TargetType = target;
		}

		public void SetTarget(AISpellCastTarget target)
		{
			TargetType = target;
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
			var category = Spell.GetSpellAICategory();
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

	#region AISpellCategory
	/// <summary>
	/// Categories of spells for AI casters
	/// </summary>
	public enum AISpellCategory
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
	public enum AISpellCastTarget
	{
		Default,

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
		public static AISpellCategory GetSpellAICategory(this Spell spell)
		{
			var beneficial = spell.HarmType == HarmType.Beneficial;
			if (spell.IsAura)
			{
				return beneficial ? AISpellCategory.AuraBeneficial : AISpellCategory.AuraHarmful;
			}
			else
			{
				return beneficial ? AISpellCategory.DirectBeneficial : AISpellCategory.DirectHarmful;
			}
		}
	}
	#endregion
}