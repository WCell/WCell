using WCell.Constants.Misc;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Auras.Effects
{
    /// <summary>
    /// Reduces victim armor by the given value in %
    /// </summary>
    public class ModArmorPenetrationHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            var owner = Owner as Character;
            if (owner != null)
                owner.ModCombatRating(CombatRating.ArmorPenetration, EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            var owner = Owner as Character;
            if (owner != null)
                owner.ModCombatRating(CombatRating.ArmorPenetration, -EffectValue);
        }
    }
}