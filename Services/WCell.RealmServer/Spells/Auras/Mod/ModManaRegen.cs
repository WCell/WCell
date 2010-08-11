/*************************************************************************
 *
 *   file		: ModManaRegen.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-08 12:46:40 +0100 (fr, 08 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1173 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using WCell.Constants;
using WCell.Util.Variables;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	/// <summary>
	/// Used by some Druid talents
	/// Regenerates mana in % of a given stat periodically (default every 5 secs)
	/// </summary>
	public class ModManaRegenHandler : AuraEffectHandler
	{
		/// <summary>
		/// Used for ModManaRegen effects that don't have an Amplitude
		/// </summary>
		[Variable("DefaultManaRegenBuffAmplitude")]
		public static int DefaultAmplitude = 5000;

		protected override void Apply()
		{
			var stat = (StatType)m_spellEffect.MiscValue;
			var power = (int)Math.Round(m_aura.Auras.Owner.GetStatValue(stat) * (EffectValue / 100f));
			m_aura.Auras.Owner.Energize(power, m_aura.CasterUnit, m_spellEffect);
		}
	}
};