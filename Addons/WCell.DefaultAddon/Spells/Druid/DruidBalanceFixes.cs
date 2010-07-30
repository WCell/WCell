using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.RealmServer.Spells.Auras.Misc;
using WCell.RealmServer.Spells.Effects;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.Addons.Default.Spells.Druid
{
	public static class DruidBalanceFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixIt()
		{
			// Nature's Grace needs to set the correct set of affecting spells
			SpellLineId.DruidBalanceNaturesGrace.Apply(spell =>
			{
				// copy AffectMask from proc effect, which has it all set correctly
				var effect = spell.GetEffect(AuraType.ProcTriggerSpell);
				var triggerSpellEffect = effect.GetTriggerSpell().GetEffect(AuraType.ModCastingSpeed);
				effect.AffectMask = triggerSpellEffect.AffectMask;
			});

			// Improved Moonkin Form is only active in Moonkin form, and applies an extra AreaAura to everyone
			SpellLineId.DruidBalanceImprovedMoonkinForm.Apply(spell =>
			{
				// only in Moonkin form
				spell.RequiredShapeshiftMask = ShapeshiftMask.Moonkin;

				// apply the extra spell to everyone (it's an AreaAura effect)
				var dummy = spell.GetEffect(AuraType.Dummy);
				dummy.AuraEffectHandlerCreator = () => new ToggleAuraHandler(SpellId.ImprovedMoonkinFormRank1);
			});

			// Force of Nature's summon entry needs to be changed to Friendly, rather than pet
			SpellHandler.GetSummonEntry(SummonType.ForceOfNature).Group = SummonGroup.Friendly;

			// Owlkin Frenzy should proc on any damage spell
			SpellLineId.DruidBalanceOwlkinFrenzy.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.ProcTriggerSpell);
				effect.ClearAffectMask();
			});

			// Starfall triggers a lot of spells in a chain - The first trigger spell effect is a dummy that limits the accumulated amount of triggers to 20
			SpellLineId.DruidBalanceStarfall.Apply(spell =>
			{
				// cancels on move
				spell.AuraInterruptFlags |= AuraInterruptFlags.OnMovement;

				// all triggered Spells are instant casts - so we need to use the Aura effect handler to remember the total amount of stars

				// use Aura handler for accumulating
				var periodicTriggerEffect = spell.GetEffect(AuraType.PeriodicTriggerSpell);
				periodicTriggerEffect.AuraEffectHandlerCreator = () => new StarfallAuraHandler();

				// use triggered spell handler for updating the accumulator in the Aura
				var triggeredDummySpell = periodicTriggerEffect.GetTriggerSpell();
				var triggeredDummyEffect = triggeredDummySpell.GetEffect(SpellEffectType.Dummy);
				triggeredDummyEffect.TriggerSpellId = (SpellId)triggeredDummyEffect.CalcEffectValue();		// trigger spell is set as effect value
				triggeredDummyEffect.SpellEffectHandlerCreator = (cast, effct) => new StarfallCountTriggerHandler(cast, effct, spell);

				// the impact should cause more damage to targets around the first target
				//var triggeredImpactSpell = triggeredDummyEffect.GetTriggerSpell();
				//var scatterEffect = triggeredImpactSpell.GetEffect(SpellEffectType.TriggerSpell);
			});

			// Earth and Moon: "Your Wrath and Starfire spells have a $h% chance to apply the Earth and Moon effect"
			SpellLineId.DruidBalanceEarthAndMoon.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.SpellCast;
				var effect = spell.GetEffect(AuraType.ProcTriggerSpell);

				// retricted to Wrath and Starfire
				effect.AddToAffectMask(SpellLineId.DruidWrath, SpellLineId.DruidStarfire);
			});

			// Eclipse has a special proc effect
			// "When you critically hit with Starfire, you have a $h1% chance of increasing damage done by Wrath by $48517s1%. 
			// When you critically hit with Wrath, you have a ${$h*0.6}% chance of increasing your critical strike chance with Starfire by $48518s1%. 
			// Each effect lasts $48518d and each has a separate $s1 sec cooldown.  Both effects cannot occur simultaneously."
			SpellLineId.DruidBalanceEclipse.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.SpellCastCritical;
				var effect1 = spell.Effects[0];

				// We set the original proc chance to that of of the more often occuring effect (Wrath) 
				// and do a second chance check for the other proc when proc'ing
				spell.ProcChance = (spell.ProcChance * 6 + 5) / 10;
				effect1.IsProc = true;
				effect1.SetAffectMask(SpellLineId.DruidWrath, SpellLineId.DruidStarfire);
				effect1.AuraEffectHandlerCreator = () => new DruidEclipseHandler();
			});

			// Improved IS: Two different kinds of procs, dependent on spell being cast
			SpellLineId.DruidBalanceImprovedInsectSwarm.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.SpellCast;

				// "Increases your damage done by your Wrath spell to targets afflicted by your Insect Swarm by $s1%"
				var effect1 = spell.Effects[0];
				effect1.IsProc = true;
				effect1.SetAffectMask(SpellLineId.DruidWrath);
				effect1.AuraEffectHandlerCreator = () => new ISWrathHandler();

				// "increases the critical strike chance of your Starfire spell by $s2% on targets afflicted by your Moonfire spell."
				var effect2 = spell.Effects[1];
				effect2.IsProc = true;
				effect2.SetAffectMask(SpellLineId.DruidStarfire);
				effect2.AuraEffectHandlerCreator = () => new ISStarfireHandler();
			});
		}
	}

	#region Improved Insect Swarm
	public class ISWrathHandler : AuraEffectHandler
	{
		public override void OnProc(Unit triggerer, IUnitAction action)
		{
			// "Increases your damage done by your Wrath spell to targets afflicted by your Insect Swarm by $s1%"
			if (triggerer.Auras.Contains(SpellLineId.DruidBalanceInsectSwarm))
			{
				var daction = action as DamageAction;
				if (daction != null)
				{
					daction.IncreaseDamagePercent(EffectValue);
				}
			}
		}
	}

	public class ISStarfireHandler : AuraEffectHandler
	{
		public override void OnProc(Unit triggerer, IUnitAction action)
		{
			// "increases the critical strike chance of your Starfire spell by $s2% on targets afflicted by your Moonfire spell."
			if (triggerer.Auras.Contains(SpellLineId.DruidMoonfire))
			{
				var daction = action as DamageAction;
				if (daction != null)
				{
					daction.AddBonusCritChance(EffectValue);
				}
			}
		}
	}
	#endregion

	#region Eclipse
	public class DruidEclipseHandler : AuraEffectHandler
	{
		private static readonly SpellId[] StarFallRanks = new[]
		{
			SpellId.StarfallRank1,
			SpellId.StarfallRank2,
			SpellId.StarfallRank3,
			SpellId.StarfallRank4
		};

		static bool IsStarFireDamageSpell(Spell spell)
		{
			return StarFallRanks.Contains(spell.SpellId);
		}

		static bool IsWrath(Spell spell)
		{
			return spell.Line != null && spell.Line.LineId == SpellLineId.DruidWrath;
		}

		public Aura CurrentlyActiveAura;

		/// <summary>
		/// "Each effect lasts $48518d and each has a separate $s1 sec cooldown"
		/// </summary>
		public DateTime NextSFProcTime, NextWrathProcTime;

		public override bool CanProcBeTriggeredBy(IUnitAction action)
		{
			if (CurrentlyActiveAura != null)
			{
				// "Both effects cannot occur simultaneously."
				return false;
			}

			if (action.Spell == null) return false;
			if (IsStarFireDamageSpell(action.Spell))
			{
				// SF
				if (DateTime.Now < NextSFProcTime)
				{
					return false;
				}

				// starfire has less of a chance than wrath, so we need to make a second proc chance check here:
				var wrathChance = m_spellEffect.Spell.ProcChance;
				var starfireChance = m_spellEffect.MiscValue;
				var sfToWrathProportion = starfireChance / wrathChance;

				// if SF had 25% of the chance of wrath, this would succeed with 25% and fail with 75%
				if (Utility.RandomFloat() < sfToWrathProportion)
				{
					return false;
				}
				return base.CanProcBeTriggeredBy(action);
			}
			else if (IsWrath(action.Spell))
			{
				// Wrath
				if (DateTime.Now < NextWrathProcTime)
				{
					return false;
				}
				return base.CanProcBeTriggeredBy(action);
			}
			return false;		// must be wrath or SF
		}

		public override void OnProc(Unit triggerer, IUnitAction action)
		{
			SpellId spellId;
			if (IsWrath(action.Spell))
			{
				// Wrath
				// "increasing your critical strike chance with Starfire by $48518s1%"
				spellId = SpellId.EclipseLunar;
				CurrentlyActiveAura = Owner.Auras.CreateAura(m_aura.CasterReference, spellId, true);
				if (CurrentlyActiveAura != null)
				{
					// "$s1 sec cooldown"
					NextWrathProcTime = DateTime.Now.AddMilliseconds(CurrentlyActiveAura.Duration + EffectValue * 1000);
				}
			}
			else
			{
				// SF
				// "chance of increasing damage done by Wrath by $48517s1%"
				spellId = SpellId.EclipseSolar;
				CurrentlyActiveAura = Owner.Auras.CreateAura(m_aura.CasterReference, spellId, true);
				if (CurrentlyActiveAura != null)
				{
					// "$s1 sec cooldown"
					NextSFProcTime = DateTime.Now.AddMilliseconds(CurrentlyActiveAura.Duration + EffectValue * 1000);
				}
			}
		}
	}
	#endregion

	#region Starfall
	/// <summary>
	/// The aura needs to keep track of the star count
	/// </summary>
	public class StarfallAuraHandler : PeriodicTriggerSpellHandler
	{
		public int FallenStars;
	}

	/// <summary>
	/// This effect needs to count the amount of stars casted
	/// </summary>
	public class StarfallCountTriggerHandler : TriggerSpellEffectHandler
	{
		public static int MaxStars = 20;
		public Spell StarFallAuraSpell { get; private set; }

		public StarfallCountTriggerHandler(SpellCast cast, SpellEffect effect, Spell starFallAuraSpell)
			: base(cast, effect)
		{
			StarFallAuraSpell = starFallAuraSpell;
		}

		public override void Apply()
		{
			if (Targets == null) return;					// must have targets

			var caster = m_cast.CasterUnit;
			if (caster == null) return;

			var aura = caster.Auras[StarFallAuraSpell];
			if (aura == null) return;

			var handler = aura.Handlers.First(handlr => handlr is StarfallAuraHandler) as StarfallAuraHandler;
			if (handler == null) return;					// we need the handler for counting

			if (handler.FallenStars + Targets.Count >= MaxStars)
			{
				// reached the max amount of stars
				var amount = MaxStars - handler.FallenStars;
				if (amount > 0)
				{
					var spellTargets = new WorldObject[amount];
					Targets.CopyTo(0, spellTargets, 0, amount);

					m_cast.Trigger(Effect.TriggerSpell, spellTargets);
				}
				aura.Remove(false);			// remove Aura
			}
			else
			{
				// we can still keep going
				handler.FallenStars += Targets.Count;
				m_cast.Trigger(Effect.TriggerSpell, Targets.ToArray());
			}
		}
	}
	#endregion
}
