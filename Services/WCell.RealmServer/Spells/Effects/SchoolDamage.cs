/*************************************************************************
 *
 *   file		: SchoolDamage.cs
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

using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
	public class SchoolDamageEffectHandler : SpellEffectHandler
	{
		public SchoolDamageEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		protected override void Apply(WorldObject target)
		{
			((Unit)target).DoSpellDamage((Unit)m_cast.Caster, Effect, CalcDamageValue());
		}

		public override ObjectTypes CasterType
		{
			get { return ObjectTypes.Unit; }
		}

		public override ObjectTypes TargetType
		{
			get { return ObjectTypes.Unit; }
		}
	}

	/// <summary>
	/// Deals EffectValue in % of Melee AP
	/// </summary>
	public class SchoolDamageByAPPctEffectHandler : SchoolDamageEffectHandler
	{
		public SchoolDamageByAPPctEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		protected override void Apply(WorldObject target)
		{
			var caster = (Unit)m_cast.Caster;
			var value = ((caster.TotalMeleeAP * CalcDamageValue()) + 50) / 100;

			((Unit)target).DoSpellDamage((Unit)m_cast.Caster, Effect, value);
		}
	}
}