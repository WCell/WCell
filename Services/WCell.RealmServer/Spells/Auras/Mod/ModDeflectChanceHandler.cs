using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModDeflectChanceHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            /*for (DamageSchool i = 0; i < DamageSchool.Count; i++)
            {
                m_aura.Auras.Owner.AddResistanceBuff(i, EffectValue);
            }*/
        }

        protected override void Remove(bool cancelled)
        {
            /*for (DamageSchool i = 0; i < DamageSchool.Count; i++)
            {
                m_aura.Auras.Owner.RemoveResistanceBuff(i, EffectValue);
            }*/
        }
    }
};
