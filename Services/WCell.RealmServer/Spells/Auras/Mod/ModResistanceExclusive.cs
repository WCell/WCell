/*************************************************************************
 *
 *   file		: ModResistanceExclusive.cs
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

using WCell.Constants;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	/// <summary>
	/// Same as ModResistance?
	/// </summary>
	public class ModResistanceExclusiveHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			foreach (DamageSchool school in m_spellEffect.MiscBitSet)
			{
				m_aura.Auras.Owner.AddResistanceBuff(school, EffectValue);
			}
		}

		protected override void Remove(bool cancelled)
		{
			foreach (DamageSchool flag in m_spellEffect.MiscBitSet)
			{
				m_aura.Auras.Owner.RemoveResistanceBuff(flag, EffectValue);
			}
		}
	}
};