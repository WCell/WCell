using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.Addons.Default.Spells.Mage
{
	public static class MageArcaneFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixMe()
		{
			// conjure water and food don't have any per level bonus
			SpellLineId.MageConjureFood.Apply(spell => spell.ForeachEffect(effect => effect.RealPointsPerLevel = 0));
			SpellLineId.MageConjureWater.Apply(spell => spell.ForeachEffect(effect => effect.RealPointsPerLevel = 0));

            SpellLineId.MageArcaneArcanePotency.Apply(spell =>
            {
                var effect = spell.GetEffect(AuraType.Dummy);
                effect.AuraEffectHandlerCreator = () => new ArcanePotencyHandler();
            });
		}
	}

    public class ArcanePotencyHandler : AttackEventEffectHandler
    {
        public override void OnAttack(DamageAction action)
        {
            if (!action.Spell.IsTriggeredSpell && !action.IsWeaponAttack)
                action.AddBonusCritChance(EffectValue);
        }
    }
}
