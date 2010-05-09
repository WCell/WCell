/*************************************************************************
 *
 *   file		: Shapeshift.cs
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

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	/// <summary>
	/// Changes the owner's form.
	/// TODO: The act of shapeshifting frees the caster of Polymorph and Movement Impairing effects.
	/// </summary>
	public class ShapeshiftHandler : AuraEffectHandler
	{
		ShapeShiftForm form;

		protected internal override void CheckInitialize(CasterInfo casterInfo, Unit target, ref SpellFailedReason failReason)
		{
			form = (ShapeShiftForm)SpellEffect.MiscValue;
			if (target.ShapeShiftForm == form)
			{
				// stances can't be undone:
				if (form != ShapeShiftForm.BattleStance &&
					form != ShapeShiftForm.BerserkerStance &&
					form != ShapeShiftForm.DefensiveStance)
				{
					target.Auras.RemoveWhere(aura => aura.Spell.Id == m_spellEffect.Spell.Id);
				}
			}
		}

		protected internal override void Apply()
		{
			m_aura.Auras.Owner.ShapeShiftForm = form;
		}

		protected internal override void Remove(bool cancelled)
		{
			m_aura.Auras.Owner.ShapeShiftForm = ShapeShiftForm.Normal;
		}
	}
};
