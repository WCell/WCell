using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Targeting
{

	public static class DefaultTargetFilters
	{
		public static void IsFriendly(SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason)
		{
			if (effectHandler.Cast.CasterObject.MayAttack(target))
			{
				failedReason = SpellFailedReason.TargetEnemy;
			}
		}

		public static void IsHostile(SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason)
		{
			var caster = effectHandler.Cast.CasterObject;
			if (!caster.MayAttack(target))
			{
				failedReason = SpellFailedReason.TargetFriendly;
			}
			else if (!target.CanBeHarmed)
			{
				failedReason = SpellFailedReason.NotHere;
			}
		}

		/// <summary>
		/// Duel target type
		/// </summary>
		/// <param name="targets"></param>
		/// <param name="target"></param>
		/// <param name="failedReason"></param>
		public static void IsHostileOrHealable(SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason)
		{
			var caster = effectHandler.Cast.CasterObject;
			var spell = effectHandler.Cast.Spell;
			var isHarmful = spell.HasHarmfulEffects;
			var isHarmfulAndBeneficial = spell.HasHarmfulEffects == spell.HasBeneficialEffects;

			if (!isHarmfulAndBeneficial)
			{
				if (isHarmful != caster.MayAttack(target))
				{
					if (isHarmful)
					{
						failedReason = SpellFailedReason.TargetFriendly;
					}
					else
					{
						failedReason = SpellFailedReason.TargetEnemy;
					}
				}
				else if (isHarmful && !target.CanBeHarmed)
				{
					failedReason = SpellFailedReason.NotHere;
				}
			}
		}

		public static void IsAllied(SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason)
		{
			//if (targets.Cast.Caster is Character && target is Character)
			//{
			//    var caster = (Character)targets.Cast.Caster;
			//    if (!caster.IsAlliedWith(target))
			//    {
			//        failedReason = SpellFailedReason.TargetNotInParty;
			//    }
			//}
			if (!effectHandler.Cast.CasterObject.IsAlliedWith(target))
			{
				failedReason = SpellFailedReason.TargetNotInParty;
			}
		}

		public static void IsSameClass(SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason)
		{
			var caster = effectHandler.Cast.CasterUnit;
			if (caster == null || !(target is Unit) || caster.Class != ((Unit)target).Class)
			{
				failedReason = SpellFailedReason.BadTargets;
			}
		}

		public static void IsSamePartyAndClass(SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason)
		{
			IsAllied(effectHandler, target, ref failedReason);
			if (failedReason == SpellFailedReason.Ok)
			{
				IsSameClass(effectHandler, target, ref failedReason);
			}
		}

		public static void IsInFrontAndHostile(SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason)
		{
			IsInFrontOfCaster(effectHandler, target, ref failedReason, IsHostile);
		}

		public static void IsInFrontAndFriendly(SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason)
		{
			IsInFrontOfCaster(effectHandler, target, ref failedReason, IsFriendly);
		}

		public static void IsInFrontOfCaster(SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason, TargetFilter filter)
		{
			var caster = effectHandler.Cast.CasterObject;
			if (caster.IsPlayer && !target.IsInFrontOf(caster))
			{
				failedReason = SpellFailedReason.NotInfront;
			}
			else
			{
				filter(effectHandler, target, ref failedReason);
			}
		}

		/// <summary>
		/// Is caster behind target?
		/// </summary>
		public static void IsCasterBehind(SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason)
		{
			var caster = effectHandler.Cast.CasterObject;
			if (!caster.IsBehind(target))
			{
				failedReason = SpellFailedReason.NotBehind;
			}
			else
			{
				var harmful = effectHandler.Cast.Spell.HasHarmfulEffects;
				if (harmful != caster.MayAttack(target))
				{
					failedReason = harmful ? SpellFailedReason.TargetFriendly : SpellFailedReason.TargetEnemy;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static void IsVisible(SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason)
		{
			failedReason = effectHandler.Cast.CasterUnit.CanSee(target) ? SpellFailedReason.Ok : SpellFailedReason.BmOrInvisgod;
		}

		/// <summary>
		/// 
		/// </summary>
		public static void IsPlayer(SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason)
		{
			failedReason = target is Character ? SpellFailedReason.Ok : SpellFailedReason.TargetNotPlayer;
		}

		/// <summary>
		/// 
		/// </summary>
		public static void IsNotPlayer(SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason)
		{
			failedReason = target is Character ? SpellFailedReason.TargetIsPlayer : SpellFailedReason.Ok;
		}

		/// <summary>
		/// Only select targets that have at least half of what the effect can heal
		/// </summary>
		public static void IsWoundedEnough(SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason)
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
					return;
				}
			}

			// not wounded or not wounded enough
			failedReason = SpellFailedReason.AlreadyAtFullHealth;
		}
	}
}
