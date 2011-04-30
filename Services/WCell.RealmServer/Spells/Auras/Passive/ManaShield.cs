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
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	public class ManaShieldHandler : AttackEventEffectHandler
	{
		private float factor, factorInverse;
		private int remaining;

		protected override void Apply()
		{
			factor = (SpellEffect.ChainAmplitude != 0 ? SpellEffect.ChainAmplitude : 1);
			factorInverse = 1 / factor;
			remaining = EffectValue;

			base.Apply();
		}

		public override void OnDefend(DamageAction action)
		{
			var defender = Owner;	// same as action.Victim
			var power = defender.Power;
			var damage = action.Damage;

			var drainAmount = Math.Min(damage, (int)(power * factorInverse));	// figure out how much to drain
			if (remaining < drainAmount)
			{
				// shield is used up
				drainAmount = remaining;
				remaining = 0;
				m_aura.Remove(false);
			}
			else
			{
				remaining -= drainAmount;
			}

			drainAmount = (int)(drainAmount * factor);

			var caster = Aura.CasterUnit;
			if (caster != null)
			{
				// see MageArcaneArcaneShielding
				drainAmount = caster.Auras.GetModifiedInt(SpellModifierType.HealingOrPowerGain, m_spellEffect.Spell, drainAmount);
			}
			defender.Power = power - drainAmount;					// drain power
			damage -= drainAmount;									// reduce damage

			action.Damage = damage;
		}
	}
};