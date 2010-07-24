/*************************************************************************
 *
 *   file		: Stun.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-14 20:02:32 +0100 (to, 14 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1194 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	public class StunHandler : AuraEffectHandler
	{
		SpellMechanic mechanic;

		protected override void Apply()
		{
			mechanic = SpellEffect.Mechanic;
			if (mechanic == SpellMechanic.None)
			{
				mechanic = SpellEffect.Spell.Mechanic;
				if (mechanic == SpellMechanic.None || mechanic == SpellMechanic.Invulnerable || mechanic == SpellMechanic.Invulnerable_2)
				{
					mechanic = SpellMechanic.Stunned;
				}
			}
			m_aura.Auras.Owner.IncMechanicCount(mechanic);
		}

		protected override void Remove(bool cancelled)
		{
			m_aura.Auras.Owner.DecMechanicCount(mechanic);
		}

		public override bool IsPositive
		{
			get
			{
				return false;
			}
		}
	}
};