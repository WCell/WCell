/*************************************************************************
 *
 *   file		: TeleportUnits.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-27 14:06:21 +0100 (on, 27 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1228 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Spells.Effects
{
	public class TeleportUnitsEffectHandler : SpellEffectHandler
	{
		public TeleportUnitsEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override void Initialize(ref SpellFailedReason failReason)
		{
			if (!Effect.Spell.IsHearthStoneSpell &&
				(m_cast.TargetLoc.X == 0 || m_cast.TargetLoc.Y == 0))
			{
				failReason = SpellFailedReason.BadTargets;
			}

			//var zone = m_cast.Caster.Zone;
			//if (zone != null && !zone.Flags.And(ZoneFlags.CanHearthAndResurrectFromArea))
			//{
			//    failReason = SpellFailedReason.NotHere;
			//}
		}

		public override SpellFailedReason CheckValidTarget(WorldObject target)
		{
			if (!((Unit)target).MayTeleport)
			{
				return SpellFailedReason.TargetAurastate;
			}
			return SpellFailedReason.Ok;
		}

		protected override void Apply(WorldObject target)
		{
			if (Effect.Spell.IsHearthStoneSpell && m_cast.CasterChar != null)
			{
				// teleport back home
				var pos = m_cast.CasterChar.BindLocation;
				target.AddMessage(() => ((Unit)target).TeleportTo(pos));
			}
			else
			{
				// teleport to given target location
				var region = m_cast.TargetRegion;
				var pos = m_cast.TargetLoc;
				var ori = m_cast.TargetOrientation;
				target.AddMessage(() => ((Unit)target).TeleportTo(region, pos, ori));
			}
		}

		public override ObjectTypes TargetType
		{
			get { return ObjectTypes.Unit; }
		}
	}
}