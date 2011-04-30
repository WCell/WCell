using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras;
using WCell.Constants.Updates;

namespace WCell.RealmServer.Spells.Effects
{
    class RemoveImpairingEffectsHandler : SpellEffectHandler
	{
		public RemoveImpairingEffectsHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{}
        protected override void Apply(WorldObject target)
        {
            var chr = target as Character;
            if (chr != null)
            {
                chr.Auras.RemoveWhere(aura => SpellConstants.MoveMechanics[(int)aura.Spell.SpellCategories.Mechanic] || aura.Handlers.Any(handler => SpellConstants.MoveMechanics[(int)handler.SpellEffect.Mechanic]));
            }
        }
	}
}
