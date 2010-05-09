using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
    public class NotImplementedEffect : SpellEffectHandler
	{
		public NotImplementedEffect(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
			if (cast.Caster is Character)
			{
				(cast.Caster as Character).SendSystemMessage(
					"Spell {0} ({1}) has not implemented Effect {2}. Please report this to the developers",
					cast.Spell.Name, cast.Spell.Id, effect.EffectType);
			}
		}

		protected override void Apply(WorldObject target)
		{
		}
	}
}
