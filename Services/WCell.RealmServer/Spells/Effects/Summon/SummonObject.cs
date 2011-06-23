/*************************************************************************
 *
 *   file		: SummonObject.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-02-04 10:13:20 +0100 (to, 04 feb 2010) $

 *   revision		: $Rev: 1246 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using NLog;
using WCell.Constants.GameObjects;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.GameObjects.Handlers;
using WCell.Constants.Spells;
using WCell.Constants.Updates;

namespace WCell.RealmServer.Spells.Effects
{
	public class SummonObjectEffectHandler : SpellEffectHandler
	{
		private static Logger log = LogManager.GetCurrentClassLogger();
		public GameObject GO;
		private Unit firstTarget;

		public SummonObjectEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override void Initialize(ref SpellFailedReason failReason)
		{
			if (m_targets != null && m_targets.Count > 0)
			{
				firstTarget = (Unit)m_targets[0];
			}
		}

		public override void Apply()
		{
			var goId = (GOEntryId)Effect.MiscValue;
			var goEntry = GOMgr.GetEntry(goId);
			var caster = m_cast.CasterUnit;
			if (goEntry != null)
			{
				GO = goEntry.Spawn(caster, caster);
				GO.State = GameObjectState.Enabled;
				GO.Orientation = caster.Orientation;
				GO.ScaleX = 1;
				GO.Faction = caster.Faction;
				GO.CreatedBy = caster.EntityId;

				// TODO: Handle SummoningRitualHandler properly
				if (GO.Handler is SummoningRitualHandler)
				{
					((SummoningRitualHandler)GO.Handler).Target = firstTarget;
				}

				if (m_cast.IsChanneling)
				{
					m_cast.CasterUnit.ChannelObject = GO;
				}
				else if (Effect.Spell.Durations.Min > 0)
				{
					// not channelled: Activate decay delay
					GO.RemainingDecayDelayMillis = Effect.Spell.Durations.Random();
				}
			}
			else
			{
				log.Error("Summoning Spell {0} refers to invalid Object: {1} ({2})", Effect.Spell, goId, (uint)goId);
			}
		}

		public override ObjectTypes CasterType
		{
			get { return ObjectTypes.Unit; }
		}
	}
}