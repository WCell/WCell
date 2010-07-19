/*************************************************************************
 *
 *   file		: Mounted.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-23 20:40:40 +0100 (lø, 23 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1211 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using NLog;
using WCell.Constants.NPCs;
using WCell.RealmServer.NPCs;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	/// <summary>
	/// Applies a mount-aura
	/// </summary>
	public class MountedHandler : AuraEffectHandler
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		protected internal override void Apply()
		{
			var creatureId = (uint)SpellEffect.MiscValue;
			var entry = NPCMgr.GetEntry(creatureId);
			if (entry != null && entry.DisplayIds.Length > 0)
			{
				var displayId = entry.DisplayIds[0];

				m_aura.Auras.Owner.Mount(displayId);

				m_aura.Auras.MountAura = m_aura;

				// some flying mounts dont have a flying effect, so we need to apply it here:
				if (m_aura.Spell.IsFlyingMount && !m_aura.Spell.HasFlyEffect)
				{
					m_aura.Auras.Owner.Flying++;
				}
			}
		}

		protected internal override void Remove(bool cancelled)
		{
			// some flying mounts dont have a flying effect, so we need to remove it here:
			if (m_aura.Spell.IsFlyingMount && !m_aura.Spell.HasFlyEffect)
			{
				m_aura.Auras.Owner.Flying--;
			}
			m_aura.Auras.Owner.DoDismount();
		}
	}
};