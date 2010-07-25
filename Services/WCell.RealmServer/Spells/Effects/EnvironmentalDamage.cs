/*************************************************************************
 *
 *   file		: EnvironmentalDamage.cs
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

using System;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
	/// <summary>
	/// Mostly caused by burning fires
	/// </summary>
	public class EnvironmentalDamageEffectHandler : SpellEffectHandler
	{
		public EnvironmentalDamageEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		protected override void Apply(WorldObject targetObj)
		{
			var dmg = CalcEffectValue();
			var target = (Unit)targetObj;
			if (dmg < 100)
			{
				// percentage
				dmg = Math.Min(1, (dmg * (int)target.MaxHealth)/100);
			}
			target.DoSpellDamage(m_cast.Caster, Effect, dmg);
		}

		public override ObjectTypes TargetType
		{
			get
			{
				return ObjectTypes.Unit;
			}
		}
	}
}