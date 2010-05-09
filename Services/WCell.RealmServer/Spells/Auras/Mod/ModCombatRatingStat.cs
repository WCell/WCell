using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.Constants;
using WCell.Constants.Misc;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>
    /// Modifies CombatRatings
    /// </summary>
    public class ModCombatRatingStat : AuraEffectHandler //mod xx combatrating based on yy stat.
    {
        int value = 0;

        protected internal override void Apply()
        {
            var owner = m_aura.Auras.Owner as Character;
            if (owner != null)
            {
                value = owner.GetStatValue((StatType)m_spellEffect.MiscValueB) / EffectValue;
                owner.ModCombatRating((CombatRating)m_spellEffect.MiscValue, value);
            }
        }

        protected internal override void Remove(bool cancelled)
        {
            var owner = m_aura.Auras.Owner as Character;
            if (owner != null)
                owner.ModCombatRating((CombatRating)m_spellEffect.MiscValue, -value);
        }
    }
};