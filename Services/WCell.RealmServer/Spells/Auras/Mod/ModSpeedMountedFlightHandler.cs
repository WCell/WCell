/*************************************************************************
 *
 *   file		: ModSpeedMountedFlightHandler.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-07-25 07:58:12 +0100 (lø, 25 jul 2010) $
 *   last author	: $LastChangedBy: walla $
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
    /// Flying mount speed effect.
    /// </summary>
    public class ModSpeedMountedFlightHandler : AuraEffectHandler
    {
        private float val;

        protected override void Apply()
        {
            m_aura.Auras.Owner.FlightSpeedFactor += (val = EffectValue / 100f);
        }

        protected override void Remove(bool cancelled)
        {
            m_aura.Auras.Owner.FlightSpeedFactor -= val;
        }
    }
};