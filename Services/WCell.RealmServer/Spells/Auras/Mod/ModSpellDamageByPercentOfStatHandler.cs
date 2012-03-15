using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    public class ModSpellDamageByPercentOfStatHandler : AuraEffectHandler
    {
        private int value;

        protected override void Apply()
        {
            var stat = (StatType)SpellEffect.MiscValueB;

            value = (Owner.GetTotalStatValue(stat) * EffectValue + 50) / 100;

            // TODO: Update when stat changes
            // TODO: Apply as white bonus, if aura is passive?
            Owner.AddDamageDoneMod(m_spellEffect.MiscBitSet, value);
        }

        protected override void Remove(bool cancelled)
        {
            Owner.RemoveDamageDoneMod(m_spellEffect.MiscBitSet, value);
        }
    }

    public class ModHealingByPercentOfStatHandler : AuraEffectHandler
    {
        private int value;

        protected override void Apply()
        {
            // TODO: Update when stat changes
            // TODO: Apply as white bonus, if aura is passive?
            if (m_aura.Auras.Owner is Character)
            {
                var stat = (StatType)SpellEffect.MiscValueB;
                value = (Owner.GetTotalStatValue(stat) * EffectValue + 50) / 100;

                // schools are ignored for this effect
                ((Character)m_aura.Auras.Owner).HealingDoneMod += value;
            }
        }

        protected override void Remove(bool cancelled)
        {
            if (m_aura.Auras.Owner is Character)
            {
                ((Character)m_aura.Auras.Owner).HealingDoneMod -= value;
            }
        }
    }
}