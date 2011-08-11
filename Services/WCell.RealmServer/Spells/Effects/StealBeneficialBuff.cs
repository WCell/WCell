/*************************************************************************
 *
 *   file		: StealBeneficialBuff.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-28 13:29:18 +0100 (to, 28 jan 2010) $

 *   revision		: $Rev: 1230 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras;
using WCell.Constants.Updates;

namespace WCell.RealmServer.Spells.Effects
{
	/// <summary>
	/// Steals a positive Aura -which has a timeout and is not channeled- off the target and applies it on oneself
	/// </summary>
	public class StealBeneficialBuffEffectHandler : SpellEffectHandler
	{
		Aura toSteal;

		public StealBeneficialBuffEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override SpellFailedReason InitializeTarget(WorldObject target)
		{
			var caster = m_cast.CasterObject.SharedReference;
			var auras = m_cast.CasterUnit.Auras;
			foreach (var aura in ((Unit)target).Auras)
			{
				// find a stealable positive auras
				if (aura.IsBeneficial && 
					aura.CanBeStolen && 
					aura.TimeLeft > 100 &&
					auras.GetAura(caster, aura.Id, aura.Spell) == null)
				{
					toSteal = aura;
					return SpellFailedReason.Ok;
				}
			}

			return SpellFailedReason.NothingToSteal;
		}

		// TODO: Make sure that if multiple casters steal from the same target at the same time,
		// that they don't just move around one buff between each other?
		protected override void Apply(WorldObject target)
		{
			var cast = m_cast;
			if (toSteal.IsAdded)
			{
				// remove from owner
				toSteal.Remove(true);

				// apply to caster

				// maximum 2 minutes or the spell's duration
				const int maxTime = 120000;
				if (toSteal.TimeLeft > maxTime)
				{
					toSteal.TimeLeft = maxTime;
				}

				cast.CasterUnit.Auras.AddAura(toSteal);
			}
		}

		public override ObjectTypes TargetType
		{
			get { return ObjectTypes.Unit; }
		}

		public override ObjectTypes CasterType
		{
			get { return ObjectTypes.Unit; }
		}
	}
}