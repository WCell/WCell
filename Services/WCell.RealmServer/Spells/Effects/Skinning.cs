/*************************************************************************
 *
 *   file		: Skinning.cs
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

using WCell.Constants;
using WCell.Constants.Looting;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Skills;

namespace WCell.RealmServer.Spells.Effects
{
	public class SkinningEffectHandler : SpellEffectHandler
	{
		public SkinningEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override SpellFailedReason CheckValidTarget(WorldObject target)
		{
			var unit = (Unit) target;
			if (Cast.CasterChar.Skills.CheckSkill(SkillHandler.GetSkill((SkinningType)Effect.MiscValue), unit.Level * 5))
			{
				return SpellFailedReason.TargetUnskinnable;
			}
			if (unit.Loot != null && !unit.Loot.IsReleased)
			{
				return SpellFailedReason.TargetNotLooted;
			}

			return SpellFailedReason.Ok;
		}

		protected override void Apply(WorldObject target)
		{
			var caster = m_cast.CasterChar;
			var unit = (Unit)target;

			caster.Emote(EmoteType.SimpleTalk);
			LootMgr.CreateAndSendObjectLoot(target, caster, LootEntryType.Skinning, false);
			unit.UnitFlags &= ~UnitFlags.Skinnable;
		}

		public override ObjectTypes CasterType
		{
			get
			{
				return ObjectTypes.Player;
			}
		}

		public override ObjectTypes TargetType
		{
			get
			{
				return ObjectTypes.Unit;
			}
		}
	}
}
