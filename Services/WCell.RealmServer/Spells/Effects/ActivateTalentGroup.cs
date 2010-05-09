using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Spells.Effects
{
    public class ActivateTalentGroupHandler : SpellEffectHandler
    {
        public ActivateTalentGroupHandler(SpellCast cast, SpellEffect effect) : base(cast, effect)
        {
        }

        public override void Apply()
        {
            var talentGroupId = Effect.BasePoints;

            if (Cast.CasterChar.SpecProfile != null)
            {
                Cast.CasterChar.SpecProfile.ApplySpec(talentGroupId);
            }
        }
    }
}
