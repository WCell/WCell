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
using WCell.RealmServer.Spells.Auras.Misc;
using WCell.RealmServer.Misc;

namespace WCell.Addons.Default.Spells.Mage
{
	public static class MageFireFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixSpells()
		{
			// Improved Scorch has the wrong trigger
			SpellLineId.MageFireImprovedScorch.Apply(spell =>
			{
				spell.SpellAuraOptions.ProcTriggerFlags = ProcTriggerFlags.SpellCast;	// proc on all spell casts of Scorch
			});

			// Impact needs the right triggers
			SpellLineId.MageFireImpact.Apply(spell =>
			{
                spell.SpellAuraOptions.ProcTriggerFlags = ProcTriggerFlags.SpellCast;
				var triggerEffect = spell.GetEffect(AuraType.ProcTriggerSpell);
				// triggerEffect.AddAffectingSpells(...); // TODO: Triggered by all damaging spells
			});
			// Impact's triggered effect also needs some adjustments
			SpellHandler.Apply(spell =>
			{
                spell.SpellAuraOptions.ProcTriggerFlags = ProcTriggerFlags.SpellCast;
				var triggerEffect = spell.GetEffect(AuraType.ProcTriggerSpell);
				triggerEffect.ImplicitTargetA = ImplicitSpellTargetType.SingleEnemy;
				triggerEffect.AddAffectingSpells(SpellLineId.MageFireBlast);		// triggered by fire blast only
			}, SpellId.EffectImpactRank1);

			// Combustion should proc
            //SpellLineId.MageFireCombustion.Apply(spell =>
            //{
            //    spell.SpellAuraOptions.ProcTriggerFlags = ProcTriggerFlags.SpellCast;

            //    var modEffect = spell.GetEffect(AuraType.AddModifierPercent);

            //    // the trigger effect is actually supposed to be proc'ed by the same spells that have their crit damage increased
            //    var triggerEffect = spell.GetEffect(SpellEffectType.TriggerSpell);
            //    triggerEffect.EffectType = SpellEffectType.ApplyAura;
            //    triggerEffect.AuraType = AuraType.ProcTriggerSpell;
            //    triggerEffect.AffectMask = modEffect.AffectMask;
            //});

            // Mage Fire Blazing Speed has wrong trigger proc id
            SpellLineId.MageFireBlazingSpeed.Apply(spell =>
            {
				var triggerEffect = spell.GetEffect(AuraType.ProcTriggerSpell);
                triggerEffect.TriggerSpellId = SpellId.ClassSkillBlazingSpeed;
            });

            SpellLineId.MageFireMasterOfElements.Apply(spell =>
            {
                var effect = spell.GetEffect(AuraType.Dummy);
                effect.AuraEffectHandlerCreator = () => new MasterOfElementsHandler();
            });
		}
	}

    #region Master of Elements
    public class MasterOfElementsHandler : AttackEventEffectHandler
    {
        public override void OnAttack(DamageAction action)
        {
            if (action.IsMagic && action.IsCritical)
            {
                var owner = m_aura.CasterUnit;
                if (owner == null) return;
                owner.Power += (action.Spell.CalcPowerCost(owner, action.UsedSchool) * 100 + 50 )/ EffectValue;
            }
        }
    }
    #endregion
}
