using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    class ModArmorByPercentOfIntellectHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Owner.Armor += (Owner.Intellect * 100 + 50) / EffectValue;
        }

        protected override void Remove(bool cancelled)
        {
            Owner.Armor -= (Owner.Intellect * 100 + 50) / EffectValue;
        }
    }
}
