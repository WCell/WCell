/*************************************************************************
 *
 *   file		: Disenchant.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-14 13:00:53 +0100 (to, 14 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1192 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants.Looting;
using WCell.Constants.Misc;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Looting;

namespace WCell.RealmServer.Spells.Effects
{
	/// <summary>
	/// Disenchant Visual (Id: 61335)
	/// </summary>
	public class DisenchantEffectHandler : SpellEffectHandler
	{
		public DisenchantEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override void Initialize(ref SpellFailedReason failReason)
		{
			var templ = m_cast.UsedItem.Template;
			if (templ.RequiredDisenchantingLevel == -1)
			{
				failReason = SpellFailedReason.CantBeDisenchanted;
			}
			else if (templ.RequiredDisenchantingLevel > m_cast.CasterChar.Skills.GetValue(SkillId.Enchanting))
			{
				failReason = SpellFailedReason.CantBeDisenchantedSkill;
			}
		}

		public override void Apply()
		{
			var caster = m_cast.CasterChar;
			caster.Emote(EmoteType.SimpleTalk);
			LootMgr.CreateAndSendObjectLoot(m_cast.UsedItem, caster, LootEntryType.Disenchanting, false);
			//m_cast.CasterChar.Inventory.AddAllUnchecked(loot, slots);
		}

		public override bool HasOwnTargets
		{
			get
			{
				return false;
			}
		}

		public override ObjectTypes CasterType
		{
			get
			{
				return ObjectTypes.Player;
			}
		}
	}
}