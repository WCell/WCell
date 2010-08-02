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

using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	/// <summary>
	/// Periodically damages the holder in %
	/// </summary>
	public class PeriodicDamagePercentHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			var holder = Owner;
			if (holder.IsAlive)
			{
				var value = (Owner.MaxHealth * EffectValue + 50) / 100;
				if (m_aura.Spell.Mechanic == SpellMechanic.Bleeding)
				{
					var bonus = m_aura.Auras.GetBleedBonusPercent();
					value += ((value * bonus) + 50) / 100;
				}
				holder.DoSpellDamage(m_aura.Caster, m_spellEffect, value, false);
			}
		}

	}
};