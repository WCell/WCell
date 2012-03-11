/*************************************************************************
 *
 *   file		: ModSpellCritChance.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-25 21:05:31 +0100 (ma, 25 jan 2010) $

 *   revision		: $Rev: 1224 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>
    /// Mods Spell crit chance in %
    /// </summary>
    public class ModSpellHitChanceHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            var owner = Owner;
            if (m_spellEffect.MiscValue == 0)
            {
                for (var s = DamageSchool.Physical; s < DamageSchool.Count; s++)
                {
                    owner.ModSpellHitChance(s, EffectValue);
                }
            }
            else
            {
                owner.ModSpellHitChance(m_spellEffect.MiscBitSet, EffectValue);
            }
        }

        protected override void Remove(bool cancelled)
        {
            var owner = Owner;
            if (m_spellEffect.MiscValue == 0)
            {
                for (var s = DamageSchool.Physical; s < DamageSchool.Count; s++)
                {
                    owner.ModSpellHitChance(s, -EffectValue);
                }
            }
            else
            {
                owner.ModSpellHitChance(m_spellEffect.MiscBitSet, -EffectValue);
            }
        }
    }
};