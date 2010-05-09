/*************************************************************************
 *
 *   file		: SkillStep.cs
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

using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.Constants.Updates;

namespace WCell.RealmServer.Spells.Effects
{
	/// <summary>
	/// Learns a new Skill-Tier
	/// </summary>
	public class SkillStepEffectHandler : SpellEffectHandler
	{
		public SkillStepEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		protected override void Apply(WorldObject target)
		{
			var skillId = (SkillId)Effect.MiscValue;
			var tier = (uint)Effect.BasePoints;

			if (!((Character)target).Skills.TryLearn(skillId, tier))
			{
				//m_cast.Cancel(SpellFailedReason.TooManySkills);
				m_cast.Cancel(SpellFailedReason.MinSkill);// TODO: find new correct reason
			}
		}

		public override ObjectTypes TargetType
		{
			get{ return ObjectTypes.Player | ObjectTypes.Unit; }
		}
	}
}
