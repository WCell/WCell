/*************************************************************************
 *
 *   file		: Energize.cs
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

using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
	/// <summary>
	/// Adds any kind of Power
	/// </summary>
	public class EnergizeEffectHandler : SpellEffectHandler
	{
		public EnergizeEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override SpellFailedReason InitializeTarget(WorldObject target)
		{
			if (((Unit)target).Power == ((Unit)target).MaxPower)
			{
				return ((Unit)target).PowerType == PowerType.Mana ? SpellFailedReason.AlreadyAtFullMana : SpellFailedReason.AlreadyAtFullPower;
			}
			return base.InitializeTarget(target);
		}

		protected override void Apply(WorldObject target)
		{
			var type = (PowerType)Effect.MiscValue;
			if (type == ((Unit)target).PowerType)
				((Unit)target).Energize(CalcEffectValue(), m_cast.CasterUnit, Effect);
		}

		public override ObjectTypes TargetType
		{
			get { return ObjectTypes.Unit; }
		}
	}

	public class EnergizePctEffectHandler : EnergizeEffectHandler
	{
		public EnergizePctEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		protected override void Apply(WorldObject target)
		{
			var type = (PowerType)Effect.MiscValue;
			if (type == ((Unit)target).PowerType)
			{
				var val = (m_cast.CasterUnit.MaxPower * CalcEffectValue() + 50) / 100;
				((Unit)target).Energize(val, m_cast.CasterUnit, Effect);
			}
		}

		public override ObjectTypes CasterType
		{
			get { return ObjectTypes.Object; }
		}
	}
}