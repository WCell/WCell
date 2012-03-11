/*************************************************************************
 *
 *   file		: SummonDeadPet.cs
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

using WCell.Constants.Spells;
using WCell.Constants.Updates;

namespace WCell.RealmServer.Spells.Effects
{
    public class SummonDeadPetEffectHandler : SummonEffectHandler
    {
        public SummonDeadPetEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason Initialize()
        {
            if (m_cast.CasterChar.ActivePet == null)
            {
                return SpellFailedReason.NoPet;
            }
            return SpellFailedReason.Ok;
        }

        public override void Apply()
        {
            m_cast.CasterChar.ActivePet.HealthPct = CalcEffectValue();
        }

        public override ObjectTypes CasterType
        {
            get { return ObjectTypes.Player; }
        }

        public override bool HasOwnTargets
        {
            get { return false; }
        }
    }
}