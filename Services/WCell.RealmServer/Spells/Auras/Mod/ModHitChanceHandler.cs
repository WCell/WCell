using WCell.Constants;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    public class ModHitChanceHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            var owner = Owner as Character;
            if (owner != null)
            {
                owner.ChangeModifier(StatModifierInt.HitChance, EffectValue);
            }
        }

        protected override void Remove(bool cancelled)
        {
            var owner = Owner as Character;
            if (owner != null)
            {
                owner.ChangeModifier(StatModifierInt.HitChance, -EffectValue);
            }
        }
    }
}