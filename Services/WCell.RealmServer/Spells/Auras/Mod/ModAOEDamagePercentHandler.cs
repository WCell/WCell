namespace WCell.RealmServer.Spells.Auras.Mod
{
    public class ModAOEDamagePercentHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Owner.AoEDamageModifierPct += EffectValue;
        }

        protected override void Remove(bool cancelled)
        {
            Owner.AoEDamageModifierPct -= EffectValue;
        }
    }
}
