/*************************************************************************
 *
 *   file		: ManaShield.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-14 17:36:47 +0100 (to, 14 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1193 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	public class ManaShieldHandler : AttackEventEffectHandler
	{
		private float factor;
		private int remaining;

		protected override void Apply()
		{
			factor = SpellEffect.ProcValue != 0 ? SpellEffect.ProcValue : 1;
			remaining = EffectValue;

			base.Apply();
		}

		public override void OnDefend(DamageAction action)
		{
			var defender = Owner;	// same as action.Victim
			var power = defender.Power;
			var damage = action.Damage;

			var amount = Math.Min(damage, (int)(power / factor));	// figure out how much to drain
			if (remaining < amount)
			{
				// shield is used up
				amount = remaining;
				remaining = 0;
				m_aura.Remove(false);
			}
			else
			{
				remaining -= amount;
			}
			defender.Power = power - (int)(amount * factor);	// drain power
			damage -= amount;									// reduce damage

			action.Damage = damage;
		}
	}
};