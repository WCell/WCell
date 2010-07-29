/*************************************************************************
 *
 *   file		: PeriodicEnergize.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-07 07:58:12 +0100 (lø, 07 mar 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 784 $
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
	public class PeriodicEnergizeHandler : AuraEffectHandler
	{

		protected override void Apply()
		{
			var type = (PowerType)m_spellEffect.MiscValue;
			if (type == m_aura.Auras.Owner.PowerType)
			{
				m_aura.Auras.Owner.Energize(EffectValue, m_aura.Caster, m_spellEffect);
			}
		}
	}

	public class PeriodicEnergizePctHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			var type = (PowerType)m_spellEffect.MiscValue;
			if (type == Owner.PowerType)
			{
				var val = (Owner.MaxPower * EffectValue + 50) / 100;
				m_aura.Auras.Owner.Energize(val, m_aura.Caster, m_spellEffect);
			}
		}
	}
};