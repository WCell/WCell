using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
	public class ActivateTalentGroupHandler : SpellEffectHandler
	{
		public ActivateTalentGroupHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override void Apply()
		{
			var talentGroupId = Effect.BasePoints;
			var target = Cast.CasterObject as Character;

			if (target != null)
			{
				target.ApplyTalentSpec(talentGroupId);
			}
		}
	}
}