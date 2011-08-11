/*************************************************************************
 *
 *   file		: ProcTriggerDamage.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-09-13 20:09:14 +0200 (sø, 13 sep 2009) $

 *   revision		: $Rev: 1095 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using NLog;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	/// <summary>
	/// Does school damage to its targets
	/// </summary>
	public class ProcTriggerDamageHandler : AuraEffectHandler
	{
		public override void OnProc(Unit triggerer, IUnitAction action)
		{
			var val = m_spellEffect.CalcEffectValue(m_aura.CasterReference);

			//if (action is IDamageAction)
			//{
			//    ((IDamageAction)action).Damage += val;
			//}
			//else

			if (Owner.MayAttack(triggerer))
			{
				Owner.DealSpellDamage(triggerer, m_spellEffect, val);
			}
			else
			{
				LogManager.GetCurrentClassLogger().Warn("Invalid damage effect on Spell {0} was triggered by {1} who cannot be attacked by Aura-Owner {2}.",
					m_aura.Spell, triggerer, Owner);
			}
		}
	}
};