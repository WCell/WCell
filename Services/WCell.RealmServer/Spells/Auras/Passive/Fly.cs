/*************************************************************************
 *
 *   file		: Fly.cs
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

using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>
    /// Enables flying (mostly triggered through flying rides)
    /// </summary>
    public class FlyHandler : AuraEffectHandler
    {
        protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference, Unit target, ref SpellFailedReason failReason)
        {
            if (target is Character)
            {
                var chr = (Character)target;
                if (!chr.CanFly)
                {
                    failReason = SpellFailedReason.NotHere;
                }
            }
        }

        protected override void Apply()
        {
            m_aura.Auras.Owner.Flying++;
        }

        protected override void Remove(bool cancelled)
        {
            m_aura.Auras.Owner.Flying--;
        }
    }
};