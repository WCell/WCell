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

using WCell.Constants;
namespace WCell.RealmServer.Spells.Auras.Handlers
{
	public interface IDamageAbsorber
	{
		int AbsorbValue
		{
			get;
			set;
		}

		DamageSchoolMask AbsorbSchool
		{
			get;
		}
	}

	/// <summary>
	/// Adds damage absorption.
	/// 
	/// Effect is now two-fold:
	/// 1. Will absorb damage every time wearer is hit
	/// 2. "You have a chance equal to your Parry chance of taking $s1% less damage from a direct damage spell."
	/// </summary>
	public class SchoolAbsorbHandler : AuraEffectHandler, IDamageAbsorber
	{
		private int remainingValue;

		public int AbsorbValue
		{
			get { return remainingValue; }
			set
			{
				remainingValue = value;
				if (remainingValue == 0)
				{
					if (m_aura.IsAdded)
					{
						m_aura.Remove(false);
					}
				}
			}
		}

		public DamageSchoolMask AbsorbSchool
		{
			get { return (DamageSchoolMask)m_spellEffect.MiscValue; }
		}

		protected internal override void Apply()
		{
			remainingValue = EffectValue;
			m_aura.Auras.Owner.AddDmgAbsorption(this);
		}


		protected internal override void Remove(bool cancelled)
		{
			m_aura.Auras.Owner.RemoveDmgAbsorption(this);
		}
	}
};
