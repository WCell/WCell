using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Constants.Talents;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;

namespace WCell.Addons.Default.Spells.Mage
{
	public static class MageFireFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixSpells()
		{
			// Burnout: "your non-periodic spell criticals cost an additional $s2% of the spell's cost"
			SpellEffect burnoutEffect = null;
			SpellHandler.Apply(spell =>
			{
				// consume mana for the given spells
				burnoutEffect = spell.GetEffect(SpellEffectType.Dummy);
				burnoutEffect.ImplicitTargetA = ImplicitSpellTargetType.None;	// no target selection (we find the target in the Apply method)
				burnoutEffect.SpellEffectHandlerCreator = (cast, effect) => new BurnoutHandler(cast, effect);
			}, SpellId.ClassSkillBurnout);
			SpellLineId.MageFireBurnout.Apply(spell =>
			{
				// always procs on critical spells of the given AffectMask
				var effect = spell.GetEffect(AuraType.Dummy);
				spell.ProcChance = 100;
				spell.ProcTriggerFlags = ProcTriggerFlags.SpellCastCritical;

				effect.AuraType = AuraType.ProcTriggerSpell;		// make it a ProcTriggerSpell
				effect.AffectMask = burnoutEffect.AffectMask;		// set the correct affect mask
				effect.TriggerSpellId = SpellId.ClassSkillBurnout;	// trigger the mana consumption spell
			});

			// Improved Scorch has the wrong trigger
			SpellLineId.MageFireImprovedScorch.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.SpellCast;	// proc on all spell casts of Scorch
			});

			// Impact needs the right triggers
			SpellLineId.MageFireImpact.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.SpellCast;
				var triggerEffect = spell.GetEffect(AuraType.ProcTriggerSpell);
				// triggerEffect.AddAffectingSpells(...); // TODO: Triggered by all damaging spells
			});
			// Impact's triggered effect also needs some adjustments
			SpellHandler.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.SpellCast;
				var triggerEffect = spell.GetEffect(AuraType.ProcTriggerSpell);
				triggerEffect.ImplicitTargetA = ImplicitSpellTargetType.SingleEnemy;
				triggerEffect.AddAffectingSpells(SpellLineId.MageFireBlast);		// triggered by fire blast only
			}, SpellId.EffectImpactRank1);

			// Combustion should proc
			SpellLineId.MageFireCombustion.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.SpellCast;

				var modEffect = spell.GetEffect(AuraType.AddModifierPercent);

				// the trigger effect is actually supposed to be proc'ed by the same spells that have their crit damage increased
				var triggerEffect = spell.GetEffect(SpellEffectType.TriggerSpell);
				triggerEffect.EffectType = SpellEffectType.ApplyAura;
				triggerEffect.AuraType = AuraType.ProcTriggerSpell;
				triggerEffect.AffectMask = modEffect.AffectMask;
			});

            // Mage Fire Blazing Speed has wrong trigger proc id
            SpellLineId.MageFireBlazingSpeed.Apply(spell =>
            {
                var triggerEffect = spell.GetEffect(SpellEffectType.TriggerSpell);
                triggerEffect.TriggerSpellId = SpellId.ClassSkillBlazingSpeed;
            });
		}
	}

	#region Burnout
	public class BurnoutHandler : SpellEffectHandler
	{
		public BurnoutHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override void Apply()
		{
			var triggerAction = m_cast.TriggerAction;	// the triggerAction is the one that proc'ed this spell
			if (triggerAction != null)					// should always be != null, but let us be safe
			{
				// get the current rank of the burnout talent to determine the power cost
				var caster = m_cast.CasterUnit as Character;
				if (caster == null) return;

				var burnoutTalent = caster.Talents.GetTalent(TalentId.MageFireBurnout);
				if (burnoutTalent == null) return;

				var burnoutEffect = burnoutTalent.Spell.GetEffect(AuraType.ProcTriggerSpell);

				// get the spell base cost from the triggerAction and the costPct modifier from the talent
				var spell = triggerAction.Spell;
				var cost = spell.CalcBasePowerCost(caster);
				var costPct = Effect.CalcEffectValue(caster, burnoutEffect.CalcEffectValue(caster));

				// deduct extra power
				var power = (cost * costPct + 50) / 100;
				caster.Power -= power;
			}
		}
	}
	#endregion
}
