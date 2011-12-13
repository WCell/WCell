/*************************************************************************
 *
 *   file		: LearnSpell.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-24 00:08:42 +0100 (s? 24 jan 2010) $

 *   revision		: $Rev: 1212 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using NLog;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
	public class LearnSpellEffectHandler : SpellEffectHandler
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		Spell toLearn;

		public LearnSpellEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override SpellFailedReason Initialize()
		{
			if ((toLearn = Effect.TriggerSpell) == null)
			{
				if (m_cast.CasterItem != null && m_cast.CasterItem.Template.TeachSpell != null)
				{
					toLearn = m_cast.CasterItem.Template.TeachSpell.Spell;
				}
				else
				{
					log.Warn("Learn-Spell {0} has invalid Spell to be taught: {1}", Effect.Spell, Effect.TriggerSpellId);
					return SpellFailedReason.Error;
				}
			}
			return SpellFailedReason.Ok;
		}

		public override SpellFailedReason InitializeTarget(WorldObject target)
		{
			if (((Unit)target).Spells.Contains(toLearn.Id))
			{
				return SpellFailedReason.SpellLearned;
			}
			return SpellFailedReason.Ok;
		}

		protected override void Apply(WorldObject target)
		{
			((Unit)target).Spells.AddSpell(toLearn);
		}

		public override ObjectTypes TargetType
		{
			get
			{
				return ObjectTypes.Player;
			}
		}
	}
}