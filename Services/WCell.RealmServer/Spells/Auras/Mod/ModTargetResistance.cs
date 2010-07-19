/*************************************************************************
 *
 *   file		: ModTargetResistance.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-10 13:00:10 +0100 (sø, 10 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1185 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	/// <summary>
	/// Makes one ignore target's resistances
	/// </summary>
	public class ModTargetResistanceHandler : AuraEffectHandler
	{
		protected internal override void Apply()
		{
			Owner.ModTargetResistanceMod(EffectValue, m_spellEffect.MiscBitSet);
		}

		protected internal override void Remove(bool cancelled)
		{
			Owner.ModTargetResistanceMod(-EffectValue, m_spellEffect.MiscBitSet);
		}
	}
};