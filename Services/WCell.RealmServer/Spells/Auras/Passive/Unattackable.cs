namespace WCell.RealmServer.Spells.Auras.Passive
{
    public class UnattackableHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Owner.Invulnerable++;
        }

        protected override void Remove(bool cancelled)
        {
            Owner.Invulnerable--;
        }
    }
}