/*************************************************************************
 *
 *   file		: PersistantAreaAura.cs
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

using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>
    /// Creates a Dynamic Object, which -contrary to what its name suggests- is a static animation in the world and
    /// applies a static <see cref="AreaAura">AreaAura</see> to everyone who is within the radius of influence
    /// </summary>
    public class PersistantAreaAuraEffectHandler : SpellEffectHandler
    {
        public PersistantAreaAuraEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override void Apply()
        {
        }

        protected override void Apply(WorldObject target)
        {
        }

        public override bool HasOwnTargets
        {
            get { return false; }
        }
    }
}