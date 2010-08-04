using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.RealmServer.Spells.Auras.Misc;
using WCell.RealmServer.Spells.Effects;
using WCell.Util;

namespace WCell.Addons.Default.Spells.DeathKnight
{
	public class DeathKnightBloodFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixIt()
		{
			// Butchery lets you "generate up to 10 runic power" upon kill
			SpellLineId.DeathKnightBloodButchery.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.Dummy);
				effect.IsProc = true;
				effect.ClearAffectMask();
				effect.AuraEffectHandlerCreator = () => new ProcEnergizeHandler();
			});

			// Bloody Vengeance has wrong trigger flags
			SpellLineId.DeathKnightBloodBloodyVengeance.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.SpellCastCritical |
											ProcTriggerFlags.MeleeCriticalHitOther |
											ProcTriggerFlags.RangedCriticalHit;
			});

			FixScentOfBlood();

			// Mark of Blood only has Dummy and None effects
			SpellLineId.DeathKnightBloodMarkOfBlood.Apply(spell =>
			{
				// "Whenever the marked enemy deals damage to a target"
				spell.ProcTriggerFlags = ProcTriggerFlags.MeleeHitOther | ProcTriggerFlags.RangedHitOther | ProcTriggerFlags.SpellCast;

				// "that target is healed for $49005s2% of its maximum health"
				var effect2 = spell.Effects[1];
				effect2.IsProc = true;
				effect2.AuraEffectHandlerCreator = () => new MarkOfBloodAuraHandler();
			});

			// Vendetta has a dummy instead of a heal effect
			SpellLineId.DeathKnightBloodVendetta.Apply(spell =>
			{
				// "Heals you for up to $s1% of your maximum health whenever you kill a target that yields experience or honor."
				spell.GetEffect(AuraType.Dummy).AuraType = AuraType.RegenPercentOfTotalHealth;
			});

			// Hysteria does not drain life
			SpellLineId.DeathKnightBloodHysteria.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.Dummy2);
				effect.AuraType = AuraType.PeriodicDamagePercent;
			});

			// Sudden Doom: "Your Blood Strikes and Heart Strikes have a $h% chance to launch a free Death Coil at your target."
			SpellLineId.DeathKnightBloodSuddenDoom.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.SpellCast;

				// only one dummy -> Trigger highest level of death coil that the caster has instead
				// set correct trigger spells
				var effect = spell.GetEffect(AuraType.Dummy);
				effect.IsProc = true;
				effect.AddToAffectMask(SpellLineId.DeathKnightBloodStrike, SpellLineId.DeathKnightBloodHeartStrike);
				effect.AuraEffectHandlerCreator = () => new SuddenDoomAuraHandler();
			});
			SpellHandler.Apply(spell =>
			{
				// TODO: Scale bloodworm strength
				// TODO: "Caster receives health when the Bloodworm deals damage"
				// Amount of bloodworms is wrong
				var effect = spell.GetEffect(SpellEffectType.Summon);
				effect.BasePoints = 1;
				effect.DiceSides = 3;
			}, SpellId.EffectBloodworm);

			// Spell Deflection only has an Absorb effect, but should have a chance to reduce spell damage
			SpellLineId.DeathKnightBloodSpellDeflection.Apply(spell =>
			{
				spell.GetEffect(AuraType.SchoolAbsorb).AuraEffectHandlerCreator = () => new SpellDeflectionHandler();
			});

			// "the Frost and Unholy Runes will become Death Runes when they activate"
			DeathKnightFixes.MakeRuneConversionProc(SpellLineId.DeathKnightBloodDeathRuneMastery, 
				SpellLineId.DeathKnightDeathStrike, SpellLineId.DeathKnightObliterate,
				RuneType.Death, RuneType.Frost, RuneType.Unholy);
		}

		#region Death Rune Mastery
		#endregion

		#region Scent of Blood
		private static void FixScentOfBlood()
		{
			// Scent of Blood is triggered by "dodging, parrying or taking  direct damage"
			SpellLineId.DeathKnightBloodScentOfBlood.Apply(spell =>
			{
				var triggerSpellEffect = spell.GetEffect(AuraType.ProcTriggerSpell);
				spell.ClearEffects();												// remove all effects

				// create custom handler
				var handler = new TriggerSpellProcHandler(
					ProcTriggerFlags.MeleeHit | ProcTriggerFlags.RangedHit,
					(target, action) =>
					{
						var daction = action as DamageAction;
						if (daction == null) return false;
						return daction.ActualDamage > 0 ||
							   daction.VictimState == VictimState.Parry || daction.VictimState == VictimState.Dodge;
					},
					SpellHandler.Get(triggerSpellEffect.TriggerSpellId)
					);

				// trigger the spell once per rank
				for (var i = 0; i < spell.Rank; i++)
				{
					spell.AddProcHandler(handler);
				}
			});
			// The proc'ed Aura of SoB triggers a non-existing spell
			SpellHandler.Apply(spell =>
			{
				// "The next successful melee attack will generate 10 runic power"
				var effect = spell.GetEffect(AuraType.ProcTriggerSpell);
				effect.BasePoints = 100;
				effect.IsProc = true;
				effect.AuraEffectHandlerCreator = () => new ProcEnergizeHandler();
			},
			 SpellId.EffectScentOfBlood);
		}
		#endregion
	}

	#region Spell Deflection
	public class SpellDeflectionHandler : AuraEffectHandler
	{
		public override bool CanProcBeTriggeredBy(IUnitAction action)
		{
			// "You have a chance equal to your Parry chance"
			return Utility.Random(0, 100) <= action.Victim.ParryChance;
		}

		public override void OnProc(Unit triggerer, IUnitAction action)
		{
			if (action is DamageAction)
			{
				// "taking $s1% less damage from a direct damage spell"
				((DamageAction) action).ModDamagePercent(-EffectValue);
			}
		}
	}
	#endregion

	#region Sudden Doom
	public class SuddenDoomAuraHandler : AuraEffectHandler
	{
		/// <summary>
		/// "launch a free Death Coil at your target"
		/// </summary>
		public override void OnProc(Unit triggerer, IUnitAction action)
		{
			// get highest level of Death Coil and throw it at the target
			var line = SpellLineId.DeathKnightDeathCoil.GetLine();
			var deathCoil = triggerer.Spells.GetHighestRankOf(line) ?? line.FirstRank;

			action.Attacker.SpellCast.Trigger(deathCoil, action.Victim);
		}
	}
	#endregion

	#region Mark of Blood
	public class MarkOfBloodAuraHandler : AuraEffectHandler
	{
		public override void OnProc(Unit triggerer, IUnitAction action)
		{
			// "target is healed for $49005s2% of its maximum health"
			triggerer.HealPercent(EffectValue, m_aura.Caster, m_spellEffect);	// who is the healer? caster or target?
		}
	}
	#endregion
}
