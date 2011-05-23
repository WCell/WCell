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
                spell.SpellAuraOptions.ProcTriggerFlags = ProcTriggerFlags.MeleeCriticalHit | ProcTriggerFlags.RangedCriticalHit;
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
            //AuraHandler.AddAuraCasterGroup(SpellLineId.WarriorBattleShout, SpellLineId.WarriorCommandingShout);
		}
	}
}
