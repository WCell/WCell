using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.Addons.Default.Spells.Paladin
{
	public static class PaladinHolyFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixIt()
		{
			// Illumination lets the caster "gain mana equal to $s2% of the base cost of the spell", also works for Holy Shock heals
			SpellLineId.PaladinHolyIllumination.Apply(spell =>
			{
				var invalidProcEffect = spell.GetEffect(AuraType.ProcTriggerSpell);
				invalidProcEffect.IsProc = false;	// don't proc (refers to non-existing spell)

				var effect = spell.GetEffect(AuraType.OverrideClassScripts);
				effect.IsProc = true;
				effect.AddToAffectMask(SpellLineId.PaladinHolyHolyShock);			// also works for Holy Shock
				effect.AuraEffectHandlerCreator = () => new IlluminationHandler();
			});

			// Improved Lay On Hands needs the lay on hands spell restriction and correct proc flags
			SpellLineId.PaladinHolyImprovedLayOnHands.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.ActionOther | ProcTriggerFlags.HealOther;
				spell.GetEffect(AuraType.ProcTriggerSpell).AddToAffectMask(SpellLineId.PaladinLayOnHands);
			});

			// Holy Shield only procs on block
			SpellLineId.PaladinProtectionHolyShield.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.Block;
			});

			// Sacred Cleansing should proc on Cleanse
			SpellLineId.PaladinHolySacredCleansing.Apply(spell =>
			{
				var procEffect = spell.GetEffect(AuraType.ProcTriggerSpell);
				procEffect.ClearAffectMask();
				procEffect.AddToAffectMask(SpellLineId.PaladinCleanse);
			});

			// Judgements of the Pure procs on all judgements
			SpellLineId.PaladinHolyJudgementsOfThePure.Apply(spell =>
			{
				var procEff = spell.GetEffect(AuraType.ProcTriggerSpell);
				procEff.ClearAffectMask();
				procEff.AddToAffectMask(SealsAndJudgements.AllJudgements);
			});

			// Holy Wrath "causing ${$m1+0.07*$SPH+0.07*$AP} to ${$M1+0.07*$SPH+0.07*$AP} Holy damage (...)  for $d"
			SpellLineId.PaladinHolyWrath.Apply(spell =>
			{
				var effect = spell.GetEffect(SpellEffectType.SchoolDamage);
				effect.APValueFactor = 0.07f;
				effect.SpellPowerValuePct = 7;
			});

			// Exorcism "Causes ${$m1+0.15*$SPH+0.15*$AP} to ${$M1+0.15*$SPH+0.15*$AP}" and crits against demons and the undead
			SpellLineId.PaladinExorcism.Apply(spell =>
			{
				var dmgEffect = spell.GetEffect(SpellEffectType.SchoolDamage);
				dmgEffect.APValueFactor = 0.15f;
				dmgEffect.SpellPowerValuePct = 15;

				//var critMonsterEffect = 
				spell.AddAuraEffect(() => new CritCreatureMaskHandler(CreatureMask.Demon | CreatureMask.Undead),
					dmgEffect.ImplicitTargetA);
			});

			// Lay on Hands applies Forbearance, if used on self
			SpellLineId.PaladinLayOnHands.Apply(spell =>
			{
				spell.AddAuraEffect(() => new ApplySelfForbearanceHandler(), ImplicitTargetType.Self);
			});
		}

		public class IlluminationHandler : AuraEffectHandler
		{
			public override void OnProc(Unit target, IUnitAction action)
			{
				if (action is HealAction && action.Spell != null)
				{
					var haction = (HealAction)action;
					if (haction.IsCritical)
					{
						var caster = action.Attacker;
						var cost = (haction.Spell.CalcBasePowerCost(caster) * EffectValue + 50) / 100;
						caster.Energize(caster, cost, SpellEffect);
					}
				}
			}
		}
	}

	public class ApplySelfForbearanceHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			if (Owner.SharedReference == m_aura.Caster.SharedReference)
			{
				// apply Forbearance when casting on self
				Owner.SpellCast.TriggerSelf(SpellId.Forbearance);
			}
		}
	}
}
