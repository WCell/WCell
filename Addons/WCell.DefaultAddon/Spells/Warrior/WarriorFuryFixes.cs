using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Effects;

namespace WCell.Addons.Default.Spells.Warrior
{
	public static class WarriorFuryFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixIt()
		{
			// Blood craze should only trigger on crit hit
			SpellLineId.WarriorFuryBloodCraze.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.MeleeCriticalHit | ProcTriggerFlags.RangedCriticalHit;
			});

			// Improved Berserker Range can only be proc'ed by Berserker Rage
			SpellLineId.WarriorFuryImprovedBerserkerRage.Apply(spell =>
			{
				spell.AddCasterProcSpells(SpellLineId.WarriorBerserkerRage);
			});

			// Blood Thirst deals damage in % of AP and triggers a proc aura
			// It's proc'ed heal spell heals in % and not a flat value
			SpellLineId.WarriorFuryBloodthirst.Apply(spell =>
			{
				var effect = spell.GetEffect(SpellEffectType.SchoolDamage);
				effect.SpellEffectHandlerCreator = (cast, eff) => new SchoolDamageByAPPctEffectHandler(cast, eff);

				var triggerEffect = spell.GetEffect(SpellEffectType.Dummy);
				triggerEffect.EffectType = SpellEffectType.TriggerSpell;
				triggerEffect.TriggerSpellId = SpellId.ClassSkillBloodthirst;
			});
			SpellHandler.Apply(spell =>
			{
				var effect = spell.GetEffect(SpellEffectType.Heal);
				effect.EffectType = SpellEffectType.RestoreHealthPercent;
				effect.BasePoints = 0;	// only 1%
			}, SpellId.EffectClassSkillBloodthirst);

			// Intercept should also deal "${$AP*0.12} damage"
			SpellLineId.WarriorIntercept.Apply(spell =>
			{
				var effect = spell.AddEffect(SpellEffectType.SchoolDamage, ImplicitSpellTargetType.SingleEnemy);
				effect.APValueFactor = 0.12f;
			});

            // There is only one shout per warrior
            AuraHandler.AddAuraCasterGroup(SpellLineId.WarriorBattleShout, SpellLineId.WarriorCommandingShout);
		}
	}
}
