/*************************************************************************
 *
 *   file		: TriggerSpell.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-11 15:22:39 +0100 (ma, 11 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1188 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using NLog;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
	/// <summary>
	/// Triggers a spell on this Effect's targets
	/// </summary>
	public class TriggerSpellEffectHandler : SpellEffectHandler
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		public TriggerSpellEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override void Initialize(ref SpellFailedReason failReason)
		{
			if (Effect.TriggerSpell == null)
			{
				failReason = SpellFailedReason.Error;
				log.Warn("Tried to cast Spell \"{0}\" which has invalid TriggerSpellId {1}", Effect.Spell, Effect.TriggerSpellId);
			}
		}

		public override void Apply()
		{
			//m_cast.Trigger(Effect.TriggerSpell, Targets != null ? Targets.ToArray() : null);
			//m_cast.Trigger(Effect.TriggerSpell, Effect);
			m_cast.Trigger(Effect.TriggerSpell, Effect, Targets != null ? Targets.ToArray() : null);
		}

		protected override void Apply(WorldObject target)
		{
		}
	}
}