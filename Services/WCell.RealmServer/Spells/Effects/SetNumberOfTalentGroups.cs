using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Entities;
using WCell.Util;

namespace WCell.RealmServer.Spells.Effects
{
    public class SetNumberOfTalentGroupsHandler : SpellEffectHandler
    {
        public SetNumberOfTalentGroupsHandler(SpellCast cast, SpellEffect effect) : base(cast, effect)
        {
        }

        public override void Apply()
        {
			var numTalentGroups = Effect.BasePoints + 1;
			var target = Cast.CasterObject as Character;

			if (target != null)
            {
				target.Talents.SpecProfileCount = numTalentGroups;
            }
        }
    }
}