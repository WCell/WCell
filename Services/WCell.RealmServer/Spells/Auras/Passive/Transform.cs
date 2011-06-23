/*************************************************************************
 *
 *   file		: Transform.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-08 12:46:40 +0100 (fr, 08 jan 2010) $

 *   revision		: $Rev: 1173 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using NLog;
using WCell.RealmServer.NPCs;
using WCell.Util;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	public class TransformHandler : AuraEffectHandler
	{
		protected static Logger log = LogManager.GetCurrentClassLogger();
		uint displayId;

		protected override void Apply()
		{
			var owner = m_aura.Auras.Owner;
			owner.Dismount();

			displayId = owner.DisplayId;

			if (m_spellEffect.MiscValue > 0)
			{
				if (NPCMgr.EntriesLoaded)
				{
					var npcEntry = NPCMgr.GetEntry((uint)m_spellEffect.MiscValue);
					if (npcEntry == null)
					{
						log.Warn("Transform spell {0} has invalid creature-id {1}", m_aura.Spell, m_spellEffect.MiscValue);
					}
					else
					{
						owner.Model = npcEntry.GetRandomModel();
					}
				}
			}
			else
			{
				log.Warn("Transform spell {0} has no creature-id set", m_aura.Spell);
			}
		}

		protected override void Remove(bool cancelled)
		{
			var owner = m_aura.Auras.Owner;
			owner.DisplayId = displayId;
		}

	}
};