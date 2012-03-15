using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>
    /// Increases healing taken in %
    /// </summary>
    public class ModHealingDonePctHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            var owner = Owner as Character;
            if (owner != null)
            {
                owner.HealingDoneModPct += EffectValue;
            }
        }

        protected override void Remove(bool cancelled)
        {
            var owner = Owner as Character;
            if (owner != null)
            {
                owner.HealingDoneModPct -= EffectValue;
            }
        }
    }
};