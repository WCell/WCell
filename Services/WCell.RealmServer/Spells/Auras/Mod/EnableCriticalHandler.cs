using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    public class EnableCriticalHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            var chr = Owner as Character;
            if (chr != null)
            {
                m_spellEffect.CopyAffectMaskTo(chr.PlayerAuras.CriticalStrikeEnabledMask);
            }
        }

        protected override void Remove(bool cancelled)
        {
            var chr = Owner as Character;
            if (chr != null)
            {
                m_spellEffect.RemoveAffectMaskFrom(chr.PlayerAuras.CriticalStrikeEnabledMask);
            }
        }
    }
}
