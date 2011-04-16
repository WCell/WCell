using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>
    /// This handles all the unused effects
    /// Technically should not be called since no spells have the effect this takes the place of
    /// </summary>
    public class UnusedEffectHandler : SpellEffectHandler
	{
		public UnusedEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		protected override void Apply(WorldObject target)
		{
		}
	}
}