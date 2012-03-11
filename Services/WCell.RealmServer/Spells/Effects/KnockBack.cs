/*************************************************************************
 *
 *   file		: KnockBack.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-27 10:06:23 +0100 (on, 27 jan 2010) $

 *   revision		: $Rev: 1227 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>
    /// Knocks all targets back
    /// </summary>
    public class KnockBackEffectHandler : SpellEffectHandler
    {
        public KnockBackEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject target)
        {
            MovementHandler.SendKnockBack(m_cast.CasterObject, target, Effect.MiscValue / 10f, CalcEffectValue() / 10f);
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Unit; }
        }
    }
}