using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    public class ModOffhandDamagePercentHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            var owner = Owner as Character;
            if (owner != null)
            {
                owner.OffhandDmgPctMod += EffectValue;
            }
        }

        protected override void Remove(bool cancelled)
        {
            var owner = Owner as Character;
            if (owner != null)
            {
                owner.OffhandDmgPctMod -= EffectValue;
            }
        }
    }
}