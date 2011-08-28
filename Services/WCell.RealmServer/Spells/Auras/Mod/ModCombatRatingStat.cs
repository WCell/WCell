using WCell.Constants;
using WCell.Constants.Misc;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>
    /// Modifies CombatRatings
    /// </summary>
    public class ModCombatRatingStat : AuraEffectHandler //mod xx combatrating based on yy stat.
    {
        int value = 0;

        protected override void Apply()
        {
            var owner = m_aura.Auras.Owner as Character;
            if (owner != null)
            {
                value = owner.GetTotalStatValue((StatType)m_spellEffect.MiscValueB) / EffectValue;
                owner.ModCombatRating((CombatRating)m_spellEffect.MiscValue, value);
            }
        }

        protected override void Remove(bool cancelled)
        {
            var owner = m_aura.Auras.Owner as Character;
            if (owner != null)
                owner.ModCombatRating((CombatRating)m_spellEffect.MiscValue, -value);
        }
    }
};