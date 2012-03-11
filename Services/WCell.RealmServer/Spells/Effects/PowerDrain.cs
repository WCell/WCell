/*************************************************************************
 *
 *   file		: PowerDrain.cs
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
    public class PowerDrainEffectHandler : SpellEffectHandler
    {
        public PowerDrainEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason InitializeTarget(WorldObject target)
        {
            if (((Unit)target).MaxPower == 0 || ((Unit)target).PowerType != (PowerType)Effect.MiscValue)
            {
                return SpellFailedReason.BadTargets;
            }
            return SpellFailedReason.Ok;
        }

        protected override void Apply(WorldObject target)
        {
            var type = (PowerType)Effect.MiscValue;

            var value = CalcEffectValue();
            if (type == PowerType.Happiness)
            {
                // for some reason, in case of Happiness, we divide by 1000 apparently
                value /= 1000;
            }

            ((Unit)target).LeechPower(value, Effect.RealPointsPerLevel, m_cast.CasterUnit, Effect);
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