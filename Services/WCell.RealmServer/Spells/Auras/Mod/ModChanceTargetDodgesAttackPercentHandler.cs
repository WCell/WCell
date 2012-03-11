using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    /// <summary>
    /// Only used for WarriorArmsWeaponMastery
    /// </summary>
    public class ModChanceTargetDodgesAttackPercentHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            var owner = Owner as Character;
            if (owner != null)
            {
                owner.IntMods[(int)StatModifierInt.TargetDodgesAttackChance] += EffectValue;
            }
        }

        protected override void Remove(bool cancelled)
        {
            var owner = Owner as Character;
            if (owner != null)
            {
                owner.IntMods[(int)StatModifierInt.TargetDodgesAttackChance] -= EffectValue;
            }
        }
    }
}