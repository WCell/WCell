using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.Util.NLog;

namespace WCell.Addons.Default.Spells.Druid
{
	public static class DruidRestorationFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixIt()
		{
			// Omen of Clarity: "has a chance"
			SpellLineId.DruidRestorationOmenOfClarity.Apply(spell =>
			{
				// TODO: Fix proc chance (100 by default)
				spell.ProcChance = 5;
				spell.ProcDelay = 5000;

				// Add all "of the Druid's damage, healing spells and auto attacks" to the affect mask
				var effect = spell.GetEffect(AuraType.ProcTriggerSpell);
				effect.AddToAffectMask(
					SpellLineId.DruidBalanceInsectSwarm, SpellLineId.DruidFeralCombatFeralChargeBear,
					SpellLineId.DruidRestorationSwiftmend, SpellLineId.DruidBalanceForceOfNature,
					SpellLineId.DruidFeralCombatMangleBear, SpellLineId.DruidRestorationWildGrowth,
					SpellLineId.DruidBalanceStarfall, SpellLineId.DruidBalanceTyphoon, SpellLineId.DruidWrath,
					SpellLineId.DruidHealingTouch, SpellLineId.DruidRip, SpellLineId.DruidClaw, SpellLineId.DruidRegrowth,
					SpellLineId.DruidStarfire, SpellLineId.DruidDemoralizingRoar, SpellLineId.DruidTranquility,
					SpellLineId.DruidRavage, SpellLineId.DruidSwipeBear, SpellLineId.DruidMoonfire,
					SpellLineId.DruidEntanglingRoots, SpellLineId.DruidRake, SpellLineId.DruidRejuvenation,
					SpellLineId.DruidMaul, SpellLineId.DruidPounce, SpellLineId.DruidShred, SpellLineId.DruidBash,
					SpellLineId.DruidFerociousBite, SpellLineId.DruidLacerate, SpellLineId.DruidHurricane,
					SpellLineId.DruidSwipeCat, SpellLineId.DruidNourish, SpellLineId.DruidThorns,
					SpellLineId.DruidLifebloom, SpellLineId.DruidMaim, SpellLineId.DruidMangleCat);
			});

			// Nature's Grace needs to set the correct set of affecting spells
			SpellLineId.DruidBalanceNaturesGrace.Apply(spell =>
			{
				// copy AffectMask from proc effect, which has it all set correctly
				var effect = spell.GetEffect(AuraType.ProcTriggerSpell);
				var triggerSpellEffect = effect.GetTriggerSpell().GetEffect(AuraType.ModCastingSpeed);
				effect.AffectMask = triggerSpellEffect.AffectMask;
			});

			// Intensity only procs for Enrage
			SpellLineId.DruidRestorationIntensity.Apply(spell =>
			{
				var proc = spell.GetEffect(AuraType.ProcTriggerSpell);
				proc.AddToAffectMask(SpellLineId.DruidEnrage);
			});

			// Swiftmend: Consume & Heal
			SpellLineId.DruidRestorationSwiftmend.Apply(spell =>
			{
				var effect = spell.GetEffect(SpellEffectType.Heal);
				effect.SpellEffectHandlerCreator = (cast, effct) => new SwiftmendHandler(cast, effct);
			});

			// these two have some kind of multiplier added to their periodic heal: "${$m1*5*$<mult>}"
			FixRegrwothAndRejuvenation(SpellLineId.DruidRejuvenation);
			FixRegrwothAndRejuvenation(SpellLineId.DruidRegrowth);
		}

		private static void FixRegrwothAndRejuvenation(SpellLineId line)
		{
			line.Apply(spell =>
			{
				// apply the AuraState, so swiftmend can be used (AuraState probably going to be replaced with an invisible Aura in later versions)
				spell.AddAuraEffect(() => new AddTargetAuraStateHandler(AuraStateMask.RejuvenationOrRegrowth));

				var effect = spell.GetEffect(AuraType.PeriodicHeal);

				// TODO: Implement <mult> from "${$m1*5*$<mult>}"
				var ticks = spell.Durations.Max/effect.Amplitude;
				effect.AuraEffectHandlerCreator = () => new ParameterizedPeriodicHealHandler(effect.CalcEffectValue() * ticks);
			});
		}
	}

	#region Regrowth & Rejuvenation & Swifmend
	public class SwiftmendHandler : SpellEffectHandler
	{
		Aura aura;

		public SwiftmendHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override SpellFailedReason CheckValidTarget(WorldObject target)
		{
			var auras = ((Unit)target).Auras;
			aura = auras[SpellLineId.DruidRejuvenation];
			if (aura == null)
			{
				aura = auras[SpellLineId.DruidRegrowth];
				if (aura == null)
				{
					return SpellFailedReason.TargetAurastate;
				}
			}
			return base.CheckValidTarget(target);
		}

		protected override void Apply(WorldObject target)
		{
			var handler = aura.GetHandler(AuraType.PeriodicHeal) as ParameterizedPeriodicHealHandler;
			if (handler == null)
			{
				LogManager.GetCurrentClassLogger().Warn("Aura does not have a ParameterizedPeriodicHealHandler: " + aura);
				return;
			}

			int secs;
			if (aura.Spell.Line.LineId == SpellLineId.DruidRejuvenation)
			{
				// "amount equal to 12 sec of Rejuvenation"
				secs = 12;
			}
			else// if (aura.Spell.Line.LineId == SpellLineId.DruidRegrowth)
			{
				// "or 18 sec. of Regrowth"
				secs = 18;
			}

			var totalSecs = aura.Spell.Durations.Max;
			var amount = (handler.TotalHeal * secs + totalSecs) / totalSecs;

			((Unit)target).Heal(m_cast.CasterUnit, amount, Effect);

			aura.Cancel();
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
