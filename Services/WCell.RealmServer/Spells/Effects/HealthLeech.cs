/*************************************************************************
 *
 *   file		: HealthLeech.cs
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

using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
    public class HealthLeechEffectHandler : SpellEffectHandler
    {
        public HealthLeechEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject target)
        {
            ((Unit)target).LeechHealth(m_cast.CasterUnit, CalcDamageValue(), Effect.ProcValue, Effect);
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