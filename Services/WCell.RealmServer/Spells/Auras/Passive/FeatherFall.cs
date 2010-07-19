using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class FeatherFallHandler : AuraEffectHandler
    {
        protected internal override void Apply()
        {
            m_aura.Auras.Owner.FeatherFalling++;
        }

        protected internal override void Remove(bool cancelled)
        {
            m_aura.Auras.Owner.FeatherFalling--;
        }
    }
}