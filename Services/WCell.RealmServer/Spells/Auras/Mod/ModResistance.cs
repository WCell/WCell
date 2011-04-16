/*************************************************************************
 *
 *   file		: ModResistance.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-11 17:46:10 +0100 (ma, 11 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1190 $
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
	/// TODO: Needs to be value in %
	/// </summary>
	public class ModResistancePctHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			for (var i = 0; i < m_spellEffect.MiscBitSet.Length; i++)
			{
				var flag = m_spellEffect.MiscBitSet[i];
				m_aura.Auras.Owner.AddResistanceBuff((DamageSchool) flag, EffectValue);
			}
		}

		protected override void Remove(bool cancelled)
		{
			for (var i = 0; i < m_spellEffect.MiscBitSet.Length; i++)
			{
				var flag = m_spellEffect.MiscBitSet[i];
				m_aura.Auras.Owner.RemoveResistanceBuff((DamageSchool) flag, EffectValue);
			}
		}
	}
};