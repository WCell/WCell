/*************************************************************************
 *
 *   file		: RegenPercentOfTotalMana.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-28 13:29:18 +0100 (to, 28 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
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
	/// Regenerates a percentage of your total Mana every tick
	/// </summary>
	public class RegenPercentOfTotalManaHandler : AuraEffectHandler
	{
		protected internal override void CheckInitialize(ObjectInfo casterInfo, Unit target, ref SpellFailedReason failReason)
		{
			//if (target.PowerType != WCell.Core.PowerType.Mana) {
			//    failReason = SpellFailedReason.AlreadyAtFullPower;
			//}
		}

		protected override void Apply()
		{
			Owner.Energize(m_aura.Caster, (EffectValue * Owner.MaxPower + 50) / 100, m_spellEffect);
		}

	}
};