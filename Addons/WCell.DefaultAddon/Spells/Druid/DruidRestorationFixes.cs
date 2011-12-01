using System;
using NLog;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.RealmServer.Spells.Auras.Misc;
using WCell.RealmServer.Spells.Effects;
using WCell.Util;

namespace WCell.Addons.Default.Spells.Druid
{
	public static class DruidRestorationFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixIt()
		{
			// Nature's Grace needs to set the correct set of affecting spells
            //SpellLineId.DruidBalanceNaturesGrace.Apply(spell =>
            //{
            //    // copy AffectMask from proc effect, which has it all set correctly
            //    var effect = spell.GetEffect(AuraType.ProcTriggerSpell);
            //    var triggerSpellEffect = effect.GetTriggerSpell().GetEffect(AuraType.ModCastingSpeed);
            //    effect.AffectMask = triggerSpellEffect.AffectMask;
            //});

			// Wild Growth: "The amount healed is applied quickly at first, and slows down"
			// see http://www.wowhead.com/spell=48438#comments
			SpellLineId.DruidRestorationWildGrowth.Apply(spell =>
			{
				// set max target effect
				spell.MaxTargetEffect = spell.GetEffect(SpellEffectType.None);

				// fix target types of all effects
				spell.ForeachEffect(effect => effect.ImplicitTargetA = ImplicitSpellTargetType.PartyAroundCaster);

				// customize the healing process
				var healEffect = spell.GetEffect(AuraType.PeriodicHeal);

				var ticks = spell.Durations.Max / healEffect.AuraPeriod;			// amount of ticks

				// bump up the amount healed, so it averages out over time to still give the correct amount:
				// sum(x / 1.1^i, i = 0 to ticks-1) = ticks * val <=> x = ticks * val / sum(1/1.1^i, i = 0 to ticks-1)
				var divisor = 0f;
				for (var i = 0; i < ticks; i++) divisor += 1 / (float)Math.Pow(WildGrowthHandler.TickPercentReduction / 100f, i);
				healEffect.BasePoints = MathUtil.RoundInt(ticks * healEffect.BasePoints / divisor);

				// set the special aura handler
				healEffect.AuraEffectHandlerCreator = () => new WildGrowthHandler();
			});

			// Living Seed applies a proc aura on crit hit that heals the target when he/she gets attacked the next time
			SpellLineId.DruidRestorationLivingSeed.Apply(spell =>
			{
				// trigger proc spell on heal crit
				var dummy = spell.GetEffect(AuraType.Dummy);
				dummy.IsProc = true;
				dummy.TriggerSpellId = SpellId.LivingSeed_2;
				dummy.AuraEffectHandlerCreator = () => new ProcTriggerSpellOnCritHandler();
			});
			SpellHandler.Apply(spell =>
			{
				var dummy = spell.GetEffect(AuraType.Dummy);
				dummy.IsProc = true;

				// need a custom proc handler to heal for a percentage of the trigger heal
				dummy.AuraEffectHandlerCreator = () => new LivingSeedHandler();
			}, SpellId.LivingSeed_2);

			// these two have some kind of multiplier added to their periodic heal: "${$m1*5*$<mult>}"
			FixRegrowthAndRejuvenation(SpellLineId.DruidRejuvenation);
			FixRegrowthAndRejuvenation(SpellLineId.DruidRegrowth);

			// Nourish "Heals for an additional 20% if you have a Rejuvenation, Regrowth, Lifebloom, or Wild Growth effect active on the target."
			SpellLineId.DruidNourish.Apply(spell =>
			{
				spell.GetEffect(SpellEffectType.Heal).SpellEffectHandlerCreator = (cast, effct) => new NourishHandler(cast, effct);
			});

			// Lifebloom: "When Lifebloom completes its duration or is dispelled, the target instantly heals themself for $s2 and the Druid regains half the cost of the spell."
			SpellLineId.DruidLifebloom.Apply(spell =>
			{
				spell.GetEffect(AuraType.Dummy).AuraEffectHandlerCreator = () => new LifebloomHandler();
			});

            SpellLineId.DruidRevive.Apply(spell =>
            {
                var effect = spell.GetEffect(SpellEffectType.Resurrect);
                effect.ImplicitTargetA = ImplicitSpellTargetType.SingleFriend;
            });
		}

		private static void FixRegrowthAndRejuvenation(SpellLineId line)
		{
			line.Apply(spell =>
			{
				// apply the AuraState, so swiftmend can be used (AuraState probably going to be replaced with an invisible Aura in later versions)
				spell.AddAuraEffect(() => new AddTargetAuraStateHandler(AuraStateMask.RejuvenationOrRegrowth));

				//var effect = spell.GetEffect(AuraType.PeriodicHeal);

				// TODO: Implement <mult> from "${$m1*5*$<mult>}"
			});
		}
	}

	#region Lifebloom
	public class LifebloomHandler : AuraEffectHandler
	{
		protected override void Remove(bool cancelled)
		{
			if (!cancelled)
			{
				// "When Lifebloom completes its duration or is dispelled, the target instantly heals themself for $s2 and the Druid regains half the cost of the spell."
				var caster = m_aura.CasterUnit;
				Owner.Heal(EffectValue, caster, m_spellEffect);
				if (caster != null)
				{
					caster.Energize((m_aura.Spell.CalcBasePowerCost(caster) + 1) / 2, caster, m_spellEffect);
				}
			}
		}
	}
	#endregion

	#region Nourish
	public class NourishHandler : HealEffectHandler
	{
		public NourishHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		protected override void Apply(WorldObject target)
		{
			var unit = (Unit)target;

			// Heals for an additional 20% if you have a Rejuvenation, Regrowth, Lifebloom, or Wild Growth effect active on the target.
			if (CheckBonus(unit, SpellLineId.DruidRejuvenation) ||
				CheckBonus(unit, SpellLineId.DruidRegrowth) ||
				CheckBonus(unit, SpellLineId.DruidLifebloom) ||
				CheckBonus(unit, SpellLineId.DruidRestorationWildGrowth))
			{
				var val = CalcDamageValue();
				val += (val * 20 + 50) / 100;
				((Unit)target).Heal(val, m_cast.CasterUnit, Effect);
			}
			else
			{
				base.Apply(target);
			}
		}

		bool CheckBonus(Unit unit, SpellLineId line)
		{
			var bonus = unit.Auras[line];
			if (bonus == null || bonus.CasterReference != Cast.CasterReference)
			{
				return false;
			}
			return true;
		}

		public override ObjectTypes TargetType
		{
			get { return ObjectTypes.Unit; }
		}
	}
	#endregion

	#region LivingSeed
	public class LivingSeedHandler : AuraEffectHandler
	{
		private int _damageAmount;

		protected override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference, Unit target, ref SpellFailedReason failReason)
		{
			// remember the damageAmount from the TriggerAction
			if (creatingCast != null && creatingCast.TriggerAction is HealAction)
			{
				_damageAmount = ((HealAction)creatingCast.TriggerAction).Value;
			}
			else
			{
				failReason = SpellFailedReason.Error;
			}
		}

		public override void OnProc(Unit triggerer, IUnitAction action)
		{
			// heal (and dispose automatically, since the single ProcCharge is used up)
			// "30% of the amount healed"
			Owner.Heal((_damageAmount * 3 + 5) / 10, m_aura.CasterUnit, m_spellEffect);
		}
	}
	#endregion

	#region WildGrowth
	public class WildGrowthHandler : PeriodicHealHandler
	{
		/// <summary>
		/// divide by 1.1 per tick: Heal in one instance went from 390 to 290 - that is 6 ticks
		/// </summary>
		public const int TickPercentReduction = 110;

		protected override void Apply()
		{
			BaseEffectValue = (BaseEffectValue * TickPercentReduction + 5) / 100;
			base.Apply();
		}
	}
	#endregion

	#region Regrowth & Rejuvenation & Swifmend
	public class SwiftmendHandler : SpellEffectHandler
	{
		Aura _aura;

		public SwiftmendHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override SpellFailedReason InitializeTarget(WorldObject target)
		{
			var auras = ((Unit)target).Auras;
			var rejuvenation = auras[SpellLineId.DruidRejuvenation];
			var regrowth = auras[SpellLineId.DruidRegrowth];

			if (rejuvenation != null)
			{
				if (regrowth != null)
				{
					_aura = (rejuvenation.TimeLeft < regrowth.TimeLeft) ? rejuvenation : regrowth;
				}
				else
				{
					_aura = rejuvenation;
				}
			}
			else
			{
				if (regrowth != null)
				{
					_aura = regrowth;
				}
				else
				{
					_aura = null;
					return SpellFailedReason.TargetAurastate;
				}
			}

			return base.InitializeTarget(target);
		}

		protected override void Apply(WorldObject target)
		{
			var handler = _aura.GetHandler(AuraType.PeriodicHeal) as PeriodicHealHandler;
			if (handler == null)
			{
				LogManager.GetCurrentClassLogger().Warn("Aura does not have a ParameterizedPeriodicHealHandler: " + _aura);
				return;
			}

            // "amount equal to 12 sec of Rejuvenation" 4 ticks "or 18 sec. of Regrowth" 6 ticks
		    int ticks = _aura.Spell.Line.LineId == SpellLineId.DruidRejuvenation ? 4 : 6;

			//var maxTicks = aura.MaxTicks;
			// TODO: Add correct heal bonuses
			var amount = handler.EffectValue * ticks;

			((Unit)target).Heal(amount, m_cast.CasterUnit, Effect);

			_aura.Cancel();
		}

		public override ObjectTypes TargetType
		{
			get { return ObjectTypes.Unit; }
		}
	}

	public class AddTargetAuraStateHandler : AuraEffectHandler
	{
		public AuraStateMask Mask { get; set; }

		public AddTargetAuraStateHandler(AuraStateMask mask)
		{
			Mask = mask;
		}

		protected override void Apply()
		{
			Owner.AuraState |= Mask;
		}

		protected override void Remove(bool cancelled)
		{
			Owner.AuraState ^= Mask;
		}
	}
	#endregion
}
