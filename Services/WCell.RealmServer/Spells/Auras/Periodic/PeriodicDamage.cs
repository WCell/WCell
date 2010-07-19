/*************************************************************************
 *
 *   file		: PeriodicDamage.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-10 13:00:10 +0100 (sø, 10 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1185 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	/// <summary>
	/// Periodically damages the holder
	/// </summary>
	public class PeriodicDamageHandler : AuraEffectHandler
	{
		protected internal override void Apply()
		{
			var holder = m_aura.Auras.Owner;
			if (holder.IsAlive)
			{
				var value = EffectValue;
				if (m_aura.Spell.Mechanic == SpellMechanic.Bleeding)
				{
					var bonus = GetBleedBonus();
					value = ((value*bonus) + 50)/100;
				}


				holder.DoSpellDamage(m_aura.Caster as Unit, m_spellEffect, value);
			}
		}

		private int GetBleedBonus()
		{
			var bonus = 0;
			{
				foreach (var aura in m_aura.Auras.ActiveAuras)
				{
					foreach (var handler in aura.Handlers)
					{
						if (handler.SpellEffect.AuraType == AuraType.IncreaseBleedEffectPct)
						{
							bonus += handler.EffectValue;
						}
					}
				}
			}
			return bonus;
		}
	}
};