﻿namespace WCell.RealmServer.Spells.Auras.Mod
{
    internal class ModArmorByPercentOfIntellectHandler : AuraEffectHandler
    {
        private int value;

        protected override void Apply()
        {
            value = (Owner.Intellect * 100 + 50) / EffectValue;
            Owner.AddResistanceBuff(Constants.DamageSchool.Physical, value);
        }

        protected override void Remove(bool cancelled)
        {
            Owner.RemoveResistanceBuff(Constants.DamageSchool.Physical, value);
        }
    }
}
