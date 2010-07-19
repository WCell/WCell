/*************************************************************************
 *
 *   file		: ModCastingSpeed.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-06-30 12:49:04 +0200 (ti, 30 jun 2009) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1042 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	public class ModCastingSpeedHandler : AuraEffectHandler
	{
		float val;

		protected internal override void Apply()
		{
			m_aura.Auras.Owner.CastSpeedFactor += val = -EffectValue / 100f;
		}

		protected internal override void Remove(bool cancelled)
		{
			m_aura.Auras.Owner.CastSpeedFactor -= val;
		}
	}
};