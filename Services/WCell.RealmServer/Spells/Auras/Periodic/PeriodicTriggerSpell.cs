/*************************************************************************
 *
 *   file		: PeriodicTriggerSpell.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-02-01 14:22:25 +0100 (ma, 01 feb 2010) $

 *   revision		: $Rev: 1240 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Util.Logging;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	/// <summary>
	/// Periodically makes the holder cast a Spell
	/// </summary>
	public class PeriodicTriggerSpellHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			TriggerSpell(m_spellEffect.TriggerSpell);
		}

		protected void TriggerSpell(Spell spell)
		{
			SpellCast cast = m_aura.SpellCast;

			if (spell == null)
			{
				LogManager.GetCurrentClassLogger().Warn("Found invalid periodic TriggerSpell in Spell {0} ({1}) ",
					m_aura.Spell,
					(uint)m_spellEffect.TriggerSpellId);
				return;
			}
			
			SpellCast.ValidateAndTriggerNew(spell, m_aura.CasterReference, Owner, Owner,  m_aura.Controller as SpellChannel, cast != null ? cast.TargetItem : null,
				null, m_spellEffect);
		}

		protected override void Remove(bool cancelled)
		{
		}


	}
};