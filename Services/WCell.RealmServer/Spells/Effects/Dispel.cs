/*************************************************************************
 *
 *   file		: Dispel.cs
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
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
	public class DispelEffectHandler : SpellEffectHandler
	{

		public DispelEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		protected override void Apply(WorldObject target)
		{
			var dispelType = (DispelType)Effect.MiscValue;
			if (dispelType == DispelType.None)
			{
				throw new Exception("Invalid DispelType None in Spell: " + Effect.Spell);
			}

			((Unit)target).Auras.RemoveWhere(aura => aura.Spell.DispelType == dispelType, CalcEffectValue());
		}

		public override ObjectTypes TargetType
		{
			get { return ObjectTypes.Unit; }
		}
	}
}