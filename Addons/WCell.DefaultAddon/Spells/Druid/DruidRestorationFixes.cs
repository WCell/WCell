using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

			// Intensity only procs on Enrage
			SpellLineId.DruidRestorationIntensity.Apply(spell =>
			{
				var proc = spell.GetEffect(AuraType.ProcTriggerSpell);
				proc.AddToAffectMask(SpellLineId.DruidEnrage);
			});

			// Natural perfection only procs on crit
			SpellLineId.DruidRestorationNaturalPerfection.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.MeleeCriticalHit | ProcTriggerFlags.RangedCriticalHit;

				var proc = spell.GetEffect(AuraType.ProcTriggerSpell);
				proc.ClearAffectMask();									// no spell restrictions for the proc
			});

			// ToL triggers a passive area aura
			SpellLineId.DruidRestorationTreeOfLifeShapeshift.Apply(spell =>
			{
				spell.AddTriggerSpellEffect(SpellId.TreeOfLifePassive);
			});

			// Master Shapeshifter "Grants an effect which lasts while the Druid is within the respective shapeshift form."
			SpellLineId.DruidRestorationMasterShapeshifter.Apply(spell =>
			{
				var bearEffect = spell.GetEffect(AuraType.Dummy);

				// Bear Form - Increases physical damage by $s1%.
				bearEffect.AuraType = AuraType.ModDamageDonePercent;
				bearEffect.RequiredShapeshiftMask = ShapeshiftMask.Bear;
				bearEffect.MiscValue = (int)DamageSchoolMask.Physical;

				// Cat Form - Increases critical strike chance by $s1%.
				var catEffect = spell.AddAuraEffect(AuraType.ModCritPercent);
				catEffect.RequiredShapeshiftMask = ShapeshiftMask.Cat;

				// Moonkin Form - Increases spell damage by $s1%.
				var moonEffect = spell.AddAuraEffect(AuraType.ModDamageDonePercent);
				catEffect.RequiredShapeshiftMask = ShapeshiftMask.Moonkin;
				catEffect.MiscValue = (int)DamageSchoolMask.MagicSchools;

				// Tree of Life Form - Increases healing by $s1%.
				var treeForm = spell.AddAuraEffect(AuraType.ModHealingDonePct);
				catEffect.RequiredShapeshiftMask = ShapeshiftMask.TreeOfLife;

				// copy the values
				bearEffect.CopyValuesTo(catEffect);
				bearEffect.CopyValuesTo(moonEffect);
				bearEffect.CopyValuesTo(treeForm);
			});

			// Wild Growth: "The amount healed is applied quickly at first, and slows down"
			// see http://www.wowhead.com/spell=53251#comments
			SpellLineId.DruidRestorationWildGrowth.Apply(spell =>
			{
				// set max target effect
				spell.MaxTargetEffect = spell.GetEffect(SpellEffectType.None);

				// fix target types of all effects
				spell.ForeachEffect(effect => effect.ImplicitTargetA = ImplicitTargetType.PartyAroundCaster);

				// customize the healing process
				var healEffect = spell.GetEffect(AuraType.PeriodicHeal);

				var ticks = spell.Durations.Max / healEffect.Amplitude;			// amount of ticks

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

			// Improved ToL has no shapeshift restriction
			SpellLineId.DruidRestorationImprovedTreeOfLife.Apply(spell =>
			{
				spell.RequiredShapeshiftMask = ShapeshiftMask.TreeOfLife;
			});

			// "Your Rejuvenation and Wild Growth spells have a $s1% chance to restore $48540s1 Energy, $/10;48541s1 Rage, $48542s1% Mana or $/10;48543s1 Runic Power per tick."
			SpellLineId.DruidRestorationRevitalize.Apply(spell =>
			{
				// make the Spell proc on heal
				spell.ProcTriggerFlags = ProcTriggerFlags.HealOther;

				var effect = spell.GetEffect(AuraType.OverrideClassScripts);
				effect.IsProc = true;
				effect.AddToAffectMask(SpellLineId.DruidRejuvenation, SpellLineId.DruidRestorationWildGrowth);
				effect.AuraEffectHandlerCreator = () => new RevitalizeHandler();
			});
			// The mana revitalizer of Revitalize should restore %, not a flat value
			SpellHandler.Apply(spell =>
				spell.GetEffect(SpellEffectType.Energize).SpellEffectHandlerCreator = (cast, effect) => new EnergizePctEffectHandler(cast, effect),
				SpellId.Revitalize_5);

			// Swiftmend: Consume & Heal
			SpellLineId.DruidRestorationSwiftmend.Apply(spell =>
			{
				var effect = spell.GetEffect(SpellEffectType.Heal);
				effect.SpellEffectHandlerCreator = (cast, effct) => new SwiftmendHandler(cast, effct);
			});

			// these two have some kind of multiplier added to their periodic heal: "${$m1*5*$<mult>}"
			FixRegrowthAndRejuvenation(SpellLineId.DruidRejuvenation);
			FixRegrowthAndRejuvenation(SpellLineId.DruidRegrowth);

			// Gift of the Earthmother: "reduces the base cooldown of your Lifebloom spell by ${$m2/-15}%"
			SpellLineId.DruidRestorationGiftOfTheEarthmother.Apply(spell =>
			{
				// fix the amount
				var lbEffect = spell.GetEffect(AuraType.AddModifierFlat);
				lbEffect.MiscValue = (int) SpellModifierType.CooldownTime;
				lbEffect.BasePoints = -((lbEffect.BasePoints + 1)/15 - 1);
			});
		}

		private static void FixRegrowthAndRejuvenation(SpellLineId line)
		{
			line.Apply(spell =>
			{
				// apply the AuraState, so swiftmend can be used (AuraState probably going to be replaced with an invisible Aura in later versions)
				spell.AddAuraEffect(() => new AddTargetAuraStateHandler(AuraStateMask.RejuvenationOrRegrowth));

				var effect = spell.GetEffect(AuraType.PeriodicHeal);

				// TODO: Implement <mult> from "${$m1*5*$<mult>}"
				var ticks = spell.Durations.Max / effect.Amplitude;
				effect.AuraEffectHandlerCreator = () => new ParameterizedPeriodicHealHandler(effect.CalcEffectValue() * ticks);
			});
		}
	}

	#region Revitalize
	public class RevitalizeHandler : AuraEffectHandler
	{
		/// <summary>
		/// "have a $s1% chance"
		/// </summary>
		public override bool CanProcBeTriggeredBy(IUnitAction action)
		{
			return Utility.Random(0, 101) < EffectValue;
		}

		public override void OnProc(Unit triggerer, IUnitAction action)
		{
			// triggerer is the one being healed
			SpellId spell;

			// $48540s1 Energy, $/10;48541s1 Rage, $48542s1% Mana or $/10;48543s1 Runic Power
			switch (triggerer.PowerType)
			{
				case PowerType.Energy:
					spell = SpellId.Revitalize_3;
					break;
				case PowerType.Rage:
					spell = SpellId.Revitalize_4;
					break;
				case PowerType.Mana:
					spell = SpellId.Revitalize_5;
					break;
				case PowerType.RunicPower:
					spell = SpellId.Revitalize_6;
					break;
				default: return;
			}

			Owner.SpellCast.TriggerSingle(spell, triggerer);
		}
	}
	#endregion

	#region LivingSeed
	public class LivingSeedHandler : AuraEffectHandler
	{
		private int damageAmount;

		protected override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference, Unit target, ref SpellFailedReason failReason)
		{
			// remember the damageAmount from the TriggerAction
			if (creatingCast != null && creatingCast.TriggerAction is HealAction)
			{
				damageAmount = ((HealAction)creatingCast.TriggerAction).Value;
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
			Owner.Heal((damageAmount * 3 + 5) / 10, m_aura.Caster, m_spellEffect);
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
		Aura aura;

		public SwiftmendHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override SpellFailedReason InitializeTarget(WorldObject target)
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
			return base.InitializeTarget(target);
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

			((Unit)target).Heal(amount, m_cast.CasterUnit, Effect);

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
