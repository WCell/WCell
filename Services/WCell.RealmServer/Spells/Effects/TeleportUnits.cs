/*************************************************************************
 *
 *   file		: TeleportUnits.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-27 14:06:21 +0100 (on, 27 jan 2010) $

 *   revision		: $Rev: 1228 $
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
    public class TeleportUnitsEffectHandler : SpellEffectHandler
    {
        public TeleportUnitsEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason Initialize()
        {
            var casterUnit = Cast.CasterUnit as NPC;
            if (casterUnit != null && casterUnit.Entry.IsEventTrigger)
            {
            }
            else if (!Effect.Spell.IsHearthStoneSpell &&
                (m_cast.TargetLoc.X == 0 || m_cast.TargetLoc.Y == 0))
            {
                return SpellFailedReason.BadTargets;
            }

            return SpellFailedReason.Ok;
        }

        public override SpellFailedReason InitializeTarget(WorldObject target)
        {
            if (!((Unit)target).MayTeleport)
            {
                return SpellFailedReason.TargetAurastate;
            }
            return SpellFailedReason.Ok;
        }

        protected override void Apply(WorldObject target)
        {
            if (Effect.Spell.IsHearthStoneSpell && m_cast.CasterChar != null)
            {
                // teleport back home
                var pos = m_cast.CasterChar.BindLocation;
                target.AddMessage(() => ((Unit)target).TeleportTo(pos));
            }
            else
            {
                if (Effect.ImplicitTargetB == ImplicitSpellTargetType.BehindTargetLocation)
                {
                    var unit = (Unit)target;
                    if (unit != null)
                    {
                        var o = unit.Orientation;
                        var newx = unit.Position.X - (unit.BoundingRadius + 0.5f) * (float)Math.Cos(o);
                        var newy = unit.Position.Y - (unit.BoundingRadius + 0.5f) * (float)Math.Sin(o);
                        var newpos = new Util.Graphics.Vector3(newx, newy, unit.Position.Z);
                        m_cast.CasterChar.TeleportTo(newpos, o);
                    }
                }
                else
                {
                    // teleport to given target location
                    var map = m_cast.TargetMap;
                    var pos = m_cast.TargetLoc;
                    var ori = m_cast.TargetOrientation;
                    target.AddMessage(() => ((Unit)target).TeleportTo(map, pos, ori));
                }
            }
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Unit; }
        }
    }
}