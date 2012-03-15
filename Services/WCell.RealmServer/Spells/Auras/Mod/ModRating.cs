/*************************************************************************
 *
 *   file		: ModRating.cs
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

using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>
    /// Modifies CombatRatings
    /// </summary>
    public class ModRatingHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            var owner = m_aura.Auras.Owner as Character;
            if (owner != null)
                owner.ModCombatRating(m_spellEffect.MiscBitSet, EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            var owner = m_aura.Auras.Owner as Character;
            if (owner != null)
                owner.ModCombatRating(m_spellEffect.MiscBitSet, -EffectValue);
        }
    }
};