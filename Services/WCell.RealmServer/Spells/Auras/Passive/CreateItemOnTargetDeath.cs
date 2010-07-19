/*************************************************************************
 *
 *   file		: CreateItemOnTargetDeath.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-24 17:50:44 +0100 (sø, 24 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1216 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using NLog;
using WCell.Constants.Items;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	public class CreateItemOnTargetDeathHandler : AuraEffectHandler
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		protected internal override void Apply()
		{
			// does nada
		}

		protected internal override void Remove(bool cancelled)
		{
			var procFlags = SpellEffect.Spell.ProcTriggerFlags;
			var owner = m_aura.Auras.Owner;
			if (!owner.IsAlive && 
				(!procFlags.HasFlag(ProcTriggerFlags.GainExperience) || owner.YieldsXpOrHonor))
			{
				var item = ItemMgr.GetTemplate(SpellEffect.ItemId);
				if (item == null)
				{
					log.Warn("Spell {0} referred to invalid Item: {1} ({2})", SpellEffect.Spell,
						(ItemId)SpellEffect.ItemId, SpellEffect.ItemId);
				}
				else
				{
					var caster = m_aura.Caster;
					if (caster != null && (caster = caster.Master as Character) != null)
					{
						var amount = Math.Max(1, EffectValue);
						((Character)caster).Inventory.TryAdd(item, ref amount, true);
					}
				}
			}
		}
	}
};