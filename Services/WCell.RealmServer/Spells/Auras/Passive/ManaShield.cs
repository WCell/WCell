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

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	public class ManaShieldHandler : AuraEffectHandler
	{

		protected override void Apply()
		{
			// mutually exclusive
			var auras = m_aura.Auras;

			auras.Owner.SetManaShield(SpellEffect.ProcValue != 0 ? SpellEffect.ProcValue : 1, EffectValue);
		}

		protected override void Remove(bool cancelled)
		{
			m_aura.Auras.Owner.SetManaShield(0, 0);
		}
	}
};