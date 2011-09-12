using System;
using WCell.RealmServer.Entities;
using WCell.Util;

namespace WCell.RealmServer.Spells.Targeting
{
	/// <summary>
	/// Set of methods to evaluate whether a target is "good" for an AI caster
	/// </summary>
	public static class DefaultTargetEvaluators
	{

		/// <summary>
		/// 
		/// </summary>
		public static int NearestEvaluator(SpellEffectHandler effectHandler, WorldObject target)
		{
			var caster = effectHandler.Cast.CasterObject;
			if (caster == null)
			{
				// no one is near or close
				return 0;
			}

			// the less the squared distance, the closer the target
			// less means better in evaluators
			return (int)(caster.GetDistanceSq(target) * 10);	// round the float to one digit after the dot
		}

		public static int RandomEvaluator(SpellEffectHandler effectHandler, WorldObject target)
		{
			return Utility.Random(0, int.MaxValue);
		}

		/// <summary>
		/// AI heal spells
		/// </summary>
		public static int MostWoundedEvaluator(SpellEffectHandler effectHandler, WorldObject target)
		{
			if (target is Unit)
			{
				// try to select the most wounded
				var unit = (Unit)target;
				var missingHealth = unit.MaxHealth - unit.Health;

				// cap at the max that the spell can heal
				// else weaker creatures might never get healed, simply because they can't lose as much health
				return -Math.Min(missingHealth, effectHandler.CalcDamageValue());
			}
			return int.MaxValue;
		}

		/// <summary>
		/// AI heal spells.
		/// This evaluator demonstrates how to losen constraints - i.e. don't go for the best choice, but go for anyone that satisfies some condition.
		/// </summary>
		public static int AnyWoundedEvaluator(SpellEffectHandler effectHandler, WorldObject target)
		{
			if (target is Unit)
			{
				// useful heal spell: Amount to be healed is greater or equal half of what the spell can heal
				// Select anyone with that problem
				var unit = (Unit)target;
				var missingHealth = unit.MaxHealth - unit.Health;
				if (missingHealth >= effectHandler.CalcDamageValue() / 2)
				{
					// good choice
					return -1;
				}
				// not so good choice
				return 0;
			}
			// should never happen
			return int.MaxValue;
		}
	}
}
