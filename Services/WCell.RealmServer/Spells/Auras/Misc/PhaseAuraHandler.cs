namespace WCell.RealmServer.Spells.Auras.Misc
{
    public class PhaseAuraHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            var phase = (uint)m_spellEffect.MiscValue;
            m_aura.Auras.Owner.Phase = phase;
        }

        protected override void Remove(bool cancelled)
        {
            m_aura.Auras.Owner.Phase = 1;
        }
    }
}