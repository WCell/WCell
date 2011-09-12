/*************************************************************************
 *
 *   file		: ModDebuffResistancePercent.cs
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
	public class ModDebuffResistancePercentHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			var type = (DamageSchool)m_spellEffect.MiscValue;
			m_aura.Auras.Owner.ModDebuffResistance(type, EffectValue);
		}

		protected override void Remove(bool cancelled)
		{
			var type = (DamageSchool)m_spellEffect.MiscValue;
			m_aura.Auras.Owner.ModDebuffResistance(type, -EffectValue);
		}
	}
};