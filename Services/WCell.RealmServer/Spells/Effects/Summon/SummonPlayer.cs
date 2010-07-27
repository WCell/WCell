/*************************************************************************
 *
 *   file		: SummonPlayer.cs
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

using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.Constants.Updates;

namespace WCell.RealmServer.Spells.Effects
{
	public class SummonPlayerEffectHandler : SummonEffectHandler
	{
		public SummonPlayerEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override SpellFailedReason InitializeTarget(WorldObject target)
		{
			var chr = (Character) target;
			if (chr.SummonRequest != null)
			{
				return SpellFailedReason.AlreadyHaveSummon;
			}

			if (!chr.MayTeleport)
			{
				return SpellFailedReason.BadTargets;
			}
			return SpellFailedReason.Ok;
		}

		protected override void Apply(WorldObject target)
		{
			((Character) target).StartSummon(m_cast.CasterUnit);
		}

		public override ObjectTypes CasterType
		{
			get
			{
				return ObjectTypes.Unit;
			}
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