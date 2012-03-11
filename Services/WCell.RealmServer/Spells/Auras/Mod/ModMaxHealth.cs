/*************************************************************************
 *
 *   file		: ModMaxHealth.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-07 07:58:12 +0100 (l�, 07 mar 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 784 $
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
    /// Modifies MaxHealth
    /// </summary>
    public class ModMaxHealthHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            m_aura.Auras.Owner.MaxHealthModFlat += EffectValue;
        }

        protected override void Remove(bool cancelled)
        {
            m_aura.Auras.Owner.MaxHealthModFlat -= EffectValue;
        }
    }
};