/*************************************************************************
 *
 *   file		: ModTimeBetweenAttacks.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-12-23 20:07:17 +0100 (on, 23 dec 2009) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1151 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	/// <summary>
	/// Haste for melee, ranged and spells
	/// </summary>
	public class ModTimeBetweenAttacksHandler : AuraEffectHandler
	{
		float val;

		protected internal override void Apply()
		{
			m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.AttackTime, val = EffectValue / 100f);
			m_aura.Auras.Owner.CastSpeedFactor += val;
		}

		protected internal override void Remove(bool cancelled)
		{
			m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.AttackTime, -val);
			m_aura.Auras.Owner.CastSpeedFactor -= val;
		}
	}
};