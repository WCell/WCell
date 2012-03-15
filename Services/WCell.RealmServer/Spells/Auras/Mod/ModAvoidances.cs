using WCell.Constants;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    public class ModAttackerSpellHitChanceHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Owner.ModAttackerSpellHitChance(m_spellEffect.MiscBitSet, EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            Owner.ModAttackerSpellHitChance(m_spellEffect.MiscBitSet, -EffectValue);
        }
    }

    public class ModAttackerMeleeHitChanceHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Owner.ChangeModifier(StatModifierInt.AttackerMeleeHitChance, EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            Owner.ChangeModifier(StatModifierInt.AttackerMeleeHitChance, -EffectValue);
        }
    }

    public class ModAttackerRangedHitChanceHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Owner.ChangeModifier(StatModifierInt.AttackerRangedHitChance, EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            Owner.ChangeModifier(StatModifierInt.AttackerRangedHitChance, -EffectValue);
        }
    }
}
