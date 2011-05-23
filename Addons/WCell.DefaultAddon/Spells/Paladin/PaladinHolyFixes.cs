using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
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
			// Sacred Cleansing should proc on Cleanse
            //SpellLineId.PaladinHolySacredCleansing.Apply(spell =>
            //{
            //    var procEffect = spell.GetEffect(AuraType.ProcTriggerSpell);
            //    procEffect.ClearAffectMask();
            //    //procEffect.AddToAffectMask(SpellLineId.PaladinCleanse);
            //    procEffect.AddAffectingSpells(SpellLineId.PaladinCleanse);
            //});

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
				spell.AddAuraEffect(() => new ApplySelfForbearanceHandler(), ImplicitSpellTargetType.Self);
			});
		}
	}

	public class ApplySelfForbearanceHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			if (Owner.SharedReference == m_aura.CasterUnit.SharedReference)
			{
				// apply Forbearance when casting on self
				Owner.SpellCast.TriggerSelf(SpellId.Forbearance);
			}
		}
	}
}
