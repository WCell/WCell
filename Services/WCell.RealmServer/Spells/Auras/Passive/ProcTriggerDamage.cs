/*************************************************************************
 *
 *   file		: ProcTriggerDamage.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-09-13 20:09:14 +0200 (sø, 13 sep 2009) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1095 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

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
			{
				if (Owner.MayAttack(triggerer))
				{
					Owner.DoSpellDamage(triggerer, m_spellEffect, val);
				}
			}
		}
	}
};