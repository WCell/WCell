/*************************************************************************
 *
 *   file		: ModDamageDone.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-25 18:19:39 +0100 (ma, 25 jan 2010) $

 *   revision		: $Rev: 1222 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModDamageDoneHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Owner.AddDamageDoneMod(m_spellEffect.MiscBitSet, EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            Owner.RemoveDamageDoneMod(m_spellEffect.MiscBitSet, EffectValue);
        }
    }
};