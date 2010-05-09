/*************************************************************************
 *
 *   file		: ModStat.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-30 10:02:00 +0100 (lø, 30 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1234 $
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
	public class ModStatHandler : AuraEffectHandler
	{
		protected internal override void Apply()
		{
			Owner.AddStatMod((StatType)m_spellEffect.MiscValue, EffectValue, SpellEffect.Spell.IsPassive);
		}

		protected internal override void Remove(bool cancelled)
		{
			Owner.RemoveStatMod((StatType)m_spellEffect.MiscValue, EffectValue, SpellEffect.Spell.IsPassive);
		}
	}
};
