/*************************************************************************
 *
 *   file		: ForgetSpecialization.cs
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

using NLog;
using WCell.RealmServer.Entities;
using WCell.Constants.Updates;

namespace WCell.RealmServer.Spells.Effects
{
	public class ForgetSpecializationEffectHandler : SpellEffectHandler
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		public ForgetSpecializationEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		protected override void Apply(WorldObject target)
		{
			var spell = Effect.TriggerSpell;
			if (spell != null)
			{
				((Unit)target).Spells.Remove(spell);
			}
			else
			{
				// oh oh!
				log.Warn("Couldn't find skill to forget: " + Effect.Spell);
			}
		}

		public override ObjectTypes TargetType
		{
			get{return ObjectTypes.Player;}
		}
	}
}