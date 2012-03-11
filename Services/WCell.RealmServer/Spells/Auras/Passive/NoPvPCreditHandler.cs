namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class NoPvPCreditHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            m_aura.Auras.Owner.YieldsXpOrHonor = false;
        }

        protected override void Remove(bool cancelled)
        {
            m_aura.Auras.Owner.YieldsXpOrHonor = true;
        }
    }
}