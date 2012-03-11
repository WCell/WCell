﻿namespace WCell.RealmServer.Spells.Auras.Mod
{
    public class ModAttackerSpellCritChanceHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Owner.AttackerSpellCritChancePercentMod += EffectValue;
        }

        protected override void Remove(bool cancelled)
        {
            Owner.AttackerSpellCritChancePercentMod -= EffectValue;
        }
    }
}
