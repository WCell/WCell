/*************************************************************************
 *
 *   file		: PeriodicTriggerSpell.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-02-01 14:22:25 +0100 (ma, 01 feb 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1240 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using NLog;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	/// <summary>
	/// Periodically makes the holder cast a Spell
	/// </summary>
	public class PeriodicTriggerSpellHandler : AuraEffectHandler
	{
		protected Spell spell;
		protected SpellCast cast, origCast;

		protected internal override void Apply()
		{
			if (spell == null)
			{
				var channel = m_aura.Controller as SpellChannel;
				if (channel != null)
				{
					origCast = channel.Cast;
				}
				else
				{
					origCast = m_aura.Auras.Owner.SpellCast;
				}

				if (origCast == null)
				{
					return;
					//throw new Exception("Cannot apply a Periodic Trigger Spell Aura on anyone but the Caster");
				}

				spell = m_spellEffect.TriggerSpell;
				if (spell == null)
				{
					LogManager.GetCurrentClassLogger().Warn("Found invalid periodic TriggerSpell in Spell {0} ({1}) ",
						m_spellEffect.Spell, 
						(uint)m_spellEffect.TriggerSpellId);
					return;
				}
			}

			cast = SpellCast.ObtainPooledCast(origCast.Caster);
			cast.TargetLoc = origCast.TargetLoc;
			cast.Selected = origCast.Selected;
			//cast.Start(spell, m_spellEffect, true);
			cast.Start(spell, true);
		}

		protected internal override void Remove(bool cancelled)
		{
			if (cast != null && cast.IsChanneling)
			{
				cast.Cancel(SpellFailedReason.Ok);
				cast.Dispose();
				cast = null;
			}
		}


	}
};