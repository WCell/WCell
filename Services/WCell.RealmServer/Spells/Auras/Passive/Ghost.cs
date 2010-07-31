/*************************************************************************
 *
 *   file		: Ghost.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-06-28 14:10:07 +0200 (sø, 28 jun 2009) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1038 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	/// <summary>
	/// Dead players wear this
	/// </summary>
	public class GhostHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			var owner = m_aura.Auras.Owner;
			if (owner is Character)
			{
				((Character)owner).PlayerFlags |= PlayerFlags.Ghost;
			}
			if (owner.Race == RaceId.NightElf)
			{
				// Whisp
				owner.SpellCast.TriggerSelf(SpellId.Ghost_3);
			}

			// run faster
			owner.SpeedFactor += Character.DeathSpeedIncrease;

			// walk on water
			owner.WaterWalk++;
			m_aura.Auras.GhostAura = m_aura;
		}

		protected override void Remove(bool cancelled)
		{
			var owner = m_aura.Auras.Owner;
			if (owner is Character)
			{
				((Character)owner).PlayerFlags &= ~PlayerFlags.Ghost;
			}
			if (owner.Race == RaceId.NightElf)
			{
				// Whisp
				m_aura.Auras.Cancel(SpellId.Ghost_3);
			}

			if (m_aura.Auras.GhostAura == m_aura)
				m_aura.Auras.GhostAura = null;

			owner.SpeedFactor -= Character.DeathSpeedIncrease;
			owner.WaterWalk--;
			m_aura.Auras.Owner.OnResurrect();
		}

		/// <summary>
		/// Not positive also means that its not removable
		/// </summary>
		public override bool IsPositive
		{
			get
			{
				return false;
			}
		}
	}
};