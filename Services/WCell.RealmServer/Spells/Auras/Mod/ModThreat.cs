/*************************************************************************
 *
 *   file		: ModThreat.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-10 13:00:10 +0100 (sø, 10 jan 2010) $

 *   revision		: $Rev: 1185 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>
    /// Modifies threat generated by attacks of the given DamageSchools
    /// </summary>
    public class ModThreatHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Owner.ModThreat(m_spellEffect.MiscBitSet, EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            Owner.ModThreat(m_spellEffect.MiscBitSet, -EffectValue);
        }
    }
};