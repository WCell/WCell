/*************************************************************************
 *
 *   file		: SummonPet.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-25 15:24:23 +0100 (ma, 25 jan 2010) $
 *   last author	: $LastChangedBy: mokrago $
 *   revision		: $Rev: 1221 $
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

namespace WCell.RealmServer.Spells.Effects
{
	/// <summary>
	/// Summons the current pet or a custom one
	/// </summary>
	public class SummonPetEffectHandler : SummonEffectHandler
	{
		private bool _ownedPet;

		public SummonPetEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override SpellFailedReason Initialize()
		{
			_ownedPet = Effect.MiscValue == 0 && m_cast.CasterObject is Character;
			if (_ownedPet)
			{
				// TODO: Check for whether Pet may be summoned
				if (((Character)m_cast.CasterObject).ActivePet == null)
				{
					return SpellFailedReason.NoPet;
				}
			}

			return base.Initialize();
		}

		public override SummonType SummonType
		{
			get { return SummonType.SummonPet; }
		}

		public override void Apply()
		{
			if (_ownedPet)
			{
				// Call Pet
				((Character)m_cast.CasterObject).IsPetActive = true;
			}
			else
			{
				Summon(SpellHandler.GetSummonEntry(SummonType));
			}
		}

		public override ObjectTypes CasterType
		{
			get { return ObjectTypes.Unit; }
		}
	}
}