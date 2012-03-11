/*************************************************************************
 *
 *   file		: ModSpellCritChanceForSchool.cs
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

using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModSpellCritChanceForSchoolHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            var owner = Owner as Character;
            if (owner != null)
            {
                owner.ModCritMod(m_spellEffect.MiscBitSet, EffectValue);
            }
        }

        protected override void Remove(bool cancelled)
        {
            var owner = Owner as Character;
            if (owner != null)
            {
                owner.ModCritMod(m_spellEffect.MiscBitSet, -EffectValue);
            }
        }
    }
};