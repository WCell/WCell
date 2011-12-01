using WCell.RealmServer.Entities;

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