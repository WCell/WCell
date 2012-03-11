/*************************************************************************
 *
 *   file		: PeriodicManaLeech.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-28 13:29:18 +0100 (to, 28 jan 2010) $

 *   revision		: $Rev: 1230 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class PeriodicManaLeechHandler : AuraEffectHandler
    {
        protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference, Unit target, ref SpellFailedReason failReason)
        {
            if (target.MaxPower == 0 || target.PowerType != (PowerType)m_spellEffect.MiscValue)
            {
                failReason = SpellFailedReason.BadTargets;
            }
        }

        protected override void Apply()
        {
            var val = EffectValue;
            var target = m_aura.Auras.Owner;
            if (m_aura.Spell.HasEffectWith((effect) => effect.AuraType == AuraType.Dummy))
            {
                // ugly fix around
                val = target.BasePower * val / 100;
            }
            target.LeechPower(val, 1f, m_aura.CasterUnit, m_spellEffect);
        }
    }
};