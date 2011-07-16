/*************************************************************************
 *
 *   file		: ForgetSpecialization.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-07 14:58:12 +0800 (Sat, 07 Mar 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 784 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Util.Logging;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
	public class BindEffectHandler : SpellEffectHandler
	{
		public BindEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		protected override void Apply(WorldObject target)
		{
			WorldZoneLocation location;
			if (Effect.ImplicitTargetA == ImplicitSpellTargetType.TeleportLocation
				|| Effect.ImplicitTargetB == ImplicitSpellTargetType.TeleportLocation)
			{
				location = new WorldZoneLocation(m_cast.TargetMap, m_cast.TargetLoc, m_cast.TargetMap.GetZone(m_cast.TargetLoc.X, m_cast.TargetLoc.Y).Template);
			}
			else
			{
				location = new WorldZoneLocation(target.Map, target.Position, target.ZoneTemplate);
			}
			((Character)target).BindTo(target, location);
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