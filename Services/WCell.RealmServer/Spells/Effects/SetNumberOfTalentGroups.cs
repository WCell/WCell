using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            if (Cast.CasterChar.SpecProfile != null)
            {
                Cast.CasterChar.SpecProfile.TalentGroupCount = numTalentGroups;
            }
        }
    }
}
