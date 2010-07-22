/*************************************************************************
 *
 *   file		: PeriodicDamagePercent.cs
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

using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	/// <summary>
	/// Periodically damages the holder
	/// </summary>
	public class PeriodicDamagePercentHandler : AuraEffectHandler
	{
		protected internal override void Apply()
		{
			var holder = m_aura.Auras.Owner;
			if (m_aura.Caster != null)
			{
				holder.DoSpellDamage(m_aura.Caster, m_spellEffect, (int)(holder.MaxHealth * (EffectValue / 100f)));
			}
		}

	}
};