/*************************************************************************
 *
 *   file		: ModPowerRegenPercent.cs
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

using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	public class ModPowerRegenPercentHandler : AuraEffectHandler
	{
		float val;

		protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference, Unit target, ref SpellFailedReason failReason)
		{
			var type = (PowerType)m_spellEffect.MiscValue;
			if (target.PowerType != type)
			{
				failReason = SpellFailedReason.BadTargets;
			}
		}

		protected override void Apply()
		{
			var type = (PowerType)m_spellEffect.MiscValue;
			if (m_aura.Auras.Owner.PowerType == type)
			{
				val = EffectValue / 100f;
				m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.PowerRegen, val);
			}
		}

		protected override void Remove(bool cancelled)
		{
			var type = (PowerType)m_spellEffect.MiscValue;
			if (m_aura.Auras.Owner.PowerType == type)
			{
				m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.PowerRegen, -val);
			}
		}
	}
};