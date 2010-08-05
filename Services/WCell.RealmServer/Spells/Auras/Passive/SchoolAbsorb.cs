/*************************************************************************
 *
 *   file		: SchoolAbsorb.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-26 00:58:48 +0100 (ti, 26 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1225 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.RealmServer.Spells.Auras.Handlers
{

	/// <summary>
	/// Adds damage absorption.
	/// 
	/// There are two kinds of absorbtions:
	/// 1. 100% absorbtion, up until a max is absorbed (usually)
	/// 2. Less than 100% absorption until time runs out (or max is absorbed -> Needs customization, since its usually different each time)
	/// </summary>
	public class SchoolAbsorbHandler : AttackEventEffectHandler
	{
		public int RemainingValue;

		protected override void Apply()
		{
			RemainingValue = EffectValue;
			base.Apply();
		}

		public override void OnBeforeAttack(DamageAction action)
		{
		}

		public override void OnAttack(DamageAction action)
		{
		}

		public override void OnDefend(DamageAction action)
		{
			if (RemainingValue <= 0)
			{
				return;
			}

			if (action.Spell != null && action.Spell.AttributesExD.HasFlag(SpellAttributesExD.CannotBeAbsorbed))
			{
				return;
			}

			var schools = (DamageSchoolMask) m_spellEffect.MiscValue;
			if (schools.HasAnyFlag(action.UsedSchool))
			{
				var value = Math.Min(action.Damage, RemainingValue);
				RemainingValue -= value;
				action.Absorbed += value;
				if (RemainingValue <= 0)
				{
					Owner.AddMessage(m_aura.Cancel);
				}
			}
		}
	}
};