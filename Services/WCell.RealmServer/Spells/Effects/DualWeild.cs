/*************************************************************************
 *
 *   file		: DualWeild.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-14 13:00:53 +0100 (to, 14 jan 2010) $

 *   revision		: $Rev: 1192 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants.Skills;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
	/// <summary>
	/// Should be a passive spell
	/// Adds the DualWeild skill
	/// </summary>
	public class DualWeildEffectHandler : SpellEffectHandler
	{
		public DualWeildEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		protected override void Apply(WorldObject target)
		{
			if (target is Character)
			{
				var skills = ((Character) target).Skills;
				if (skills != null)
				{
					skills.TryLearn(SkillId.DualWield);
				}
				// TODO: What if it cannot be learnt?
			}
			else
			{
				// TODO: Do NPCs need this skill for anything really?
			}
		}
	}
}