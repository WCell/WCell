using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Misc
{
    /// <summary>
    /// Heals the wearer for EffectValue % when dealing damage
    /// </summary>
    public class LifeLeechPercentAuraHandler : AttackEventEffectHandler
    {
        public override void OnBeforeAttack(DamageAction action)
        {
        }

        public override void OnAttack(DamageAction action)
        {
            //if (!action.IsDot)
            {
                var amount = action.GetDamagePercent(EffectValue);
                Owner.Heal(amount, m_aura.CasterUnit, m_spellEffect);
            }
        }

        public override void OnDefend(DamageAction action)
        {
        }
    }
}
