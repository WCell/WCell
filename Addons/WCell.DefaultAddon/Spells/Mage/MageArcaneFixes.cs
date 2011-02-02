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

    #region Arcane Potency
    public class ArcanePotencyHandler : AttackEventEffectHandler
    {
        private bool trigger;
        public override void OnAttack(DamageAction action)
        {
            if (action.Spell == SpellHandler.Get(SpellId.MageArcanePresenceOfMind) ||
                action.Spell == SpellHandler.Get(SpellId.EffectClearcasting))
            {
                trigger = true;
            }


            if (action.IsSpellCast)
            {
                if (trigger)
                {
                    trigger = false;
                    action.AddBonusCritChance(EffectValue);
                }
            }
        }
    }
    #endregion  
}
