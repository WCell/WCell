/*************************************************************************
 *
 *   file		: Skill.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-28 05:18:24 +0100 (to, 28 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1229 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants.Skills;

namespace WCell.RealmServer.Spells.Effects
{
	/// <summary>
	/// Uses a Skill (add skill checks here?)
	/// </summary>
	public class SkillEffectHandler : SpellEffectHandler
	{
		public SkillEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override void Apply()
		{
			SkillId skillId = (SkillId)Effect.MiscValue;
			var tier = Effect.BasePoints;
		}

		public override bool HasOwnTargets
		{
			get { return false; }
		}
	}
}