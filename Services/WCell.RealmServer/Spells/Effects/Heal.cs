/*************************************************************************
 *
 *   file		: Heal.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-04-23 15:13:50 +0200 (fr, 23 apr 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1282 $
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
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Spells.Effects
{

	/// <summary>
	/// Either just heals or trades one Rejuvenation or Regrowth for a lot of healing
	/// </summary>
	public class HealEffectHandler : SpellEffectHandler
	{
		public HealEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		protected override void Apply(WorldObject target)
		{
			((Unit)target).Heal(m_cast.CasterUnit, CalcDamageValue(), Effect);
		}

		public override ObjectTypes TargetType
		{
			get { return ObjectTypes.Unit; }
		}
	}
}