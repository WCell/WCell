/*************************************************************************
 *
 *   file		: NormalizedWeaponDamagePlus.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-14 13:00:53 +0100 (to, 14 jan 2010) $

 *   revision		: $Rev: 1192 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>
    /// Adds flat melee damage to the attack
    /// </summary>
    public class NormalizedWeaponDamagePlusEffectHandler : WeaponDamageEffectHandler
    {
        public NormalizedWeaponDamagePlusEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }
    }
}