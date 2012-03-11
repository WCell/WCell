using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModKillXpPctHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Character chr = m_aura.Auras.Owner as Character;
            if (chr != null)
                chr.KillExperienceGainModifierPercent += EffectValue;
        }

        protected override void Remove(bool cancelled)
        {
            Character chr = m_aura.Auras.Owner as Character;
            if (chr != null)
                chr.KillExperienceGainModifierPercent -= EffectValue;
        }
    }
};