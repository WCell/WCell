/*************************************************************************
 *
 *   file		: Stun.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-14 20:02:32 +0100 (to, 14 jan 2010) $

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

		protected override void Apply()
		{
			if (m_aura.Spell.SchoolMask == Constants.DamageSchoolMask.Frost)
				m_aura.Auras.Owner.IncMechanicCount(SpellMechanic.Frozen);

			m_aura.Auras.Owner.IncMechanicCount(SpellMechanic.Stunned);
		}

		protected override void Remove(bool cancelled)
		{
			if (m_aura.Spell.SchoolMask == Constants.DamageSchoolMask.Frost)
				m_aura.Auras.Owner.DecMechanicCount(SpellMechanic.Frozen);

			m_aura.Auras.Owner.DecMechanicCount(SpellMechanic.Stunned);
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